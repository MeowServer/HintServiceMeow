using System;
using System.Collections.Generic;
using System.Linq;
using MEC;

using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.UI.Utilities;
using PluginAPI.Core;

namespace HintServiceMeow.UI.Models
{
    public class CommonHint
    {
        private static PlayerUIConfig Config => PluginConfig.Instance.PlayerUIConfig;

        public readonly ReferenceHub ReferenceHub;
        public PlayerUI PlayerUI => PlayerUI.Get(ReferenceHub);
        public PlayerDisplay PlayerDisplay => PlayerDisplay.Get(ReferenceHub);
            
        #region Common Hints
        private CoroutineHandle _commonHintUpdateCoroutine;

        private DateTime _itemHintTimeToRemove = DateTime.MinValue;
        private readonly List<Hint> _itemHints = new List<Hint>
        {
            new Hint()
            {
                FontSize = 25
            },
            new Hint()
            {
                YCoordinate = 725,
                FontSize = 25
            }
        };

        private DateTime _mapHintTimeToRemove = DateTime.MinValue;
        private readonly List<Hint> _mapHints = new List<Hint>{
            new Hint()
            {
                YCoordinate = 200,
                FontSize = 25
            },
            new Hint()
            {
                YCoordinate = 225,
                FontSize = 25
            }
        };

        private DateTime _roleHintTimeToRemove = DateTime.MinValue;
        private readonly List<Hint> _roleHints = new List<Hint>{
            new Hint()
            {
                YCoordinate = 100,
                FontSize = 30
            },
            new Hint()
            {
                YCoordinate = 130,
                FontSize = 25
            },
            new Hint()
            {
                YCoordinate = 155,
                FontSize = 25
            },
            new Hint()
            {
                YCoordinate = 180,
                FontSize = 25
            }
        };

        private DateTime _otherHintTimeToRemove = DateTime.MinValue;
        private readonly List<Hint> _otherHints = new List<Hint>
        {
            new Hint()
            {
                YCoordinate = 700,
                FontSize = 20
            },
            new Hint()
            {
                YCoordinate = 720,
                FontSize = 20
            },
            new Hint()
            {
                YCoordinate = 740,
                FontSize = 20
            },
            new Hint()
            {
                YCoordinate = 760,
                FontSize = 20
            }
        };
        #endregion

        #region Common Hint Methods

        #region Common Item Hints Methods
        public void ShowItemHint(string itemName) => ShowItemHint(itemName, Config.ShortItemHintDisplayTime);

        public void ShowItemHint(string itemName, float time) => ShowItemHint(itemName, new string[]{},time);

        public void ShowItemHint(string itemName, string description) => ShowItemHint(itemName, new string[] { description }, Config.ItemHintDisplayTime);

        public void ShowItemHint(string itemName, string description, float time) => ShowItemHint(itemName, new string[] { description }, time);

        public void ShowItemHint(string itemName, string[] description) => ShowItemHint(itemName, description, Config.ItemHintDisplayTime);

        public void ShowItemHint(string itemName, string[] description, float time)
        {
            _itemHintTimeToRemove = DateTime.Now + TimeSpan.FromSeconds(time);

            _itemHints[0].Text = itemName;
            _itemHints[0].Hide = false;

            for (int i = 1; i < _itemHints.Count; i++)
            {
                if (!description.TryGet(i - 1, out string element))
                    break;

                _itemHints[i].Text = element;
                _itemHints[i].Hide = false;
            }
        }
        #endregion Common Item Hints Methods

        # region Common Map Hints Methods
        public void ShowMapHint(string roomName) => ShowMapHint(roomName, Config.ShortMapHintDisplayTime);

        public void ShowMapHint(string roomName, float time) => ShowMapHint(roomName, new string[]{}, time);

        public void ShowMapHint(string roomName, string description) => ShowMapHint(roomName, new string[1] { description }, Config.ItemHintDisplayTime);

        public void ShowMapHint(string roomName, string description, float time) => ShowMapHint(roomName, new string[1] { description }, time);

        public void ShowMapHint(string roomName, string[] description) => ShowMapHint(roomName, description, Config.MapHintDisplayTime);

        public void ShowMapHint(string roomName, string[] description, float time)
        {
            _mapHintTimeToRemove = DateTime.Now + TimeSpan.FromSeconds(time);

            _mapHints.ForEach(x => x.Hide = true);

            _mapHints[0].Text = roomName;
            _mapHints[0].Hide = false;

            for (int i = 1; i < _mapHints.Count; i++)
            {
                if (!description.TryGet(i - 1, out string element))
                    break;

                _mapHints[i].Text = element;
                _mapHints[i].Hide = false;
            }
        }
        #endregion Common Map Hints Methods

        # region Common Role Hints Methods
        public void ShowRoleHint(string roleName) => ShowRoleHint(roleName, Config.ShortRoleHintDisplayTime);

        public void ShowRoleHint(string roleName, float time) => ShowRoleHint(roleName, new string[]{}, time);

        public void ShowRoleHint(string roleName, string description) => ShowRoleHint(roleName, new string[] { description }, Config.ItemHintDisplayTime);

        public void ShowRoleHint(string roleName, string description, float time) => ShowRoleHint(roleName, new string[] { description }, time);

        public void ShowRoleHint(string roleName, string[] description)=> ShowRoleHint(roleName, description, Config.RoleHintDisplayTime);

        public void ShowRoleHint(string roleName, string[] description, float time)
        {
            _roleHintTimeToRemove = DateTime.Now + TimeSpan.FromSeconds(time);

            _roleHints.ForEach(x => x.Hide = true);

            _roleHints[0].Text = roleName;
            _roleHints[0].Hide = false;

            for(int i = 1; i < _roleHints.Count; i++)
            {
                if(!description.TryGet(i - 1, out string element)) 
                    break;

                _roleHints[i].Text = element;
                _roleHints[i].Hide = false;
            }
        }
        #endregion Common Role Hints Methods

        # region Common Other Hints Methods
        public void ShowOtherHint(string hint) => ShowOtherHint(hint, Config.OtherHintDisplayTime);

        public void ShowOtherHint(string hint, float time) => ShowOtherHint(new string[] { hint }, time);

        public void ShowOtherHint(string[] hints) => ShowOtherHint(hints, Config.OtherHintDisplayTime * hints.Length);

        public void ShowOtherHint(string[] hints, float time)
        {
            _otherHintTimeToRemove = DateTime.Now + TimeSpan.FromSeconds(time);

            _otherHints.ForEach(x => x.Hide = true);

            for (int i = 0; i < _otherHints.Count; i++)
            {
                if (!hints.TryGet(i, out string element))
                    break;

                _otherHints[i].Text = element;
                _otherHints[i].Hide = false;
            }
        }
        #endregion Common Other Hints Methods

        #endregion Common Hint Methods

        #region Constructor and Destructors Methods

        internal CommonHint(ReferenceHub referenceHub)
        {
            this.ReferenceHub = referenceHub;

            //Add hint
            PlayerDisplay.AddHint(_itemHints);
            PlayerDisplay.AddHint(_mapHints);
            PlayerDisplay.AddHint(_roleHints);
            PlayerDisplay.AddHint(_otherHints);

            //Start coroutine
            _commonHintUpdateCoroutine = Timing.RunCoroutine(CommonHintCoroutineMethod());
        }

        internal void Destruct()
        {
            if (_commonHintUpdateCoroutine.IsRunning)
            {
                Timing.KillCoroutines(_commonHintUpdateCoroutine);
            }

            PlayerDisplay.RemoveHint(_itemHints);
            PlayerDisplay.RemoveHint(_mapHints);
            PlayerDisplay.RemoveHint(_roleHints);
            PlayerDisplay.RemoveHint(_otherHints);
        }

        #endregion

        # region Private Common Hints Methods
        private IEnumerator<float> CommonHintCoroutineMethod()
        {
            while (true)
            {
                DateTime currentTime = DateTime.Now;

                try
                {
                    if (currentTime > _itemHintTimeToRemove)
                    {
                        _itemHints.ForEach(x => x.Hide = true);
                    }

                    if (currentTime > _mapHintTimeToRemove)
                    {
                        _mapHints.ForEach(x => x.Hide = true);
                    }

                    if (currentTime > _roleHintTimeToRemove)
                    {
                        _roleHints.ForEach(x => x.Hide = true);
                    }

                    if (currentTime > _otherHintTimeToRemove)
                    {
                        _otherHints.ForEach(x => x.Hide = true);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }

                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        #endregion
    }

}
