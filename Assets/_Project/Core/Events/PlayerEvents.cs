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
    }
}