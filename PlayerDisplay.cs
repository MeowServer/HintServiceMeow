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

        public Player player;//The player this instance is refering to

        public string currentDisplayStr = string.Empty;

        //Can be optimize by sorting hints while adding them
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

        //Update Interval
        internal TimeSpan lastTimeUpdate { get; set; } = TimeSpan.Zero;
        private bool plannedUpdate = false; // Tells whether a update had been planned

        public void UpdateWhenReady()
        {
            if (plannedUpdate)
                return;//return if a update had been planned.

            float UpdateInterval = 0.5f;

            if ((Round.ElapsedTime - lastTimeUpdate).TotalSeconds >= UpdateInterval)
            {
                plannedUpdate = true;
                Timing.CallDelayed(0.05f, () =>
                {
                    UpdateHint();
                    plannedUpdate = false;
                });//Waiting for other hints to update before update the hint
            }
            else
            {
                plannedUpdate = true;
                var TimeToWait = (float)(UpdateInterval - (Round.ElapsedTime - lastTimeUpdate).TotalSeconds);
                Timing.CallDelayed(TimeToWait, () =>
                {
                    UpdateHint();
                    plannedUpdate = false;
                });
            }
        }

        public PlayerDisplay(Player player)
        {
            this.player = player;

            if (HintRefreshCoroutine == null|| !HintRefreshCoroutine.IsRunning)
            {
                HintRefreshCoroutine = Timing.RunCoroutine(RefreshHintEveryFiveSeconds());
            }

            playerDisplayList.Add(this);
        }

        //Player Display Functions
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

        internal static void RemovePlayerDisplay(Player player)
        {
            playerDisplayList.RemoveAll(x => x.player == player);
        }

        internal static void ClearPlayerDisplay()
        {
            playerDisplayList.Clear();
        }

        //Regular Hint Functions
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

            UpdateWhenReady();
            dynamicHintList.Find(x => x.id == id).HintUpdated -= UpdateWhenReady;

            if (hintList.Any(x => x.id == id))
            {
                hintList.Remove(hintList.Find(x => x.id == id));
            }
            else if (dynamicHintList.Any(x => x.id == id))
            {
                dynamicHintList.Remove(dynamicHintList.Find(x => x.id == id));
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

        private void UpdateHint()
        {

            List<Hint> displayHintList = new List<Hint>(hintList);

            //Arrange Hints
            displayHintList.Sort(delegate (Hint a, Hint b)
            {
                return a.topYCoordinate.CompareTo(b.topYCoordinate);
            });

            //remove hided hints
            displayHintList.RemoveAll(x => x.hide);
            displayHintList.RemoveAll(x => x.message == string.Empty || x.message == null);
            if (displayHintList.Count <= 0)
            {
                return;
            }

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

            //Sort Dynamic Hints
            dynamicHintList.Sort(delegate (DynamicHint a, DynamicHint b)
            {
                return a.hintField.topYCoordinate.CompareTo(b.hintField.topYCoordinate);
            });

            //Insert Dynamic Hints by transforming them into static hints
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
                if(displayHintList.FindIndex(x => x.bottomYCoordinate >= topYCoordinate) == -1)
                {
                    displayHintList.Insert(0, tempHintA);
                }
                else
                {
                    displayHintList.Insert(displayHintList.FindIndex(x => x.bottomYCoordinate >= topYCoordinate), tempHintA);
                }

                //Bottom Position Flags
                tempHintB.topYCoordinate = bottomYCoordinate;
                if (displayHintList.FindIndex(x => x.topYCoordinate >= bottomYCoordinate) == -1)
                {
                    displayHintList.Add(tempHintB);
                }
                else
                {
                    displayHintList.Insert(displayHintList.FindIndex(x => x.topYCoordinate >= bottomYCoordinate), tempHintB);
                }

                for (var index = displayHintList.IndexOf(tempHintA); index < displayHintList.IndexOf(tempHintB); index++)
                {
                    int space = displayHintList[index + 1].topYCoordinate - displayHintList[index].bottomYCoordinate;

                    if (space >= dynamicHint.fontSize)
                    {
                        var hint = new Hint(dynamicHint, displayHintList[index].bottomYCoordinate);
                        displayHintList.Insert(index + 1, hint);

                        break;
                    }
                }

                displayHintList.Remove(tempHintA);
                displayHintList.Remove(tempHintB);//Remove flags
            }

            

            List<string> message = new List<string>();

            int placeHolderHeight = displayHintList.First().topYCoordinate;//+ 5;//+10 to make sure the hint on y=0 can be displayed
            GetPlaceHolder(placeHolderHeight, message);

            //middle hints
            for (int i = 0; i < displayHintList.Count - 1; i++)
            {
                message.Add(displayHintList[i].GetText());

                //place the placeholder, 
                placeHolderHeight = displayHintList[i + 1].topYCoordinate - displayHintList[i].bottomYCoordinate;
                GetPlaceHolder(placeHolderHeight, message);
            }

            //last hint
            message.Add(displayHintList.Last().GetText());
            placeHolderHeight = placeHolderMaximumHeightPX - displayHintList.Last().bottomYCoordinate;
            GetPlaceHolder(placeHolderHeight, message);

            string text = string.Join("\n", message);

            //reset CountDown
            lastTimeUpdate = Round.ElapsedTime;
            currentDisplayStr = text;

            player.ShowHint(text, 9999);
        }

        private void GetPlaceHolder(int size, List<string> messages)
        {

            for (; size > 40; size -= 40)
            {
                messages.Add("<size=40>　</size>");//█
            }

            messages.Add($"<size={size}>　</size>");
        }
    }
}
