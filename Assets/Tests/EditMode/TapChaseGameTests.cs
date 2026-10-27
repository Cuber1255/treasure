using NUnit.Framework;

namespace Treasure.Tests
{
    public class TapChaseGameTests
    {
        [Test]
        public void ReachingRequiredTaps_Captures()
        {
            TapChaseGame game = new TapChaseGame(3, 10f);
            game.RegisterHit();
            game.RegisterHit();
            Assert.IsFalse(game.IsCaptured);

            game.RegisterHit();
            Assert.IsTrue(game.IsCaptured);
            Assert.IsTrue(game.IsFinished);
        }

        [Test]
        public void TimeOut_FailsAndIgnoresLaterTaps()
        {
            TapChaseGame game = new TapChaseGame(3, 1f);
            game.Tick(1.5f);
            Assert.IsTrue(game.IsFailed);

            game.RegisterHit();
            Assert.AreEqual(0, game.Taps);
            Assert.IsTrue(game.IsFailed);
        }

        [Test]
        public void AddTime_IncreasesRemaining()
        {
            TapChaseGame game = new TapChaseGame(3, 10f);
            game.Tick(4f);
            game.AddTime(5f);
            Assert.AreEqual(11f, game.TimeRemaining, 0.001f);
        }

        [Test]
        public void FillGauge_AddsHalfAndStopsAtCap()
        {
            TapChaseGame game = new TapChaseGame(8, 10f);
            game.FillGauge(0.5f);
            Assert.AreEqual(4, game.Taps);

            game.RegisterHit();
            game.FillGauge(0.5f);
            Assert.AreEqual(8, game.Taps);
        }

        [Test]
        public void Slow_AppliesOnlyForDuration()
        {
            TapChaseGame game = new TapChaseGame(8, 20f);
            game.ApplySlow(0.4f, 5f);
            Assert.AreEqual(0.4f, game.SpeedMultiplier, 0.001f);

            game.Tick(4f);
            Assert.AreEqual(0.4f, game.SpeedMultiplier, 0.001f);

            game.Tick(1.5f);
            Assert.AreEqual(1f, game.SpeedMultiplier, 0.001f);
        }
    }
}
