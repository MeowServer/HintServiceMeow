using HintServiceMeow.Core.Models;

namespace HintServiceMeow.Core.Interface
{
    public interface IHintParser
    {
        string Parse(HintCollection collection);
    }
}
