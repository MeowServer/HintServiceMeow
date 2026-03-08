namespace HintServiceMeow.Core.Effects
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;
    using UnityEngine;

    public class OutlineEffect : IHintEffect
    {
        public Color32 OutlineColor { get; set; }
        public float OutlineWidth { get; set; }
        public float StartScalar { get; set; }
        public float DurationScalar { get; set; }

        public OutlineEffect(Color32 outlineColor, float outlineWidth, float startScalar = 0f, float durationScalar = 1f)
        {
            this.OutlineColor = outlineColor;
            this.OutlineWidth = outlineWidth;
            this.StartScalar = startScalar;
            this.DurationScalar = durationScalar;
        }

        public HintEffect GetScpslHintEffect()
        {
            return new global::Hints.OutlineEffect(this.OutlineColor, this.OutlineWidth, this.StartScalar, this.DurationScalar);
        }
    }
}
