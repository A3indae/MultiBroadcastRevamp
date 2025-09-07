using CommandSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MultiBroadcast.Commands.Subcommands
{
    public class SetPriority : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: multibroadcast setpriority <id> <priority>";
                return false;
            }
            int[] args;
            if (!CommandUtilities.GetIntArguments(arguments.At<string>(0), out args))
            {
                response = "Usage: multibroadcast setpriority <id> <priority>";
                return false;
            }
            byte result;
            if (!byte.TryParse(arguments.At<string>(1), out result))
            {
                response = "Usage: multibroadcast setpriority <id> <priority>";
                return false;
            }
            bool flag = MultiBroadcast.API.MultiBroadcast.SetPriority(result, args);
            string stringFromArray = CommandUtilities.GetStringFromArray<int>((IEnumerable<int>)args);
            response = !flag ? "Error on setting priority for broadcast with id " + stringFromArray : "Set priority for broadcast with id " + stringFromArray;
            return true;
        }

        public string Command { get; } = "setpriority";

        public string[] Aliases { get; } = new string[1] { "sp" };

        public string Description { get; } = "Set priority of a broadcast.";

        public bool SanitizeResponse { get; } = false;
    }

}
