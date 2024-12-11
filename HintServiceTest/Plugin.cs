using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;

using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;

using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.UI.Extension;
using HintServiceMeow.UI.Utilities;

using MEC;
using PlayerRoles;
using UnityEngine;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;

namespace HintServiceTest
{
    /// <summary>
    /// This is an Exiled only example of how to create a simple ui for players using Hint and PlayerDisplay.
    /// </summary>
    public class Plugin : Plugin<Config>
    {
        public override string Name => "HintServiceTest";

        public override void OnEnabled()
        {
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
        }
    }

    public static class TestB
    {
    }
}
