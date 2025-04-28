//
using Discord;
using Discord.WebSocket;

namespace SylvaBot
{
    // Main class
    class Start
    {
        private DiscordSocketClient _client;

        public Start() => _client = new DiscordSocketClient();


        // Placeholders
        private string logMessages = "";

        // Main method
#pragma warning disable IDE0060 // Remove unused parameter
        static async Task Main(string[] args) => await new Start().MainAsync();
#pragma warning restore IDE0060 // Remove unused parameter

        public async Task MainAsync()
        {
            if (string.IsNullOrEmpty(Secret.token))
            {
                Console.WriteLine("Bot token not found.");
                return;
            }

            _client = new DiscordSocketClient();

            _client.Ready += Ready;
            _client.Log += LogAsync;
            _client.MessageReceived += MessageReceivedAsync;

            await _client.LoginAsync(TokenType.Bot, Secret.token);
            await _client.StartAsync();


            // Block the program from exiting
            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            if (log.Severity != LogSeverity.Info)
                logMessages += $"{log.Severity}  |  {DateTime.Now}  |  {log.Source}\n    {log.Message}\n\n";

            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private async Task Ready()
        {
            //var mainChannel = _client.GetChannel(Variables.MainChannelID) as IMessageChannel;
            IMessageChannel logChannel = (IMessageChannel)_client.GetChannel(Variables.LogChannelID);

            // Set a status.
            await _client.SetStatusAsync(UserStatus.Idle);
            await _client.SetCustomStatusAsync("discord.gg/FJyChnMW5j - Support Server");

            await logChannel.SendMessageAsync("Sylva Connected.");

            while (true)
            {
                await Task.Delay(10000);

                // If we don't have any log messages, skip this iteration.
                if (logMessages == "") continue;

                await logChannel.SendMessageAsync($"```\n{logMessages}\n```");
                logMessages = "";
            }
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            var response = new Methods.Prompt(_client).UserPrompt(message.Author, message.Content);

            if (response == "")
                return;

            await message.Channel.SendMessageAsync(response);
        }
    }
}