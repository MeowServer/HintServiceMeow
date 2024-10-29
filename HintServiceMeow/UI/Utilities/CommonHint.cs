using System;
using System.Collections.Generic;

using MEC;

using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;

using PluginAPI.Core;

namespace HintServiceMeow.UI.Utilities
{
    public class CommonHint
    {
        private static readonly string HintGroupID = "HSM_CommonHint";

        private static PluginConfig Config => PluginConfig.Instance;

        private ReferenceHub ReferenceHub { get; }
        private PlayerDisplay PlayerDisplay => PlayerDisplay.Get(ReferenceHub);

        #region Common Hints
        private readonly TaskScheduler _itemHintsHideScheduler;
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

        private readonly TaskScheduler _mapHintsHideScheduler;
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

        private readonly TaskScheduler _roleHintsHideScheduler;
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

        #region Constructor and Destructors Methods

        internal CommonHint(ReferenceHub referenceHub)
        {
            this.ReferenceHub = referenceHub;

            _itemHintsHideScheduler = new TaskScheduler(TimeSpan.MinValue, () => _itemHints.ForEach(x => x.Hide = true));
            _mapHintsHideScheduler = new TaskScheduler(TimeSpan.MinValue, () => _mapHints.ForEach(x => x.Hide = true));
            _roleHintsHideScheduler = new TaskScheduler(TimeSpan.MinValue, () => _roleHints.ForEach(x => x.Hide = true));

            //Add hint
            PlayerDisplay.InternalAddHint(HintGroupID, _itemHints);
            PlayerDisplay.InternalAddHint(HintGroupID, _mapHints);
            PlayerDisplay.InternalAddHint(HintGroupID, _roleHints);
        }

        internal void Destruct()
        {
            PlayerDisplay.InternalClearHint(HintGroupID);
        }

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
            _itemHintsHideScheduler.StartAction(time, TaskScheduler.DelayType.Override);

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
            _mapHintsHideScheduler.StartAction(time, TaskScheduler.DelayType.Override);

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
            _roleHintsHideScheduler.StartAction(time, TaskScheduler.DelayType.Override);

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
    }
}
