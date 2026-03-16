namespace HintServiceMeow.Core.Models.Hints
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.Arguments;
    using HintServiceMeow.Core.Models.HintContent;
    using HintServiceMeow.Core.Models.Transition;
    using HintServiceMeow.Core.Utilities;
    using HintServiceMeow.Core.Utilities.Tools;

    /// <summary>
    /// Represents the base class for all hints displayed on a player's screen.
    /// Provides common properties such as text content, font size, sync speed, and visibility.
    /// </summary>
    public abstract class AbstractHint : INotifyPropertyChanged
    {
        private readonly Guid guid = Guid.NewGuid();

        private IUpdateAnalyser analyser = new UpdateAnalyzer();

        private string id = string.Empty;

        private HintSyncSpeed syncSpeed = HintSyncSpeed.Normal;

        private int previousFontSize = 20;
        private int fontSize = 20;
        private Transition? fontSizeTransition = null;
        private TransitionState? fontSizeTransitionState = null;

        private float lineHeight;

        private AbstractHintContent content = new StringContent(string.Empty);

        private bool hide;

        private HintParameterCollection parameters = new();

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractHint"/> class with default values.
        /// </summary>
        protected AbstractHint()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractHint"/> class by copying properties from an existing hint.
        /// </summary>
        /// <param name="hint">The hint whose properties are copied into this instance.</param>
        protected AbstractHint(AbstractHint hint)
        {
            Lock.EnterWriteLock();
            try
            {
                id = hint.id;
                syncSpeed = hint.syncSpeed;
                fontSize = hint.fontSize;
                lineHeight = hint.lineHeight;
                content = hint.content;
                hide = hint.hide;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }
        #endregion

        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the update analyser used to track and estimate hint update timing.
        /// </summary>
        public IUpdateAnalyser UpdateAnalyser
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return analyser;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    analyser = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets the unique identifier for this hint instance.
        /// </summary>
        public Guid Guid
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return guid;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets the logical identifier used to group or retrieve this hint.
        /// </summary>
        public string Id
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return id;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    id = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets the synchronization speed that controls how quickly this hint's updates are sent to the display.
        /// </summary>
        public HintSyncSpeed SyncSpeed
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return syncSpeed;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    if (syncSpeed == value)
                        return;

                    syncSpeed = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(SyncSpeed));
            }
        }

        /// <summary>
        /// Gets or sets the font size of the hint text.
        /// </summary>
        public int FontSize
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return fontSize;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    if (fontSize == value)
                        return;

                    previousFontSize = value;
                    fontSize = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(FontSize));
            }
        }

        /// <summary>
        /// Gets or sets the transition effect applied when the font size changes.
        /// </summary>
        public Transition? FontSizeTransition
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return fontSizeTransition;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    if (fontSizeTransition == value)
                        return;

                    fontSizeTransition = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(FontSizeTransition));
            }
        }

        /// <summary>
        /// Gets or sets the line height offset for the hint text.
        /// </summary>
        public float LineHeight
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return lineHeight;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    if (lineHeight.Equals(value))
                        return;

                    lineHeight = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(LineHeight));
            }
        }

        /// <summary>
        /// Gets or sets the content displayed by this hint.
        /// </summary>
        public AbstractHintContent Content
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return content;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    if (content == value)
                        return;

                    Content.ContentUpdated -= OnContentUpdate;

                    content = value;
                    content.ContentUpdated += OnContentUpdate;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(Content));
            }
        }

        /// <summary>
        /// Gets or sets the plain text of this hint when the content is a <see cref="StringContent"/>.
        /// Returns <see langword="null"/> if the current content is not a <see cref="StringContent"/>.
        /// </summary>
        public string? Text
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    if (Content is StringContent)
                    {
                        return Content.GetText();
                    }

                    return null;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    if (Content is StringContent textContent)
                    {
                        textContent.Text = value;
                    }
                    else
                    {
                        content.ContentUpdated -= OnContentUpdate;
                        content = new StringContent(value);
                        content.ContentUpdated += OnContentUpdate;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error(ex);
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(Text));
            }
        }

        /// <summary>
        /// Gets or sets the auto-text handler used to dynamically generate hint content.
        /// Setting this property replaces the current content with an <see cref="AutoContent"/> instance.
        /// Returns <see langword="null"/> if the current content is not an <see cref="AutoContent"/>.
        /// </summary>
        public AutoContent.TextUpdateHandler? AutoText
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    if (Content is AutoContent autoContent)
                    {
                        return autoContent.AutoText;
                    }

                    return null;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    content.ContentUpdated -= OnContentUpdate;
                    content = new AutoContent(value);
                    content.ContentUpdated += OnContentUpdate;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(AutoText));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this hint is hidden from the player's display.
        /// </summary>
        public bool Hide
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return hide;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    if (hide == value)
                        return;

                    hide = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(Hide));
            }
        }

        public HintParameterCollection Parameters
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return parameters;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        internal TransitionState? FontSizeTransitionState
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return fontSizeTransitionState;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    fontSizeTransitionState = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        internal float CurrentFontSize
        {
            get
            {
                Lock.EnterReadLock();

                try
                {
                    if (fontSizeTransitionState is null)
                        return fontSize;

                    return fontSizeTransitionState.CurrentValue;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        internal int PreviousFontSize
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return previousFontSize;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets the reader/writer lock used to synchronize access to this hint's fields.
        /// </summary>
        protected ReaderWriterLockSlim Lock { get; } = new(LockRecursionPolicy.SupportsRecursion);
        #endregion

        #region Methods

        /// <summary>
        /// Attempts to update the hint content in response to an update-available event.
        /// </summary>
        /// <param name="ev">The event arguments containing the player display context.</param>
        public virtual void TryUpdateHint(UpdateAvailableEventArg ev)
        {
            Content.TryUpdate(new ContentUpdateArg(this, ev.PlayerDisplay));
        }

        /// <summary>
        /// Not thread friendly, should only be used in pool.
        /// </summary>
        /// <param name="copyFrom">Copy parameter from.</param>
        internal void CopyFieldsFrom(AbstractHint copyFrom)
        {
            this.id = copyFrom.Id;
            this.syncSpeed = copyFrom.SyncSpeed;
            this.previousFontSize = copyFrom.PreviousFontSize;
            this.fontSize = copyFrom.FontSize;
            this.lineHeight = copyFrom.LineHeight;
            this.content = copyFrom.Content;
            this.hide = copyFrom.Hide;
            this.parameters = copyFrom.Parameters;
            this.fontSizeTransition = copyFrom.FontSizeTransition;
            this.fontSizeTransitionState = copyFrom.FontSizeTransitionState;
        }

        /// <summary>
        /// Not thread friendly, should only be used in pool.
        /// </summary>
        internal void ResetFields()
        {
            this.id = string.Empty;
            this.content = null!;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event and notifies the update analyser of a change.
        /// </summary>
        /// <param name="argumentName">The name of the property that changed.</param>
        protected virtual void OnHintUpdated(string argumentName)
        {
            analyser.OnUpdate();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(argumentName));
        }

        private void OnContentUpdate()
        {
            OnHintUpdated(nameof(Content));
        }
        #endregion
    }
}
