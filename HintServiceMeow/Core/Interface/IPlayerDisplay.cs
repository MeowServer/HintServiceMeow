using HintServiceMeow.Core.Models.Hints;
using System.Collections.Generic;

namespace HintServiceMeow.Core.Interface
{
    public interface IPlayerDisplay
    {
        IHintParser HintParser { get; set; }
        ICompatibilityAdaptor CompatibilityAdaptor { get; set; }

        void AddDisplayOutput(IDisplayOutput output);
        void RemoveDisplayOutput(IDisplayOutput output);
        void RemoveDisplayOutput<T>() where T : IDisplayOutput;

        void AddHint(AbstractHint hint);
        void RemoveHint(AbstractHint hint);
        void ClearHint();

        IEnumerable<AbstractHint> GetHints(string id);
        IEnumerable<AbstractHint> GetHints();

        void ForceUpdate(bool useFastUpdate = false);
    }
}

