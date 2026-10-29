namespace MetalRebels.Core.Interfaces
{
    public enum PickupType
    {
        Health,         // Hồi máu (+30 HP)
        ShotgunAmmo,    // Băng đạn Shotgun (+20 viên)
        RifleAmmo,      // Băng đạn Súng trường (+30 viên)
        RocketWeapon,   // Khẩu Rocket mới (+1 viên)
        RocketAmmo,     // Đạn Rocket (+2 viên)
        Grenade         // Lựu đạn (+2 quả)
    }

    /// <summary>
    /// Hợp đồng giữa vật phẩm trên màn chơi (Module_Environment) và nhân vật Marco (Module_Player).
    /// Trả về true nếu nhân vật tiếp nhận thành công, false nếu từ chối (ví dụ: đã đầy đạn/máu).
    /// </summary>
    public interface IPickupCollector
    {
        bool TryCollect(PickupType pickupType, int amount);
    }
}

