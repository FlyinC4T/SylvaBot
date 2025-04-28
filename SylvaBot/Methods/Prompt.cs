//
using Discord;
using Discord.WebSocket;
namespace SylvaBot.Methods
{
    public class Prompt
    {
        private readonly DiscordSocketClient _client;

        public Prompt(DiscordSocketClient client) => _client = client;


        public string UserPrompt(SocketUser promptUser, string prompt)
        {
            int promptAttentionLevel = 0;
            // Declares how the bot will respond to a user prompt.
            //   0 = No attention
            //   1 = Low
            //   2 = Moderate
            //   3 = High


            //int promptAttentionType; //Not used yet
            // Declares what kind of attention the bot will give to a user prompt.
            // This will handle if a user is asking for help, or if they are just talking to the bot.
            // This could also be used to handle if a user is trying to opt-out.
            //   0 = Normal conversation.
            //   1 = Asking for help.
            //   2 = Requesting to Opt-out and delete data.
            //   3 = First attention - Requesting to Opt-in and start storing data.

            // Test-case for attention level.
            if (prompt.Contains(_client.CurrentUser.Id.ToString()))
            {
                promptAttentionLevel = 3; // High attention


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