using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using nauth_asp;
using nauth_asp.AuthHandlers;
using nauth_asp.AuthHandlers.AuthorizationRequirments;
using nauth_asp.DbContexts;
using nauth_asp.Exceptions;
using nauth_asp.Helpers;
using nauth_asp.Repositories;
using nauth_asp.Services;
using nauth_asp.Services.ObjectStorage;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace nauth_asp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            while(true)
            {
                Console.Write($"\r{SnowflakeGlobal.Generate()}");
                Console.ReadKey();
            }



            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            };
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            builder.Services.AddSingleton(jsonSerializerOptions);


            // Add services to the container
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy;
                    options.JsonSerializerOptions.WriteIndented = jsonSerializerOptions.WriteIndented;
                    foreach (var converter in jsonSerializerOptions.Converters)
                    {
                        options.JsonSerializerOptions.Converters.Add(converter);
                    }
                });

            // Add DbContext
            builder.Services.AddDbContext<NauthDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add AutoMapper
            builder.Services.AddAutoMapper(cfg =>
            {

                cfg.LicenseKey = builder.Configuration["AutoMapper:licenceKey"];
                cfg.AddMaps(typeof(Program));
            });

            // Register Repositories
            builder.Services.AddScoped<EmailTemplateRepository>();
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<SessionRepository>();
            builder.Services.AddScoped<PermissionRepository>();
            builder.Services.AddScoped<_2FARepository>();
            builder.Services.AddScoped<EmailActionRepository>();
            builder.Services.AddScoped<ServiceRepository>();
            builder.Services.AddScoped<UserPermissionRepository>();


            // Register Services
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<EmailTemplateService>();
            builder.Services.AddScoped<IObjectStorageService, ObjectStorageService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<SessionService>();
            builder.Services.AddScoped<PermissionService>();
            builder.Services.AddScoped<_2FAService>();
            builder.Services.AddScoped<EmailActionService>();
            builder.Services.AddScoped<ServiceService>();
            builder.Services.AddScoped<NauthService>();
            builder.Services.AddScoped<UserPermissionService>();

            builder.Services.AddSignalR();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //helpers
            builder.Services.AddScoped<ApplyTokenProvider>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<SessionValidator>();
            builder.Services.AddScoped<IAuthorizationHandler, ValidSessionHandler>();

            // Claims enrichment after authentication
            builder.Services.AddTransient<IClaimsTransformation, PermissionClaimsTransformer>();

            var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: myAllowSpecificOrigins,
                                  policy =>
                                  {
                                      var corsOrigins = builder.Configuration.GetSection("CorsConfig").Value;
                                      var env = builder.Environment.EnvironmentName;
                                      if (!string.IsNullOrEmpty(corsOrigins))
                                      {
                                          var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                                  .Select(o => o.Trim())
                                                                  .ToArray();
                                          policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                                      }
                                      else
                                      {
                                          // No configured origins; in development, allow localhost defaults, otherwise deny by default
                                          if (builder.Environment.IsDevelopment())
                                          {
                                              policy.WithOrigins("http://localhost:3000", "http://localhost:5035").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                                          }
                                      }
                                  });
            });

            builder.Services
                   .AddAuthentication(options =>
                   {
                       options.DefaultAuthenticateScheme = "Smart";
                       options.DefaultChallengeScheme = "Smart";
                   })
                   .AddPolicyScheme("Smart", "Smart", options =>
                   {
                       options.ForwardDefaultSelector = ctx =>
                       {
                           var authHeader = ctx.Request.Headers.Authorization.ToString();
                           if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                           {
                               var cookieKey = builder.Configuration["JWT:Cookiekey"];
                               if (cookieKey != null && ctx.Request.Cookies.TryGetValue(cookieKey, out var cookieValue) && !string.IsNullOrEmpty(cookieValue))
                               {
                                   ctx.Request.Headers.Authorization = "Bearer " + cookieValue;
                               }
                           }
                           return JwtBearerDefaults.AuthenticationScheme;
                       };
                   })
                   .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                   {
                       options.TokenValidationParameters = new TokenValidationParameters
                       {
                           ValidateIssuer = true,
                           ValidateAudience = true,
                           ValidateLifetime = true,
                           ValidateIssuerSigningKey = true,
                           ValidIssuer = builder.Configuration["JWT:Issuer"],
                           ValidAudience = builder.Configuration["JWT:Audience"],
                           IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:secretKey"]!)),
                           ClockSkew = TimeSpan.Zero
                       };

                       options.Events = new JwtBearerEvents
                       {
                           OnTokenValidated = async ctx =>
                           {
                               var sub = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                               var sid = ctx.Principal?.FindFirst("sid")?.Value;
                               var serviceId = ctx.Principal?.FindFirst("serviceId")?.Value;
                               if (serviceId  == null  && (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(sid)))
                               {
                                   ctx.Fail("Missing sub or sid");
                                   ctx.HttpContext.AddAuthenticationFailureReason(AuthFailureReasons.SessionExpired);
                                   return;
                               }

                               var validator = ctx.HttpContext.RequestServices.GetRequiredService<SessionValidator>();
                               var result = await validator.ValidateAsync(long.Parse(sub ?? "0"), long.Parse(sid ?? "0"), serviceId != null ? long.Parse(serviceId) : null);

                               if (!result.IsActive)
                               {
                                   ctx.Fail("Session invalid");
                                   ctx.HttpContext.AddAuthenticationFailureReason(AuthFailureReasons.SessionExpired);
                                   return;
                               }

                               ctx.HttpContext.Items["Session"] = result.Session;
                               ctx.HttpContext.Items["User"] = result.User;
                               ctx.HttpContext.Items["Service"] = result.Service;
                           }
                       };
                   });

            // Authorization with policies
            builder.Services.AddAuthorization(options =>
            {

                var defaultPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);

                defaultPolicyBuilder.RequireAuthenticatedUser();
                defaultPolicyBuilder.Requirements.Add(new ValidSessionRequirement());
                options.DefaultPolicy = defaultPolicyBuilder.Build();

                options.AddPolicy("allowNoEmail", policy =>
                {
                    policy.Requirements.Clear();
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new ValidSessionRequirement(requireVerifiedEmail: false));
                });

                options.AddPolicy("allowDisabled", policy =>
                {
                    policy.Requirements.Clear();
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new ValidSessionRequirement(requireEnabledUser: false, requireVerifiedEmail: false));
                });

                options.AddPolicy("allowNo2FA", policy =>
                {
                    policy.Requirements.Clear();
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new ValidSessionRequirement(ignore2FA: true, requireVerifiedEmail: false));
                });



                foreach (var key in Enum.GetValues<NauthPermissions>().Select(k => k.ToString()))
                {
                    options.AddPolicy(key, policy =>
                    {
                        policy.Requirements.Clear();
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new ValidSessionRequirement(requireVerifiedEmail: true, requireEnabledUser: true, require2FASetup: true, ignore2FA: false));
                        policy.Requirements.Add(new PermissionRequirement(key));
                    });
                }

                options.AddPolicy("ValidService", policy =>
                {
                    policy.Requirements.Clear();
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new ValidSessionRequirement(isServiceSession: true, requireVerifiedEmail: true, requireEnabledUser: true, require2FASetup: false, ignore2FA: true));
                });

                options.AddPolicy("UserOwnsService", p => p.Requirements.Add(new UserOwnsServiceRequirement()));
                options.AddPolicy("UserOwnsSession", p => p.Requirements.Add(new UserOwnsSessionRequirement()));
                options.AddPolicy("UserOwnsEmailAction", p => p.Requirements.Add(new UserOwnsEmailActionRequirement()));

            });

            builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, UserOwnsServiceHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, UserOwnsSessionHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, UserOwnsEmailActionHandler>();
            var app = builder.Build();

            app.UseCors(myAllowSpecificOrigins);

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ErrorHandlerMiddleware>(jsonSerializerOptions);

            app.UseStatusCodePages(async context =>
            {
                var response = context.HttpContext.Response;
                var requestServices = context.HttpContext.RequestServices;
                var config = requestServices.GetRequiredService<IConfiguration>();
                var env = requestServices.GetRequiredService<IWebHostEnvironment>();
                var jsonOptions = requestServices.GetRequiredService<JsonSerializerOptions>();

                response.ContentType = "application/json";

                if (response.StatusCode == 403)
                {
                    Console.WriteLine($"401 (Unauthorized) response intercepted on {context.HttpContext.Request.Path}");

                    var reasons = context.HttpContext.GetAuthenticationFailureReasons();
                    if (reasons?.Contains(AuthFailureReasons.SessionExpired) == true)
                    {
                        response.Cookies.Delete(config["JWT:Cookiekey"]!);
                        var cooptions = new CookieOptions { Path = "/", IsEssential = true, HttpOnly = false, Secure = false, Domain = config["Frontend:CookieDomain"] };
                        response.Cookies.Append("authenticated", "false", cooptions);
                    }
                    await response.WriteAsync(JsonSerializer.Serialize(new ResponseWrapper<string>(WrResponseStatus.Forbidden, null, reasons), jsonOptions));
                }
                else if (response.StatusCode == 401)
                {
                    Console.WriteLine($"401 (Unauthorized) response intercepted on {context.HttpContext.Request.Path}");

                    response.Cookies.Delete(config["JWT:Cookiekey"]!);
                    var cooptions = new CookieOptions
                    {
                        Path = "/",
                        IsEssential = true,
                        HttpOnly = false,
                        Secure = false,
                        Domain = config["Frontend:CookieDomain"]
                    };
                    response.Cookies.Append("authenticated", "false", cooptions);
                    await response.WriteAsync(JsonSerializer.Serialize(new ResponseWrapper<string>(WrResponseStatus.Unauthorized), jsonOptions));
                }
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<SignalRHubs.AuthHub>("/authhub");


            using (var scope = app.Services.CreateScope())
            {
                var permissionService = scope.ServiceProvider.GetRequiredService<PermissionService>();

                var serviceService = scope.ServiceProvider.GetRequiredService<ServiceService>();

                await serviceService.EnsureMother();

                await permissionService.InjectPermissions();
            }

            await app.RunAsync();
        }
    }
}





public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, JsonSerializerOptions jsonSerializerOptions)
    {
        _next = next;
        _logger = logger;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            Console.WriteLine("Exception caught in ErrorHandlerMiddleware: " + error.Message);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("The response has already started, the error handler will not be executed.");
                throw;
            }

            var response = context.Response;
            response.ContentType = "application/json";

            object responseBody;

            switch (error)
            {
                case NauthException authEx:
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    responseBody = new ResponseWrapper<string>(authEx.Status);
                    break;
                default:
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    _logger.LogError(error, "An unhandled exception has occurred.");
                    responseBody = new ResponseWrapper<string>(WrResponseStatus.InternalError);
                    break;
            }

            var result = JsonSerializer.Serialize(responseBody, _jsonSerializerOptions);
            await response.WriteAsync(result);
        }
    }
}