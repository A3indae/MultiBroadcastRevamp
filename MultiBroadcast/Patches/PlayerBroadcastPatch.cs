using CommandSystem;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.Text;
using Utils;

namespace MultiBroadcast.Patches
{
    [HarmonyPatch(typeof(PlayerBroadcastCommand), "OnExecute")]
    internal class PlayerBroadcastPatch
    {
        public static bool Prefix(
          PlayerBroadcastCommand __instance,
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
            string[] newargs;
            List<ReferenceHub> referenceHubList = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out newargs);
            if (newargs == null || newargs.Length < __instance.MinimumArguments)
            {
                response = $"To execute this command provide at least 3 arguments!\nUsage: {arguments.At<string>(0)} {__instance.DisplayCommandUsage()}";
                __result = false;
                return false;
            }
            string inputDuration = newargs[0];
            ushort time;
            if (!IsValidDuration(inputDuration, out time))
            {
                response = $"Invalid argument for duration: {inputDuration} Usage: {arguments.At<string>(0)} {__instance.DisplayCommandUsage()}";
                return false;
            }
            Broadcast.BroadcastFlags broadcastFlag;
            bool flag = HasInputFlag(newargs[1], out broadcastFlag, __instance.MinimumArguments, newargs.Length);
            string text = RAUtils.FormatArguments(newargs.Segment<string>(1), flag ? 1 : 0);
            StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
            int num = 0;
            List<int> intList = ListPool<int>.Shared.Rent();
            List<Player> list = ListPool<Player>.Shared.Rent();
            foreach (ReferenceHub me in referenceHubList)
            {
                if (num++ != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(me.LoggedNameFromRefHub());
                Player player = Player.Get(me);
                MultiBroadcast.API.Broadcast broadcast = MultiBroadcast.API.MultiBroadcast.AddPlayerBroadcast(player, time, text);
                intList.Add(broadcast.Id);
                list.Add(player);
            }
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, $"{sender.LogName} broadcast text \"{text}\" to {num} players. Duration: {time} seconds. Affected players: {stringBuilder}. Broadcast Flag: {broadcastFlag}.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
            StringBuilderPool.Shared.Return(stringBuilder);
            response = num >= 2 ? $"Added broadcast for {num} players with id {string.Join<int>(", ", (IEnumerable<int>)intList)}" : $"Added broadcast for {list[0].Nickname} with id {intList[0].ToString()}";
            ListPool<Player>.Shared.Return(list);
            ListPool<int>.Shared.Return(intList);
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
