using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow
{
    internal enum PluginType
    {
        Exiled,
        NwAPI
    }

    internal interface IPlugin
    {
        PluginType Type { get; }

        PluginConfig PluginConfig { get; }

        void BindEvent();

        void UnbindEvent();
    }
}
