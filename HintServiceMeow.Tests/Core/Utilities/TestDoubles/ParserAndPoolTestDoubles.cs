using System;
using System.Collections.Concurrent;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Models.Parser;
using HintServiceMeow.Core.Models.Parser.Style;

namespace HintServiceMeow.Tests.Core.Utilities.TestDoubles;

internal sealed class RecordingPool<T> : IPool<T>
    where T : class
{
    private readonly Func<T> factory;

    public RecordingPool(Func<T> factory) => this.factory = factory;

    public int RentCount { get; private set; }

    public int ReturnCount { get; private set; }

    public bool ThrowOnRent { get; set; }

    public bool ThrowOnReturn { get; set; }

    public T Rent()
    {
        RentCount++;
        if (ThrowOnRent)
            throw new InvalidOperationException("Rent failed");
        return factory();
    }

    public void Return(T item)
    {
        ReturnCount++;
        if (ThrowOnReturn)
            throw new InvalidOperationException("Return failed");
    }
}

internal sealed class ConcurrentCache<TKey, TValue> : ICache<TKey, TValue>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TValue> map = new();

    public int AddCount { get; private set; }

    public void Add(TKey key, TValue data)
    {
        AddCount++;
        map[key] = data;
    }

    public bool TryGet(TKey key, out TValue item) => map.TryGetValue(key, out item!);

    public bool TryRemove(TKey key, out TValue item) => map.TryRemove(key, out item!);
}

internal sealed class StubCoordinateTools : ICoordinateTools
{
    public Func<AbstractHint, float> TextWidth { get; set; } = _ => 100f;
    public Func<AbstractHint, float> TextHeight { get; set; } = _ => 20f;
    public Func<Hint, HintVerticalAlign, float> YConverter { get; set; } = (h, _) => h.YCoordinate;

    public float GetYCoordinate(Hint hint, HintVerticalAlign to) => YConverter(hint, to);

    public float GetYCoordinate(Hint hint, HintVerticalAlign from, HintVerticalAlign to) => YConverter(hint, to);

    public float GetYCoordinate(float rawYCoordinate, float textHeight, HintVerticalAlign from, HintVerticalAlign to) => rawYCoordinate;

    public float GetCurrentYCoordinate(Hint hint, HintVerticalAlign to) => YConverter(hint, to);

    public float GetCurrentYCoordinate(Hint hint, HintVerticalAlign from, HintVerticalAlign to) => YConverter(hint, to);

    public float GetCurrentYCoordinate(float rawYCoordinate, float textHeight, HintVerticalAlign from, HintVerticalAlign to) => rawYCoordinate;

    public float GetXCoordinateWithAlignment(Hint hint) => hint.XCoordinate;

    public float GetXCoordinateWithAlignment(Hint hint, HintAlignment alignment) => hint.XCoordinate;

    public float GetTextWidth(AbstractHint hint) => TextWidth(hint);

    public float GetTextWidth(string text, float fontSize, HintAlignment align = HintAlignment.Center) => 100f;

    public float GetTextHeight(AbstractHint hint) => TextHeight(hint);

    public float GetTextHeight(string text, float fontSize, float lineHeight) => 20f;

    public LineInfo[] GetLineInfos(string text, float fontSize, HintAlignment align = HintAlignment.Center)
        => [new LineInfo([], LineStyle.Default, string.Empty)];

    public float GetEdgeOffset(float xyRatio, HintAlignment alignment) => 0f;
}
