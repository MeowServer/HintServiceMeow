using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using PluginAPI.Core;
using UnityEngine.UIElements;

namespace HintServiceMeow.Core.Utilities
{
    internal static class HintParser
    {
        public static string GetMessage(HashSet<AbstractHint> rawHintList, PlayerDisplay pd)
        {
            List<Hint> hintList = rawHintList
                .Where(x => !x.Hide)
                .OfType<Hint>()
                .ToList();

            List<DynamicHint> dynamicHintList = rawHintList
                .Where(x => !x.Hide)
                .OfType<DynamicHint>()
                .ToList();

            foreach (var dynamicHint in dynamicHintList)
            {
                InsertDynamicHint(dynamicHint, hintList);
            }

            return GetText(hintList, pd);
        }

        public static void InsertDynamicHint(DynamicHint dynamicHint, List<Hint> hintList)
        {
            var queue = new Queue<Tuple<float, float>>();
            var visited = new HashSet<Tuple<float, float>>();

            queue.Enqueue(Tuple.Create(dynamicHint.TargetX, dynamicHint.TargetY));

            while (queue.Count > 0)
            {
                //Represent bottom center coordinate of the hint
                var (x, y) = queue.Dequeue();

                if (visited.Contains(Tuple.Create(x, y)))
                    continue;

                visited.Add(Tuple.Create(x, y));

                var hasIntersectedHint = hintList.Any(hint =>
                {
                    float widthA = FontTool.GetTextWidth(hint);
                    float leftA = GetActualXCoordinate(hint) - widthA / 2;
                    float rightA = GetActualXCoordinate(hint) + widthA / 2;
                    float topA = hint.YCoordinate + hint.FontSize;
                    float bottomA = hint.YCoordinate;

                    float widthB = FontTool.GetTextWidth(dynamicHint);
                    float leftB = x - widthB / 2;
                    float rightB = x + widthB / 2;
                    float topB = y + dynamicHint.FontSize;
                    float bottomB = y;

                    return !(rightA < leftB || leftA > rightB || topA < bottomB || bottomA > topB);
                });

                if (!hasIntersectedHint)
                {
                    hintList.Add(new Hint(dynamicHint, x, y));
                    return;
                }

                if (x + 1 < dynamicHint.RightBoundary)
                    queue.Enqueue(Tuple.Create(x + 1, y));
                if (x - 1 > dynamicHint.LeftBoundary)
                    queue.Enqueue(Tuple.Create(x - 1, y));
                if(y+1 < dynamicHint.BottomBoundary)
                    queue.Enqueue(Tuple.Create(x, y + 1));
                if(y-1 > dynamicHint.TopBoundary)
                    queue.Enqueue(Tuple.Create(x, y - 1));
            }

            if(dynamicHint.Strategy == DynamicHintStrategy.StayInPosition)
                hintList.Add(new Hint(dynamicHint, dynamicHint.TargetX, dynamicHint.TargetY));
        }

        private static string GetText(List<Hint> hintList, PlayerDisplay pd)
        {
            hintList.Sort((x, y) => x.YCoordinate.CompareTo(y.YCoordinate));

            List<string> textList = hintList
                .Select(hint => ToRichText(hint, pd))
                .Where(text => !string.IsNullOrEmpty(text))
                .ToList();

            textList.Insert(0, "<line-height=0><voffset=10000><size=40>PlaceHolder </voffset></size>");
            textList.Add("<line-height=0><voffset=-10000><size=40>PlaceHolder</voffset></size>");

            return string.Join("\n", textList);
        }

        private static string ToRichText(Hint hint, PlayerDisplay playerDisplay)
        {
            StringBuilder sb = new StringBuilder();

            string text = hint.Content.GetText(new AbstractHint.TextUpdateArg(hint, playerDisplay))??string.Empty;

            text = RemoveIllegalTag(text);

            if (string.IsNullOrEmpty(text))
                return null;

            float y = GetActualYCoordinate(hint);

            //Position Tags
            if (hint.XCoordinate != 0) sb.Append($"<pos={hint.XCoordinate:0.#}>");
            if (hint.Alignment != HintAlignment.Center) sb.Append($"<align={hint.Alignment}>");
            sb.Append("<Line-height=0>");
            if (y != 0) sb.Append($"<voffset={y:0.#}>");

            sb.Append($"<size={hint.FontSize}>");

            sb.Append(text);

            sb.Append("</size>");

            //Position Tags
            sb.Append("</voffset>");
            sb.Append("</align>");

            return sb.ToString();
        }

        private static string RemoveIllegalTag(string rawText)
        {
            rawText = Regex.Replace(rawText, @"<line-height=(\d+(\.\d+)?)>|<voffset=(-?\d+(\.\d+)?)>|<pos=(\d+(\.\d+)?)>|<align=(left|center|right)>", string.Empty);

            return rawText
                .Replace("</voffset>", string.Empty)
                .Replace("</align>", string.Empty);
        }

        private static float GetActualYCoordinate(Hint hint)
        {
            return GetActualYCoordinate(hint, hint.YCoordinateAlign);
        }

        private static float GetActualYCoordinate(Hint hint, HintVerticalAlign align)
        {
            float sizeOffset;

            switch (align)
            {
                case HintVerticalAlign.Top:
                    sizeOffset = -hint.FontSize;
                    break;
                case HintVerticalAlign.Middle:
                    sizeOffset = -(hint.FontSize / 2);
                    break;
                case HintVerticalAlign.Bottom:
                    sizeOffset = 0;
                    break;
                default:
                    sizeOffset = 0;
                    break;
            }

            return 700 - hint.YCoordinate + sizeOffset;
        }

        private static float GetActualXCoordinate(Hint hint)
        {
            return GetActualXCoordinate(hint, hint.Alignment);
        }

        private static float GetActualXCoordinate(Hint hint, HintAlignment alignment)
        {
            float alignOffset;

            switch (alignment)
            {
                case HintAlignment.Left:
                    alignOffset = -1200;
                    break;
                case HintAlignment.Right:
                    alignOffset = 1200;
                    break;
                case HintAlignment.Center:
                    alignOffset = 0;
                    break;
                default:
                    alignOffset = 0;
                    break;
            }

            return hint.XCoordinate + alignOffset;
        }
    }
}

//Not used
//private static void InsertDynamicHint(List<DynamicHint> dynamicHintList, List<Hint> hintList)
//{
//    dynamicHintList.Sort((x, y) => -x.Priority.CompareTo(y.Priority));

//    //Turn a hint into a rectangle
//    Rectangle GetRec(Hint hint)
//    {
//        var rec = new Rectangle();

//        rec.LeftX = hint.XCoordinate - FontTool.GetTextWidth(hint) / 2;
//        rec.TopY = hint.YCoordinate - hint.FontSize;
//        rec.Width = FontTool.GetTextWidth(hint);
//        rec.Height = hint.FontSize;

//        if (hint.Alignment == HintAlignment.Left)
//            rec.LeftX -= 1200;
//        else if (hint.Alignment == HintAlignment.Right)
//            rec.LeftX += 1200;

//        return rec;
//    }

//    foreach (var dynamicHint in dynamicHintList)
//    {
//        //Define Usable Area
//        var usableArea = new Rectangle()
//        {
//            LeftX = dynamicHint.LeftBoundary,
//            TopY = dynamicHint.TopBoundary,
//            Width = dynamicHint.RightBoundary - dynamicHint.LeftBoundary,
//            Height = dynamicHint.BottomBoundary - dynamicHint.TopBoundary,
//        };

//        //Initialize scanLine
//        var scanLine = new Rectangle()
//        {
//            LeftX = usableArea.LeftX,
//            Width = usableArea.Width,
//            TopY = 0,
//            Height = 0,
//        };

//        //Found all other texts that interfere with the dynamic hint
//        List<Rectangle> interferingRectangles = hintList
//            .Select(GetRec)
//            .Where(x => x.CheckIntersects(usableArea))
//            .ToList();

//        //Initialize the turn points list. This list store all the turn point of availability
//        List<TurnPoint> turnPoints = new List<TurnPoint>()
//        {
//            new TurnPoint() { TurnTo = true, Y = 0 }
//        };

//        //Find all the turn points
//        foreach (var interferingRec in interferingRectangles)
//        {
//            //Scan top side
//            scanLine.TopY = interferingRec.TopY;

//            List<Rectangle> scannedRec = interferingRectangles
//                .Where(x => x.TopY != scanLine.TopY)
//                .Where(x => x.CheckIntersects(scanLine))
//                .ToList();

//            scannedRec.Sort((x, y) => x.LeftX.CompareTo(y.LeftX));

//            List<Line> availableLines = new List<Line>()
//            {
//                new Line()
//                {
//                    LeftX = scanLine.LeftX,
//                    RightX = scannedRec.Count == 0 ? scanLine.LeftX + scanLine.Width : scannedRec[0].LeftX,
//                }
//            };

//            for (var i = 1; i < scannedRec.Count; i++)
//            {
//                var leftRec = scannedRec[i - 1];
//                var rightRec = scannedRec[i];

//                var distance = rightRec.LeftX - (leftRec.LeftX + leftRec.Width);

//                if (distance > 0)
//                    availableLines.Add(new Line()
//                    {
//                        LeftX = leftRec.LeftX + leftRec.Width,
//                        RightX = rightRec.LeftX,
//                    });
//            }

//            //Add turnpoint
//            if (availableLines.Max(x => x.GetLength()) >= FontTool.GetTextWidth(dynamicHint))
//            {
//                turnPoints.Add(new TurnPoint() { TurnTo = true, Y = interferingRec.TopY });
//            }
//            else
//            {
//                turnPoints.Add(new TurnPoint() { TurnTo = false, Y = interferingRec.TopY });
//            }
//        }

//        for (var i = 1; i < turnPoints.Count; i++)
//        {
//            if (turnPoints[i - 1].TurnTo == turnPoints[i].TurnTo)
//            {
//                turnPoints.RemoveAt(i);
//            }
//        }
//    }
//
//private struct Rectangle
//{
//    public float LeftX { get; set; }
//    public float TopY { get; set; }
//    public float Width { get; set; }
//    public float Height { get; set; }

//    public bool CheckIntersects(Rectangle rec)
//    {
//        return !(rec.LeftX > this.LeftX + this.Width ||
//                 rec.LeftX + rec.Width < this.LeftX ||
//                 rec.TopY > this.TopY + this.Height ||
//                 rec.TopY + rec.Height < this.TopY);
//    }

//    public bool IsContainedWithin(Rectangle rec)
//    {
//        return (this.LeftX >= rec.LeftX &&
//                this.TopY >= rec.TopY &&
//                this.LeftX + this.Width <= rec.LeftX + rec.Width &&
//                this.TopY + this.Height <= rec.TopY + rec.Height);
//    }
//}

//private struct TurnPoint
//{
//    public float Y;
//    public bool TurnTo;

//    public TurnPoint(float y, bool turnTo)
//    {
//        Y = y;
//        TurnTo = turnTo;
//    }
//}

//private struct Line
//{
//    public float LeftX;
//    public float RightX;

//    public float GetLength()
//    {
//        return RightX - LeftX;
//    }
//}


