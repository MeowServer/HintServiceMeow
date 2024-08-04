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

        private HintPriority _priority = HintPriority.Medium;
        private DynamicHintStrategy _strategy = DynamicHintStrategy.Hide;

        #region Constructors

        public DynamicHint()
        {
        }

        public DynamicHint(DynamicHint hint) : base(hint)
        {
            this._topBoundary = hint._topBoundary;
            this._bottomBoundary = hint._bottomBoundary;

            this._leftBoundary = hint._leftBoundary;
            this._rightBoundary = hint._rightBoundary;

            this._priority = hint._priority;
        }

        #endregion

        /// <summary>
        /// The top boundary of the dynamic hint
        /// </summary>
        public float TopBoundary
        {
            get => _topBoundary;
            set
            {
                if (_topBoundary.Equals(value))
                    return;

                _topBoundary = value;

                OnHintUpdated();
            }
        }

        /// <summary>
        /// The bottom boundary of the dynamic hint
        /// </summary>
        public float BottomBoundary
        {
            get => _bottomBoundary;
            set
            {
                if (_bottomBoundary.Equals(value))
                    return;

                _bottomBoundary = value;

                OnHintUpdated();
            }
        }

        /// <summary>
        /// The left boundary of the dynamic hint. Should be more than -1200
        /// </summary>
        public float LeftBoundary
        {
            get => _leftBoundary;
            set
            {
                if (_leftBoundary.Equals(value))
                    return;

                _leftBoundary = value;

                OnHintUpdated();
            }
        }

        /// <summary>
        /// The right boundary of the dynamic hint. Should be less than 1200
        /// </summary>
        public float RightBoundary
        {
            get => _rightBoundary;
            set
            {
                if (_rightBoundary.Equals(value))
                    return;

                _rightBoundary = value;

                OnHintUpdated();
            }
        }

        public float TargetY
        {
            get => _targetY;
            set
            {
                if (_targetY.Equals(value))
                    return;

                _targetY = value;

                OnHintUpdated();
            }
        }

        public float TargetX
        {
            get => _targetX;
            set
            {
                if (_targetX.Equals(value))
                    return;

                _targetX = value;

                OnHintUpdated();
            }
        }

        /// <summary>
        /// The priority of the hint, higher priority means the hint is less likely to be covered by other hint.
        /// </summary>
        public HintPriority Priority
        {
            get => _priority;
            set
            {
                if (_priority == value)
                    return;

                _priority = value;
                OnHintUpdated();
            }
        }

        public DynamicHintStrategy Strategy
        {
            get => _strategy;
            set
            {
                if (_strategy == value)
                    return;

                _strategy = value;
                OnHintUpdated();
            }
        }
    }
}
