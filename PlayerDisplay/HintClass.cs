using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow
{

    public enum HintAlignment
    {
        Left,
        Right,
        Center
    }

    public abstract class AbstractHint
    {
        public delegate void UpdateHandler();
        public event UpdateHandler HintUpdated;

        private string _id;
        public string id
        {
            get
            {
                return _id;
            }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    if (!hide) OnHintUpdated();
                }
            }
        }

        private HintAlignment _alignment;
        public HintAlignment alignment
        {
            get
            {
                return _alignment;
            }
            set
            {
                if (_alignment != value)
                {
                    _alignment = value;
                    if (!hide) OnHintUpdated();
                }
            }
        }

        private int _fontSize = 20;
        public int fontSize
        {
            get
            {
                return _fontSize;
            }
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    if (!hide) OnHintUpdated();
                }
            }
        }

        private string _message;
        public string message
        {
            get
            {
                return _message;
            }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    if (!hide) OnHintUpdated();
                }
            }
        }

        private bool _hide = false;
        public bool hide
        {
            get
            {
                return _hide;
            }
            set
            {
                if (_hide != value)
                {
                    _hide = value;
                    OnHintUpdated();
                }
            }
        }

        public AbstractHint(HintAlignment Alignment, string Message, string id = null)
        {
            this.alignment = Alignment;

            this.message = Message;

            this.id = id;
        }

        public AbstractHint(AbstractHint hint)
        {
            this.fontSize = hint.fontSize;

            this.alignment = hint.alignment;

            this.message = hint.message;

            this.id = hint.id;

            this.hide = hint.hide;
        }

        public abstract override string ToString();

        public abstract string GetText();

        protected void OnHintUpdated()
        {
            HintUpdated?.Invoke();
        }
    }

    public class Hint : AbstractHint
    {
        public int _yCoordinate;
        public int topYCoordinate
        {
            get
            {
                return _yCoordinate;
            }
            set
            {
                if (_yCoordinate != value)
                {
                    _yCoordinate = value;
                    if (!hide) OnHintUpdated();
                }
            }
        }

        public int bottomYCoordinate
        {
            get
            {
                return topYCoordinate + fontSize;
            }
            set
            {
                if (_yCoordinate != value - fontSize)
                {
                    _yCoordinate = value - fontSize;
                    if (!hide) OnHintUpdated();
                }
            }
        }

        public Hint(int Y, HintAlignment Alignment, string Message) : base(Alignment, Message)
        {
            this.topYCoordinate = Y;
        }

        public Hint(int Y, HintAlignment Alignment, string Message, string Id) : base(Alignment, Message, Id)
        {
            this.topYCoordinate = Y;
        }

        public Hint(int Y, HintAlignment Alignment, string Message, string Id, bool hide) : base(Alignment, Message, Id)
        {
            this.topYCoordinate = Y;

            this.hide = hide;
        }

        public Hint(Hint hint) : base(hint)
        {
            this.topYCoordinate = hint.topYCoordinate;
        }

        public Hint(DynamicHint dynamicHint, int Y) : base(dynamicHint)
        {
            this.topYCoordinate = Y;
        }

        //Used for printing log
        public override string ToString()
        {
            string str = $"Content:{message}, name:{id}, Y:{topYCoordinate}, align:{alignment}, Hide:{hide}";

            return str;
        }

        public override string GetText()
        {
            string text;
            text = "<line-height={lineHeight}><size={height}><align=\"{alignment}\"><Color=#FFFFFF>{message}</color></align></size></line-height>"
                .Replace("{height}", fontSize.ToString())
                .Replace("{alignment}", alignment.ToString())
                .Replace("{message}", message)
                .Replace("{lineHeight}", "50%");


            return text;
        }

        public Hint setFontSize(int size)
        {
            this.fontSize = size;
            return this;
        }
    }

    public class DynamicHintField
    {
        public int topYCoordinate;
        public int bottomYCoordinate;

        public DynamicHintField(int topYCoordinate, int bottomYCoordinate)
        {
            this.topYCoordinate = topYCoordinate;
            this.bottomYCoordinate = bottomYCoordinate;
        }
    }

    /*
     * Updation plan:
     *  Dynamic Hint priority
     */
    public class DynamicHint : AbstractHint
    {
        public DynamicHintField _hintField;
        public DynamicHintField hintField
        {
            get
            {
                return _hintField;
            }
            set
            {
                if (_hintField != value)
                {
                    _hintField = value;
                    if (!hide) OnHintUpdated();
                }
            }
        }

        public DynamicHint(DynamicHintField HintField, HintAlignment Alignment, string Message) : base(Alignment, Message)
        {
            this.hintField = HintField;
        }

        public DynamicHint(DynamicHintField HintField, HintAlignment Alignment, string Message, string Id) : base(Alignment, Message, Id)
        {
            this.hintField = HintField;
        }

        public DynamicHint(DynamicHintField HintField, HintAlignment Alignment, string Message, string Id, bool hide) : base(Alignment, Message, Id)
        {
            this.hintField = HintField;

            this.hide = hide;
        }

        public DynamicHint(int TopYCoordinate, int BottomYCoordinate, HintAlignment Alignment, string Message) : base(Alignment, Message)
        {
            this.hintField = new DynamicHintField(TopYCoordinate, BottomYCoordinate);
        }

        public DynamicHint(int TopYCoordinate, int BottomYCoordinate, HintAlignment Alignment, string Message, string Id) : base(Alignment, Message, Id)
        {
            this.hintField = new DynamicHintField(TopYCoordinate, BottomYCoordinate);
        }

        public DynamicHint(int TopYCoordinate, int BottomYCoordinate, HintAlignment Alignment, string Message, string Id, bool hide) : base(Alignment, Message, Id)
        {
            this.hintField = new DynamicHintField(TopYCoordinate, BottomYCoordinate);

            this.hide = hide;
        }

        public DynamicHint(DynamicHint hint) : base(hint)
        {
            this.hintField = hint.hintField;
        }

        public override string ToString()
        {
            string str = $"Content:{message}, name:{id}, HintField:{hintField}, align:{alignment}, Hide:{hide}";

            return str;
        }

        public override string GetText()
        {
            string text;
            text = "<line-height={lineHeight}><size={height}><align=\"{alignment}\"><Color=#FFFFFF>{message}</color></align></size></line-height>"
                .Replace("{height}", fontSize.ToString())
                .Replace("{alignment}", alignment.ToString())
                .Replace("{message}", message)
                .Replace("{lineHeight}", "0%");

            return text;
        }

        public DynamicHint setFontSize(int size)
        {
            this.fontSize = size;
            return this;
        }
    }
}
