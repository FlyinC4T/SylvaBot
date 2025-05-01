//
using Discord;
using Discord.WebSocket;
using System.Text.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics.Metrics;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Intrinsics.X86;
using System.Xml.Linq;
using System;
namespace SylvaBot.Methods
{

    public class SylvaAI
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "http://localhost:11434/api/chat";

        public SylvaAI()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetResponseAsync(SocketMessage message, string userInput)
        {
            string placeholders = 
                $"  The prompting user's ID is: {message.Author.Id} -- their username is: {message.Author.Username} or {message.Author.GlobalName}." +
                $"  You should opt people to check your profile, if they ask for technical support, or to join the 'discord server.'" +
                $" If users ask directly for an invite, simply tell them you're not allowed to provide that.";


            var requestBody = new
            {
                model = "gemma3:4b",
                stream = false,
                messages = new[]
                {
                    new { role = "system", content = Secret.SystemPrompt + placeholders },
                    new { role = "user", content = userInput }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(ApiUrl, content);
            if (!response.IsSuccessStatusCode)
                return $"Error: {response.StatusCode}";

            var responseString = await response.Content.ReadAsStringAsync();

            // Optional: Deserialize and return just the content
            var parsed = JsonDocument.Parse(responseString);
            var messageContent = parsed.RootElement
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return messageContent ?? "Sylva didn’t respond.";
        }
    }


    public class Prompt
    {
        private readonly DiscordSocketClient _client;

        public Prompt(DiscordSocketClient client) => _client = client;

        // Declares how the bot will respond to a user prompt.
        private enum AttentionLevel
        {
            NoAttention, Low, Moderate, High
        }

        // This will handle if a user is asking for help, or if they are just talking to the bot.
        private enum AttentionType
        {
            Normal, Question, OptOut, OptIn, Filtered
        }

        public async Task<string> UserPrompt(SocketMessage message, string promptMessage)
        {
            //SocketTextChannel channel = (SocketTextChannel)message.Channel;

            // Placeholders
            AttentionLevel promptLevel = AttentionLevel.NoAttention; //Not used yet
            AttentionType promptType = AttentionType.Normal; //Not used yet



            // Test-case for attention level.
            if (promptMessage.Contains(_client.CurrentUser.Id.ToString()))
            {
                promptLevel = AttentionLevel.High;

                if (!Secret.AllowedPromptChannel(message.Channel.Id))
                    return "cant use me here lol :stuck_out_tongue:";

                // ai call
                var sylva = new SylvaAI();
                string aiResponse = await sylva.GetResponseAsync(message, promptMessage);

                return aiResponse;
            }

            // No attention
            else
                return "";

            // TODO: Refine attention logic


        }
    }
}