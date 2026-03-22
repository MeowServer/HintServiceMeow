using System;
using System.Runtime.Serialization;
using System.Threading;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Core.Utilities.Parser;
using HintServiceMeow.Plugin;
using HintServiceMeow.Tests.Core.Utilities.TestDoubles;
using HintServiceMeow.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HintServiceMeow.Tests.Core.Utilities;

[TestClass]
public class CompatibilityAdaptorTests
{
    [TestInitialize]
    public void Setup() => EnsurePluginConfig();

    [TestMethod]
    public void ShowHint_WhenArgIsNull_Throws()
    {
        // Arrange
        CompatibilityAdaptor adaptor = CreateAdaptor(out _);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => adaptor.ShowHint(null!));
    }

    [TestMethod]
    public void ShowHint_WhenDurationIsNonPositive_ClearsGroup()
    {
        // Arrange
        CompatibilityAdaptor adaptor = CreateAdaptor(out PlayerDisplay display);
        display.InternalAddHint("CompatibilityAdaptor-asm", new Hint { Text = "old" });

        // Act
        adaptor.ShowHint(new("asm", "any", 0));

        // Assert
        Assert.AreEqual(0, display.InternalGetHints("CompatibilityAdaptor-asm").Length);
    }

    [TestMethod]
    public void ShowHint_WhenAssemblyIsDisabled_IgnoresHint()
    {
        // Arrange
        Plugin.Plugin.Instance.Config.DisabledCompatAssemblies.Clear();
        Plugin.Plugin.Instance.Config.DisabledCompatAssemblies.Add("blocked");
        CompatibilityAdaptor adaptor = CreateAdaptor(out PlayerDisplay display);

        // Act
        adaptor.ShowHint(new("blocked", "content", 1));
        Thread.Sleep(50);

        // Assert
        Assert.AreEqual(0, display.InternalGetHints("CompatibilityAdaptor-blocked").Length);
    }

    [TestMethod]
    public void Destruct_WhenCalledMultipleTimes_IsIdempotentAndPreventsFurtherUpdates()
    {
        // Arrange
        CompatibilityAdaptor adaptor = CreateAdaptor(out PlayerDisplay display);

        // Act
        ((HintServiceMeow.Core.Interface.IDestructible)adaptor).Destruct();
        ((HintServiceMeow.Core.Interface.IDestructible)adaptor).Destruct();
        adaptor.ShowHint(new("asm", "later", 1f));
        Thread.Sleep(50);

        // Assert
        Assert.AreEqual(0, display.InternalGetHints("CompatibilityAdaptor-asm").Length);
    }

    private static CompatibilityAdaptor CreateAdaptor(out PlayerDisplay display)
    {
        TestPlayerContext context = new() { IsStillValid = true };
        display = new PlayerDisplay(
            context,
            updateScheduler: new TestTaskScheduler(),
            adaptor: new TestCompatibilityAdaptor(),
            hintParser: new TestHintParser(),
            coroutineRunner: new TestCoroutineRunner(),
            dispatcher: new TestMainThreadDispatcher());

        return new CompatibilityAdaptor(display, new RecordingPool<RichTextParser>(() => new RichTextParser()), new TestCoroutineRunner());
    }

    private static void EnsurePluginConfig()
    {
        if (ReflectionHelper.GetStaticFieldValue<object>(typeof(Plugin.Plugin), "<Instance>k__BackingField") is not null)
            return;

        Plugin.Plugin fakePlugin = (Plugin.Plugin)FormatterServices.GetUninitializedObject(typeof(Plugin.Plugin));
        ReflectionHelper.SetFieldValue(fakePlugin, "<Config>k__BackingField", new PluginConfig { DisabledCompatAssemblies = [] });
        ReflectionHelper.SetStaticFieldValue(typeof(Plugin.Plugin), "<Instance>k__BackingField", fakePlugin);
    }
}
