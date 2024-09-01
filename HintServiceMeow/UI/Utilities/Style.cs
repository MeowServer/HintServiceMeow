using HintServiceMeow.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using HintServiceMeow.Core.Models.Hints;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Linq;

namespace HintServiceMeow.UI.Utilities
{
    public class Style
    {
        private Dictionary<string, List<Hint>> _currentStyleHints = new Dictionary<string, List<Hint>>();

        private const string StyleHintSuffix = "BoldGroup";

        private static readonly Hint BoldStartHint = new Hint()
        {
            Text = "<b>",
            FontSize = 0,
            YCoordinateAlign = Core.Enum.HintVerticalAlign.Bottom
        };

        private static readonly Hint BoldEndHint = new Hint()
        {
            Text = "</b>",
            FontSize = 0,
            YCoordinateAlign = Core.Enum.HintVerticalAlign.Bottom
        };

        private static readonly Hint ItalicStartHint = new Hint()
        {
            Text = "<i>",
            FontSize = 0,
            YCoordinateAlign = Core.Enum.HintVerticalAlign.Bottom
        };

        private static readonly Hint ItalicEndHint = new Hint()
        {
            Text = "</i>",
            FontSize = 0,
            YCoordinateAlign = Core.Enum.HintVerticalAlign.Bottom
        };

        private ReferenceHub ReferenceHub { get; set; }
        private PlayerDisplay PlayerDisplay => PlayerDisplay.Get(ReferenceHub);

        private readonly Dictionary<string, List<Area>> _boldArea = new Dictionary<string, List<Area>>();
        private readonly Dictionary<string, List<Area>> _italicArea = new Dictionary<string, List<Area>>();

        private readonly Dictionary<string, List<ColorArea>> _colorArea = new Dictionary<string, List<ColorArea>>();

        [Flags]
        public enum StyleType
        {
            None = 0b00,
            Bold = 0b01,
            Italic = 0b10,
            BoldItalic = 0b11
        }

        internal Style(ReferenceHub referenceHub)
        {
            ReferenceHub = referenceHub;
        }

        public void SetStyle(float topY, float bottomY, StyleType style)
        {
            var name = Assembly.GetCallingAssembly().FullName;

            TryInitialize(name);

            var area = new Area()
            {
                TopY = topY,
                BottomY = bottomY
            };

            if((style & StyleType.Bold) != 0)
            {
                AddArea(area, _boldArea[name]);
            }
            else
            {
                RemoveArea(area, _boldArea[name]);
            }

            if((style & StyleType.Italic) != 0)
            {
                AddArea(area, _italicArea[name]);
            }
            else
            {
                RemoveArea(area, _italicArea[name]);
            }

            UpdateHint(name);
        }

        public void SetColor(float topY, float bottomY, Color color)
        {
            var name = Assembly.GetCallingAssembly().FullName;

            TryInitialize(name);

            var area = new ColorArea()
            {
                TopY = topY,
                BottomY = bottomY,
                Color = color
            };

            RemoveArea(area, _colorArea[name].Select(x => (Area)x).ToList());
            AddArea(area, _colorArea[name].Select(x => (Area)x).ToList());

            UpdateHint(name);
        }

        private void TryInitialize(string name)
        {
            if(!_boldArea.ContainsKey(name))
            {
                _boldArea.Add(name, new List<Area>());
            }

            if(!_italicArea.ContainsKey(name))
            {
                _italicArea.Add(name, new List<Area>());
            }

            if(!_colorArea.ContainsKey(name))
            {
                _colorArea.Add(name, new List<ColorArea>());
            }

            if(!_currentStyleHints.ContainsKey(name))
            {
                _currentStyleHints.Add(name, new List<Hint>());
            }
        }

        private void AddArea(Area areaToAdd, List<Area> existingArea)
        {
            existingArea.Add(areaToAdd);
            existingArea.Sort((x, y) => x.TopY.CompareTo(y.TopY));

            for(int i = 0; i < existingArea.Count - 1; i++)
            {
                if(IsIntersected(existingArea[i], existingArea[i + 1]))
                {
                    existingArea[i].BottomY = Math.Max(existingArea[i].BottomY, existingArea[i + 1].BottomY);
                    existingArea.RemoveAt(i + 1);
                    i--;
                }
            }

        }

        private void RemoveArea(Area areaToRemove, List<Area> existingArea)
        {
            foreach(Area area in existingArea)
            {
                if(IsIntersected(area, areaToRemove))
                {
                    if(area.TopY > areaToRemove.TopY)
                    {
                        area.TopY = areaToRemove.BottomY;
                    }

                    if(area.BottomY < areaToRemove.BottomY)
                    {
                        area.BottomY = areaToRemove.TopY;
                    }
                }
            }

            existingArea.RemoveAll(area => area.TopY >= area.BottomY || area.BottomY - area.TopY <= 0.1);
        }

        private void UpdateHint(string assemblyName)
        {
            foreach(var hint in _currentStyleHints[assemblyName])
            {
                PlayerDisplay.InternalRemoveHint(assemblyName, hint);
            }

            foreach(var area in _boldArea[assemblyName])
            {
                var startHint = new Hint(BoldStartHint)
                {
                    YCoordinate = area.TopY - 0.01f
                };
                var stopHint = new Hint(BoldEndHint)
                {
                    YCoordinate = area.BottomY + 0.01f
                };

                PlayerDisplay.InternalAddHint(assemblyName, startHint);
                PlayerDisplay.InternalAddHint(assemblyName, stopHint);

                _currentStyleHints[assemblyName].Add(startHint);
                _currentStyleHints[assemblyName].Add(stopHint);
            }

            foreach(var area in _italicArea[assemblyName])
            {
                var startHint = new Hint(ItalicStartHint)
                {
                    YCoordinate = area.TopY - 0.01f
                };
                var stopHint = new Hint(ItalicEndHint)
                {
                    YCoordinate = area.BottomY + 0.01f
                };

                PlayerDisplay.InternalAddHint(assemblyName, startHint);
                PlayerDisplay.InternalAddHint(assemblyName, stopHint);

                _currentStyleHints[assemblyName].Add(startHint);
                _currentStyleHints[assemblyName].Add(stopHint);
            }

            foreach(var area in _colorArea[assemblyName])
            {
                var startHint = GetColorStartHint(area.Color);
                var stopHint = GetColorEndHint();

                startHint.YCoordinate = area.TopY - 0.01f;
                stopHint.YCoordinate = area.BottomY + 0.01f;

                PlayerDisplay.InternalAddHint(assemblyName, startHint);
                PlayerDisplay.InternalAddHint(assemblyName, stopHint);

                _currentStyleHints[assemblyName].Add(startHint);
                _currentStyleHints[assemblyName].Add(stopHint);
            }
        }

        private static Hint GetColorStartHint(Color color)
        {
            return new Hint()
            {
                Text = $"<color=#{color.ToHex()}>",
                FontSize = 0,
                YCoordinateAlign = Core.Enum.HintVerticalAlign.Bottom
            };
        }

        private static Hint GetColorEndHint()
        {
            return new Hint()
            {
                Text = "</color>",
                FontSize = 0,
                YCoordinateAlign = Core.Enum.HintVerticalAlign.Bottom
            };
        }

        private bool IsIntersected(Area area1, Area area2)
        {
            return area1.TopY <= area2.BottomY && area1.BottomY >= area2.TopY;
        }

        private class Area
        {
            public float TopY { get; set; }
            public float BottomY { get; set; }
        }

        private class ColorArea : Area
        {
            public Color Color { get; set; }
        }
    }
}