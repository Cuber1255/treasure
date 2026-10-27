using System;
using NUnit.Framework;

namespace Treasure.Tests
{
    public class PlayerProgressTests
    {
        private static readonly DateTime NOW = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        private static PlayerProgress CreateProgress(int coins = 300)
        {
            return new PlayerProgress(new SaveData { coins = coins });
        }

        [Test]
        public void CollectNextPiece_GivesCandidatesInOrderThenMinusOne()
        {
            PlayerProgress progress = CreateProgress();
            int[] candidates = { 4, 5 };

            Assert.AreEqual(4, progress.CollectNextPiece(candidates));
            Assert.AreEqual(5, progress.CollectNextPiece(candidates));
            Assert.AreEqual(-1, progress.CollectNextPiece(candidates));
            Assert.AreEqual(2, progress.CollectedPieceCount);
        }

        [Test]
        public void CollectNextPiece_SkipsOwnedPiece()
        {
            PlayerProgress progress = CreateProgress();
            progress.CollectNextPiece(new[] { 4 });

            Assert.AreEqual(5, progress.CollectNextPiece(new[] { 4, 5 }));
        }

        [Test]
        public void TrySpendCoins_FailsWithoutChangingBalanceWhenInsufficient()
        {
            PlayerProgress progress = CreateProgress(50);

            Assert.IsFalse(progress.TrySpendCoins(100));
            Assert.AreEqual(50, progress.Coins);
            Assert.IsTrue(progress.TrySpendCoins(50));
            Assert.AreEqual(0, progress.Coins);
        }

        [Test]
        public void Cooldown_CountsDownByRealTime()
        {
            PlayerProgress progress = CreateProgress();
            progress.StartCooldown("Park228", NOW, 180f);

            Assert.AreEqual(120f, progress.GetCooldownRemaining("Park228", NOW.AddSeconds(60)), 0.01f);
            Assert.AreEqual(0f, progress.GetCooldownRemaining("Park228", NOW.AddSeconds(181)));
            Assert.AreEqual(0f, progress.GetCooldownRemaining("Other", NOW));
        }

        [Test]
        public void ClearCooldowns_RemovesAllWaiting()
        {
            PlayerProgress progress = CreateProgress();
            progress.StartCooldown("Park228", NOW, 180f);
            progress.StartCooldown("Other", NOW, 60f);

            progress.ClearCooldowns();

            Assert.AreEqual(0f, progress.GetCooldownRemaining("Park228", NOW));
            Assert.AreEqual(0f, progress.GetCooldownRemaining("Other", NOW));
        }

        [Test]
        public void TryUseItem_DecrementsAndFailsAtZero()
        {
            PlayerProgress progress = CreateProgress();
            progress.AddItem("Hourglass");

            Assert.IsTrue(progress.TryUseItem("Hourglass"));
            Assert.AreEqual(0, progress.GetItemCount("Hourglass"));
            Assert.IsFalse(progress.TryUseItem("Hourglass"));
        }

        [Test]
        public void RegisterCard_ReturnsFalseForDuplicate()
        {
            PlayerProgress progress = CreateProgress();

            Assert.IsTrue(progress.RegisterCard("Ipari"));
            Assert.IsFalse(progress.RegisterCard("Ipari"));
            Assert.IsTrue(progress.HasCard("Ipari"));
        }

        [Test]
        public void IsPuzzleComplete_RequiresAllPieces()
        {
            PlayerProgress progress = CreateProgress();
            for (int i = 0; i < 15; i++)
            {
                progress.CollectNextPiece(new[] { i });
            }
            Assert.IsFalse(progress.IsPuzzleComplete(16));

            progress.CollectNextPiece(new[] { 15 });
            Assert.IsTrue(progress.IsPuzzleComplete(16));
        }
    }
}
