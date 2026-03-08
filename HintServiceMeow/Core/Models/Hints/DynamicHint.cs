namespace HintServiceMeow.Core.Models.Hints
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Transition;

    /// <summary>
    /// Represents a hint that dynamically positions itself within defined boundaries on the player's screen,
    /// avoiding overlap with other hints.
    /// </summary>
    public class DynamicHint : AbstractHint
    {
        private float topBoundary = 0;
        private float bottomBoundary = 1000;

        private float leftBoundary = -1200;
        private float rightBoundary = 1200;

        private float targetY = 700;
        private float targetX = 0;

        private float previousXCoordinate = 0;
        private float previousYCoordinate = 700;

        private float topMargin = 5;
        private float bottomMargin = 5;
        private float leftMargin = 100;
        private float rightMargin = 100;

        private Transition? xCoordinateTransition = null;
        private Transition? yCoordinateTransition = null;
        private TransitionState? xCoordinateTransitionState = null;
        private TransitionState? yCoordinateTransitionState = null;

        private HintPriority priority = HintPriority.Medium;
        private DynamicHintStrategy strategy = DynamicHintStrategy.Hide;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicHint"/> class with default values.
        /// </summary>
        public DynamicHint()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicHint"/> class by copying properties from an existing dynamic hint.
        /// </summary>
        /// <param name="hint">The dynamic hint whose properties are copied into this instance.</param>
        public DynamicHint(DynamicHint hint)
            : base(hint)
        {
            Lock.EnterWriteLock();
            try
            {
                topBoundary = hint.topBoundary;
                bottomBoundary = hint.bottomBoundary;

                leftBoundary = hint.leftBoundary;
                rightBoundary = hint.rightBoundary;

                targetY = hint.targetY;
                targetX = hint.targetX;

                previousYCoordinate = hint.previousYCoordinate;
                previousXCoordinate = hint.previousXCoordinate;

                topMargin = hint.topMargin;
                bottomMargin = hint.bottomMargin;
                leftMargin = hint.leftMargin;
                rightMargin = hint.rightMargin;

                priority = hint.priority;
                strategy = hint.strategy;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the previous Y coordinate for animation purposes.
        /// </summary>
        public float PreviousYCoordinate
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return previousYCoordinate;
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
                    if (previousYCoordinate.Equals(value))
                        return;

                    previousYCoordinate = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(PreviousYCoordinate));
            }
        }

        /// <summary>
        /// Gets or sets the previous X coordinate for animation purposes.
        /// </summary>
        public float PreviousXCoordinate
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return previousXCoordinate;
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
                    if (previousXCoordinate.Equals(value))
                        return;

                    previousXCoordinate = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(PreviousXCoordinate));
            }
        }

        /// <summary>
        /// Gets or sets the top boundary of the dynamic hint.
        /// </summary>
        public float TopBoundary
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return topBoundary;
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
                    if (topBoundary.Equals(value))
                        return;

                    topBoundary = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(TopBoundary));
            }
        }

        /// <summary>
        /// Gets or sets the bottom boundary of the dynamic hint.
        /// </summary>
        public float BottomBoundary
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return bottomBoundary;
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
                    if (bottomBoundary.Equals(value))
                        return;

                    bottomBoundary = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(BottomBoundary));
            }
        }

        /// <summary>
        /// Gets or sets the left boundary of the dynamic hint. Should be more than -1200.
        /// </summary>
        public float LeftBoundary
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return leftBoundary;
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
                    if (leftBoundary.Equals(value))
                        return;

                    leftBoundary = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(LeftBoundary));
            }
        }

        /// <summary>
        /// Gets or sets the right boundary of the dynamic hint. Should be less than 1200.
        /// </summary>
        public float RightBoundary
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return rightBoundary;
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
                    if (rightBoundary.Equals(value))
                        return;

                    rightBoundary = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(RightBoundary));
            }
        }

        /// <summary>
        /// Gets or sets the Y coordinate that dynamic hint will try to reach.
        /// </summary>
        public float TargetY
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return targetY;
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
                    if (targetY.Equals(value))
                        return;

                    targetY = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(TargetY));
            }
        }

        /// <summary>
        /// Gets or sets the X coordinate that dynamic hint will try to reach.
        /// </summary>
        public float TargetX
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return targetX;
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
                    if (targetX.Equals(value))
                        return;

                    targetX = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(TargetX));
            }
        }

        /// <summary>
        /// Gets or sets the top margin in pixels between this hint and any hint placed above it.
        /// </summary>
        public float TopMargin
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return topMargin;
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
                    if (topMargin.Equals(value))
                        return;

                    topMargin = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(TopMargin));
            }
        }

        /// <summary>
        /// Gets or sets the bottom margin in pixels between this hint and any hint placed below it.
        /// </summary>
        public float BottomMargin
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return bottomMargin;
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
                    if (bottomMargin.Equals(value))
                        return;

                    bottomMargin = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(BottomMargin));
            }
        }

        /// <summary>
        /// Gets or sets the left margin in pixels applied when this hint is positioned horizontally.
        /// </summary>
        public float LeftMargin
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return leftMargin;
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
                    if (leftMargin.Equals(value))
                        return;

                    leftMargin = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(LeftMargin));
            }
        }

        /// <summary>
        /// Gets or sets the right margin in pixels applied when this hint is positioned horizontally.
        /// </summary>
        public float RightMargin
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return rightMargin;
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
                    if (rightMargin.Equals(value))
                        return;

                    rightMargin = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(RightMargin));
            }
        }

        public Transition XCoordinateTransition
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return xCoordinateTransition;
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
                    if (xCoordinateTransition == value)
                        return;

                    xCoordinateTransition = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(XCoordinateTransition));
            }
        }

        public Transition? YCoordinateTransition
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return yCoordinateTransition;
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
                    if (yCoordinateTransition == value)
                        return;

                    yCoordinateTransition = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(YCoordinateTransition));
            }
        }

        /// <summary>
        /// Gets or sets the priority of the hint, higher priority means the hint is less likely to be covered by other hint.
        /// </summary>
        public HintPriority Priority
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return priority;
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
                    if (priority == value)
                        return;

                    priority = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(Priority));
            }
        }

        /// <summary>
        /// Gets or sets the fallback strategy used when no valid display position is available.
        /// </summary>
        public DynamicHintStrategy Strategy
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return strategy;
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
                    if (strategy == value)
                        return;

                    strategy = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(Strategy));
            }
        }

        internal TransitionState? XTransitionState
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return xCoordinateTransitionState;
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
                    xCoordinateTransitionState = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        internal TransitionState? YTransitionState
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return yCoordinateTransitionState;
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
                    yCoordinateTransitionState = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }
    }
}