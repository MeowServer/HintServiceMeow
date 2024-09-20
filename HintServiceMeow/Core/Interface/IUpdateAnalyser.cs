using System;

namespace HintServiceMeow.Core.Interface
{
    internal interface IUpdateAnalyser
    {
        void OnUpdate();

        DateTime EstimateNextUpdate();
    }
}