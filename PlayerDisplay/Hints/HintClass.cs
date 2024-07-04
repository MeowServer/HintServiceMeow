

using System;
using static Utils.Networking.HintReaderWriter;


namespace HintServiceMeow
{
    public abstract class AbstractHint
    {
        public delegate void UpdateHandler();
        public event UpdateHandler HintUpdated;

        private string _id = null;
        public string id
        {
            get => _id;
            set => _id = value;
        }

        private HintPriority _priority = HintPriority.Medium;
        public HintPriority priority
        {
            get => _priority;
            set
            {
                if (_priority == value)
                    return;
                
                _priority = value;
                if (!hide) OnHintUpdated();
            }
        }

        private HintAlignment _alignment = HintAlignment.Center;
        public HintAlignment alignment
        {
            get => _alignment;
            set
            {
                if (_alignment == value)
                    return;

                _alignment = value;
                if (!hide) OnHintUpdated();
            }
        }

        private int _fontSize = 20;
        public int fontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize == value)
                    return;

                _fontSize = value;
                if (!hide) OnHintUpdated();
            }
        }

        private string _message = string.Empty;
        public string message
        {
            get => _message;
            set
            {
                if (_message == value)
                    return;

                _message = value;
                if (!hide) OnHintUpdated();
            }
        }

        private bool _hide = false;
        public bool hide
        {
            get => _hide;
            set
            {
                if (_hide == value)
                    return;

                _hide = value;
                OnHintUpdated();
            }
        }

        #region Constructors
        protected AbstractHint()
        {
        }

        protected AbstractHint(HintAlignment alignment, string message)
        {
            this.alignment = alignment;

            this.message = message;
        }

        protected AbstractHint(AbstractHint hint)
        {
            this.fontSize = hint.fontSize;

            this.alignment = hint.alignment;

            this.message = hint.message;

            this.id = hint.id;

            this.hide = hint.hide;
        }
        #endregion

        #region Property Setters
        public virtual AbstractHint SetId(string id)
        {
            this.id = id;
            return this;
        }

        public virtual AbstractHint SetPriority(HintPriority priority)
        {
            this.priority = priority;
            return this;
        }

        public virtual AbstractHint SetAlignment(HintAlignment alignment)
        {
            this.alignment = alignment;
            return this;
        }

        public virtual AbstractHint SetFontSize(int fontSize)
        {
            this.fontSize = fontSize;
            return this;
        }

        public virtual AbstractHint SetMessage(string message)
        {
            this.message = message;
            return this;
        }

        public virtual AbstractHint SetHide(bool hide)
        {
            this.hide = hide;
            return this;
        }
        #endregion

        public abstract override string ToString();

        public abstract string GetText();

        protected void OnHintUpdated()
        {
            HintUpdated?.Invoke();
        }
    }

    public class Hint : AbstractHint
    {
        private int _yCoordinate = 500;
        public int topYCoordinate
        {
            get => _yCoordinate;
            set
            {
                if (_yCoordinate == value)
                    return;

                _yCoordinate = value;
                if (!hide) OnHintUpdated();
            }
        }

        public int bottomYCoordinate
        {
            get => topYCoordinate + fontSize;
            set => topYCoordinate = value - fontSize;
        }

        #region Constructors
        public Hint(): base()
        {
        }

        public Hint(int Y, HintAlignment Alignment, string Message) : base(Alignment, Message)
        {
            this.topYCoordinate = Y;
        }

        public Hint(int Y, HintAlignment Alignment, string Message, string Id) : base(Alignment, Message)
        {
            this.topYCoordinate = Y;
        }

        public Hint(int Y, HintAlignment Alignment, string Message, string Id, bool hide) : base(Alignment, Message)
        {
            this.topYCoordinate = Y;

            this.hide = hide;
        }

        public Hint(HintConfig config)
        {
            this.id = config.Id;
            this.priority = config.Priority;
            this.alignment = config.Position.Alignment;
            this.fontSize = config.Position.FontSize;
            this.message = config.Content.Message;
            this.hide = config.Content.Hide;
            this.topYCoordinate = config.Position.YCoordinate;
        }

        public Hint(Hint hint) : base(hint)
        {
            this.topYCoordinate = hint.topYCoordinate;
        }

        public Hint(DynamicHint dynamicHint, int Y) : base(dynamicHint)
        {
            this.topYCoordinate = Y;
        }
        #endregion

        #region Property Setters
        public new Hint SetId(string id) => (Hint)base.SetId(id);

        public new Hint SetPriority(HintPriority priority) => (Hint)base.SetPriority(priority);

        public new Hint SetAlignment(HintAlignment alignment) => (Hint)base.SetAlignment(alignment);

        public new Hint SetFontSize(int fontSize) => (Hint)base.SetFontSize(fontSize);

        public new Hint SetMessage(string message) => (Hint)base.SetMessage(message);

        public new Hint SetHide(bool hide) => (Hint)base.SetHide(hide);

        public Hint SetTopYCoordinate(int y)
        {
            this.topYCoordinate = y;
            return this;
        }

        public Hint SetBottomYCoordinate(int y)
        {
            this.topYCoordinate = y + fontSize;
            return this;
        }
        #endregion

        public override string ToString()
        {
            string str = $"Content:{message}, name:{id}, Y:{topYCoordinate}, align:{alignment}, Hide:{hide}";

            return str;
        }

        public override string GetText()
        {
            string text =
                "<line-height={lineHeight}><size={height}><align=\"{alignment}\"><Color=#FFFFFF>{message}</color></align></size></line-height>"
                    .Replace("{height}", fontSize.ToString())
                    .Replace("{alignment}", alignment.ToString())
                    .Replace("{message}", message)
                    .Replace("{lineHeight}", "50%");

            return text;
        }
    }

    public class DynamicHint : AbstractHint
    {
        private DynamicHintField _hintField = null;
        public DynamicHintField hintField
        {
            get
            {
                if (_hintField == null)
                {
                    _hintField = new DynamicHintField(300, 700);
                    _hintField.OnUpdate += OnHintUpdated;
                }

                return _hintField;
            }
            set
            {
                if (_hintField == value)
                    return;

                if (_hintField == null)
                    _hintField = new DynamicHintField(300, 700);

                _hintField.OnUpdate -= OnHintUpdated;
                value.OnUpdate += OnHintUpdated;

                _hintField = value;

                if (!hide) OnHintUpdated();
            }
        }

        #region Constructors
        public DynamicHint() : base()
        {
        }

        public DynamicHint(DynamicHintField HintField, HintAlignment Alignment, string Message) : base(Alignment, Message)
        {
            this.hintField = HintField;
        }

        public DynamicHint(DynamicHintField HintField, HintAlignment Alignment, string Message, string Id) : base(Alignment, Message)
        {
            this.hintField = HintField;
        }

        public DynamicHint(DynamicHintField HintField, HintAlignment Alignment, string Message, string Id, bool hide) : base(Alignment, Message)
        {
            this.hintField = HintField;

            this.hide = hide;
        }

        public DynamicHint(int TopYCoordinate, int BottomYCoordinate, HintAlignment Alignment, string Message) : base(Alignment, Message)
        {
            this.hintField = new DynamicHintField(TopYCoordinate, BottomYCoordinate);
        }

        public DynamicHint(int TopYCoordinate, int BottomYCoordinate, HintAlignment Alignment, string Message, string Id) : base(Alignment, Message)
        {
            this.hintField = new DynamicHintField(TopYCoordinate, BottomYCoordinate);
        }

        public DynamicHint(int TopYCoordinate, int BottomYCoordinate, HintAlignment Alignment, string Message, string Id, bool hide) : base(Alignment, Message)
        {
            this.hintField = new DynamicHintField(TopYCoordinate, BottomYCoordinate);

            this.hide = hide;
        }

        public DynamicHint(DynamicHintConfig config)
        {
            this.id = config.Id;
            this.priority = config.Priority;
            this.alignment = config.Position.Alignment;
            this.fontSize = config.Position.FontSize;
            this.message = config.Content.Message;
            this.hide = config.Content.Hide;
            this.hintField = new DynamicHintField(config.Position.TopYCoordinate, config.Position.BottomYCoordinate);
        }

        public DynamicHint(DynamicHint hint) : base(hint)
        {
            this.hintField = hint.hintField;
        }
        #endregion

        #region Property Setters
        public new DynamicHint SetId(string id) => (DynamicHint)base.SetId(id);

        public new DynamicHint SetPriority(HintPriority priority) => (DynamicHint)base.SetPriority(priority);

        public new DynamicHint SetAlignment(HintAlignment alignment) => (DynamicHint)base.SetAlignment(alignment);

        public new DynamicHint SetFontSize(int fontSize) => (DynamicHint)base.SetFontSize(fontSize);

        public new DynamicHint SetMessage(string message) => (DynamicHint)base.SetMessage(message);

        public new DynamicHint SetHide(bool hide) => (DynamicHint)base.SetHide(hide);

        public DynamicHint SetHintField(DynamicHintField hintField)
        {
            this.hintField = hintField;
            return this;
        }
        #endregion

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
    }
}
