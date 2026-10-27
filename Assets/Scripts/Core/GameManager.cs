using System;
using UnityEngine;

namespace Treasure
{
    // 진행 상황, 설정, 위치를 한곳에 모아 다른 스크립트에 제공한다
    // 다른 스크립트의 Awake/Start보다 먼저 Progress를 준비해야 한다
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;
        [SerializeField] private LocationProvider _location;

        // 모바일 배터리를 고려한 목표 프레임
        private const int TARGET_FRAME_RATE = 60;

        public GameConfig Config => _config;
        public PlayerProgress Progress { get; private set; }
        public LocationProvider Location => _location;
        public int TotalPieces => _config.puzzleColumns * _config.puzzleRows;

        private void Awake()
        {
            Application.targetFrameRate = TARGET_FRAME_RATE;
            Progress = new PlayerProgress(SaveSystem.Load(_config.startingCoins));
            Progress.Changed += () => SaveSystem.Save(Progress.Data);
            _location.Initialize(_config.originLatitude, _config.originLongitude);
        }

        public double DistanceTo(SpotData spot)
        {
            return GeoUtil.DistanceMeters(_location.Latitude, _location.Longitude, spot.latitude, spot.longitude);
        }

        public float GetCooldownRemaining(SpotData spot)
        {
            return Progress.GetCooldownRemaining(spot.spotId, DateTime.UtcNow);
        }

        public SpotState GetSpotState(SpotData spot)
        {
            return SpotRules.Evaluate(spot.isFinalSpot, Progress.IsCleared, Progress.IsPuzzleComplete(TotalPieces),
                GetCooldownRemaining(spot), DistanceTo(spot), spot.captureRadiusMeters);
        }

        // 지금 잡을 수 있는 스팟 중 가장 가까운 곳. 없으면 null
        public SpotData FindCapturableSpot()
        {
            return FindNearest(SpotState.Capturable);
        }

        // 반경 밖이지만 도전 가능한 스팟 중 가장 가까운 곳 (힌트용)
        public SpotData FindNearestOpenSpot()
        {
            return FindNearest(SpotState.OutOfRange);
        }

        private SpotData FindNearest(SpotState state)
        {
            SpotData nearest = null;
            double nearestDistance = double.MaxValue;
            foreach (SpotData spot in _config.spots)
            {
                if (GetSpotState(spot) != state)
                {
                    continue;
                }
                double distance = DistanceTo(spot);
                if (distance < nearestDistance)
                {
                    nearest = spot;
                    nearestDistance = distance;
                }
            }
            return nearest;
        }

        public CaptureReward CompleteCapture(SpotData spot)
        {
            CaptureReward reward = new CaptureReward();
            reward.coins = _config.coinsPerCapture;
            Progress.AddCoins(reward.coins);
            reward.pieceIndex = Progress.CollectNextPiece(spot.puzzlePieceIndices ?? Array.Empty<int>());
            reward.isNewCard = Progress.RegisterCard(spot.character.characterId);
            Progress.StartCooldown(spot.spotId, DateTime.UtcNow, _config.successCooldownSeconds);
            reward.isFinal = spot.isFinalSpot;
            if (spot.isFinalSpot)
            {
                Progress.MarkCleared();
            }
            return reward;
        }

        public void FailCapture(SpotData spot)
        {
            Progress.StartCooldown(spot.spotId, DateTime.UtcNow, _config.failCooldownSeconds);
        }
    }
}
