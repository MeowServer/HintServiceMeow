namespace HintServiceMeow.UI.Extension
{
    using HintServiceMeow.UI.Utilities;
    using LabApi.Features.Wrappers;

    /// <summary>
    /// Provides extension methods for NW LabApi <c>Player</c> to retrieve the associated <see cref="PlayerUI"/>.
    /// </summary>
    public static class NWPlayerExtension
    {
        /// <summary>
        /// Gets the <see cref="PlayerUI"/> associated with the specified LabApi player.
        /// </summary>
        /// <param name="player">The LabApi player whose UI is retrieved.</param>
        /// <returns>The <see cref="PlayerUI"/> for the given player.</returns>
        public static PlayerUI GetPlayerUi(this LabApi.Features.Wrappers.Player player)
        {
            return PlayerUI.Get(player.ReferenceHub);
        }
        #region Common Hint Methods

        #region Common Item Hints Methods

        /// <summary>
        /// Displays an item hint with only the item name, using the configured short display time.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="itemName">The name of the item to display.</param>
        public static void ShowItemHint(this Player player, string itemName) => PlayerUI.Get(player).CommonHint.ShowItemHint(itemName);

        /// <summary>
        /// Displays an item hint with only the item name for the specified duration.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="itemName">The name of the item to display.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public static void ShowItemHint(this Player player, string itemName, float time) => PlayerUI.Get(player).CommonHint.ShowItemHint(itemName, [], time);

        /// <summary>
        /// Displays an item hint with a name and a single description line, using the configured display time.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="itemName">The name of the item to display.</param>
        /// <param name="description">A description line shown below the item name.</param>
        public static void ShowItemHint(this Player player, string itemName, string description) => PlayerUI.Get(player).CommonHint.ShowItemHint(itemName, [description]);

        /// <summary>
        /// Displays an item hint with a name and a single description line for the specified duration.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="itemName">The name of the item to display.</param>
        /// <param name="description">A description line shown below the item name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public static void ShowItemHint(this Player player, string itemName, string description, float time) => PlayerUI.Get(player).CommonHint.ShowItemHint(itemName, [description], time);

        /// <summary>
        /// Displays an item hint with a name and multiple description lines, using the configured display time.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="itemName">The name of the item to display.</param>
        /// <param name="description">An array of description lines shown below the item name.</param>
        public static void ShowItemHint(this Player player, string itemName, string[] description) => PlayerUI.Get(player).CommonHint.ShowItemHint(itemName, description);

        /// <summary>
        /// Displays an item hint with a name and multiple description lines for the specified duration.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="itemName">The name of the item to display.</param>
        /// <param name="description">An array of description lines shown below the item name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public static void ShowItemHint(this Player player, string itemName, string[] description, float time) => PlayerUI.Get(player).CommonHint.ShowItemHint(itemName, description, time);
        #endregion Common Item Hints Methods

        #region Common Map Hints Methods

        /// <summary>
        /// Displays a map hint with only the room name, using the configured short display time.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="roomName">The name of the room to display.</param>
        public static void ShowMapHint(this Player player, string roomName) => PlayerUI.Get(player).CommonHint.ShowMapHint(roomName);

        /// <summary>
        /// Displays a map hint with only the room name for the specified duration.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="roomName">The name of the room to display.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public static void ShowMapHint(this Player player, string roomName, float time) => PlayerUI.Get(player).CommonHint.ShowMapHint(roomName, [], time);

        /// <summary>
        /// Displays a map hint with a room name and a single description line, using the configured display time.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="roomName">The name of the room to display.</param>
        /// <param name="description">A description line shown below the room name.</param>
        public static void ShowMapHint(this Player player, string roomName, string description) => PlayerUI.Get(player).CommonHint.ShowMapHint(roomName, [description]);

        /// <summary>
        /// Displays a map hint with a room name and a single description line for the specified duration.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="roomName">The name of the room to display.</param>
        /// <param name="description">A description line shown below the room name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public static void ShowMapHint(this Player player, string roomName, string description, float time) => PlayerUI.Get(player).CommonHint.ShowMapHint(roomName, [description], time);

        /// <summary>
        /// Displays a map hint with a room name and multiple description lines, using the configured display time.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="roomName">The name of the room to display.</param>
        /// <param name="description">An array of description lines shown below the room name.</param>
        public static void ShowMapHint(this Player player, string roomName, string[] description) => PlayerUI.Get(player).CommonHint.ShowMapHint(roomName, description);

        /// <summary>
        /// Displays a map hint with a room name and multiple description lines for the specified duration.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="roomName">The name of the room to display.</param>
        /// <param name="description">An array of description lines shown below the room name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public static void ShowMapHint(this Player player, string roomName, string[] description, float time) => PlayerUI.Get(player).CommonHint.ShowMapHint(roomName, description, time);
        #endregion Common Map Hints Methods

        #region Common Role Hints Methods

        /// <summary>
        /// Displays a role hint with only the role name, using the configured short display time.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="roleName">The name of the role to display.</param>
        public static void ShowRoleHint(this Player player, string roleName) => PlayerUI.Get(player).CommonHint.ShowRoleHint(roleName);

        /// <summary>
        /// Displays a role hint with only the role name for the specified duration.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="roleName">The name of the role to display.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public static void ShowRoleHint(this Player player, string roleName, float time) => PlayerUI.Get(player).CommonHint.ShowRoleHint(roleName, [], time);

        /// <summary>
        /// Displays a role hint with a role name and a single description line, using the configured display time.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="roleName">The name of the role to display.</param>
        /// <param name="description">A description line shown below the role name.</param>
        public static void ShowRoleHint(this Player player, string roleName, string description) => PlayerUI.Get(player).CommonHint.ShowRoleHint(roleName, [description]);

        /// <summary>
        /// Displays a role hint with a role name and a single description line for the specified duration.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="roleName">The name of the role to display.</param>
        /// <param name="description">A description line shown below the role name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public static void ShowRoleHint(this Player player, string roleName, string description, float time) => PlayerUI.Get(player).CommonHint.ShowRoleHint(roleName, [description], time);

        /// <summary>
        /// Displays a role hint with a role name and multiple description lines, using the configured display time.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="roleName">The name of the role to display.</param>
        /// <param name="description">An array of description lines shown below the role name.</param>
        public static void ShowRoleHint(this Player player, string roleName, string[] description) => PlayerUI.Get(player).CommonHint.ShowRoleHint(roleName, description);

        /// <summary>
        /// Displays a role hint with a role name and multiple description lines for the specified duration.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="roleName">The name of the role to display.</param>
        /// <param name="description">An array of description lines shown below the role name.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public static void ShowRoleHint(this Player player, string roleName, string[] description, float time) => PlayerUI.Get(player).CommonHint.ShowRoleHint(roleName, description, time);
        #endregion Common Role Hints Methods

        #region Common Other Hints Methods

        /// <summary>
        /// Displays a single general-purpose hint message using the configured display time.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="messages">The message to display.</param>
        public static void ShowOtherHint(this Player player, string messages) => PlayerUI.Get(player).CommonHint.ShowOtherHint(messages);

        /// <summary>
        /// Displays a single general-purpose hint message for the specified duration.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="messages">The message to display.</param>
        /// <param name="time">The duration in seconds to show the hint.</param>
        public static void ShowOtherHint(this Player player, string messages, float time) => PlayerUI.Get(player).CommonHint.ShowOtherHint([messages], time);

        /// <summary>
        /// Displays multiple general-purpose hint messages, each using the configured display time scaled by the number of messages.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="messages">The messages to display.</param>
        public static void ShowOtherHint(this Player player, string[] messages) => PlayerUI.Get(player).CommonHint.ShowOtherHint(messages);

        /// <summary>
        /// Displays multiple general-purpose hint messages, each shown for the specified duration.
        /// </summary>
        /// <param name="player">The LabAPI player to show the hint to.</param>
        /// <param name="messages">The messages to display.</param>
        /// <param name="time">The duration in seconds to show each hint.</param>
        public static void ShowOtherHint(this Player player, string[] messages, float time) => PlayerUI.Get(player).CommonHint.ShowOtherHint(messages, time);
        #endregion Common Other Hints Methods

        #endregion Common Hint Methods
    }
}