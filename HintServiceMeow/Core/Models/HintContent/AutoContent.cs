using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Tools;
using System;

namespace HintServiceMeow.Core.Models.HintContent.HintContent
{
    public class AutoContent : AbstractHintContent
    {
        private DateTime NextUpdateTime;

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

        public override string GetText() => _text;

        public override void TryUpdate(AbstractHint.TextUpdateArg ev)
        {
            if (NextUpdateTime > DateTime.Now)
                return;

            try
            {
                string newText = _autoText.Invoke(ev);

                if (_text != newText)
                {
                    _text = newText;
                    OnUpdated();
                }

                NextUpdateTime = DateTime.Now.AddSeconds(ev.NextUpdateDelay);
            }
            catch (Exception ex)
            {
                _text = string.Empty;
                LogTool.Error(ex);
            }
        }
    }
}
