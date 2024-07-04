using Exiled.API.Features;
using MEC;

using HintServiceMeow.Effect;
using HintServiceMeow.Config;

using System;
using System.Collections.Generic;
using System.Linq;

namespace HintServiceMeow
{
    /// <summary>
    /// A UI based on PlayerDisplay
    /// contain 3 main parts: common hints, UI Template, and _player _effects
    /// </summary>
    public class PlayerUI
    {
        private static PlayerUIConfig config => PluginConfig.Instance.PlayerUIConfig;

        private static readonly List<PlayerUI> PlayerUIList = new List<PlayerUI>();

        private readonly Player _player;
        private readonly PlayerDisplay _playerDisplay;

        #region Effects
        private CoroutineHandle _effectsUpdateCoroutine;

        private readonly List<IUIEffect> _effects = new List<IUIEffect>();
        #endregion
            
        #region Common Hints
        private CoroutineHandle _commonHintUpdateCoroutine;

        private DateTime _itemHintTimeToRemove = DateTime.MinValue;
        private readonly List<DynamicHint> _itemHints = new List<DynamicHint>();

        private DateTime _mapHintTimeToRemove = DateTime.MinValue;
        private readonly List<DynamicHint> _mapHints = new List<DynamicHint>();

        private DateTime _roleHintTimeToRemove = DateTime.MinValue;
        private readonly List<DynamicHint> _roleHints = new List<DynamicHint>();

        private DateTime _otherHintTimeToRemove = DateTime.MinValue;
        private readonly List<DynamicHint> _otherHints = new List<DynamicHint>();
        #endregion

        #region Effect Methods
        public void AddEffect(IUIEffect effect)
        {
            effect.SetEffect();
            _effects.Add(effect);
        }

        public void AddEffect<TEffectType>()
        {
            TEffectType instance = (TEffectType)Activator.CreateInstance(typeof(TEffectType));

            if(instance is IUIEffect effect)
            {
                AddEffect(effect);
            }
        }

        public void RemoveEffect(IUIEffect effect)
        {
            effect.DestructEffect();
            _effects.Remove(effect);
        }

        public void RemoveEffect<TEffectType >()
        {
            foreach(IUIEffect effect in _effects)
            {
                if(effect is TEffectType)
                {
                    RemoveEffect(effect);
                    return;
                }
            }
        }
        #endregion

        #region Common Hint Methods

        #region Broadcast Other Hint Methods
        public static void BroadcastOtherHint(string hint)
        {
            foreach(PlayerUI ui in PlayerUIList)
            {
                ui.ShowOtherHint(hint);
            }
        }

        public static void BroadcastOtherHint(string[] hints)
        {
            foreach (PlayerUI ui in PlayerUIList)
            {
                ui.ShowOtherHint(hints);
            }
        }

        public static void BroadcastOtherHints(string hint, int time)
        {
            foreach (PlayerUI ui in PlayerUIList)
            {
                ui.ShowOtherHint(hint, time);
            }
        }

        public static void BroadcastOtherHints(string[] hints, int time)
        {
            foreach (PlayerUI ui in PlayerUIList)
            {
                ui.ShowOtherHint(hints, time);
            }
        }
        #endregion Broadcast Other Hint Methods

        #region Common Item Hints Methods
        public void ShowItemHint(string itemName) => ShowItemHint(itemName, config.ShortItemHintDisplayTime);

        public void ShowItemHint(string itemName, int time) => ShowItemHint(itemName, new string[]{},time);

        public void ShowItemHint(string itemName, string description) => ShowItemHint(itemName, new string[] { description }, config.ItemHintDisplayTime);

        public void ShowItemHint(string itemName, string description, int time) => ShowItemHint(itemName, new string[] { description }, time);

        public void ShowItemHint(string itemName, string[] description) => ShowItemHint(itemName, description, config.ItemHintDisplayTime);

        public void ShowItemHint(string itemName, string[] description, int time)
        {
            _itemHintTimeToRemove = DateTime.Now + TimeSpan.FromSeconds(time);

            _itemHints[0].message = itemName;
            _itemHints[0].hide = false;

            for (int i = 1; i < _itemHints.Count; i++)
            {
                if (!description.TryGet(i - 1, out string element))
                    break;

                _itemHints[i].message = element;
                _itemHints[i].hide = false;
            }
        }
        #endregion Common Item Hints Methods

        # region Common Map Hints Methods
        public void ShowMapHint(string roomName) => ShowMapHint(roomName, config.ShortMapHintDisplayTime);

        public void ShowMapHint(string roomName, int time) => ShowMapHint(roomName, new string[]{}, time);

        public void ShowMapHint(string roomName, string description) => ShowMapHint(roomName, new string[1] { description }, config.ItemHintDisplayTime);

        public void ShowMapHint(string roomName, string description, int time) => ShowMapHint(roomName, new string[1] { description }, time);

        public void ShowMapHint(string roomName, string[] description) => ShowMapHint(roomName, description, config.MapHintDisplayTime);

        public void ShowMapHint(string roomName, string[] description, int time)
        {
            _mapHintTimeToRemove = DateTime.Now + TimeSpan.FromSeconds(time);

            _mapHints.ForEach(x => x.hide = true);

            _mapHints[0].message = roomName;
            _mapHints[0].hide = false;

            for (int i = 1; i < _mapHints.Count; i++)
            {
                if (!description.TryGet(i - 1, out string element))
                    break;

                _mapHints[i].message = element;
                _mapHints[i].hide = false;
            }
        }
        #endregion Common Map Hints Methods

        # region Common Role Hints Methods
        public void ShowRoleHint(string roleName) => ShowRoleHint(roleName, config.ShortRoleHintDisplayTime);

        public void ShowRoleHint(string roleName, int time) => ShowRoleHint(roleName, new string[]{}, time);

        public void ShowRoleHint(string roleName, string description) => ShowRoleHint(roleName, new string[] { description }, config.ItemHintDisplayTime);

        public void ShowRoleHint(string roleName, string description, int time) => ShowRoleHint(roleName, new string[] { description }, time);

        public void ShowRoleHint(string roleName, string[] description)=> ShowRoleHint(roleName, description, config.RoleHintDisplayTime);

        public void ShowRoleHint(string roleName, string[] description, int time)
        {
            _roleHintTimeToRemove = DateTime.Now + TimeSpan.FromSeconds(time);

            _roleHints.ForEach(x => x.hide = true);

            _roleHints[0].message = roleName;
            _roleHints[0].hide = false;

            for(int i = 1; i < _roleHints.Count; i++)
            {
                if(!description.TryGet(i - 1, out string element)) 
                    break;

                _roleHints[i].message = element;
                _roleHints[i].hide = false;
            }
        }
        #endregion Common Role Hints Methods

        # region Common Other Hints Methods
        public void ShowOtherHint(string hint) => ShowOtherHint(hint, config.OtherHintDisplayTime);

        public void ShowOtherHint(string hint, int time) => ShowOtherHint(new string[] { hint }, time);

        public void ShowOtherHint(string[] hints) => ShowOtherHint(hints, config.OtherHintDisplayTime * hints.Length);

        public void ShowOtherHint(string[] hints, int time)
        {
            _otherHintTimeToRemove = DateTime.Now + TimeSpan.FromSeconds(time);

            _otherHints.ForEach(x => x.hide = true);

            for (int i = 0; i < _otherHints.Count; i++)
            {
                if (!hints.TryGet(i, out string element))
                    break;

                _otherHints[i].message = element;
                _otherHints[i].hide = false;
            }
        }
        #endregion Common Other Hints Methods

        #endregion Common Hint Methods

        #region Public PlayerUIConfig tools
        public static PlayerUI Get(Player player)
        {
            return PlayerUIList.Find(x => x._player == player);
        }

        public static PlayerUI Get(PlayerDisplay display)
        {
            return PlayerUIList.Find(x => x._playerDisplay == display);
        }
        #endregion Public PlayerUIConfig tools

        #region Internal PlayerUIConfig Methods
        internal PlayerUI(Player player)
        {
            if (PlayerUIList.Any(x => x._player == player))
            {
                Log.Error($"A PlayerUIConfig for this _player had already been created for this _player : {player.Nickname}");
                return;
            }

            this._player = player;
            this._playerDisplay = PlayerDisplay.Get(player);

            SetUpEffect();

            SetUpCommonHints();

            PlayerUIList.Add(this);
        }

        internal void Destruct()
        {
            //Effects
            DestructEffect();

            //Common Hints
            DestructCommonHints();

            PlayerUIList.Remove(this);
        }

        internal static void RemovePlayerUI(Player player)
        {
            PlayerUIList.Find(x => x._player == player)?.Destruct();
        }

        internal static void ClearPlayerUI()
        {
            foreach (PlayerUI ui in PlayerUIList)
            {
                ui.Destruct();
            }
        }
        #endregion Internal PlayerUIConfig Methods

        #region Private Effect Methods
        private IEnumerator<float> EffectCoroutineMethod()
        {
            while (true)
            {
                foreach (IUIEffect effect in _effects)
                {
                    effect.UpdateEffect();
                }

                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        private void SetUpEffect()
        {
            _effectsUpdateCoroutine = Timing.RunCoroutine(EffectCoroutineMethod());
        }

        private void DestructEffect()
        {
            if (_effectsUpdateCoroutine.IsRunning)
            {
                Timing.KillCoroutines(_effectsUpdateCoroutine);
            }

            foreach (IUIEffect effect in _effects)
            {
                effect.DestructEffect();
            }
        }
        #endregion

        # region Private Common Hints Methods
        private IEnumerator<float> CommonHintCoroutineMethod()
        {
            while (true)
            {
                try
                {
                    DateTime currentTime = DateTime.Now;

                    if (currentTime > _itemHintTimeToRemove)
                    {
                        _itemHints.ForEach(x => x.hide = true);
                    }

                    if (currentTime > _mapHintTimeToRemove)
                    {
                        _mapHints.ForEach(x => x.hide = true);
                    }

                    if (currentTime > _roleHintTimeToRemove)
                    {
                        _roleHints.ForEach(x => x.hide = true);
                    }

                    if (currentTime > _otherHintTimeToRemove)
                    {
                        _otherHints.ForEach(x => x.hide = true);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }

                var timeToWait =
                    new[]
                    {
                        _itemHintTimeToRemove,
                        _mapHintTimeToRemove,
                        _roleHintTimeToRemove,
                        _otherHintTimeToRemove
                    }.Min() - DateTime.Now;

                yield return Timing.WaitForSeconds((float)timeToWait.TotalMilliseconds + 0.01f);
            }
        }

        private void SetUpCommonHints()
        {
            _itemHints.AddRange(config.ItemHints
                .Select(x => new DynamicHint(x)
                {
                    hide = true,
                    message = "",
                    priority = HintPriority.Medium,
                }));

            _mapHints.AddRange(config.MapHints
                .Select(x => new DynamicHint(x)
                {
                    hide = true,
                    message = "",
                    priority = HintPriority.Medium,
                }));

            _roleHints.AddRange(config.RoleHints
                .Select(x => new DynamicHint(x)
                {
                    hide = true,
                    message = "",
                    priority = HintPriority.Medium,
                }));

            _otherHints.AddRange(config.OtherHints
                .Select(x => new DynamicHint(x)
                {
                    hide = true,
                    message = "",
                    priority = HintPriority.Medium,
                }));

            _playerDisplay.AddHints(_itemHints);
            _playerDisplay.AddHints(_mapHints);
            _playerDisplay.AddHints(_roleHints);
            _playerDisplay.AddHints(_otherHints);

            _commonHintUpdateCoroutine = Timing.RunCoroutine(CommonHintCoroutineMethod());
        }

        private void DestructCommonHints()
        {
            if (_commonHintUpdateCoroutine.IsRunning)
            {
                Timing.KillCoroutines(_commonHintUpdateCoroutine);
            }

            _playerDisplay.RemoveHints(_itemHints);
            _playerDisplay.RemoveHints(_mapHints);
            _playerDisplay.RemoveHints(_roleHints);
            _playerDisplay.RemoveHints(_otherHints);
        }
        #endregion
    }

}
