//
using Discord;
using Discord.WebSocket;
using System.Diagnostics;

using SylvaBot.Methods;
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
        private static string logMessages = "";

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

            _commandHandler.Initialize();

            // Block the program from exiting
            await Task.Delay(-1);
        }

        public static Task LogAsync(LogMessage log)
        {
            //if (log.Severity != LogSeverity.Info)
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
            await _client.SetCustomStatusAsync("WIP AI Chatbot - discord.gg/FJyChnMW5j");

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

                    long unixLoopTime = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();

                    double memoryUsage = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);

                    var ollamaProcess = Process.GetProcessesByName("ollama").FirstOrDefault();

                    var embed = new EmbedBuilder()
                        .WithTitle("Live Sylva Status 🌸 (60s Delay)")
                        .AddField("Client Latency", $"{_client.Latency} ms", true)
                        .AddField("Memory", $"{memoryUsage} / 2 GB", true)
                        .AddField("Up since", $"<t:{unixStartTime}:R>", true)
                        .AddField("Online", $"<t:{unixLoopTime}:R> (If past 1 minute, she's offline.)", false)
                        .AddField("Ollama Memory Usage", $"{ollamaProcess?.WorkingSet64 / (1024 * 1024)} MB", false)
                        .WithColor(Variables.BaseColor)
                        .WithFooter(footer => footer.Text = "Sylva Live Status")
                        .WithCurrentTimestamp()
                        .Build();

                    // Modify already-sent message.
                    await statusChannel.ModifyMessageAsync(messageId, (msg) =>
                    {
                        msg.Embed = embed;
                        msg.Content = " ";
                    });

                    await Task.Delay(60000);
                }
            });
        }

        private readonly SemaphoreSlim _messageQueueSemaphore = new SemaphoreSlim((int)ClientLimits.MaxResponseTasks); // Limit to 10 concurrent tasks

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id || message.Author.IsBot)
                return;

            await _messageQueueSemaphore.WaitAsync(); // Wait for an available slot in the queue
            try
            {
                _ = Task.Run(async () =>
                {
                    if (message.ToString().Contains(_client.CurrentUser.Id.ToString()))
                        await message.Channel.TriggerTypingAsync();

                    string response = await new Prompt(_client).UserPrompt(message, message.Content);

                    int maxLength = (int)ClientLimits.MaxResponseLength;
                    if (response.Length > maxLength)
                        response = response.Substring(0, maxLength) + $"\n\n-# *This response was limited to <{maxLength} characters due to message limit.*";

                    // Only send a response if it's not empty
                    if (!string.IsNullOrWhiteSpace(response))
                        await message.Channel.SendMessageAsync(response);
                });
            }
            finally
            {
                _messageQueueSemaphore.Release(); // Release the slot in the queue
            }
        }
    }
}