using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Models
{
    public class RichTextSize
    {
        public float Width { get; set; }
        public float Height { get; set; }

        public RichTextSize()
        {
            Width = 0;
            Height = 0;
        }

        public RichTextSize(float width, float height)
        {
            Width = width;
            Height = height;
        }
    }
}
