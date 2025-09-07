using CommandSystem;
using Exiled.API.Features;
using MultiBroadcast.API;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace MultiBroadcast.Commands.Subcommands
{
    public class List : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count > 0)
            {
                Player player = Player.Get(string.Join(" ", (IEnumerable<string>)arguments));
                if (player == null)
                {
                    response = "Player not found.";
                    return false;
                }
                StringBuilder stringBuilder1 = new StringBuilder($"\n<b>{player.Nickname}'s Broadcast List:</b>\n");
                foreach (MultiBroadcast.API.Broadcast broadcast in player.GetBroadcasts())
                {
                    StringBuilder stringBuilder2 = stringBuilder1;
                    string str;
                    if (!string.IsNullOrEmpty(broadcast.Tag))
                        str = $" - ID: {broadcast.Id}, Duration: {broadcast.Duration}, Priority: {broadcast.Priority}, Tag: {broadcast.Tag}, Text: {broadcast.Text}\n";
                    else
                        str = $" - ID: {broadcast.Id}, Duration: {broadcast.Duration}, Priority: {broadcast.Priority}, Text: {broadcast.Text}\n";
                    stringBuilder2.Append(str);
                }
                if (player.GetBroadcasts().Count() == 0)
                    stringBuilder1.Append("No broadcasts found.");
                response = stringBuilder1.ToString();
                return true;
            }
            StringBuilder stringBuilder3 = new StringBuilder("\n<b>Current Broadcast List:</b>\n");

            //!
            var all = MultiBroadcast.API.MultiBroadcast.GetAllBroadcasts();
            foreach (var broadcast in all.Values.SelectMany(list => list))
            {
                StringBuilder stringBuilder4 = stringBuilder3;
                string str;
                if (!string.IsNullOrEmpty(broadcast.Tag))
                    str = $" - ID: {broadcast.Id}, Duration: {broadcast.Duration}, Priority: {broadcast.Priority}, Tag: {broadcast.Tag}, Text: {broadcast.Text}\n";
                else
                    str = $" - ID: {broadcast.Id}, Duration: {broadcast.Duration}, Priority: {broadcast.Priority}, Text: {broadcast.Text}\n";
                stringBuilder4.Append(str);
            }
            if (MultiBroadcast.API.MultiBroadcast.GetAllBroadcasts().Count == 0)
                stringBuilder3.Append("No broadcasts found.");
            response = stringBuilder3.ToString();
            return true;
        }

        public string Command { get; } = "list";

        public string[] Aliases { get; } = new string[1] { "l" };

        public string Description { get; } = "List all broadcasts.";

        public bool SanitizeResponse { get; } = false;
    }

}
