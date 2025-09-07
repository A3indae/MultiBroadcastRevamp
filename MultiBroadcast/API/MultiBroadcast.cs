using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Features;
using Exiled.Events.Handlers;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

using ServerEvents = Exiled.Events.Handlers.Server;
using PlayerEvents = Exiled.Events.Handlers.Player;

namespace MultiBroadcast.API
{
    public static class MultiBroadcast
    {
        private static readonly bool IsDependency;

        static MultiBroadcast()
        {
            ServerEvents.RestartingRound += new CustomEventHandler(MultiBroadcast.OnRestarting);
            PlayerEvents.Left += new CustomEventHandler<LeftEventArgs>(MultiBroadcast.OnLeft);
            MultiBroadcast.IsDependency = Plugin.Instance == null;
            Log.Debug("MultiBroadcast class initialized.");
        }

        private static void OnRestarting()
        {
            Log.Debug("OnRestarting event triggered.");
            MultiBroadcast.RestartBroadcasts();
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            if (((JoinedEventArgs)ev).Player == null) return;
            MultiBroadcast.PlayerBroadcasts.Remove(((JoinedEventArgs)ev).Player.UserId);
        }

        private static Dictionary<string, List<Broadcast>> PlayerBroadcasts { get; } = new Dictionary<string, List<Broadcast>>();

        public static int Id { get; private set; }

        public static IEnumerable<Broadcast> AddMapBroadcast(
          ushort duration,
          string text,
          byte priority = 0,
          string tag = "")
        {
            Log.Debug($"AddMapBroadcast called with duration: {duration}, text: {text}, priority: {priority}, tag: {tag}");
            if (duration > (ushort)300 || duration == (ushort)0)
            {
                Log.Debug($"AddMapBroadcast early return due to invalid duration: {duration}");
                return (IEnumerable<Broadcast>)null;
            }
            List<Broadcast> broadcastList = new List<Broadcast>();
            foreach (Exiled.API.Features.Player player in (IEnumerable<Exiled.API.Features.Player>)Exiled.API.Features.Player.List)
            {
                if (player != null && !player.IsNPC)
                {
                    ++MultiBroadcast.Id;
                    Broadcast broadcast = new Broadcast(player, text, MultiBroadcast.Id, (int)duration, priority, tag);
                    Timing.RunCoroutine(MultiBroadcast.AddPlayerBroadcastCoroutine(broadcast), "MBroadcast" + MultiBroadcast.Id.ToString());
                    Log.Debug($"Added broadcast for {player.Nickname} with id {MultiBroadcast.Id}");
                    broadcastList.Add(broadcast);
                }
            }
            return (IEnumerable<Broadcast>)broadcastList;
        }

        public static Broadcast AddPlayerBroadcast(
          Exiled.API.Features.Player player,
          ushort duration,
          string text,
          byte priority = 0,
          string tag = "")
        {
            Log.Debug($"AddPlayerBroadcast called for player {player?.Nickname}, duration: {duration}, text: {text}, priority: {priority}, tag: {tag}");
            if (player == null || player.IsNPC || duration == (ushort)0 || duration > (ushort)300)
            {
                Log.Debug($"AddPlayerBroadcast early return for player {player?.Nickname} due to invalid parameters.");
                return (Broadcast)null;
            }
            ++MultiBroadcast.Id;
            Broadcast broadcast = new Broadcast(player, text, MultiBroadcast.Id, (int)duration, priority, tag);
            Timing.RunCoroutine(MultiBroadcast.AddPlayerBroadcastCoroutine(broadcast), "MBroadcast" + MultiBroadcast.Id.ToString());
            Log.Debug($"Added broadcast for {player.Nickname} with id {MultiBroadcast.Id}");
            return broadcast;
        }

        private static IEnumerator<float> AddPlayerBroadcastCoroutine(Broadcast broadcast)
        {
            Log.Debug($"AddPlayerBroadcastCoroutine started for player {broadcast.Player.Nickname} with duration {broadcast.Duration}");
            Exiled.API.Features.Player player = broadcast.Player;
            string playerId = player.UserId;
            if (!MultiBroadcast.PlayerBroadcasts.ContainsKey(playerId))
                MultiBroadcast.PlayerBroadcasts.Add(playerId, new List<Broadcast>(1)
      {
        broadcast
      });
            else
                MultiBroadcast.PlayerBroadcasts[playerId].Add(broadcast);
            MultiBroadcast.RefreshBroadcast(player);
            yield return Timing.WaitForSeconds((float)broadcast.Duration);
            if (MultiBroadcast.PlayerBroadcasts.ContainsKey(playerId) && MultiBroadcast.PlayerBroadcasts[playerId].Contains(broadcast))
                MultiBroadcast.PlayerBroadcasts[playerId].Remove(broadcast);
            MultiBroadcast.RefreshBroadcast(player);
        }

        private static void RefreshBroadcast(Exiled.API.Features.Player player)
        {
            Log.Debug("Refreshing broadcasts for player " + player.Nickname);
            if (!MultiBroadcast.PlayerBroadcasts.ContainsKey(player.UserId))
                return;
            string data = string.Join("\n", ((MultiBroadcast.IsDependency ? 0 : (int)Plugin.Instance.Config.Order) == 0 ? (IEnumerable<Broadcast>)MultiBroadcast.PlayerBroadcasts[player.UserId].OrderByDescending<Broadcast, byte>((Func<Broadcast, byte>)(x => x.Priority)).ThenByDescending<Broadcast, int>((Func<Broadcast, int>)(y => y.Id)).ToList<Broadcast>() : (IEnumerable<Broadcast>)MultiBroadcast.PlayerBroadcasts[player.UserId].OrderByDescending<Broadcast, byte>((Func<Broadcast, byte>)(x => x.Priority)).ThenBy<Broadcast, int>((Func<Broadcast, int>)(y => y.Id)).ToList<Broadcast>()).Select<Broadcast, string>((Func<Broadcast, string>)(b => b.Text)));
            //엑자일드시켜버리기
            Exiled.API.Features.Server.Broadcast.TargetClearElements(player.Connection);
            Exiled.API.Features.Server.Broadcast.TargetAddElement(player.Connection, data, (ushort)300, global::Broadcast.BroadcastFlags.Normal);
            //Server.Broadcast.TargetClearElements(player.Connection);
            //Server.Broadcast.TargetAddElement(player.Connection, data, (ushort)300, global::Broadcast.BroadcastFlags.Normal);
        }

        public static bool EditBroadcast(string text, params int[] ids)
        {
            foreach (int id in ids)
            {
                Broadcast broadcast = MultiBroadcast.GetBroadcast(id);
                if (broadcast == null)
                {
                    Log.Debug($"Error while editing: Broadcast with id {id} not found.");
                    return false;
                }
                broadcast.Text = text;
                Log.Debug($"Edited broadcast with id {id} to {text}");
                MultiBroadcast.RefreshBroadcast(broadcast.Player);
            }
            return true;
        }

        public static bool EditBroadcast(string text, params Broadcast[] broadcasts)
        {
            foreach (Broadcast broadcast in broadcasts)
            {
                if (broadcast == null)
                {
                    Log.Debug("Error while editing: Broadcast not found.");
                    return false;
                }
                broadcast.Text = text;
                Log.Debug($"Edited broadcast with id {broadcast.Id} to {text}");
                MultiBroadcast.RefreshBroadcast(broadcast.Player);
            }
            return true;
        }

        public static bool EditBroadcast(string text, string tag)
        {
            List<Broadcast> list = MultiBroadcast.PlayerBroadcasts.Values.SelectMany<List<Broadcast>, Broadcast>((Func<List<Broadcast>, IEnumerable<Broadcast>>)(broadcasts => (IEnumerable<Broadcast>)broadcasts)).Where<Broadcast>((Func<Broadcast, bool>)(broadcast => broadcast.Tag == tag)).ToList<Broadcast>();
            if (list.Count == 0)
            {
                Log.Debug($"Error while editing: Broadcast with tag {tag} not found.");
                return false;
            }
            foreach (Broadcast broadcast in list)
            {
                broadcast.Text = text;
                Log.Debug($"Edited broadcast with tag {tag} to {text}");
                MultiBroadcast.RefreshBroadcast(broadcast.Player);
            }
            return true;
        }

        public static bool EditBroadcast(
          string text,
          ushort duration,
          out Broadcast[] newBroadcasts,
          params int[] ids)
        {
            newBroadcasts = (Broadcast[])null;
            ushort num = duration;
            if (num <= (ushort)300)
            {
                if (num == (ushort)0)
                    return MultiBroadcast.EditBroadcast(text, ids);
                List<Broadcast> broadcastList = new List<Broadcast>();
                foreach (int id in ids)
                {
                    Broadcast broadcast1 = MultiBroadcast.GetBroadcast(id);
                    if (broadcast1 == null)
                    {
                        Log.Debug($"Error while editing: Broadcast with id {id} not found.");
                        return false;
                    }
                    Timing.KillCoroutines("MBroadcast" + id.ToString());
                    MultiBroadcast.PlayerBroadcasts[broadcast1.Player.UserId].Remove(broadcast1);
                    MultiBroadcast.RefreshBroadcast(broadcast1.Player);
                    Broadcast broadcast2 = new Broadcast(broadcast1.Player, text, id, (int)duration, broadcast1.Priority, broadcast1.Tag);
                    broadcastList.Add(broadcast2);
                    Timing.RunCoroutine(MultiBroadcast.AddPlayerBroadcastCoroutine(broadcast2), "MBroadcast" + id.ToString());
                    Log.Debug($"Edited broadcast with id {id} to {text} with duration {duration}");
                }
                newBroadcasts = broadcastList.ToArray();
                return true;
            }
            Log.Debug("Error while editing: Duration exceeds the maximum allowed value.");
            return false;
        }

        public static bool EditBroadcast(
          string text,
          ushort duration,
          out Broadcast[] newBroadcasts,
          params Broadcast[] broadcasts)
        {
            newBroadcasts = (Broadcast[])null;
            ushort num = duration;
            if (num <= (ushort)300)
            {
                if (num == (ushort)0)
                    return MultiBroadcast.EditBroadcast(text, broadcasts);
                List<Broadcast> broadcastList = new List<Broadcast>();
                foreach (Broadcast broadcast1 in broadcasts)
                {
                    if (broadcast1 == null)
                    {
                        Log.Debug("Error while editing: Broadcast not found.");
                        return false;
                    }
                    int id = broadcast1.Id;
                    Timing.KillCoroutines("MBroadcast" + id.ToString());
                    MultiBroadcast.PlayerBroadcasts[broadcast1.Player.UserId].Remove(broadcast1);
                    MultiBroadcast.RefreshBroadcast(broadcast1.Player);
                    Broadcast broadcast2 = new Broadcast(broadcast1.Player, text, broadcast1.Id, (int)duration, broadcast1.Priority, broadcast1.Tag);
                    broadcastList.Add(broadcast2);
                    IEnumerator<float> coroutine = MultiBroadcast.AddPlayerBroadcastCoroutine(broadcast2);
                    id = broadcast1.Id;
                    string tag = "MBroadcast" + id.ToString();
                    Timing.RunCoroutine(coroutine, tag);
                    Log.Debug($"Edited broadcast with id {broadcast1.Id} to {text} with duration {duration}");
                }
                newBroadcasts = broadcastList.ToArray();
                return true;
            }
            Log.Debug("Error while editing: Duration exceeds the maximum allowed value.");
            return false;
        }

        public static bool EditBroadcast(
          string text,
          ushort duration,
          out Broadcast[] newBroadcasts,
          string tag)
        {
            newBroadcasts = (Broadcast[])null;
            ushort num = duration;
            if (num <= (ushort)300)
            {
                if (num == (ushort)0)
                    return MultiBroadcast.EditBroadcast(text, tag);
                List<Broadcast> list = MultiBroadcast.PlayerBroadcasts.Values.SelectMany<List<Broadcast>, Broadcast>((Func<List<Broadcast>, IEnumerable<Broadcast>>)(broadcasts => (IEnumerable<Broadcast>)broadcasts)).Where<Broadcast>((Func<Broadcast, bool>)(broadcast => broadcast.Tag == tag)).ToList<Broadcast>();
                if (list.Count == 0)
                {
                    Log.Debug($"Error while editing: Broadcast with tag {tag} not found.");
                    return false;
                }
                List<Broadcast> broadcastList = new List<Broadcast>();
                foreach (Broadcast broadcast1 in list)
                {
                    int id = broadcast1.Id;
                    Timing.KillCoroutines("MBroadcast" + id.ToString());
                    MultiBroadcast.PlayerBroadcasts[broadcast1.Player.UserId].Remove(broadcast1);
                    MultiBroadcast.RefreshBroadcast(broadcast1.Player);
                    Broadcast broadcast2 = new Broadcast(broadcast1.Player, text, broadcast1.Id, (int)duration, broadcast1.Priority, tag);
                    broadcastList.Add(broadcast2);
                    IEnumerator<float> coroutine = MultiBroadcast.AddPlayerBroadcastCoroutine(broadcast2);
                    id = broadcast1.Id;
                    string tag1 = "MBroadcast" + id.ToString();
                    Timing.RunCoroutine(coroutine, tag1);
                    Log.Debug($"Edited broadcast with tag {tag} to {text} with duration {duration}");
                }
                newBroadcasts = broadcastList.ToArray();
                return true;
            }
            Log.Debug("Error while editing: Duration exceeds the maximum allowed value.");
            return false;
        }

        public static bool SetPriority(byte priority, params int[] ids)
        {
            foreach (int id in ids)
            {
                Broadcast broadcast = MultiBroadcast.GetBroadcast(id);
                if (broadcast == null)
                {
                    Log.Debug($"Error while setting priority: Broadcast with id {id} not found.");
                    return false;
                }
                broadcast.Priority = priority;
                Log.Debug($"Set priority of broadcast with id {id} to {priority}");
                MultiBroadcast.RefreshBroadcast(broadcast.Player);
            }
            return true;
        }

        public static bool SetPriority(byte priority, params Broadcast[] broadcasts)
        {
            foreach (Broadcast broadcast in broadcasts)
            {
                if (broadcast == null)
                {
                    Log.Debug("Error while setting priority: Broadcast not found.");
                    return false;
                }
                broadcast.Priority = priority;
                Log.Debug($"Set priority of broadcast with id {broadcast.Id} to {priority}");
                MultiBroadcast.RefreshBroadcast(broadcast.Player);
            }
            return true;
        }

        public static bool SetTag(string tag, params Broadcast[] broadcasts)
        {
            foreach (Broadcast broadcast in broadcasts)
            {
                if (broadcast == null)
                {
                    Log.Debug("Error while setting tag: Broadcast not found.");
                    return false;
                }
                broadcast.Tag = tag;
                Log.Debug($"Set tag of broadcast with id {broadcast.Id} to {tag}");
                MultiBroadcast.RefreshBroadcast(broadcast.Player);
            }
            return true;
        }

        public static bool RemoveBroadcast(params int[] ids)
        {
            foreach (int id in ids)
            {
                Broadcast broadcast = MultiBroadcast.GetBroadcast(id);
                if (broadcast == null)
                {
                    Log.Debug($"Error while removing: Broadcast with id {id} not found.");
                    return false;
                }
                Timing.KillCoroutines("MBroadcast" + id.ToString());
                MultiBroadcast.PlayerBroadcasts[broadcast.Player.UserId].Remove(broadcast);
                Log.Debug($"Removed broadcast with id {id}");
                MultiBroadcast.RefreshBroadcast(broadcast.Player);
            }
            return true;
        }

        public static bool RemoveBroadcast(params Broadcast[] broadcasts)
        {
            foreach (Broadcast broadcast in broadcasts)
            {
                if (broadcast == null)
                {
                    Log.Debug("Error while removing: Broadcast not found.");
                    return false;
                }
                Timing.KillCoroutines("MBroadcast" + broadcast.Id.ToString());
                MultiBroadcast.PlayerBroadcasts[broadcast.Player.UserId].Remove(broadcast);
                Log.Debug($"Removed broadcast with id {broadcast.Id}");
                MultiBroadcast.RefreshBroadcast(broadcast.Player);
            }
            return true;
        }

        public static bool RemoveBroadcast(string tag)
        {
            List<Broadcast> list = MultiBroadcast.PlayerBroadcasts.Values.SelectMany<List<Broadcast>, Broadcast>((Func<List<Broadcast>, IEnumerable<Broadcast>>)(broadcasts => (IEnumerable<Broadcast>)broadcasts)).Where<Broadcast>((Func<Broadcast, bool>)(broadcast => broadcast.Tag == tag)).ToList<Broadcast>();
            if (list.Count == 0)
            {
                Log.Debug($"Error while removing: Broadcast with tag {tag} not found.");
                return false;
            }
            foreach (Broadcast broadcast in list)
            {
                Timing.KillCoroutines("MBroadcast" + broadcast.Id.ToString());
                MultiBroadcast.PlayerBroadcasts[broadcast.Player.UserId].Remove(broadcast);
                Log.Debug("Removed broadcast with tag " + tag);
                MultiBroadcast.RefreshBroadcast(broadcast.Player);
            }
            return true;
        }

        public static Broadcast GetBroadcast(int id)
        {
            Log.Debug($"Getting broadcast with id {id}");
            Broadcast broadcast1 = MultiBroadcast.PlayerBroadcasts.Values.SelectMany<List<Broadcast>, Broadcast>((Func<List<Broadcast>, IEnumerable<Broadcast>>)(broadcasts => (IEnumerable<Broadcast>)broadcasts)).FirstOrDefault<Broadcast>((Func<Broadcast, bool>)(broadcast => broadcast.Id == id));
            if (broadcast1 == null)
                Log.Debug($"Broadcast with id {id} not found.");
            return broadcast1;
        }

        public static IEnumerable<Broadcast> GetBroadcast(params int[] ids)
        {
            Log.Debug("Getting broadcasts with ids: " + string.Join<int>(", ", (IEnumerable<int>)ids));
            List<Broadcast> list = MultiBroadcast.PlayerBroadcasts.Values.SelectMany<List<Broadcast>, Broadcast>((Func<List<Broadcast>, IEnumerable<Broadcast>>)(broadcasts => (IEnumerable<Broadcast>)broadcasts)).Where<Broadcast>((Func<Broadcast, bool>)(broadcast => ids.Contains<int>(broadcast.Id))).ToList<Broadcast>();
            foreach (int id1 in ids)
            {
                int id = id1;
                if (list.All<Broadcast>((Func<Broadcast, bool>)(broadcast => broadcast.Id != id)))
                    Log.Debug($"Broadcast with id {id} not found.");
            }
            return (IEnumerable<Broadcast>)list;
        }

        public static bool HasBroadcast(string tag)
        {
            return MultiBroadcast.PlayerBroadcasts.Values.SelectMany<List<Broadcast>, Broadcast>((Func<List<Broadcast>, IEnumerable<Broadcast>>)(broadcasts => (IEnumerable<Broadcast>)broadcasts)).Any<Broadcast>((Func<Broadcast, bool>)(broadcast => broadcast.Tag == tag));
        }

        public static IEnumerable<Broadcast> GetPlayerBroadcasts(Exiled.API.Features.Player player)
        {
            List<Broadcast> broadcastList;
            return MultiBroadcast.PlayerBroadcasts.TryGetValue(player.UserId, out broadcastList) ? (IEnumerable<Broadcast>)broadcastList.ToArray() : (IEnumerable<Broadcast>)(Broadcast[])null;
        }

        public static Dictionary<string, List<Broadcast>> GetAllBroadcasts()
        {
            return MultiBroadcast.PlayerBroadcasts;
        }

        private static void RestartBroadcasts()
        {
            foreach (List<Broadcast> broadcastList in MultiBroadcast.PlayerBroadcasts.Values)
                broadcastList.Clear();
            foreach (Exiled.API.Features.Player player in (IEnumerable<Exiled.API.Features.Player>)Exiled.API.Features.Player.List)
                MultiBroadcast.RefreshBroadcast(player);
            Log.Debug("Cleared all broadcasts");
            for (int index = 0; index < MultiBroadcast.Id; ++index)
                Timing.KillCoroutines("MBroadcast" + index.ToString());
            MultiBroadcast.Id = 0;
            MultiBroadcast.PlayerBroadcasts.Clear();
        }

        public static void ClearAllBroadcasts()
        {
            foreach (List<Broadcast> broadcastList in MultiBroadcast.PlayerBroadcasts.Values)
                broadcastList.Clear();
            foreach (Exiled.API.Features.Player player in (IEnumerable<Exiled.API.Features.Player>)Exiled.API.Features.Player.List)
                MultiBroadcast.RefreshBroadcast(player);
            Log.Debug("Cleared all broadcasts");
            for (int index = 0; index < MultiBroadcast.Id; ++index)
                Timing.KillCoroutines("MBroadcast" + index.ToString());
        }

        public static void ClearPlayerBroadcasts(Exiled.API.Features.Player player)
        {
            if (!MultiBroadcast.PlayerBroadcasts.ContainsKey(player.UserId))
                return;
            List<int> list = MultiBroadcast.PlayerBroadcasts[player.UserId].Select<Broadcast, int>((Func<Broadcast, int>)(broadcast => broadcast.Id)).ToList<int>();
            MultiBroadcast.PlayerBroadcasts[player.UserId].Clear();
            Log.Debug("Cleared all broadcasts for " + player.Nickname);
            MultiBroadcast.RefreshBroadcast(player);
            foreach (int num in list)
                Timing.KillCoroutines("MBroadcast" + num.ToString());
        }
    }
}
