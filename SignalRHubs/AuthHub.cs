using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using nauth_asp.Helpers;
using nauth_asp.Services;
using System.Text.RegularExpressions;

namespace nauth_asp.SignalRHubs
{
    [Authorize("allowNoEmail")]
    public class AuthHub() : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var user = Context.GetHttpContext().NauthUser() ?? throw new InvalidOperationException("User cannot be null on an authorized hub.");

            var sub = user.Id.ToString();
            await Groups.AddToGroupAsync(Context.ConnectionId, sub);

            var sid = Context.GetHttpContext().NauthSession().Id.ToString();
            await Groups.AddToGroupAsync(Context.ConnectionId, sid);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            var user = Context.GetHttpContext().NauthUser() ?? throw new InvalidOperationException("User cannot be null on an authorized hub.");

            var sub = user.Id.ToString();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sub);

            var sid = Context.GetHttpContext().NauthSession().Id.ToString();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sid);
        }
    }
}
