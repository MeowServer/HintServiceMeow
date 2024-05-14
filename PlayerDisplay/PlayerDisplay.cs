using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HintServiceMeow
{
    public class PlayerDisplay
    {
        //List of all the PlayerDisplay instances
        private static List<PlayerDisplay> playerDisplayList = new List<PlayerDisplay>();

        private const int placeHolderMaximumHeightPX = 920;//The space that the place holder must fill 1160

        //The player this player display is refering to
        public Player player;

        //Can be optimize by sorting hints when adding them
        private List<Hint> hintList = new List<Hint>();//List of hints shows to the player
        private List<DynamicHint> dynamicHintList = new List<DynamicHint>();//List of dynamic hints shows to the player

        private static CoroutineHandle HintRefreshCoroutine;
        private static IEnumerator<float> RefreshHintEveryFiveSeconds()
        {
            while(true)
            {
                try
                {
                    foreach (PlayerDisplay pd in playerDisplayList)
                    {
                        if ((Round.ElapsedTime - pd.lastTimeUpdate).TotalSeconds >= 5)
                        {
                            pd.UpdateWhenReady();
                        }
                    }
                }
                catch(Exception e)
                {
                    Log.Error(e);
                }
                

                yield return Timing.WaitForSeconds(5f);
            }
        }

        //Update Rate Management stuff
        private float UpdateInterval { get; } = 0.5f;   //By experiment, the fastest update rate is 2 times per second.
        internal TimeSpan lastTimeUpdate { get; set; } = TimeSpan.Zero;
        private bool plannedUpdate { get; set; } = false;   // Tells whether a update had been planned

        //Update Methods for hint change events to call
        internal void UpdateWhenReady()
        {
            if (plannedUpdate)
                return;//return if a update had been planned.

            plannedUpdate = true;

            var TimeToWait = (float)(UpdateInterval - (Round.ElapsedTime - lastTimeUpdate).TotalSeconds);
            TimeToWait = TimeToWait < 0.05f ? 0.05f : TimeToWait; //0.05f to make sure that all of the changes are updated beofre the next update

            Timing.CallDelayed(TimeToWait, () =>
            {
                UpdateHint();
                plannedUpdate = false;
            });
        }

        //Private Update Methods
        private void UpdateHint()
        {
            var displayHintList = GetRegularDisplayHints();
            displayHintList = InsertDynamicHints(displayHintList);
            string text = ToMessage(displayHintList);

            //reset CountDown
            lastTimeUpdate = Round.ElapsedTime;

            player.ShowHint(text, 20);
        }

        private void GetPlaceHolder(int size, List<string> messages)
        {
            /*
            for (; size > 40; size -= 40)
            {
                messages.Add("<size=40>　</size>");//█
            }*/

            messages.Add($"<size={size}>　</size>");
        }

        private List<Hint> InsertDynamicHints(List<Hint> hintList)
        {
            var NewHintList = new List<Hint>(hintList);

            //Arrange Dynamic Hints
            dynamicHintList.Sort(delegate (DynamicHint a, DynamicHint b)
            {
                return a.hintField.topYCoordinate.CompareTo(b.hintField.topYCoordinate);
            });

            //Insert Dynamic Hints into the display hint list by transforming them into regular hints
            var tempHintA = new Hint(0, HintAlignment.Center, "TempFirst").setFontSize(0);
            var tempHintB = new Hint(0, HintAlignment.Center, "TempLast").setFontSize(0);

            foreach (DynamicHint dynamicHint in dynamicHintList)
            {
                //skip hided hints
                if (dynamicHint.hide == true) continue;
                if (dynamicHint.message == string.Empty && dynamicHint.message == null) continue;

                var topYCoordinate = dynamicHint.hintField.topYCoordinate;
                var bottomYCoordinate = dynamicHint.hintField.bottomYCoordinate;

                //Top Position Flags
                tempHintA.bottomYCoordinate = topYCoordinate;
                if (NewHintList.FindIndex(x => x.bottomYCoordinate >= topYCoordinate) == -1)
                {
                    NewHintList.Insert(0, tempHintA);
                }
                else
                {
                    NewHintList.Insert(NewHintList.FindIndex(x => x.bottomYCoordinate >= topYCoordinate), tempHintA);
                }

                //Bottom Position Flags
                tempHintB.topYCoordinate = bottomYCoordinate;
                if (NewHintList.FindIndex(x => x.topYCoordinate >= bottomYCoordinate) == -1)
                {
                    NewHintList.Add(tempHintB);
                }
                else
                {
                    NewHintList.Insert(NewHintList.FindIndex(x => x.topYCoordinate >= bottomYCoordinate), tempHintB);
                }

                for (var index = NewHintList.IndexOf(tempHintA); index < NewHintList.IndexOf(tempHintB); index++)
                {
                    int space = NewHintList[index + 1].topYCoordinate - NewHintList[index].bottomYCoordinate;

                    if (space >= dynamicHint.fontSize)
                    {
                        var hint = new Hint(dynamicHint, NewHintList[index].bottomYCoordinate);
                        NewHintList.Insert(index + 1, hint);

                        break;
                    }
                }

                NewHintList.Remove(tempHintA);
                NewHintList.Remove(tempHintB);//Remove flags
            }

            return NewHintList;
        }

        private List<Hint> GetRegularDisplayHints()
        {
            List<Hint> displayHintList =
                hintList
                .Where(x => x.hide == false && x.message != string.Empty && x.message != null)
                .ToList();

            //Arrange Hints
            displayHintList.Sort(delegate (Hint a, Hint b)
            {
                return a.topYCoordinate.CompareTo(b.topYCoordinate);
            });

            //Detect overlap hints
            for (var index = 0; index < displayHintList.Count - 1; index++)
            {
                if (displayHintList[index].bottomYCoordinate > displayHintList[index + 1].topYCoordinate)
                {
                    Log.Warn("Two Hints are overlapping each others");
                    Log.Warn("First Hint: " + displayHintList[index].ToString());
                    Log.Warn("Second Hint: " + displayHintList[index + 1].ToString());

                    displayHintList.RemoveAt(index + 1);
                }
            }

            return displayHintList;
        }

        private string ToMessage(List<Hint> displayHintList)
        {
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
            placeHolderHeight = placeHolderMaximumHeightPX - displayHintList.Last().bottomYCoordinate;
            GetPlaceHolder(placeHolderHeight, messages);

            string message = string.Join("\n", messages);

            return message;
        }

        //Constructor/Destructors
        internal PlayerDisplay(Player player)
        {
            this.player = player;

            if (HintRefreshCoroutine == null|| !HintRefreshCoroutine.IsRunning)
            {
                HintRefreshCoroutine = Timing.RunCoroutine(RefreshHintEveryFiveSeconds());
            }

            playerDisplayList.Add(this);
        }

        internal static void RemovePlayerDisplay(Player player)
        {
            playerDisplayList.RemoveAll(x => x.player == player);
        }

        internal static void ClearPlayerDisplay()
        {
            playerDisplayList.Clear();
        }

        //Player Display Methods
        public static PlayerDisplay Get(Player player)
        {
            foreach (PlayerDisplay playerDisplay in playerDisplayList)
            {
                if (playerDisplay.player.Id == player.Id)
                {
                    return playerDisplay;
                }
            }

            return null;
        }

        //Regular Hint Methods
        public void AddHint(AbstractHint hint)
        {
            if (hint == null)
            {
                throw new NullReferenceException();
            }

            if (hintList.Contains(hint))
                return;

            UpdateWhenReady();
            hint.HintUpdated += UpdateWhenReady;

            if(hint is Hint h)
            {
                hintList.Add(h);
            }else if(hint is DynamicHint dh)
            {
                dynamicHintList.Add(dh);
            }
            
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
            {
                throw new NullReferenceException();
            }

            UpdateWhenReady();
            hint.HintUpdated -= UpdateWhenReady;

            if (hint is Hint h)
            {
                hintList.Remove(h);
            }
            else if (hint is DynamicHint dh)
            {
                dynamicHintList.Remove(dh);
            }
        }

        public void RemoveHint(string id)
        {
            if (id == null)
            {
                throw new Exception("A null name had been passed to RemoveHint");
            
            }

            foreach(AbstractHint hint in hintList)
            {
                RemoveHint(hint);
            }

            foreach(AbstractHint hint in dynamicHintList)
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
            if (id == null)
            {
                throw new Exception("A null name had been passed to FindHint");
            }

            if (hintList.Any(x => x.id == id))
            {
                return hintList.Find(x => x.id == id);
            }
            else if (dynamicHintList.Any(x => x.id == id))
            {
                return dynamicHintList.Find(x => x.id == id);
            }

            return null;
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
    }
}
