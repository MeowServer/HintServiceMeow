using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Config.PlayerDisplayConfigs
{
    public abstract class AbstractHintConfig
    {
        private HintAlignment alignment;

        private int fontSize;

        private string message;

        public AbstractHintConfig(string message, HintAlignment alignment, int fontSize)
        {
            this.message = message;
            this.alignment = alignment;
            this.fontSize = fontSize;
        }
    }

    public class HintConfig : AbstractHintConfig
    {
        public int yCoordinate;

        public HintConfig(int yCoordinate,string message, HintAlignment alignment, int fontSize) : base(message, alignment, fontSize)
        {
            this.yCoordinate = yCoordinate;
        }
    }

    public class DynamicHintConfig : AbstractHintConfig
    {
        public DynamicHintField hintField;

        public DynamicHintConfig(DynamicHintField hintField, string message, HintAlignment alignment, int fontSize, int topYCoordinate, int bottomYCoordinate) : base(message, alignment, fontSize)
        {
            this.hintField = hintField;
        }
    }
}
