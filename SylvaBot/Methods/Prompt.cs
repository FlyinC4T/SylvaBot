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

        private readonly string filter = "*filtered.*";


        public SylvaAI()
        {
            _httpClient = new HttpClient();
        }
        
        public async Task<string> GetResponseAsync(SocketMessage message, string userInput)
        {
            // Remove <@688689235066224672> from userInput using regex, because sylva is a lil dumb sometimes :sob:
            userInput = System.Text.RegularExpressions.Regex.Replace(userInput, @"<@688689235066224672>", "Sylva");

            string placeholders =

                "  You should opt people to check your profile, if they ask for technical support, or to join the 'discord server.'" +
                " If users ask directly for an invite, simply tell them you're not allowed to provide that." +
                " You can ping users, and you can ping this user by converting their ID by formatting it, <@>, you can replace the numbers in this format with their actual ID." +
                "  You are not allowed to use @everyone, or @here, or any roles that are put here." +
                "  If anything is related to illegal actions, drugs, cyber criminality or relative, do not act on it." +

                "  You are given messages from various chatters, never read them out loud, only answer them with your response, you are in their chat." +

                "  Pinging requires an '@' symbol, before the ID, to work." +

                "  You have a variety of emojis you can use as expressions, these have their use-cases and they are entirely optional." +
                " Emojis can be formated with :emoji: and some can also be formated with <:emoji:emoji-ID>" +

                " - When something is lovely, use :heart:" +
                " - When feeling tired, use :sleepy_face:" +
                " - When something is shocking, use :flushed:" +
                " - When confused or clueless, use :face_with_raised_eyebrow:" +
                " - When something sounds positive, and you're happy, use :sweat_smile: or :smile:" +
                " - When something is cool, use <:gigachad:1367922141839753327>" +

                "  You do not have to use emojis every time.";

            userInput = $"{message.Author.Id} | {message.Author.Username}: " + userInput;

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

            if (messageContent != null)
                messageContent = messageContent.Replace("@everyone", filter).Replace("@here", filter);

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

                await Logger.LoggerAsync(LogSeverity.Info, $"{message.Author.Username} {message.Content}");

                // ai call
                var sylva = new SylvaAI();
                string response = await sylva.GetResponseAsync(message, promptMessage);

                await Logger.LoggerAsync(LogSeverity.Info, "Sylva: " + response);

                return response;
            }

            // No attention
            else
                return "";

            // TODO: Refine attention logic


        }
    }
}