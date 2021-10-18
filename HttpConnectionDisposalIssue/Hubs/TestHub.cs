using Microsoft.AspNetCore.SignalR;

using System.Threading.Tasks;

namespace HttpConnectionDisposalIssue
{
    public class TestHub : Hub
    {
        public async Task Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            await Clients.All.SendAsync("broadcastMessage", name, message);
        }
    }
}