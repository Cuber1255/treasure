using System;

namespace Treasure
{
    // 저장 데이터를 감싸 진행 규칙(코인, 아이템, 퍼즐, 도감, 쿨다운)을 처리한다
    public class PlayerProgress
    {
        public SaveData Data { get; }

        // 값이 바뀔 때마다 발생한다 (저장, 화면 갱신용)
        public event Action Changed;

        public PlayerProgress(SaveData data)
        {
            Data = data;
        }

        // ---- 코인 ----

        public int Coins => Data.coins;

        public void AddCoins(int amount)
        {
            Data.coins += amount;
            Changed?.Invoke();
        }

        // 잔액이 부족하면 아무것도 바꾸지 않고 false를 반환한다
        public bool TrySpendCoins(int amount)
        {
            if (Data.coins < amount)
            {
                return false;
            }
            Data.coins -= amount;
            Changed?.Invoke();
            return true;
        }

        // ---- 아이템 ----

        public int GetItemCount(string itemId)
        {
            ItemStack stack = FindItem(itemId);
            return stack == null ? 0 : stack.count;
        }

        public void AddItem(string itemId)
        {
            ItemStack stack = FindItem(itemId);
            if (stack == null)
            {
                stack = new ItemStack { itemId = itemId, count = 0 };
                Data.items.Add(stack);
            }
            stack.count++;
            Changed?.Invoke();
        }

        public bool TryUseItem(string itemId)
        {
            ItemStack stack = FindItem(itemId);
            if (stack == null || stack.count <= 0)
            {
                return false;
            }
            stack.count--;
            Changed?.Invoke();
            return true;
        }

        private ItemStack FindItem(string itemId)
        {
            foreach (ItemStack stack in Data.items)
            {
                if (stack.itemId == itemId)
                {
                    return stack;
                }
            }
            return null;
        }

        // ---- 퍼즐 ----

        public bool HasPiece(int index) => Data.collectedPieces.Contains(index);

        public int CollectedPieceCount => Data.collectedPieces.Count;

        // 후보 순서대로 아직 없는 첫 조각을 지급한다. 줄 조각이 없으면 -1
        public int CollectNextPiece(int[] candidates)
        {
            foreach (int index in candidates)
            {
                if (!HasPiece(index))
                {
                    Data.collectedPieces.Add(index);
                    Changed?.Invoke();
                    return index;
                }
            }
            return -1;
        }

        public bool IsPuzzleComplete(int totalPieces) => CollectedPieceCount >= totalPieces;

        // ---- 도감 ----

        public bool HasCard(string characterId) => Data.capturedCharacters.Contains(characterId);

        // 새 카드면 true, 이미 있으면 false
        public bool RegisterCard(string characterId)
        {
            if (HasCard(characterId))
            {
                return false;
            }
            Data.capturedCharacters.Add(characterId);
            Changed?.Invoke();
            return true;
        }

        // ---- 쿨다운 ----

        // 남은 재도전 대기 시간(초). 대기 중이 아니면 0
        public float GetCooldownRemaining(string spotId, DateTime nowUtc)
        {
            SpotCooldown cooldown = FindCooldown(spotId);
            if (cooldown == null)
            {
                return 0f;
            }
            double seconds = (new DateTime(cooldown.endUtcTicks, DateTimeKind.Utc) - nowUtc).TotalSeconds;
            return seconds > 0 ? (float)seconds : 0f;
        }

        public void StartCooldown(string spotId, DateTime nowUtc, float seconds)
        {
            SpotCooldown cooldown = FindCooldown(spotId);
            if (cooldown == null)
            {
                cooldown = new SpotCooldown { spotId = spotId };
                Data.cooldowns.Add(cooldown);
            }
            cooldown.endUtcTicks = nowUtc.AddSeconds(seconds).Ticks;
            Changed?.Invoke();
        }

        // 디버그용: 모든 스팟의 재도전 대기를 없앤다
        public void ClearCooldowns()
        {
            Data.cooldowns.Clear();
            Changed?.Invoke();
        }

        private SpotCooldown FindCooldown(string spotId)
        {
            foreach (SpotCooldown cooldown in Data.cooldowns)
            {
                if (cooldown.spotId == spotId)
                {
                    return cooldown;
                }
            }
            return null;
        }

        // ---- 클리어 ----

        public bool IsCleared => Data.cleared;

        public void MarkCleared()
        {
            Data.cleared = true;
            Changed?.Invoke();
        }
    }
}
