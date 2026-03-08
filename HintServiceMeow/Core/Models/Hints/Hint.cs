namespace HintServiceMeow.Core.Models.Hints
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Transition;

    /// <summary>
    /// Represents a hint displayed at a fixed position on the player's screen.
    /// </summary>
    public class Hint : AbstractHint
    {
        private HintAlignment alignment = HintAlignment.Center;
        private HintVerticalAlign yCoordinateAlign = HintVerticalAlign.Middle;

        private float xCoordinate = 0;
        private float yCoordinate = 700;

        private Transition? xCoordinateTransition = null;
        private Transition? yCoordinateTransition = null;
        private TransitionState? xCoordinateTransitionState = null;
        private TransitionState? yCoordinateTransitionState = null;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Hint"/> class with default values.
        /// </summary>
        public Hint()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hint"/> class by copying properties from an existing hint.
        /// </summary>
        /// <param name="hint">The hint whose properties are copied into this instance.</param>
        public Hint(Hint hint)
            : base(hint)
        {
            Lock.EnterWriteLock();
            try
            {
                yCoordinate = hint.yCoordinate;
                xCoordinate = hint.xCoordinate;
                alignment = hint.alignment;
                yCoordinateAlign = hint.yCoordinateAlign;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        internal Hint(DynamicHint hint, float x, float y)
            : base(hint)
        {
            Lock.EnterWriteLock();
            try
            {
                yCoordinate = y;
                xCoordinate = x;
                alignment = HintAlignment.Center;
                yCoordinateAlign = HintVerticalAlign.Bottom;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets the Y coordinate of the hint. Higher Y coordinate means lower position
        /// Select from 0 to 1080 on any screen.
        /// </summary>
        public float YCoordinate
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return yCoordinate;
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
                    if (yCoordinate.Equals(value))
                        return;

                    yCoordinate = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(YCoordinate));
            }
        }

        /// <summary>
        /// Gets or sets the horizontal offset of the hint. Higher X coordinate means more to the right
        /// This value should be between -1200 to 1200 including text length.
        /// </summary>
        public float XCoordinate
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return xCoordinate;
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
                    if (xCoordinate.Equals(value))
                        return;

                    xCoordinate = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(XCoordinate));
            }
        }

        /// <summary>
        /// Gets or sets alignment of the hint.
        /// </summary>
        public HintAlignment Alignment
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return alignment;
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
                    if (alignment == value)
                        return;

                    alignment = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(Alignment));
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment reference point used when interpreting <see cref="YCoordinate"/>.
        /// </summary>
        public HintVerticalAlign YCoordinateAlign
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return yCoordinateAlign;
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
                    if (yCoordinateAlign == value)
                        return;

                    yCoordinateAlign = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(YCoordinateAlign));
            }
        }

        public Transition? XCoordinateTransition
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

        /// <summary>
        /// Not thread safe. Should only be used when there's no other thread using it.
        /// </summary>
        /// <param name="dynamicHint">The dynamic hint to be transform.</param>
        /// <param name="x">The X Coordinate.</param>
        /// <param name="y">The Y Coordinate.</param>
        internal void Set(DynamicHint dynamicHint, float x, float y)
        {
            this.CopyFieldsFrom(dynamicHint);

            this.xCoordinateTransition = dynamicHint.XCoordinateTransition;
            this.yCoordinateTransition = dynamicHint.YCoordinateTransition;
            this.xCoordinateTransitionState = dynamicHint.XTransitionState;
            this.yCoordinateTransitionState = dynamicHint.YTransitionState;

            this.xCoordinate = x;
            this.yCoordinate = y;
            this.alignment = HintAlignment.Center;
            this.yCoordinateAlign = HintVerticalAlign.Bottom;
        }
    }
}