using Exiled.API.Interfaces;
using System.ComponentModel;

namespace MultiBroadcast
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        public bool Debug { get; set; } = false;

        public bool ReplaceBroadcastCommand { get; set; } = true;

        public BroadcastOrder Order { get; set; } = BroadcastOrder.Descending;
    }
}