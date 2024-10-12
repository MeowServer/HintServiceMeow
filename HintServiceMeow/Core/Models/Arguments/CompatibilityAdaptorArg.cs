using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Models.Arguments
{
    public class CompatibilityAdaptorArg
    {
        public string AssemblyName { get; }
        public string Content { get; }
        public float Duration { get; }

        internal CompatibilityAdaptorArg(string assemblyName, string content, float duration)
        {
            this.AssemblyName = assemblyName;
            this.Content = content;
            this.Duration = duration;
        }
    }
}
