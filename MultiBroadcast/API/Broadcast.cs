using Exiled.API.Features;

namespace MultiBroadcast.API
{
    public class Broadcast
    {
        public Player Player { get; set; }

        public string Text { get; set; }

        public int Id { get; }

        public int Duration { get; }

        public byte Priority { get; set; }

        public string Tag { get; set; }

        public Broadcast(Player player, string text, int id, int duration, byte priority = 0, string tag = "")
        {
            this.Player = player;
            this.Text = text;
            this.Id = id;
            this.Priority = priority;
            this.Duration = duration;
            this.Tag = tag;
        }
    }
}
