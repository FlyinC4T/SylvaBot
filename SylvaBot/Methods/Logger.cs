//
using Discord;

namespace SylvaBot.Methods
{
    public class Logger
    {
        public static void LoggerAsync(LogSeverity severity, string logMessage)
        {
            // Do some logging.
            LogMessage log = new LogMessage(severity, "Logger", logMessage);
            Start.LogAsync(log);

        }
    }
}
