namespace Treasure
{
    // 포획 성공 시 받은 보상 요약 (결과 팝업 표시용)
    public struct CaptureReward
    {
        public int coins;
        // 받은 퍼즐 조각 번호. 받지 못했으면 -1
        public int pieceIndex;
        public bool isNewCard;
        public bool isFinal;
    }
}
