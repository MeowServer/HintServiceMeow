using Exiled.API.Interfaces;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Integrations
{
    internal static class Integrater
    {
        public static void StartAllIntegration()
        {
            bool HascriptEvent = Loader.Plugins.FirstOrDefault(plugin => plugin.Assembly.GetName().Name == "ScriptedEvents") != null;

            if(HascriptEvent)
            {
            }
        }
    }
}
