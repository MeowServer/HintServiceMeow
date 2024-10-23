using System;

namespace HintServiceMeow.Core.Interface
{
    public interface IUpdateAnalyser
    {
        void OnUpdate();

        DateTime EstimateNextUpdate();
    }
}