using Exiled.API.Interfaces;
using System.ComponentModel;

namespace RemoteKeycard
{
    public class Config : IConfig
    {
        public bool Debug { get; set; } = false;

        [Description("Is the plugin enabled?")]
        public bool IsEnabled { get; set; } = true;
    }
}