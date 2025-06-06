﻿using HintServiceMeow.Core.Models.Arguments;

namespace HintServiceMeow.Core.Models.HintContent
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

        public override string GetText() => Text;

        public override void TryUpdate(ContentUpdateArg ev) { }
    }
}
