using CommandSystem;
using HintServiceMeow.Core.Utilities.Pools;
using System;
using System.Collections.Generic;
using System.Text;

namespace HintServiceMeow
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class GetCompatAssemblyName : ICommand
    {
        internal static readonly HashSet<string> RegisteredAssemblies = new();

        public string Command => "GetCompatAssemblyName";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "Get the name of all the assemblies that are using Compatibility Adaptor in HintServiceMeow";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder sb = StringBuilderPool.Rent();

            sb.AppendLine("The following assemblies are using Compatibility Adaptor in HintServiceMeow:");

            foreach (string name in RegisteredAssemblies)
            {
                sb.Append("- ");
                sb.AppendLine(name);
            }

            response = StringBuilderPool.ToStringReturn(sb);
            return true;
        }
    }
}
