//
using Discord;
using System.Drawing;

namespace SylvaBot
{
    public class Variables
    {
        public enum PublicIds : ulong
        {
            MainServer = 1121545096593154150, // Project Sylva AI
            StatusChannel = 1366793583121928344 // #live-status
        }

        public static Discord.Color BaseColor { get; } = new Discord.Color(120, 200, 220); // Silver
    }
}