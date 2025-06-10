using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Utilities.Tools;
using System;

namespace HintServiceMeow.Core.Models.HintContent
{
    public class AutoContent : AbstractHintContent
    {
        private DateTime _nextUpdateTime;
        private TimeSpan _defaultUpdateTime = TimeSpan.FromSeconds(0.1);

        private string _text;

        public delegate string TextUpdateHandler(AutoContentUpdateArg ev);
        private TextUpdateHandler _autoText;

        public AutoContent(TextUpdateHandler autoText)
        {
            _autoText = autoText;
        }

        public TextUpdateHandler AutoText
        {
            get => _autoText;
            set
            {
                _autoText = value;
                _nextUpdateTime = DateTime.MinValue;// Reset Update Time
            }
        }

        public override string GetText() => _text;

        public override void TryUpdate(ContentUpdateArg ev)
        {
            if (_nextUpdateTime > DateTime.Now)
                return;

            AutoContentUpdateArg autoContentUpdateArg = new AutoContentUpdateArg(ev.Hint, ev.PlayerDisplay, _defaultUpdateTime);

            try
            {
                string newText = _autoText.Invoke(autoContentUpdateArg);

                if (_text != newText)
                {
                    _text = newText;
                    OnUpdated();
                }

                _nextUpdateTime = DateTime.Now.Add(autoContentUpdateArg.NextUpdateDelay);
                _defaultUpdateTime = autoContentUpdateArg.DefaultUpdateDelay;
            }
            catch (Exception ex)
            {
                _text = string.Empty;
                Logger.Instance.Error(ex);
            }
        }
    }
}
