using CommandSystem;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace MultiBroadcast.Patches
{
    [HarmonyPatch(typeof(BroadcastCommand), "OnExecute")]
    internal class BroadcastPatch
    {
        public static bool Prefix(
          BroadcastCommand __instance,
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
            string inputDuration = arguments.At<string>(0);
            ushort time;
            if (!IsValidDuration(inputDuration, out time))
            {
                response = $"Invalid argument for duration: {inputDuration} Usage: {arguments.At<string>(0)} {__instance.DisplayCommandUsage()}";
                __result = false;
                return false;
            }
            Broadcast.BroadcastFlags broadcastFlag;
            bool flag = HasInputFlag(arguments.At<string>(1), out broadcastFlag, __instance.MinimumArguments, arguments.Count);
            string text = RAUtils.FormatArguments(arguments, flag ? 2 : 1);
            IEnumerable<MultiBroadcast.API.Broadcast> source = MultiBroadcast.API.MultiBroadcast.AddMapBroadcast(time, text);
            List<int> list = source != null ? source.Select<MultiBroadcast.API.Broadcast, int>((Func<MultiBroadcast.API.Broadcast, int>)(bc => bc.Id)).ToList<int>() : (List<int>)null;
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, $"{sender.LogName} broadcast text \"{text}\". Duration: {inputDuration} seconds. Broadcast Flag: {broadcastFlag}.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
            response = source == null ? "Error on adding broadcast" : "Added broadcast for all players with id " + string.Join<int>(", ", (IEnumerable<int>)list);
            __result = true;
            return false;
        }

        //protected
        private static bool HasInputFlag(string inputFlag, out Broadcast.BroadcastFlags broadcastFlag, int minimumArguments, int argumentCount = 0)
        {
            bool num = RAUtils.IsDigit.IsMatch(inputFlag);
            broadcastFlag = Broadcast.BroadcastFlags.Normal;
            if (!num && argumentCount >= minimumArguments + 1)
            {
                return Enum.TryParse<Broadcast.BroadcastFlags>(inputFlag, ignoreCase: true, out broadcastFlag);
            }

            return false;
        }
        private static bool IsValidDuration(string inputDuration, out ushort time)
        {
            if (ushort.TryParse(inputDuration, out time))
            {
                return time > 0;
            }

            return false;
        }
    }
}
