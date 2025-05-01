//
using Discord;
using Discord.WebSocket;
using SylvaBot.Methods;
using System.Reflection;

namespace SylvaBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly Dictionary<string, ISlashCommand> _commands = new();

        public CommandHandler(DiscordSocketClient client)
        {
            _client = client;
        }

        public void Initialize()
        {
            _client.Ready += Ready;
            _client.SlashCommandExecuted += CommandExecutedAsync;
        }

        private async Task Ready()
        {
            var guild = _client.GetGuild((ulong)Variables.PublicIds.MainServer);

            // Dynamically load all ISlashCommand classes
            var commandTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(ISlashCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in commandTypes)
            {
                var command = Activator.CreateInstance(type) as ISlashCommand;

                if (command != null) // Ensure command is not null
                {
                    _commands.Add(command.Name, command);
                    await guild.CreateApplicationCommandAsync(command.BuildCommand().Build());
                }
            }

            Logger.LoggerAsync(LogSeverity.Debug, $"[Slash Commands] {_commands.Count} commands registered!");
        }

        private async Task CommandExecutedAsync(SocketSlashCommand command)
        {
            if (_commands.TryGetValue(command.Data.Name, out var slashCommand))
                await slashCommand.ExecuteAsync(command);

            else await command.RespondAsync("Unknown command.");
        }
    }
}
