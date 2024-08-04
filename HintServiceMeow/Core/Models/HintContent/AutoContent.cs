using System;
using HintServiceMeow.Core.Models.Hints;
using PluginAPI.Core;

namespace HintServiceMeow.Core.Models.HintContent.HintContent
{
    public class AutoContent : AbstractHintContent
    {
        private string _oldText;
        private string _text;

        public delegate string TextUpdateHandler(AbstractHint.TextUpdateArg ev);
        private TextUpdateHandler _autoText;

        public AutoContent(TextUpdateHandler autoText)
        {
            _autoText = autoText;
        }

        public TextUpdateHandler AutoText
        {
            get => _autoText;
            set => _autoText = value;
        }

        public override string GetText(AbstractHint.TextUpdateArg ev)
        {
            return _text;
        }

        public override void TryUpdate(AbstractHint.TextUpdateArg ev)
        {
            try
            {
                _text = _autoText.Invoke(ev);

                if(_oldText != _text)
                {
                    OnUpdated();
                    _oldText = _text;
                }
            }
            catch(Exception ex)
            {
                _text = string.Empty;
                Log.Error(ex.ToString());
            }
        }
    }
}
