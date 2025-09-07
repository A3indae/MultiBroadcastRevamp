using CommandSystem;
using MultiBroadcast.Commands.Subcommands;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static PlayerRoles.Spectating.SpectatableModuleBase;
using System.Windows.Input;

namespace MultiBroadcast.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class MultiBroadcastParentCommand : ParentCommand
    {
        public MultiBroadcastParentCommand() => this.LoadGeneratedCommands();

        public override void LoadGeneratedCommands()
        {
            this.RegisterCommand((CommandSystem.ICommand)new Add());
            this.RegisterCommand((CommandSystem.ICommand)new Edit());
            this.RegisterCommand((CommandSystem.ICommand)new Remove());
            this.RegisterCommand((CommandSystem.ICommand)new List());
            this.RegisterCommand((CommandSystem.ICommand)new SetPriority());
        }

        protected override bool ExecuteParent(
          ArraySegment<string> arguments,
          ICommandSender sender,
          out string response)
        {
            response = this.AllCommands
    .Where(cmd => sender.CheckPermission(PlayerPermissions.Broadcasting, out _))
    .Aggregate(
        "\nPlease enter a valid subcommand:",
        (current, cmd) =>
            $"{current}\n\n<color=yellow><b>- {cmd.Command} ({string.Join(", ", cmd.Aliases)})</b></color>\n" +
            $"<color=white>{cmd.Description}</color>"
    );

            return false;
        }

        public override string Command { get; } = "multibroadcast";

        public override string[] Aliases { get; } = new string[1]
        {
    "mbc"
        };

        public override string Description { get; } = "The parent command for MultiBroadcast.";
    }
}
