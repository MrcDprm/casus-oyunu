using Microsoft.AspNetCore.SignalR;
using casus_oyunu.Models;
using System.Threading.Tasks;

namespace casus_oyunu.Hubs
{
    public class GameHub : Hub
    {
        public async Task JoinRoom(string roomCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Group(roomCode).SendAsync("UserJoined", Context.User?.Identity?.Name ?? "Anonymous");
        }

        public async Task LeaveRoom(string roomCode)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Group(roomCode).SendAsync("UserLeft", Context.User?.Identity?.Name ?? "Anonymous");
        }

        public async Task UpdateGameState(string roomCode, object gameState)
        {
            await Clients.Group(roomCode).SendAsync("GameStateUpdated", gameState);
        }

        public async Task SendVote(string roomCode, int targetParticipantId, string voterName)
        {
            await Clients.Group(roomCode).SendAsync("VoteReceived", targetParticipantId, voterName);
        }

        public async Task StartGame(string roomCode)
        {
            await Clients.Group(roomCode).SendAsync("GameStarted", roomCode);
        }

        public async Task EndGame(string roomCode, string result)
        {
            await Clients.Group(roomCode).SendAsync("GameEnded", result);
        }

        public async Task SendMessage(string roomCode, string message)
        {
            await Clients.Group(roomCode).SendAsync("MessageReceived", Context.User?.Identity?.Name ?? "Anonymous", message);
        }

        public async Task PlayerRoleAssigned(string roomCode, string playerName, bool isSpy, string assignedWord)
        {
            await Clients.Group(roomCode).SendAsync("RoleAssigned", playerName, isSpy, assignedWord);
        }

        public async Task TimerUpdate(string roomCode, int timeLeft)
        {
            await Clients.Group(roomCode).SendAsync("TimerUpdated", timeLeft);
        }

        public async Task VoteResult(string roomCode, object voteResults)
        {
            await Clients.Group(roomCode).SendAsync("VoteResults", voteResults);
        }
    }
} 