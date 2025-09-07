using Exiled.API.Features;
using HarmonyLib;
using System;

namespace MultiBroadcast
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }
        public virtual string Name => "MultiBroadcastRevamp";
        public virtual string Author => "Cocoa, Revamp by A3indae";
        public virtual Version Version { get; } = new Version(1, 0, 0);
        private Harmony Harmony { get; set; }
        public override void OnEnabled()
        {
            base.OnEnabled();
            Plugin.Instance = this;
            this.Harmony = new Harmony($"a3indae.multi_broadcastrevamp.{DateTime.Now.Ticks}");
            this.Harmony.PatchAll();
        }
        public override void OnDisabled()
        {
            this.Harmony.UnpatchAll(null);
            this.Harmony = null;
            Plugin.Instance = null;
            base.OnDisabled();
        }
    }
}