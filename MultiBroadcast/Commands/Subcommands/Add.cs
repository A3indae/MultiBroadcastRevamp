using CommandSystem;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MultiBroadcast.Commands.Subcommands
{
    public class Add : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response) //RIP [UnscopedRef]
        {
            if (arguments.Count < 1)
            {
                response = "Usage: multibroadcast add <map/player>";
                return false;
            }
            switch (arguments.At<string>(0).ToLower()[0])
            {
                case 'm':
                    if (arguments.Count < 3)
                    {
                        response = "Usage: multibroadcast add map <duration> <text>";
                        return false;
                    }
                    ushort result1;
                    if (!ushort.TryParse(arguments.At<string>(1), out result1))
                    {
                        response = "Usage: multibroadcast add map <duration> <text>";
                        return false;
                    }
                    string text1 = string.Join(" ", arguments.Skip<string>(2));
                    IEnumerable<MultiBroadcast.API.Broadcast> source = MultiBroadcast.API.MultiBroadcast.AddMapBroadcast(result1, text1);
                    int[] array = source != null ? source.Select<MultiBroadcast.API.Broadcast, int>((Func<MultiBroadcast.API.Broadcast, int>)(bc => bc.Id)).ToArray<int>() : (int[])null;
                    response = source == null ? "Error on adding broadcast" : "Added broadcast for all players with id " + string.Join<int>(", ", (IEnumerable<int>)array);
                    return true;
                case 'p':
                    if (arguments.Count < 4)
                    {
                        response = "Usage: multibroadcast add player <player> <duration> <text>";
                        return false;
                    }
                    Player player = Player.Get(arguments.At<string>(1));
                    if (player == null)
                    {
                        response = "Player not found";
                        return false;
                    }
                    ushort result2;
                    if (!ushort.TryParse(arguments.At<string>(2), out result2))
                    {
                        response = "Usage: multibroadcast add player <player> <duration> <text>";
                        return false;
                    }
                    string text2 = string.Join(" ", arguments.Skip<string>(3));
                    MultiBroadcast.API.Broadcast broadcast = MultiBroadcast.API.MultiBroadcast.AddPlayerBroadcast(player, result2, text2);
                    int? id = broadcast?.Id;
                    response = broadcast == null ? "Error on adding broadcast to " + player.Nickname : $"Added broadcast for {player.Nickname} with id {id}";
                    return true;
                default:
                    response = "Usage: multibroadcast add <map/player>";
                    return false;
            }
        }

        public string Command { get; } = "add";

        public string[] Aliases { get; } = new string[1] { "a" };

        public string Description { get; } = "Add a broadcast.";

        public bool SanitizeResponse { get; } = false;
    }
}
