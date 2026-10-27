using UnityEngine;

namespace MetalRebels.Core.Data
{
    public enum WeaponType
    {
        Pistol,          // Súng lục (Mặc định)
        Shotgun,         // Shotgun
        AssaultRifle,    // Súng trường
        RocketLauncher,  // Súng phóng Rocket
        Grenade          // Lựu đạn
    }

    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "MetalRebels/Data/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [Header("Thông tin chung")]
        public WeaponType weaponType = WeaponType.Pistol;
        public string weaponName = "Default Weapon";
        public Sprite weaponIcon;
        public GameObject projectilePrefab; // Prefab đạn hoặc lựu đạn

        [Header("Chỉ số sát thương & Nhịp bắn")]
        public int damage = 20;               // Sát thương / viên hoặc vụ nổ
        public float fireRate = 2f;           // Số phát bắn tối đa trong 1 giây (VD: Súng trường = 5, Rocket = 0.5)
        public bool isAutomatic = false;      // Giữ nút bắn liên tục hay phải bấm nhấp từng phát

        [Header("Cơ chế Băng đạn & Đạn dự trữ")]
        public bool isInfiniteAmmo = false;   // True đối với Súng lục
        public int maxAmmo = 30;              // Đạn tối đa có thể mang (Shotgun: 30, Rifle: 120, Rocket: 5, Grenade: 5)
        public int magazineSize = 8;          // Số đạn trong 1 băng (Shotgun: 8, Rifle: 30, Rocket: 1)
        public float reloadTime = 1.5f;       // Thời gian nạp đạn (giây)
        public int ammoPerPickup = 20;        // Số đạn nhận được khi nhặt hộp đạn trong màn

        [Header("Đặc tính Đạn / Vật nổ (Rocket & Lựu đạn)")]
        public float bulletSpeed = 15f;       // Tốc độ bay của đạn/quả nổ
        public float bulletLifetime = 3f;     // Thời gian tồn tại của đạn
        public bool isExplosive = false;      // True đối với Rocket & Lựu đạn
        public float explosionRadius = 3f;    // Bán kính vụ nổ diện rộng (m) - Mặc định 3m
        public float fuseTime = 0f;           // Thời gian đếm ngược trước khi nổ (Lựu đạn = 3s, Rocket = 0s)
    }
}