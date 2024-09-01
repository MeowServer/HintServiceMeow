using CommandSystem;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Core.Utilities.Patch;
using System;

namespace HintServiceMeow.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class GetCompatAssemblyName: ICommand
    {
        public string Command => "GetCompatAssemblyName";

        public string[] Aliases => new string[] { "GCAN" };

        public string Description => "Get the name of all the assemblies that are using Compatibility Adaptor in HintServiceMeow";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("The following assemblies are using Compatibility Adaptor in HintServiceMeow:");

            foreach(var name in CompatibilityAdaptor.RegisteredAssemblies)
            {
                sb.Append("- ");
                sb.AppendLine(name);
            }

            response = sb.ToString();
            return true;
        }
    }
}
