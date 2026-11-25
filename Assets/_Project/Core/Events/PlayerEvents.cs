using System;

namespace MetalRebels.Core.Events
{
    
    public static class PlayerEvents
    {
        // Thông báo khi máu người chơi thay đổi: (Máu hiện tại, Máu tối đa)
        public static Action<int, int> OnHealthChanged;

        // Thông báo khi số mạng thay đổi: (Số mạng còn lại)
        public static Action<int> OnLivesChanged;

        // Thông báo khi người chơi mất 1 mạng hoặc chết hẳn
        public static Action OnPlayerDied;

        // Thông báo khi người chơi hồi sinh tại Checkpoint
        public static Action OnPlayerRespawned;

        // Thông báo khi thay đổi vũ khí: (Tên vũ khí, Số đạn hiện tại)
        public static Action<string, int> OnWeaponChanged;

        // Thông báo khi số lượng đạn thay đổi: (Đạn hiện tại, Đạn dự trữ)
        public static Action<int, int> OnAmmoChanged;

        // Thông báo khi số lựu đạn thay đổi: (Lựu đạn hiện tại, Tối đa)
        public static Action<int, int> OnGrenadeChanged;

        // Thông báo khi đang nạp đạn
        public static Action<bool> OnReloadingChanged;

        // Thông báo khi hết sạch mạng -> Game Over
        public static Action OnAllLivesLost;

        // Thông báo điểm số & hoàn thành màn chơi -> Victory
        public static Action<int> OnScoreChanged;
        public static Action OnMissionCompleted;

        // Các hàm helper gọi sự kiện an toàn cho toàn bộ các module
        public static void TriggerHealthChanged(int currentHp, int maxHp) => OnHealthChanged?.Invoke(currentHp, maxHp);
        public static void TriggerLivesChanged(int lives) => OnLivesChanged?.Invoke(lives);
        public static void TriggerPlayerDied() => OnPlayerDied?.Invoke();
        public static void TriggerPlayerRespawned() => OnPlayerRespawned?.Invoke();
        public static void TriggerWeaponChanged(string weaponName, int currentAmmo) => OnWeaponChanged?.Invoke(weaponName, currentAmmo);
        public static void TriggerAmmoChanged(int currentAmmo, int reserveAmmo) => OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
        public static void TriggerGrenadeChanged(int currentGrenades, int maxGrenades) => OnGrenadeChanged?.Invoke(currentGrenades, maxGrenades);
        public static void TriggerReloadingChanged(bool isReloading) => OnReloadingChanged?.Invoke(isReloading);
        public static void TriggerAllLivesLost() => OnAllLivesLost?.Invoke();
        public static void TriggerMissionCompleted() => OnMissionCompleted?.Invoke();

        // Sự kiện phát âm thanh hiệu ứng (SFX) & tiếng súng theo tên vũ khí
        public static Action<string> OnPlaySFX;
        public static Action<string> OnWeaponFired;

        public static void TriggerPlaySFX(string sfxName) => OnPlaySFX?.Invoke(sfxName);
        public static void TriggerWeaponFired(string weaponName) => OnWeaponFired?.Invoke(weaponName);

        // Trạng thái đấu Boss: true = gặp Boss (bật nhạc), false = tiêu diệt Boss (tắt nhạc)
        public static Action<bool> OnBossBattleChanged;

        public static void TriggerBossBattleChanged(bool isFighting) => OnBossBattleChanged?.Invoke(isFighting);
    }
}