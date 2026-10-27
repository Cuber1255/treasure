using UnityEngine;

namespace Treasure
{
    // 탭 추격 포획 규칙 (화면과 분리된 순수 로직)
    public class TapChaseGame
    {
        public int TapsRequired { get; }
        public int Taps { get; private set; }
        public float TimeRemaining { get; private set; }

        public bool IsCaptured => Taps >= TapsRequired;
        public bool IsFailed => !IsCaptured && TimeRemaining <= 0f;
        public bool IsFinished => IsCaptured || IsFailed;
        public float Progress01 => TapsRequired <= 0 ? 1f : Mathf.Clamp01((float)Taps / TapsRequired);

        // 감속 아이템 효과가 남아 있으면 감속 배율, 아니면 1
        public float SpeedMultiplier => _slowRemaining > 0f ? _slowMultiplier : 1f;

        private float _slowRemaining;
        private float _slowMultiplier = 1f;

        public TapChaseGame(int tapsRequired, float timeLimitSeconds)
        {
            TapsRequired = tapsRequired;
            TimeRemaining = timeLimitSeconds;
        }

        public void Tick(float dt)
        {
            if (IsFinished)
            {
                return;
            }
            TimeRemaining = Mathf.Max(0f, TimeRemaining - dt);
            _slowRemaining = Mathf.Max(0f, _slowRemaining - dt);
        }

        public void RegisterHit()
        {
            if (IsFinished)
            {
                return;
            }
            Taps++;
        }

        public void AddTime(float seconds)
        {
            TimeRemaining += seconds;
        }

        public void ApplySlow(float multiplier, float duration)
        {
            _slowMultiplier = multiplier;
            _slowRemaining = duration;
        }

        // 필요 탭 수의 ratio만큼 게이지를 채운다 (상한 = 필요 탭 수)
        public void FillGauge(float ratio)
        {
            Taps = Mathf.Min(TapsRequired, Taps + Mathf.CeilToInt(TapsRequired * ratio));
        }
    }
}
