using Microsoft.AspNetCore.SignalR;
using nauth_asp.SignalRHubs;
using System.Collections.Concurrent;

namespace nauth_asp.Services
{
    public interface IUserRefreshService
    {
        void QueueUserRefresh(long userId);
        Task SendPendingRefreshesAsync();
    }

    public class UserRefreshService : IUserRefreshService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<AuthHub> _hubContext;
        private const string UserRefreshQueueKey = "UserRefreshQueue";

        public UserRefreshService(IHttpContextAccessor httpContextAccessor, IHubContext<AuthHub> hubContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubContext;
        }

        public void QueueUserRefresh(long userId)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var queue = context.Items[UserRefreshQueueKey] as ConcurrentDictionary<long, bool>;
            if (queue == null)
            {
                queue = new ConcurrentDictionary<long, bool>();
                context.Items[UserRefreshQueueKey] = queue;
            }

            queue.TryAdd(userId, true);
        }

        public async Task SendPendingRefreshesAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            if (context.Items.TryGetValue(UserRefreshQueueKey, out var queueObj) && queueObj is ConcurrentDictionary<long, bool> queue)
            {
                foreach (var userId in queue.Keys)
                {
                    await _hubContext.Clients.Group(userId.ToString()).SendAsync("RefreshData");
                }
                queue.Clear();
            }
        }
    }
}
