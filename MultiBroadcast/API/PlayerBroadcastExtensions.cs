namespace MultiBroadcast.API
{
    public static class PlayerBroadcastExtensions
    {
        public static bool Edit(this Broadcast broadcast, string text)
        {
            return MultiBroadcast.EditBroadcast(text, broadcast);
        }

        public static bool Edit(
          this Broadcast broadcast,
          string text,
          out Broadcast[] newBroadcasts,
          ushort duration)
        {
            return MultiBroadcast.EditBroadcast(text, duration, out newBroadcasts, broadcast);
        }

        public static bool Remove(this Broadcast broadcast) => MultiBroadcast.RemoveBroadcast(broadcast);

        public static bool SetPriority(this Broadcast broadcast, byte priority)
        {
            return MultiBroadcast.SetPriority(priority, broadcast);
        }

        public static bool SetTag(this Broadcast broadcast, string tag)
        {
            return MultiBroadcast.SetTag(tag, broadcast);
        }
    }
}
