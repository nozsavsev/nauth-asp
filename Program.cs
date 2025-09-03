
using Microsoft.EntityFrameworkCore;
using nauth_asp.DbContexts;

namespace nauth_asp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            
            // Add DbContext
            builder.Services.AddDbContext<NauthDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            
            // Add AutoMapper
            builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program)));
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
