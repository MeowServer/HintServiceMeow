using HintServiceMeow.Core.Enum;

namespace HintServiceMeow.Core.Models.Hints
{
    public class DynamicHint : AbstractHint
    {
        private float _topBoundary = 0;
        private float _bottomBoundary = 1000;

        private float _leftBoundary = -1200;
        private float _rightBoundary = 1200;

        private float _targetY = 700;
        private float _targetX = 0;

        private float _topMargin = 5;
        private float _bottomMargin = 5;
        private float _leftMargin = 100;
        private float _rightMargin = 100;

        private HintPriority _priority = HintPriority.Medium;
        private DynamicHintStrategy _strategy = DynamicHintStrategy.Hide;

        #region Constructors

        public DynamicHint()
        {
        }

        public DynamicHint(DynamicHint hint) : base(hint)
        {
            Lock.EnterWriteLock();
            try
            {
                this._topBoundary = hint._topBoundary;
                this._bottomBoundary = hint._bottomBoundary;

                this._leftBoundary = hint._leftBoundary;
                this._rightBoundary = hint._rightBoundary;

                this._targetY = hint._targetY;
                this._targetX = hint._targetX;

                this._topMargin = hint._topMargin;
                this._bottomMargin = hint._bottomMargin;
                this._leftMargin = hint._leftMargin;
                this._rightMargin = hint._rightMargin;

                this._priority = hint._priority;
                this._strategy = hint._strategy;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        #endregion

        /// <summary>
        /// The top boundary of the dynamic hint
        /// </summary>
        public float TopBoundary
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _topBoundary;
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
                    if (_topBoundary.Equals(value))
                        return;

                    _topBoundary = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// The bottom boundary of the dynamic hint
        /// </summary>
        public float BottomBoundary
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _bottomBoundary;
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
                    if (_bottomBoundary.Equals(value))
                        return;

                    _bottomBoundary = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// The left boundary of the dynamic hint. Should be more than -1200
        /// </summary>
        public float LeftBoundary
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _leftBoundary;
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
                    if (_leftBoundary.Equals(value))
                        return;

                    _leftBoundary = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// The right boundary of the dynamic hint. Should be less than 1200
        /// </summary>
        public float RightBoundary
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _rightBoundary;
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
                    if (_rightBoundary.Equals(value))
                        return;

                    _rightBoundary = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// The Y coordinate that dynamic hint will try to reach
        /// </summary>
        public float TargetY
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _targetY;
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
                    if (_targetY.Equals(value))
                        return;

                    _targetY = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// The X coordinate that dynamic hint will try to reach
        /// </summary>
        public float TargetX
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _targetX;
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
                    if (_targetX.Equals(value))
                        return;

                    _targetX = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        public float TopMargin
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _topMargin;
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
                    if (_topMargin.Equals(value))
                        return;

                    _topMargin = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        public float BottomMargin
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _bottomMargin;
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
                    if (_bottomMargin.Equals(value))
                        return;

                    _bottomMargin = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        public float LeftMargin
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _leftMargin;
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
                    if (_leftMargin.Equals(value))
                        return;

                    _leftMargin = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        public float RightMargin
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _rightMargin;
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
                    if (_rightMargin.Equals(value))
                        return;

                    _rightMargin = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// The priority of the hint, higher priority means the hint is less likely to be covered by other hint.
        /// </summary>
        public HintPriority Priority
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _priority;
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
                    if (_priority == value)
                        return;

                    _priority = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        public DynamicHintStrategy Strategy
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _strategy;
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
                    if (_strategy == value)
                        return;

                    _strategy = value;
                    OnHintUpdated();
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }
    }

}
