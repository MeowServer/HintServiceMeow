using System;
using System.Collections.Generic;
using System.Linq;
using MEC;

using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.UI.Utilities;
using PluginAPI.Core;
using System.Diagnostics;

namespace HintServiceMeow.UI.Utilities
{
    public class CommonHint
    {
        private static PlayerUIConfig Config => PluginConfig.Instance.PlayerUIConfig;

        private readonly ReferenceHub ReferenceHub;
        private PlayerDisplay PlayerDisplay => PlayerDisplay.Get(ReferenceHub);

        #region Constructor and Destructors Methods

        internal CommonHint(ReferenceHub referenceHub)
        {
            this.ReferenceHub = referenceHub;

            //Add hint
            PlayerDisplay.AddHint(_itemHints);
            PlayerDisplay.AddHint(_mapHints);
            PlayerDisplay.AddHint(_roleHints);

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
        }

        #endregion

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
                FontSize = 30,
                Alignment = Core.Enum.HintAlignment.Left
            },
            new Hint()
            {
                YCoordinate = 130,
                FontSize = 25,
                Alignment = Core.Enum.HintAlignment.Left
            },
            new Hint()
            {
                YCoordinate = 155,
                FontSize = 25,
                Alignment = Core.Enum.HintAlignment.Left
            },
            new Hint()
            {
                YCoordinate = 180,
                FontSize = 25,
                Alignment = Core.Enum.HintAlignment.Left
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
        public void ShowOtherHint(string messages) => ShowOtherHint(messages, Config.OtherHintDisplayTime);

        public void ShowOtherHint(string messages, float time) => ShowOtherHint(new string[] { messages }, time);

        public void ShowOtherHint(string[] messages) => ShowOtherHint(messages, Config.OtherHintDisplayTime * messages.Length);

        public void ShowOtherHint(string[] messages, float time)
        {
            foreach(var message in messages)
            {
                var dynamicHint = new DynamicHint
                {
                    Text = message,
                    TopBoundary = 400,
                    BottomBoundary = 1000,
                    TargetY = 700,
                };

                PlayerDisplay.AddHint(dynamicHint);
                Timing.CallDelayed(time, () =>
                {
                    PlayerDisplay?.RemoveHint(dynamicHint);
                });
            }
        }

        #endregion Common Other Hints Methods

        #endregion Common Hint Methods

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
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        #endregion
    }
}
