//
using Discord;
using Discord.WebSocket;
using System.Diagnostics;

using static SylvaBot.Secret;
using static SylvaBot.Variables;

namespace SylvaBot
{
    // Main class
    class Start
    {
        private readonly DateTime startTime = Process.GetCurrentProcess().StartTime.ToUniversalTime();

        private DiscordSocketClient _client;
        //private CommandHandler _commandHandler;

        public Start()
        {
            _client = new DiscordSocketClient();
        }

        // Placeholders
        private string logMessages = "";

        // Main method
#pragma warning disable IDE0060 // Remove unused parameter
        static async Task Main(string[] args) => await new Start().MainAsync();
#pragma warning restore IDE0060 // Remove unused parameter

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            var _commandHandler = new CommandHandler(_client);

            _client.Ready += Ready;
            _client.Log += LogAsync;
            _client.MessageReceived += MessageReceivedAsync;


            await _client.LoginAsync(TokenType.Bot, Secret.token);
            await _client.StartAsync();

            await _commandHandler.InitializeAsync();

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
            IMessageChannel logChannel = (IMessageChannel)_client.GetChannel( (ulong)PrivateIds.LogChannel );
            IMessageChannel statusChannel = (IMessageChannel)_client.GetChannel( (ulong)PublicIds.StatusChannel );

            // Set a status.
            await _client.SetStatusAsync(UserStatus.Idle);
            await _client.SetCustomStatusAsync("discord.gg/FJyChnMW5j - Support Server");

            await logChannel.SendMessageAsync("Sylva Connected.");

            _ = Task.Run(async () => {

                // Placeholders
                ulong messageId = 0;
                string messageContent = "";

                while (true)
                {
                    await Task.Delay(10000);

                    double memoryUsage = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);

                    if (memoryUsage > 2048)
                    {
                        Console.WriteLine("Memory usage too high. Exiting...");
                        await logChannel.SendMessageAsync("# :warning: Memory usage too high. Exiting...");

                        Environment.Exit(1); // Kill process
                    }

                    // If we don't have any log messages, skip this iteration.
                    if (logMessages == "") continue;

                    // Update placeholders with new settings, and send the message.
                    messageContent = $"```\n{logMessages}\n```";
                    messageId = (await logChannel.SendMessageAsync(messageContent)).Id;

                    // Reset log messages.
                    logMessages = "";
                }
            });
            
            _ = Task.Run(async () =>
            {
                // Placeholders
                ulong messageId = 1366796274728177704;
                
                long unixStartTime = ((DateTimeOffset)startTime).ToUnixTimeSeconds();

                while (true)
                {
                    await Task.Delay(60000);

                    long unixLoopTime = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();

                    double memoryUsage = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);

                    var embed = new EmbedBuilder()
                        .WithTitle("Live Sylva Status 🌸 (60s Delay)")
                        .AddField("Client Latency", $"{_client.Latency} ms", true)
                        .AddField("Memory", $"{memoryUsage} / 2 GB", true)
                        .AddField("Up since", $"<t:{unixStartTime}:R>", true)
                        .AddField("Up since", $"<t:{unixLoopTime}:R> (If past 1 minute, she's offline.)", true)

                        .WithColor(Variables.BaseColor)
                        .WithFooter(footer => footer.Text = "Sylva Status")
                        .WithCurrentTimestamp()
                        .Build();
                    // Modify already-sent message.
                    await statusChannel.ModifyMessageAsync(messageId, (msg) =>
                    {
                        msg.Embed = embed;
                        msg.Content = " ";
                    });
                }
            });
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // Ignore messages from the bot itself or system messages
            if (message.Author.Id == _client.CurrentUser.Id || message.Author.IsBot)
                return;

            string response = new Methods.Prompt(_client).UserPrompt(message.Author, message.Content);

            // Only send a response if it's not empty
            if (!string.IsNullOrWhiteSpace(response))
                await message.Channel.SendMessageAsync(response);
        }
    }
}