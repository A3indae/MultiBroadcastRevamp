using CommandSystem;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using Exiled.API.Features;
using HarmonyLib;
using System;

namespace MultiBroadcast.Patches
{
    [HarmonyPatch(typeof(ClearBroadcastCommand), "Execute")]
    internal class ClearBroadcastPatch
    {
        public static bool Prefix(
          ArraySegment<string> arguments,
          ICommandSender sender,
          ref string response,
          ref bool __result)
        {
            if (!Plugin.Instance.Config.ReplaceBroadcastCommand)
            {
                Log.Info("Broadcast command is not replaced");
                return true;
            }
            if (!sender.CheckPermission(PlayerPermissions.Broadcasting, out response))
            {
                __result = false;
                return false;
            }
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, sender.LogName + " cleared all broadcasts.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
            MultiBroadcast.API.MultiBroadcast.ClearAllBroadcasts();
            response = "All broadcasts cleared.";
            __result = true;
            return false;
        }
    }

}
