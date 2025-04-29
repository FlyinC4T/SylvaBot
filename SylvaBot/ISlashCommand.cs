//
using Discord;
using Discord.WebSocket;

namespace SylvaBot
{
    public interface ISlashCommand
    {
        string Name { get; }
        string Description { get; }
        SlashCommandBuilder BuildCommand();
        Task ExecuteAsync(SocketSlashCommand command);
    }
}
