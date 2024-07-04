using Exiled.API.Features;
using Hints;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using HintServiceMeow.Config;

namespace HintServiceMeow
{
    public class PlayerDisplay
    {
        //List of all the PlayerDisplay instances
        private static readonly List<PlayerDisplay> PlayerDisplayList = new List<PlayerDisplay>();

        private const int PlaceHolderMaximumHeightPx = 920;//1120;

        //Public const
        public const int DisplayableMinHeight = 0;
        public const int DisplayableMaxHeight = 720;

        private static PlayerDisplayConfig config => PluginConfig.Instance.PlayerDisplayConfig;

        //The player this player display binds to
        public Player player;

        private readonly List<Hint> _hintList = new List<Hint>();//List of hints shows to the player
        private readonly List<DynamicHint> _dynamicHintList = new List<DynamicHint>();//List of dynamic hints shows to the player

        //Patch
        private bool _allowNextUpdate = false;
        internal bool AllowPatchUpdate
        {
            get
            {
                var allow = _allowNextUpdate;
                _allowNextUpdate = false;
                return allow;
            }
        }

        /// <summary>
        /// Called periodically when player display is ready to update. Default period is 0.1s (10 times per second)
        /// </summary>
        public event UpdateAvailableEventHandler UpdateAvailable;
        public delegate void UpdateAvailableEventHandler(PlayerDisplay playerDisplay);
        private static CoroutineHandle _hintRefreshCoroutine;

        //Update Rate Management
        private static TimeSpan UpdateInterval => config.MinUpdateInterval;
        private bool UpdateReady => (DateTime.Now - _lastTimeUpdate) >= UpdateInterval;
        private DateTime _lastTimeUpdate = DateTime.MinValue;
        private bool _plannedUpdate = false;   // Tells whether an update had been planned

        #region Update Management Methods
        internal void UpdateWhenReady()
        {
            if (_plannedUpdate)
                return;//return if an update had already been planned.

            _plannedUpdate = true;

            var timeToWait = (float)((_lastTimeUpdate + UpdateInterval) - DateTime.Now).TotalMilliseconds;
            var minDelay = config.MinTimeDelayBeforeUpdate;
            timeToWait = Math.Max(timeToWait, minDelay); //Having a min delay to make sure that all the changes are updated before next update

            Timing.CallDelayed(timeToWait/1000, () =>
            {
                try
                {
                    UpdateHint();
                    _plannedUpdate = false;
                }
                catch(Exception ex)
                {
                   Log.Error(ex);
                   _plannedUpdate = false;
                }
            });
        }

        //Coroutine Method
        private static IEnumerator<float> RefreshCoroutineMethod()
        {
            while (true)
            {
                try
                {
                    if (PlayerDisplayList.Count <= 0)
                    {
                        yield break;
                    }

                    foreach (PlayerDisplay pd in PlayerDisplayList)
                    {
                        if ((DateTime.Now - pd._lastTimeUpdate).TotalSeconds >= config.ForceUpdateInterval)
                            pd.UpdateWhenReady();

                        if (pd.UpdateReady)
                            pd.UpdateAvailable?.Invoke(pd);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

                yield return Timing.WaitForSeconds(config.HintUpdateInterval);
            }
        }
        #endregion

        # region Private Update Methods
        private void UpdateHint()
        {
            string text = ToMessage(GetDisplayableHints());

            //reset CountDown
            _lastTimeUpdate = DateTime.Now;

            //Allow patch
            _allowNextUpdate = true;

            //Display the hint
            player.HintDisplay.Show(
                new TextHint(
                    text,
                    new HintParameter[]
                    {
                        new StringHintParameter(text)
                    },
                    new HintEffect[]
                    {
                        HintEffectPresets.TrailingPulseAlpha(1, 1, 1)
                    },
                    float.MaxValue
                )
            );
        }

        private static void GetPlaceHolder(int size, List<string> messages)
        {
            for (; size > 40; size -= 40)
            {
                messages.Add("<size=40>　</size>");//█
            }

            messages.Add($"<size={size}>　</size>");
        }

        private void InsertDynamicHints(List<Hint> hintList)
        {
            if (_dynamicHintList.Count == 0)
                return;

            //Arrange Dynamic Hints
            _dynamicHintList.Sort((a, b) =>
            {
                int result = a.priority.CompareTo(b.priority);
                
                if(result == 0)
                    result = a.hintField.topYCoordinate.CompareTo(b.hintField.topYCoordinate);

                return result;
            });

            List<DynamicHint> displayableDynamicHints = this._dynamicHintList
                .FindAll(x => x.hide == false && !string.IsNullOrEmpty(x.message));

            //Flags used to indicate the max and min y value of the dynamic hint
            Hint tempHintA = new Hint()
            {
                fontSize = 0,
            };

            Hint tempHintB = new Hint()
            {
                fontSize = 0,
            };
            
            foreach (DynamicHint dynamicHint in displayableDynamicHints)
            {
                var topYCoordinate = dynamicHint.hintField.topYCoordinate;
                var bottomYCoordinate = dynamicHint.hintField.bottomYCoordinate;

                //Insert Top Position Flags
                tempHintA.bottomYCoordinate = topYCoordinate;
                var index1 = hintList.FindIndex(x => x.bottomYCoordinate >= topYCoordinate);
                if (index1 == -1)
                {
                    hintList.Insert(0, tempHintA);
                }
                else
                {
                    hintList.Insert(index1, tempHintA);
                }

                //Insert Bottom Position Flags
                tempHintB.topYCoordinate = bottomYCoordinate;
                var index2 = hintList.FindIndex(x => x.topYCoordinate >= bottomYCoordinate);
                if (index2 == -1)
                {
                    hintList.Add(tempHintB);
                }
                else
                {
                    
                    hintList.Insert(index2, tempHintB);
                }

                for (var index = hintList.IndexOf(tempHintA); index < hintList.IndexOf(tempHintB); index++)
                {
                    int space = hintList[index + 1].topYCoordinate - hintList[index].bottomYCoordinate;

                    if (space < dynamicHint.fontSize)
                        continue;

                    //Insert Dynamic Hints into the display hint list by transforming them into regular hints
                    var hint = new Hint(dynamicHint, hintList[index].bottomYCoordinate);
                    hintList.Insert(index + 1, hint);

                    break;
                }

                //Remove flags
                hintList.Remove(tempHintA);
                hintList.Remove(tempHintB);
            }
        }

        private List<Hint> GetDisplayableHints()
        {
            List<Hint> displayHintList = new List<Hint>();

            if (_hintList.Count != 0)
            {
                displayHintList = _hintList
                    .FindAll(x => x.hide == false && !string.IsNullOrEmpty(x.message));

                //Arrange Hints
                displayHintList.Sort((a, b) => a.topYCoordinate.CompareTo(b.topYCoordinate));

                //Remove low priority hints that are overlapped by high priority hints
                for (var index = 0; index < displayHintList.Count - 1; index++)
                {
                    if (displayHintList[index].bottomYCoordinate <= displayHintList[index + 1].topYCoordinate)
                        continue;

                    if (displayHintList[index].priority > displayHintList[index + 1].priority)
                        displayHintList.RemoveAt(index + 1);
                    else
                        displayHintList.RemoveAt(index);
                }
            }

            InsertDynamicHints(displayHintList);

            return displayHintList;
        }

        //public static int count = 28; // Dev only
        private string ToMessage(List<Hint> displayHintList)
        {
            if (displayHintList.Count == 0)
                return string.Empty;

            List<string> messages = new List<string>();

            int placeHolderHeight = displayHintList.First().topYCoordinate;//+ 5;//+10 to make sure the hint on y=0 can be displayed
            GetPlaceHolder(placeHolderHeight, messages);

            //middle hints
            for (int i = 0; i < displayHintList.Count - 1; i++)
            {
                messages.Add(displayHintList[i].GetText());

                //place the placeholder, 
                placeHolderHeight = displayHintList[i + 1].topYCoordinate - displayHintList[i].bottomYCoordinate;
                GetPlaceHolder(placeHolderHeight, messages);
            }

            //last hint
            messages.Add(displayHintList.Last().GetText());
            placeHolderHeight = PlaceHolderMaximumHeightPx - displayHintList.Last().bottomYCoordinate;
            GetPlaceHolder(placeHolderHeight, messages);

            string message = string.Join("\n", messages);

            message = RemoveIllegalTags(message);

            return message;
        }

        private string RemoveIllegalTags(string rawText)
        {
            return rawText
                .Replace("{", string.Empty)
                .Replace("}", string.Empty);
        }
        #endregion

        #region Constructor and Destructors
        internal PlayerDisplay(Player player)
        {
            this.player = player;

            if (!_hintRefreshCoroutine.IsRunning)
                _hintRefreshCoroutine = Timing.RunCoroutine(RefreshCoroutineMethod());

            PlayerDisplayList.Add(this);
        }

        internal static void RemovePlayerDisplay(Player player)
        {
            PlayerDisplayList.RemoveAll(x => x.player == player);
        }

        internal static void ClearPlayerDisplay()
        {
            PlayerDisplayList.Clear();
        }
        #endregion

        #region Player Display Methods
        public static PlayerDisplay Get(Player player) => PlayerDisplayList.Find(x => x.player == player);
        #endregion

        #region Regular Hint Methods
        public void AddHint(AbstractHint hint)
        {
            if (hint == null)
            {
                throw new NullReferenceException();
            }

            if (_hintList.Contains(hint))
                return;
            
            hint.HintUpdated += UpdateWhenReady;

            if(hint is Hint h)
            {
                _hintList.Add(h);
            }
            else if(hint is DynamicHint dh)
            {
                _dynamicHintList.Add(dh);
            }

            UpdateWhenReady();
        }

        public void AddHints(IEnumerable<AbstractHint> hints)
        {
            foreach(AbstractHint hint in hints)
            {
                AddHint(hint);
            }
        }

        public void RemoveHint(AbstractHint hint)
        {
            if (hint == null)
                throw new NullReferenceException();

            hint.HintUpdated -= UpdateWhenReady;

            if (hint is Hint h)
            {
                _hintList.Remove(h);
            }
            else if (hint is DynamicHint dh)
            {
                _dynamicHintList.Remove(dh);
            }

            UpdateWhenReady();
        }

        public void RemoveHint(string id)
        {
            if (id == null)
            {
                throw new Exception("A null id had been passed to RemoveHint");
            
            }

            foreach(AbstractHint hint in _hintList.Where(x => x.id == id))
            {
                RemoveHint(hint);
            }

            foreach(AbstractHint hint in _dynamicHintList.Where(x => x.id == id))
            {
                RemoveHint(hint);
            }
        }

        public void RemoveHints(IEnumerable<AbstractHint> hints)
        {
            foreach(AbstractHint hint in hints)
            {
                RemoveHint(hint);
            }
        }

        public AbstractHint FindHint(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new Exception("A null name had been passed to FindHint");
            }

            var hint = _hintList.Find(x => x.id == id);
            var dynamicHint = _dynamicHintList.Find(x => x.id == id);
            return hint??(AbstractHint)dynamicHint;
        }

        public void RemoveHintAfter(Hint hint, float time)
        {
            Timing.CallDelayed(time, () =>
            {
                RemoveHint(hint);
            });
        }

        public void RemoveHintAfter(DynamicHint hint, float time)
        {
            Timing.CallDelayed(time, () =>
            {
                RemoveHint(hint);
            });
        }

        public void HideHintAfter(Hint hint, float time)
        {
            Timing.CallDelayed(time, () =>
            {
                hint.hide = true;
            });
        }

        public void HideHintAfter(DynamicHint hint, float time)
        {
            Timing.CallDelayed(time, () =>
            {
                hint.hide = true;
            });
        }
        #endregion
    }
}
