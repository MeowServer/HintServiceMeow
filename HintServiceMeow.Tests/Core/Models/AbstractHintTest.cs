using System;
using System.Threading.Tasks;
using HintServiceMeow.Core.Models.Hints;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HintServiceMeow.Tests.Core.Models;

/// <summary>
/// Regression tests for a deadlock that occurred when AbstractHint property setters
/// invoked OnHintUpdated while still holding the write lock. PropertyChanged handlers
/// (e.g. PlayerDisplay.ScheduleUpdate) would read other hints' properties, requiring
/// their read locks. Two threads modifying different hints simultaneously could form
/// a circular wait (A-write → B-read vs B-write → A-read).
/// The fix moved all OnHintUpdated calls outside the write lock.
/// </summary>
[TestClass]
public class AbstractHintTest
{
    [TestMethod]
    [Timeout(15000)]
    public void ConcurrentPropertyModification_WithCrossHintPropertyReads_DoesNotDeadlock()
    {
        // Arrange
        Hint hintA = new();
        Hint hintB = new();

        // Simulate ScheduleUpdate behavior: when one hint changes,
        // read properties of the other hint.
        hintA.PropertyChanged += (_, _) =>
        {
            _ = hintB.SyncSpeed;
            _ = hintB.FontSize;
            _ = hintB.Hide;
            _ = hintB.YCoordinate;
        };

        hintB.PropertyChanged += (_, _) =>
        {
            _ = hintA.SyncSpeed;
            _ = hintA.FontSize;
            _ = hintA.Hide;
            _ = hintA.YCoordinate;
        };

        // Act — two threads modify their respective hints in tight loops
        Task taskA = Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                hintA.Text = $"textA{i}";
                hintA.FontSize = 20 + (i % 10);
                hintA.Hide = i % 2 == 0;
                hintA.YCoordinate = 700 + i;
            }
        });

        Task taskB = Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                hintB.Text = $"textB{i}";
                hintB.FontSize = 20 + (i % 10);
                hintB.Hide = i % 2 == 0;
                hintB.YCoordinate = 700 + i;
            }
        });

        // Assert — timeout means deadlock
        bool completed = Task.WaitAll(new[] { taskA, taskB }, TimeSpan.FromSeconds(10));
        Assert.IsTrue(completed, "Tasks did not complete within timeout — possible deadlock detected.");
    }

    [TestMethod]
    [Timeout(15000)]
    public void TextSetterElseBranch_ConcurrentWithCrossHintReads_DoesNotDeadlock()
    {
        // Arrange
        Hint hintA = new();
        Hint hintB = new();

        // Start hintA with AutoContent so the Text setter takes the else branch
        // (backing field direct assignment path).
        hintA.AutoText = _ => "auto";

        hintA.PropertyChanged += (_, _) =>
        {
            _ = hintB.SyncSpeed;
            _ = hintB.FontSize;
            _ = hintB.Hide;
        };

        hintB.PropertyChanged += (_, _) =>
        {
            _ = hintA.SyncSpeed;
            _ = hintA.FontSize;
            _ = hintA.Text;
        };

        // Act
        Task taskA = Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                // Reset to AutoContent, then set Text to trigger the else branch
                hintA.AutoText = _ => "auto";
                hintA.Text = $"text{i}";
            }
        });

        Task taskB = Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                hintB.FontSize = 20 + (i % 10);
                hintB.Hide = i % 2 == 0;
                hintB.YCoordinate = 700 + i;
            }
        });

        // Assert — timeout means deadlock
        bool completed = Task.WaitAll(new[] { taskA, taskB }, TimeSpan.FromSeconds(10));
        Assert.IsTrue(completed, "Tasks did not complete within timeout — possible deadlock detected in Text setter else branch.");
    }
}
