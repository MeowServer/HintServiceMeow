using CommandSystem;
using System;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Core.Utilities.Pools;

namespace HintServiceMeow
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class GetCompatAssemblyName: ICommand
    {
        public string Command => "GetCompatAssemblyName";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "Get the name of all the assemblies that are using Compatibility Adaptor in HintServiceMeow";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var sb = StringBuilderPool.Rent();

            sb.AppendLine("The following assemblies are using Compatibility Adaptor in HintServiceMeow:");

            foreach (var name in CompatibilityAdaptor.RegisteredAssemblies)
            {
                sb.Append("- ");
                sb.AppendLine(name);
            }

            response = StringBuilderPool.ToStringReturn(sb);
            return true;
        }
    }
}
