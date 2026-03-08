namespace HintServiceMeow.UI.Utilities
{
    using System;
    using System.Collections.Generic;
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Extension;
    using HintServiceMeow.Core.Models.Hints;
    using HintServiceMeow.Core.Utilities;
    using HintServiceMeow.Plugin;

    /// <summary>
    /// Provides pre-configured hint slots for common in-game UI scenarios such as item, map, role, and other general-purpose hints.
    /// </summary>
    public class CommonHint : Core.Interface.IDestructible
    {
        private const string HintGroupId = "HSM_CommonHint";

        #region Common Hints
        private readonly TaskScheduler itemHintsHideScheduler;

        private readonly List<Hint> itemHints =
        [
            new()
            {
                FontSize = 25,
            },

            new()
            {
                YCoordinate = 725,
                FontSize = 25,
            },
        ];

        private readonly TaskScheduler mapHintsHideScheduler;
        private readonly List<Hint> mapHints =
        [
            new()
            {
                YCoordinate = 200,
                FontSize = 25,
            },

            new()
            {
                YCoordinate = 225,
                FontSize = 25,
            },
        ];

        private readonly TaskScheduler roleHintsHideScheduler;
        private readonly List<Hint> roleHints =
        [
            new()
            {
                YCoordinate = 100,
                FontSize = 30,
                Alignment = HintAlignment.Left,
            },

            new()
            {
                YCoordinate = 130,
                FontSize = 25,
                Alignment = HintAlignment.Left,
            },

            new()
            {
                YCoordinate = 155,
                FontSize = 25,
                Alignment = HintAlignment.Left,
            },

            new()
            {
                YCoordinate = 180,
                FontSize = 25,
                Alignment = HintAlignment.Left,
            },
        ];
        #endregion

        #region Constructor

        internal CommonHint(ReferenceHub referenceHub)
        {
            ReferenceHub = referenceHub;

            itemHintsHideScheduler = new TaskScheduler();
            mapHintsHideScheduler = new TaskScheduler();
            roleHintsHideScheduler = new TaskScheduler();

            itemHintsHideScheduler.Start(TimeSpan.Zero, () => itemHints.ForEach(x => x.Hide = true));
            mapHintsHideScheduler.Start(TimeSpan.Zero, () => mapHints.ForEach(x => x.Hide = true));
            roleHintsHideScheduler.Start(TimeSpan.Zero, () => roleHints.ForEach(x => x.Hide = true));

            // Add hint
            foreach (Hint itemHint in itemHints)
                PlayerDisplay.InternalAddHint(HintGroupId, itemHint);
            foreach (Hint mapHint in mapHints)
                PlayerDisplay.InternalAddHint(HintGroupId, mapHint);
            foreach (Hint roleHint in roleHints)
                PlayerDisplay.InternalAddHint(HintGroupId, roleHint);
        }
        #endregion

        #region Properties
        private static PluginConfig Config => Plugin.Instance.Config;

        private ReferenceHub ReferenceHub { get; }

        private PlayerDisplay PlayerDisplay => PlayerDisplay.Get(ReferenceHub);
        #endregion

        void Core.Interface.IDestructible.Destruct()
        {
            PlayerDisplay.InternalClearHint(HintGroupId);
        }

        #region Common Hint Methods

        #region Common Item Hints Methods

        /// <summary>
        /// Displays an item hint with only the item name, using the configured short display time.
        /// </summary>
        /// <param name="itemName">The name of the item to display.</param>
        public void ShowItemHint(string itemName) => ShowItemHint(itemName, Config.ShortItemHintDisplayTime);

        /// <summary>
        /// Displays an item hint with only the item name for the specified duration.
        /// </summary>
        /// <param name="itemName">The name of the item to display.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowItemHint(string itemName, float time) => ShowItemHint(itemName, [], time);

        /// <summary>
        /// Displays an item hint with a name and a single description line, using the configured display time.
        /// </summary>
        /// <param name="itemName">The name of the item to display.</param>
        /// <param name="description">A description line shown below the item name.</param>
        public void ShowItemHint(string itemName, string description) => ShowItemHint(itemName, [description], Config.ItemHintDisplayTime);

        /// <summary>
        /// Displays an item hint with a name and a single description line for the specified duration.
        /// </summary>
        /// <param name="itemName">The name of the item to display.</param>
        /// <param name="description">A description line shown below the item name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowItemHint(string itemName, string description, float time) => ShowItemHint(itemName, [description], time);

        /// <summary>
        /// Displays an item hint with a name and multiple description lines, using the configured display time.
        /// </summary>
        /// <param name="itemName">The name of the item to display.</param>
        /// <param name="description">An array of description lines shown below the item name.</param>
        public void ShowItemHint(string itemName, string[] description) => ShowItemHint(itemName, description, Config.ItemHintDisplayTime);

        /// <summary>
        /// Displays an item hint with a name and multiple description lines for the specified duration.
        /// </summary>
        /// <param name="itemName">The name of the item to display.</param>
        /// <param name="description">An array of description lines shown below the item name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowItemHint(string itemName, string[] description, float time)
        {
            itemHintsHideScheduler.Invoke(time, DelayType.Override);

            itemHints[0].Text = itemName;
            itemHints[0].Hide = false;

            for (int i = 1; i < itemHints.Count; i++)
            {
                if (!description.TryGet(i - 1, out string element))
                    break;

                itemHints[i].Text = element;
                itemHints[i].Hide = false;
            }
        }
        #endregion Common Item Hints Methods

        #region Common Map Hints Methods

        /// <summary>
        /// Displays a map hint with only the room name, using the configured short display time.
        /// </summary>
        /// <param name="roomName">The name of the room to display.</param>
        public void ShowMapHint(string roomName) => ShowMapHint(roomName, Config.ShortMapHintDisplayTime);

        /// <summary>
        /// Displays a map hint with only the room name for the specified duration.
        /// </summary>
        /// <param name="roomName">The name of the room to display.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowMapHint(string roomName, float time) => ShowMapHint(roomName, [], time);

        /// <summary>
        /// Displays a map hint with a room name and a single description line, using the configured display time.
        /// </summary>
        /// <param name="roomName">The name of the room to display.</param>
        /// <param name="description">A description line shown below the room name.</param>
        public void ShowMapHint(string roomName, string description) => ShowMapHint(roomName, [description], Config.MapHintDisplayTime);

        /// <summary>
        /// Displays a map hint with a room name and a single description line for the specified duration.
        /// </summary>
        /// <param name="roomName">The name of the room to display.</param>
        /// <param name="description">A description line shown below the room name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowMapHint(string roomName, string description, float time) => ShowMapHint(roomName, [description], time);

        /// <summary>
        /// Displays a map hint with a room name and multiple description lines, using the configured display time.
        /// </summary>
        /// <param name="roomName">The name of the room to display.</param>
        /// <param name="description">An array of description lines shown below the room name.</param>
        public void ShowMapHint(string roomName, string[] description) => ShowMapHint(roomName, description, Config.MapHintDisplayTime);

        /// <summary>
        /// Displays a map hint with a room name and multiple description lines for the specified duration.
        /// </summary>
        /// <param name="roomName">The name of the room to display.</param>
        /// <param name="description">An array of description lines shown below the room name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowMapHint(string roomName, string[] description, float time)
        {
            mapHintsHideScheduler.Invoke(time, DelayType.Override);

            mapHints.ForEach(x => x.Hide = true);

            mapHints[0].Text = roomName;
            mapHints[0].Hide = false;

            for (int i = 1; i < mapHints.Count; i++)
            {
                if (!description.TryGet(i - 1, out string element))
                    break;

                mapHints[i].Text = element;
                mapHints[i].Hide = false;
            }
        }
        #endregion Common Map Hints Methods

        #region Common Role Hints Methods

        /// <summary>
        /// Displays a role hint with only the role name, using the configured short display time.
        /// </summary>
        /// <param name="roleName">The name of the role to display.</param>
        public void ShowRoleHint(string roleName) => ShowRoleHint(roleName, Config.ShortRoleHintDisplayTime);

        /// <summary>
        /// Displays a role hint with only the role name for the specified duration.
        /// </summary>
        /// <param name="roleName">The name of the role to display.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowRoleHint(string roleName, float time) => ShowRoleHint(roleName, [], time);

        /// <summary>
        /// Displays a role hint with a role name and a single description line, using the configured display time.
        /// </summary>
        /// <param name="roleName">The name of the role to display.</param>
        /// <param name="description">A description line shown below the role name.</param>
        public void ShowRoleHint(string roleName, string description) => ShowRoleHint(roleName, [description], Config.RoleHintDisplayTime);

        /// <summary>
        /// Displays a role hint with a role name and a single description line for the specified duration.
        /// </summary>
        /// <param name="roleName">The name of the role to display.</param>
        /// <param name="description">A description line shown below the role name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowRoleHint(string roleName, string description, float time) => ShowRoleHint(roleName, [description], time);

        /// <summary>
        /// Displays a role hint with a role name and multiple description lines, using the configured display time.
        /// </summary>
        /// <param name="roleName">The name of the role to display.</param>
        /// <param name="description">An array of description lines shown below the role name.</param>
        public void ShowRoleHint(string roleName, string[] description) => ShowRoleHint(roleName, description, Config.RoleHintDisplayTime);

        /// <summary>
        /// Displays a role hint with a role name and multiple description lines for the specified duration.
        /// </summary>
        /// <param name="roleName">The name of the role to display.</param>
        /// <param name="description">An array of description lines shown below the role name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowRoleHint(string roleName, string[] description, float time)
        {
            roleHintsHideScheduler.Invoke(time, DelayType.Override);

            roleHints.ForEach(x => x.Hide = true);

            roleHints[0].Text = roleName;
            roleHints[0].Hide = false;

            for (int i = 1; i < roleHints.Count; i++)
            {
                if (!description.TryGet(i - 1, out string element))
                    break;

                roleHints[i].Text = element;
                roleHints[i].Hide = false;
            }
        }
        #endregion Common Role Hints Methods

        #region Common Other Hints Methods

        /// <summary>
        /// Displays a single general-purpose hint message using the configured display time.
        /// </summary>
        /// <param name="messages">The message to display.</param>
        public void ShowOtherHint(string messages) => ShowOtherHint(messages, Config.OtherHintDisplayTime);

        /// <summary>
        /// Displays a single general-purpose hint message for the specified duration.
        /// </summary>
        /// <param name="messages">The message to display.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowOtherHint(string messages, float time) => ShowOtherHint([messages], time);

        /// <summary>
        /// Displays multiple general-purpose hint messages, each using the configured display time scaled by the number of messages.
        /// </summary>
        /// <param name="messages">The messages to display.</param>
        public void ShowOtherHint(string[] messages) => ShowOtherHint(messages, Config.OtherHintDisplayTime * messages.Length);

        /// <summary>
        /// Displays multiple general-purpose hint messages, each shown for the specified duration.
        /// </summary>
        /// <param name="messages">The messages to display.</param>
        /// <param name="time">The duration in seconds to show each hint.</param>
        public void ShowOtherHint(string[] messages, float time)
        {
            foreach (string message in messages)
            {
                DynamicHint dynamicHint = new()
                {
                    Text = message,
                    TopBoundary = 400,
                    BottomBoundary = 1000,
                    TargetY = 700,
                };

                PlayerDisplay.InternalAddHint("Other Hint", dynamicHint);
                PlayerDisplay.RemoveAfter(dynamicHint, time);
            }
        }
        #endregion Common Other Hints Methods

        #endregion Common Hint Methods
    }
}
