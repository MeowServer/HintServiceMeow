using HintServiceMeow.Core.Models.Arguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Interface
{
    public interface ICompatibilityAdaptor
    {
        void ShowHint(CompatibilityAdaptorArg ev);
    }
}
