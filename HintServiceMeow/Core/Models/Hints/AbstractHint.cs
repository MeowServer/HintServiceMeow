using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.HintContent.HintContent;
using HintServiceMeow.Core.Utilities;

namespace HintServiceMeow.Core.Models.Hints
{
    public abstract class AbstractHint
    {
        internal readonly UpdateAnalyser Analyser = new UpdateAnalyser();

        private string _id = string.Empty;

        private HintSyncSpeed _syncSpeed = HintSyncSpeed.Normal;

        private int _fontSize = 20;

        private AbstractHintContent _content = new StringContent("");

        private bool _hide = false;

        #region Delegates

        internal delegate void UpdateHandler(AbstractHint ev);

        #endregion

        #region Events

        internal event UpdateHandler HintUpdated;

        #endregion

        #region Constructors

        protected AbstractHint()
        {
        }

        protected AbstractHint(AbstractHint hint)
        {
            this._id = hint._id;
            this._syncSpeed = hint._syncSpeed;
            this._fontSize = hint._fontSize;
            this._content = hint._content;
            this._hide = hint._hide;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The id of the hint, used to indicate the hint in player display
        /// </summary>
        public string Id { get => _id; set => _id = value; }

        /// <summary>
        /// The sync speed of the hint. Higher speed means faster to sync.
        /// Overly high speed might jam the updater.
        /// </summary>
        public HintSyncSpeed SyncSpeed
        {
            get => _syncSpeed;
            set
            {
                if (_syncSpeed == value)
                    return;

                _syncSpeed = value;
                OnHintUpdated();
            }
        }

        /// <summary>
        /// The height of the Font
        /// </summary>
        public int FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize == value)
                    return;

                _fontSize = value;
                OnHintUpdated();
            }
        }

        /// <summary>
        /// Get or set the content of the hint
        /// </summary>
        public AbstractHintContent Content
        {
            get => _content;
            set
            {
                if (_content == value)
                    return;

                _content = value;
                _content.ContentUpdated += OnHintUpdated;
                OnHintUpdated();
            }
        }

        /// <summary>
        /// Set the text displayed by the hint. This will override current content
        /// </summary>
        public string Text
        {
            get
            {
                if (Content is StringContent)
                {
                    return Content.GetText(new TextUpdateArg(this, null));
                }

                return null;
            }
            set
            {
                Content = new StringContent(value);
                OnHintUpdated();
            }
        }

        /// <summary>
        /// Set the auto text of the hint. This will override current content
        /// </summary>
        public AutoContent.TextUpdateHandler AutoText
        {
            get
            {
                if (Content is AutoContent content)
                {
                    return content.AutoText;
                }

                return null;
            }
            set
            {
                Content = new AutoContent(value);
                OnHintUpdated();
            }
        }

        /// <summary>
        /// Whether this hint was hided
        /// </summary>
        public bool Hide
        {
            get => _hide;
            set
            {
                if (_hide == value)
                    return;

                _hide = value;
                OnHintUpdated();

                if(Hide) HintUpdated?.Invoke(this);
            }
        }

        #endregion

        #region Methods

        internal void TryUpdateHint(PlayerDisplay.UpdateAvailableEventArg ev)
        {
            Content.TryUpdate(new TextUpdateArg(this, ev.PlayerDisplay));
        }

        protected void OnHintUpdated()
        {
            Analyser.OnUpdate();

            if (!Hide)
            {
                HintUpdated?.Invoke(this);
            }
        }

        #endregion

        public class TextUpdateArg
        {
            public ReferenceHub Player => PlayerDisplay?.ReferenceHub; 
            public AbstractHint Hint { get; }
            public PlayerDisplay PlayerDisplay { get; }

            internal TextUpdateArg(AbstractHint hint, PlayerDisplay playerDisplay)
            {
                this.Hint = hint;
                this.PlayerDisplay = playerDisplay;
            }
        }
    }
}
