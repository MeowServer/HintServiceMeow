using HintServiceMeow.Core.Models.Hints;

namespace HintServiceMeow.Core.Models.HintContent.HintContent
{
    public class StringContent : AbstractHintContent
    {
        private string _text = string.Empty;

        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;

                _text = value;

                OnUpdated();
            }
        }

        public StringContent(string content)
        {
            this.Text = content;
        }

        public override string GetText(AbstractHint.TextUpdateArg ev)
        {
            return Text;
        }

        public override void TryUpdate(AbstractHint.TextUpdateArg ev)
        {
        }
    }
}
