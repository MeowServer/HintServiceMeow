using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.HintContent.HintContent;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Core.Utilities.Tools;
using System;
using System.Threading;

namespace HintServiceMeow.Core.Models.Hints
{
    public abstract class AbstractHint
    {
        protected ReaderWriterLockSlim Lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private IUpdateAnalyser _analyser = new UpdateAnalyzer();

        private readonly Guid _guid = Guid.NewGuid();
        private string _id = string.Empty;

        private HintSyncSpeed _syncSpeed = HintSyncSpeed.Normal;

        private int _fontSize = 20;

        private float _lineHeight = 0;

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
            Lock.EnterWriteLock();
            try
            {
                this._id = hint._id;
                this._syncSpeed = hint._syncSpeed;
                this._fontSize = hint._fontSize;
                this._lineHeight = hint._lineHeight;
                this._content = hint._content;
                this._hide = hint._hide;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        #endregion

        #region Properties

        public IUpdateAnalyser UpdateAnalyser
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _analyser;
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
                    _analyser = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        public Guid Guid
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _guid;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        public string Id
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _id;
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
                    _id = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        public HintSyncSpeed SyncSpeed
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _syncSpeed;
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
                    if (_syncSpeed == value)
                        return;

                    _syncSpeed = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        public int FontSize
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _fontSize;
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
                    if (_fontSize == value)
                        return;

                    _fontSize = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        public float LineHeight
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _lineHeight;
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
                    if (_lineHeight.Equals(value))
                        return;

                    _lineHeight = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        public AbstractHintContent Content
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _content;
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
                    if (_content == value)
                        return;

                    _content = value;
                    _content.ContentUpdated += OnHintUpdated;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        public string Text
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
                        Content = new StringContent(value);
                    }

                    OnHintUpdated();
                }
                catch (Exception ex)
                {
                    LogTool.Error(ex);
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        public AutoContent.TextUpdateHandler AutoText
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    if (Content is AutoContent content)
                    {
                        return content.AutoText;
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
                    Content = new AutoContent(value);
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        public bool Hide
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _hide;
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
                    if (_hide == value)
                        return;

                    _hide = value;
                    OnHintUpdated();

                    if (_hide)
                    {
                        HintUpdated?.Invoke(this);
                    }
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        #endregion

        #region Methods

        public virtual void TryUpdateHint(PlayerDisplay.UpdateAvailableEventArg ev)
        {
            Content.TryUpdate(new TextUpdateArg(this, ev.PlayerDisplay));
        }

        protected virtual void OnHintUpdated()
        {
            _analyser.OnUpdate();

            if (!_hide)
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

            public float NextUpdateDelay { get; set; } = 0.1f;

            internal TextUpdateArg(AbstractHint hint, PlayerDisplay playerDisplay)
            {
                this.Hint = hint;
                this.PlayerDisplay = playerDisplay;
            }
        }
    }
}
