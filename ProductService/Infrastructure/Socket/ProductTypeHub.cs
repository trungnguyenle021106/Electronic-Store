using Microsoft.AspNetCore.SignalR;

namespace ProductService.Infrastructure.Socket
{
    public class ProductTypeHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            // Server sẽ gửi tin nhắn này đến tất cả các client đã kết nối
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"[SignalR] Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"[SignalR] Client disconnected: {Context.ConnectionId}, Exception: {exception?.Message}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
