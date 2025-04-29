//
using Discord;
using Discord.WebSocket;
using System.Diagnostics;

namespace SylvaBot.Commands
{
    public class StatsCommand : ISlashCommand
    {
        public string Name => "stats";
        public string Description => "Memory, Uptime, Ping, Avg. Ping, and other info.";

        private static readonly Stopwatch Uptime = Stopwatch.StartNew();

        public SlashCommandBuilder BuildCommand()
        {
            return new SlashCommandBuilder()
                .WithName(Name)
                .WithDescription(Description);
        }

        public async Task ExecuteAsync(SocketSlashCommand command)
        {
            var process = Process.GetCurrentProcess();

            var memoryUsage = process.WorkingSet64 / (1024 * 1024);

            var elapsed = Uptime.ElapsedMilliseconds;
            string uptime = $"{elapsed / (1000 * 60 * 60)} Hours";

            var cpuUsage = $"{Environment.ProcessorCount} Threads";


            var embed = new EmbedBuilder()
                .WithTitle("Sylva System Status 🌸")
                .AddField("Memory", $"{memoryUsage} / 2 GB", true)
                .AddField("Uptime", uptime, true)
                //.AddField("CPU", cpuUsage, true)
                .WithColor(Variables.BaseColor)
                .WithFooter(footer => footer.Text = "Sylva Status")
                .WithCurrentTimestamp()
                .Build();

            await command.RespondAsync(embed: embed);
        }
    }
}
