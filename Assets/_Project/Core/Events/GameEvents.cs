using System;

namespace MetalRebels.Core.Events
{
    public static class GameEvents
    {
        // Sự kiện Khóa màn hình (Event Lock)
        public static Action OnEventLockStart;
        public static Action OnEventLockCleared;

        // Sự kiện liên quan đến Boss
        public static Action OnBossSpawned;
        public static Action OnBossKilled;

        // Kết thúc màn chơi: true = Chiến thắng, false = Thất bại
        public static Action<bool> OnGameOver;

        // Tạm dừng trò chơi: true = Pause, false = Resume
        public static Action<bool> OnGamePaused;
    }
}