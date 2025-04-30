//
using Discord;
using Discord.WebSocket;
namespace SylvaBot.Methods
{
    public class Prompt
    {
        private readonly DiscordSocketClient _client;

        public Prompt(DiscordSocketClient client) => _client = client;

        private enum AttentionLevel
        {
            NoAttention, Low, Moderate, High
        }
        
        private enum AttentionType
        {
            Normal, Question, OptOut, OptIn, Filtered
        }

        public string UserPrompt(SocketUser promptUser, string prompt)
        {
            // Declares how the bot will respond to a user prompt.
            AttentionLevel promptLevel = AttentionLevel.NoAttention; //Not used yet

            // This will handle if a user is asking for help, or if they are just talking to the bot.
            AttentionType promptType = AttentionType.Normal; //Not used yet

            // Test-case for attention level.
            if (prompt.Contains(_client.CurrentUser.Id.ToString()))
            {
                promptLevel = AttentionLevel.High;


                // Test message.
                return $"Hi <@{promptUser.Id}>!";
            }

            // No attention
            else
                return "";

            // TODO:
            // AI Logic, etc.
            // Handling of attention level.


        }
    }
}