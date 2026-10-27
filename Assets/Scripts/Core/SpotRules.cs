namespace Treasure
{
    // 지도에서 보이는 스팟 상태
    public enum SpotState
    {
        Locked,
        Cooldown,
        OutOfRange,
        Capturable,
        Cleared
    }

    public static class SpotRules
    {
        // 위에서부터 먼저 맞는 조건이 이긴다
        public static SpotState Evaluate(bool isFinalSpot, bool isCleared, bool puzzleComplete,
            float cooldownRemaining, double distanceMeters, float radiusMeters)
        {
            if (isFinalSpot && isCleared)
            {
                return SpotState.Cleared;
            }
            if (isFinalSpot && !puzzleComplete)
            {
                return SpotState.Locked;
            }
            if (cooldownRemaining > 0f)
            {
                return SpotState.Cooldown;
            }
            if (distanceMeters <= radiusMeters)
            {
                return SpotState.Capturable;
            }
            return SpotState.OutOfRange;
        }
    }
}
