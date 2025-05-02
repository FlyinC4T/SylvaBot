//
using Discord;

namespace SylvaBot.Methods
{
    public class Logger
    {
        public static async Task LoggerAsync(LogSeverity severity, string logMessage)
        {
            // Do some logging.
            LogMessage log = new LogMessage(severity, "Logger", logMessage);
            await Start.LogAsync(log);
        }
    }
}
