using NUnit.Framework;

namespace Treasure.Tests
{
    public class SpotRulesTests
    {
        [Test]
        public void FinalSpot_IsLockedUntilPuzzleCompleteEvenInRange()
        {
            Assert.AreEqual(SpotState.Locked, SpotRules.Evaluate(true, false, false, 0f, 5, 40f));
        }

        [Test]
        public void Cooldown_WinsOverInRange()
        {
            Assert.AreEqual(SpotState.Cooldown, SpotRules.Evaluate(false, false, false, 10f, 5, 40f));
        }

        [Test]
        public void DistanceEqualToRadius_IsCapturable()
        {
            Assert.AreEqual(SpotState.Capturable, SpotRules.Evaluate(false, false, false, 0f, 40, 40f));
        }

        [Test]
        public void OutsideRadius_IsOutOfRange()
        {
            Assert.AreEqual(SpotState.OutOfRange, SpotRules.Evaluate(false, false, false, 0f, 40.01, 40f));
        }

        [Test]
        public void ClearedFinalSpot_IsCleared()
        {
            Assert.AreEqual(SpotState.Cleared, SpotRules.Evaluate(true, true, true, 0f, 5, 40f));
        }
    }
}
