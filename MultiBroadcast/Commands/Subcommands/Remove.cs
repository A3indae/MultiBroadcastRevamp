using CommandSystem;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MultiBroadcast.Commands.Subcommands
{
    public class Remove : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Usage: multibroadcast remove <all/player/id>";
                return false;
            }
            switch (arguments.At<string>(0).ToLower()[0])
            {
                case 'a':
                    MultiBroadcast.API.MultiBroadcast.ClearAllBroadcasts();
                    response = "Removed all broadcasts";
                    return true;
                case 'p':
                    if (arguments.Count < 2)
                    {
                        response = "Usage: multibroadcast remove player <player>";
                        return false;
                    }
                    Player player = Player.Get(arguments.At<string>(1));
                    MultiBroadcast.API.MultiBroadcast.ClearPlayerBroadcasts(player);
                    response = "Removed all broadcasts for " + player.Nickname;
                    return true;
                default:
                    int[] args;
                    if (!CommandUtilities.GetIntArguments(arguments.At<string>(0), out args))
                    {
                        response = "Usage: multibroadcast remove <id> <text>";
                        return false;
                    }
                    bool flag = MultiBroadcast.API.MultiBroadcast.RemoveBroadcast(args);
                    string stringFromArray = CommandUtilities.GetStringFromArray<int>((IEnumerable<int>)args);
                    response = !flag ? "Error on removing broadcast with id " + stringFromArray : "Removed broadcast with id " + stringFromArray;
                    return true;
            }
        }

        public string Command { get; } = "remove";

        public string[] Aliases { get; } = new string[1] { "r" };

        public string Description { get; } = "Remove a broadcast.";

        public bool SanitizeResponse { get; } = false;
    }
}
