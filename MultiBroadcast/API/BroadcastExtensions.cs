using Exiled.API.Features;
using System.Collections.Generic;

namespace MultiBroadcast.API
{
    public static class BroadcastExtensions
    {
        public static Broadcast AddBroadcast(
    this Player player,
    ushort duration,
    string message,
    byte priority = 0,
    string tag = "")
        {
            return MultiBroadcast.AddPlayerBroadcast(player, duration, message, priority, tag);
        }

        public static bool HasBroadcast(this Player player, string tag)
        {
            return MultiBroadcast.HasBroadcast(tag);
        }

        public static bool EditBroadcast(this Player player, string text, string tag)
        {
            return MultiBroadcast.EditBroadcast(text, tag);
        }

        public static bool EditBroadcast(
          this Player player,
          string text,
          ushort duration,
          out Broadcast[] newBroadcasts,
          string tag)
        {
            return MultiBroadcast.EditBroadcast(text, duration, out newBroadcasts, tag);
        }

        public static bool RemoveBroadcast(this Player player, string tag)
        {
            return MultiBroadcast.RemoveBroadcast(tag);
        }

        public static void ClearPlayerBroadcasts(this Player player)
        {
            MultiBroadcast.ClearPlayerBroadcasts(player);
        }

        public static IEnumerable<Broadcast> GetBroadcasts(this Player player)
        {
            return MultiBroadcast.GetPlayerBroadcasts(player);
        }
    }
}
