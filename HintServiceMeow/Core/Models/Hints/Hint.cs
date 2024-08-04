using System;
using HintServiceMeow.Core.Enum;

namespace HintServiceMeow.Core.Models.Hints
{
    public class Hint : AbstractHint
    {
        /// <summary>
        /// Represent the hint alignment. This value directly related to Align tag
        /// </summary>
        private HintAlignment _alignment = HintAlignment.Center;

        /// <summary>
        /// Represent the vertical alignment of the hint.
        /// </summary>
        private HintVerticalAlign _yCoordinateAlign = HintVerticalAlign.Middle;

        /// <summary>
        /// Represent the X Coordinate of the hint. This value directly related to Pos tag
        /// Higher X coordinate means more to the right
        /// </summary>
        private float _xCoordinate = 0;

        /// <summary>
        /// Represent the Y coordinate of the hint. This value directly related to VOffset tag
        /// Higher Y coordinate means lower position. Select from 0 to 1080
        /// </summary>
        private float _yCoordinate = 700;

        #region Constructors
        public Hint() : base()
        {
        }

        public Hint(Hint hint) : base(hint)
        {
            this.YCoordinate = hint.YCoordinate;
            this.XCoordinate = hint.XCoordinate;
        }

        internal Hint(DynamicHint hint, float x, float y) : base(hint)
        {
            this.YCoordinate = y;
            this.XCoordinate = x;
        }
        #endregion

        /// <summary>
        /// The Y coordinate of the hint. Higher Y coordinate means lower position
        /// Select from 0 to 1080
        /// </summary>
        public float YCoordinate
        {
            get => _yCoordinate;
            set
            {
                if (_yCoordinate.Equals(value))
                    return;

                value = Math.Max(0, value);
                value = Math.Min(1080, value);

                _yCoordinate = value;
                OnHintUpdated();
            }
        }

        /// <summary>
        /// The horizontal offset of the hint. Higher X coordinate means more to the right
        /// This value should be between -1200 to 1200 including text length
        /// </summary>
        public float XCoordinate
        {
            get => _xCoordinate;
            set
            {
                if (_xCoordinate.Equals(value))
                    return;

                value = Math.Max(-1200, value);
                value = Math.Min(1200, value);

                _xCoordinate = value;
                OnHintUpdated();
            }
        }

        /// <summary>
        /// Alignment of the hint
        /// </summary>
        public HintAlignment Alignment
        {
            get => _alignment;
            set
            {
                if (_alignment == value)
                    return;

                _alignment = value;
                if (!Hide) OnHintUpdated();
            }
        }

        /// <summary>
        /// VerticalAlign of the hint
        /// </summary>
        public HintVerticalAlign YCoordinateAlign
        {
            get => _yCoordinateAlign;
            set
            {
                if (_yCoordinateAlign == value)
                    return;

                _yCoordinateAlign = value;
                if (!Hide) OnHintUpdated();
            }
        }
    }
}