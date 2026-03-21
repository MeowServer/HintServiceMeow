namespace HintServiceMeow.UI.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        /// Displays an item hint with only the item name for the specified duration.
        /// </summary>
        /// <param name="itemName">The name of the item to display.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowItemHint(string itemName, float? time = null) => ShowItemHint(itemName, [], time ?? Config.ShortItemHintDisplayTime);

        /// <summary>
        /// Displays an item hint with a name and a single description line for the specified duration.
        /// </summary>
        /// <param name="itemName">The name of the item to display.</param>
        /// <param name="description">A description line shown below the item name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowItemHint(string itemName, string description, float? time = null) => ShowItemHint(itemName, [description], time ?? Config.ItemHintDisplayTime);

        /// <summary>
        /// Displays an item hint with a name and multiple description lines for the specified duration.
        /// </summary>
        /// <param name="itemName">The name of the item to display.</param>
        /// <param name="description">An array of description lines shown below the item name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowItemHint(string itemName, string[] description, float? time = null)
        {
            if (time == null)
            {
                time = Config.ItemHintDisplayTime;
            }

            itemHintsHideScheduler.Invoke(time.Value, DelayType.Override);

            itemHints.ForEach(x => x.Hide = true);

            itemHints[0].Text = itemName;
            itemHints[0].Hide = false;

            for (int i = 0; i < description.Length; i++)
            {
                if (itemHints.Count < i + 2)
                {
                    Hint newHint = new()
                    {
                        YCoordinate = itemHints.Last().YCoordinate + 25,
                        FontSize = 25,
                    };

                    PlayerDisplay.AddHint(newHint);
                    itemHints.Add(newHint);
                }

                itemHints[i + 1].Text = description[i];
                itemHints[i + 1].Hide = false;
            }
        }
        #endregion Common Item Hints Methods

        #region Common Map Hints Methods

        /// <summary>
        /// Displays a map hint with only the room name for the specified duration.
        /// </summary>
        /// <param name="roomName">The name of the room to display.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowMapHint(string roomName, float? time = null) => ShowMapHint(roomName, [], time ?? Config.ShortMapHintDisplayTime);

        /// <summary>
        /// Displays a map hint with a room name and a single description line for the specified duration.
        /// </summary>
        /// <param name="roomName">The name of the room to display.</param>
        /// <param name="description">A description line shown below the room name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowMapHint(string roomName, string description, float? time = null) => ShowMapHint(roomName, [description], time ?? Config.MapHintDisplayTime);

        /// <summary>
        /// Displays a map hint with a room name and multiple description lines for the specified duration.
        /// </summary>
        /// <param name="roomName">The name of the room to display.</param>
        /// <param name="description">An array of description lines shown below the room name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowMapHint(string roomName, string[] description, float? time = null)
        {
            if (time == null)
            {
                time = Config.MapHintDisplayTime;
            }

            mapHintsHideScheduler.Invoke(time.Value, DelayType.Override);

            mapHints.ForEach(x => x.Hide = true);

            mapHints[0].Text = roomName;
            mapHints[0].Hide = false;

            for (int i = 0; i < description.Length; i++)
            {
                if (mapHints.Count < i + 2)
                {
                    Hint newHint = new()
                    {
                        YCoordinate = mapHints.Last().YCoordinate + 25,
                        FontSize = 25,
                    };

                    PlayerDisplay.AddHint(newHint);
                    mapHints.Add(newHint);
                }

                mapHints[i + 1].Text = description[i];
                mapHints[i + 1].Hide = false;
            }
        }
        #endregion Common Map Hints Methods

        #region Common Role Hints Methods

        /// <summary>
        /// Displays a role hint with only the role name for the specified duration.
        /// </summary>
        /// <param name="roleName">The name of the role to display.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowRoleHint(string roleName, float? time = null) => ShowRoleHint(roleName, [], time ?? Config.ShortRoleHintDisplayTime);

        /// <summary>
        /// Displays a role hint with a role name and a single description line for the specified duration.
        /// </summary>
        /// <param name="roleName">The name of the role to display.</param>
        /// <param name="description">A description line shown below the role name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowRoleHint(string roleName, string description, float? time = null) => ShowRoleHint(roleName, [description], time ?? Config.RoleHintDisplayTime);

        /// <summary>
        /// Displays a role hint with a role name and multiple description lines for the specified duration.
        /// </summary>
        /// <param name="roleName">The name of the role to display.</param>
        /// <param name="description">An array of description lines shown below the role name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowRoleHint(string roleName, string[] description, float? time = null)
        {
            if (time == null)
            {
                time = Config.RoleHintDisplayTime;
            }

            roleHintsHideScheduler.Invoke(time.Value, DelayType.Override);

            roleHints.ForEach(x => x.Hide = true);

            roleHints[0].Text = roleName;
            roleHints[0].Hide = false;

            for (int i = 0; i < description.Length; i++)
            {
                if (roleHints.Count < i + 2)
                {
                    Hint newHint = new()
                    {
                        YCoordinate = roleHints.Last().YCoordinate + 25,
                        FontSize = 25,
                        Alignment = HintAlignment.Left,
                    };

                    PlayerDisplay.AddHint(newHint);
                    roleHints.Add(newHint);
                }

                roleHints[i + 1].Text = description[i];
                roleHints[i + 1].Hide = false;
            }
        }
        #endregion Common Role Hints Methods

        #region Common Other Hints Methods

        /// <summary>
        /// Displays a single general-purpose hint message for the specified duration.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public void ShowOtherHint(string message, float? time = null) => ShowOtherHint([message], time ?? Config.OtherHintDisplayTime);

        /// <summary>
        /// Displays multiple general-purpose hint messages, each shown for the specified duration.
        /// </summary>
        /// <param name="messages">The messages to display.</param>
        /// <param name="time">The duration in seconds to show each hint.</param>
        public void ShowOtherHint(string[] messages, float? time = null)
        {
            if (time == null)
                time = Config.OtherHintDisplayTime;

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
                PlayerDisplay.RemoveAfter(dynamicHint, time.Value);
            }
        }
        #endregion Common Other Hints Methods

        #endregion Common Hint Methods
    }
}
