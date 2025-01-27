using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Utilities;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;

namespace HintServiceTest
{
    /// <summary>
    /// This is the plugin I made to test the HintServiceMeow.
    /// </summary>
    public class Plugin : Plugin<Config>
    {
        public override string Name => "HintServiceTest";

        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Player.Verified += EventHandler.OnVerified;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Verified -= EventHandler.OnVerified;

            base.OnDisabled();
        }
    }

    public static class EventHandler
    {
        public static void OnVerified(VerifiedEventArgs ev)
        {
            //PluginAPI.Core.Player.Get(ev.Player.ReferenceHub).ReceiveHint("Hello Meow~", 10f);

            //Hint hint = new Hint
            //{
            //    AutoText = updateArg =>
            //    {
            //        Log.Info("AutoText Called");
            //        updateArg.NextUpdateDelay = 1f;
            //        return "Hello World";
            //    }
            //};

            //PlayerDisplay playerDisplay = PlayerDisplay.Get(ev.Player);
            //playerDisplay.AddHint(hint);
        }
    }
}
