using CommandSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MultiBroadcast.Commands.Subcommands
{
    public class Edit : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: multibroadcast edit <id> <text>";
                return false;
            }
            int[] args;
            if (!CommandUtilities.GetIntArguments(arguments.At<string>(0), out args))
            {
                response = "Usage: multibroadcast edit <id> <text>";
                return false;
            }
            string text = string.Join(" ", arguments.Skip<string>(1));
            bool flag = MultiBroadcast.API.MultiBroadcast.EditBroadcast(text, args);
            string stringFromArray = CommandUtilities.GetStringFromArray<int>((IEnumerable<int>)args);
            response = !flag ? "Error on editing broadcast with id " + stringFromArray : $"Edited broadcast with id {stringFromArray} to {text}";
            return true;
        }

        public string Command { get; } = "edit";

        public string[] Aliases { get; } = new string[1] { "e" };

        public string Description { get; } = "Edit a broadcast.";

        public bool SanitizeResponse { get; } = false;
    }
}
