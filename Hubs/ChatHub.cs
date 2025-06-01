using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    // Send message from sender to receiver by email
    public async Task SendMessage(string receiverEmail, string message)
    {
        var senderEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

        if (!string.IsNullOrEmpty(receiverEmail) && !string.IsNullOrEmpty(senderEmail))
        {
            // Send message to receiver
            await Clients.User(receiverEmail).SendAsync("ReceiveMessage", senderEmail, message);

            // Echo message back to sender to update sender UI
            await Clients.User(senderEmail).SendAsync("ReceiveMessage", senderEmail, message);
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

        if (!string.IsNullOrEmpty(userEmail))
        {
            // Add the connection to a group identified by user email so that Clients.User(email) works
            await Groups.AddToGroupAsync(Context.ConnectionId, userEmail);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(System.Exception exception)
    {
        var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

        if (!string.IsNullOrEmpty(userEmail))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userEmail);
        }

        await base.OnDisconnectedAsync(exception);
    }
}

// Email-based IUserIdProvider to identify users by email for SignalR


public class EmailBasedUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        // Use email claim as user identifier
        return connection.User?.FindFirst(ClaimTypes.Email)?.Value;
    }
}

