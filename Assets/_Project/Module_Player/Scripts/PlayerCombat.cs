using System.Collections;
using System.Collections.Generic;
using MetalRebels.Core.Data;
using MetalRebels.Core.Events;
using UnityEngine;

namespace MetalRebels.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [System.Serializable]
        public class WeaponSlot
        {
            public WeaponData weaponData;
            public bool isUnlocked = false;
            public int currentClip = 0;
            public int reserveAmmo = 0;

            public WeaponSlot(WeaponData data, bool unlocked, int clip, int reserve)
            {
                weaponData = data;
                isUnlocked = unlocked;
                currentClip = clip;
                reserveAmmo = reserve;
            }
        }

        [Header("Điểm bắn đạn")]
        [SerializeField] private Transform firePointForward;
        [SerializeField] private Transform firePointUp;
        [SerializeField] private Transform firePointDown;
        [SerializeField] private Transform firePointCrouch;

        [Header("Cấu hình Dữ liệu Vũ khí")]
        [SerializeField] private WeaponData pistolData;
        [SerializeField] private WeaponData shotgunData;
        [SerializeField] private WeaponData assaultRifleData;
        [SerializeField] private WeaponData rocketLauncherData;

        [Header("Lựu đạn")]
        [SerializeField] private GameObject grenadePrefab;
        [SerializeField] private Sprite grenadeSprite;
        [SerializeField] private int maxGrenades = 5;
        [SerializeField] private float grenadeThrowForceX = 10f;
        [SerializeField] private float grenadeThrowForceY = 6f;

        [Header("Hiệu ứng nổ dùng chung")]
        [SerializeField] private GameObject explosionPrefab;

        private PlayerController playerController;
        private PlayerInputHandler inputHandler;
        private PlayerAnimation playerAnimation;

        private List<WeaponSlot> weaponSlots = new List<WeaponSlot>();
        private int currentWeaponIndex = 0;
        private int currentGrenades;
        private float nextFireTime;
        private bool isReloading = false;
        private Coroutine reloadCoroutine;

        public WeaponSlot CurrentWeaponSlot => (weaponSlots.Count > currentWeaponIndex && currentWeaponIndex >= 0) ? weaponSlots[currentWeaponIndex] : null;
        public WeaponData CurrentWeapon => CurrentWeaponSlot?.weaponData;
        public bool IsShotgunEquipped => CurrentWeapon?.weaponType == WeaponType.Shotgun;
        public int CurrentGrenades => currentGrenades;
        public bool IsReloading => isReloading;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            playerAnimation = GetComponent<PlayerAnimation>();
            inputHandler = GetComponent<PlayerInputHandler>();
            if (inputHandler == null)
            {
                inputHandler = gameObject.AddComponent<PlayerInputHandler>();
            }

            InitWeapons();
        }

        private void Start()
        {
            currentGrenades = maxGrenades;
            EquipWeaponByIndex(0);
            PlayerEvents.OnGrenadeChanged?.Invoke(currentGrenades, maxGrenades);
        }

        private void InitWeapons()
        {
            weaponSlots.Clear();

            // 0: Pistol (Luôn mở khóa, vô hạn đạn)
            if (pistolData != null)
            {
                weaponSlots.Add(new WeaponSlot(pistolData, true, 1, -1));
            }
            else
            {
                // Fallback nếu inspector chưa gán ScriptableObject
                WeaponData defaultPistol = ScriptableObject.CreateInstance<WeaponData>();
                defaultPistol.weaponType = WeaponType.Pistol;
                defaultPistol.weaponName = "Handgun";
                defaultPistol.damage = 20;
                defaultPistol.fireRate = 2f;
                defaultPistol.isInfiniteAmmo = true;
                weaponSlots.Add(new WeaponSlot(defaultPistol, true, 1, -1));
            }

            // 1: Shotgun (Mở khóa khi nhặt súng hoặc đạn)
            if (shotgunData != null)
            {
                weaponSlots.Add(new WeaponSlot(shotgunData, false, 0, 0));
            }

            // 2: Assault Rifle
            if (assaultRifleData != null)
            {
                weaponSlots.Add(new WeaponSlot(assaultRifleData, false, 0, 0));
            }

            // 3: Rocket Launcher
            if (rocketLauncherData != null)
            {
                weaponSlots.Add(new WeaponSlot(rocketLauncherData, false, 0, 0));
            }
        }

        private void Update()
        {
            if (!playerController.IsAlive) return;

            HandleWeaponSwitch();
            HandleShooting();
            HandleReloadInput();
            HandleGrenade();
        }

        private void HandleShooting()
        {
            WeaponSlot slot = CurrentWeaponSlot;
            if (slot == null || slot.weaponData == null) return;

            // Người chơi không thể bắn khi đang bất tử tạm thời (theo đề cương)
            if (!playerController.CanShoot) return;
            if (isReloading) return;

            bool shootTriggered = inputHandler.AttackPressed;
            bool shootHeld = inputHandler.AttackHeld;

            // Bắn theo nhịp: nhấn phát bắn ngay, hoặc giữ nút bắn theo nhịp tối đa
            if ((shootTriggered || shootHeld) && Time.time >= nextFireTime)
            {
                if (slot.weaponData.isInfiniteAmmo)
                {
                    FireCurrentWeapon();
                }
                else
                {
                    if (slot.currentClip > 0)
                    {
                        FireCurrentWeapon();
                    }
                    else if (slot.reserveAmmo > 0)
                    {
                        // Hết đạn trong băng: tự động nạp đạn
                        StartReload();
                    }
                    else
                    {
                        // Hết sạch đạn dự trữ: tự động quay về súng lục
                        EquipWeaponByIndex(0);
                    }
                }
            }
        }

        private void FireCurrentWeapon()
        {
            WeaponSlot slot = CurrentWeaponSlot;
            WeaponData weapon = slot.weaponData;

            Vector2 aimDirection = GetAimDirection();
            Transform spawnPoint = GetSpawnPoint(aimDirection);
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

            playerAnimation?.PlayShootAnimation();

            // Khởi tạo projectile
            if (weapon.weaponType == WeaponType.Shotgun)
            {
                FireShotgunSpread(spawnPos, aimDirection, weapon);
            }
            else if (weapon.weaponType == WeaponType.RocketLauncher)
            {
                FireRocket(spawnPos, aimDirection, weapon);
            }
            else
            {
                FireStandardBullet(spawnPos, aimDirection, weapon);
            }

            nextFireTime = Time.time + (1f / Mathf.Max(0.1f, weapon.fireRate));

            // Trừ đạn trong băng
            if (!weapon.isInfiniteAmmo)
            {
                slot.currentClip--;
                PlayerEvents.OnAmmoChanged?.Invoke(slot.currentClip, slot.reserveAmmo);

                if (slot.currentClip <= 0)
                {
                    if (slot.reserveAmmo > 0)
                    {
                        StartReload();
                    }
                    else
                    {
                        EquipWeaponByIndex(0);
                    }
                }
            }
        }

        private void FireStandardBullet(Vector3 position, Vector2 direction, WeaponData weapon)
        {
            if (weapon.projectilePrefab != null)
            {
                GameObject bulletObj = Instantiate(weapon.projectilePrefab, position, Quaternion.identity);
                PlayerBullet bullet = bulletObj.GetComponent<PlayerBullet>();
                if (bullet != null)
                {
                    bullet.Init(direction, weapon.damage, weapon.bulletSpeed, weapon.bulletLifetime);
                }
            }
            else
            {
                // Fallback đạn mặc định
                CreateFallbackBullet(position, direction, weapon.damage, weapon.bulletSpeed, weapon.bulletLifetime);
            }
        }

        private void FireShotgunSpread(Vector3 position, Vector2 direction, WeaponData weapon)
        {
            // Bắn chùm 5 mảnh đạn tỏa góc nón (-12, -6, 0, 6, 12 độ)
            float[] spreadAngles = { -12f, -6f, 0f, 6f, 12f };
            int pelletDamage = Mathf.Max(1, weapon.damage / spreadAngles.Length); // Chia đều hoặc 10/pellet

            foreach (float angle in spreadAngles)
            {
                Vector2 spreadDir = Quaternion.Euler(0, 0, angle) * direction;

                if (weapon.projectilePrefab != null)
                {
                    GameObject bulletObj = Instantiate(weapon.projectilePrefab, position, Quaternion.identity);
                    PlayerBullet bullet = bulletObj.GetComponent<PlayerBullet>();
                    if (bullet != null)
                    {
                        bullet.Init(spreadDir, pelletDamage, weapon.bulletSpeed, weapon.bulletLifetime);
                    }
                }
                else
                {
                    CreateFallbackBullet(position, spreadDir, pelletDamage, weapon.bulletSpeed, weapon.bulletLifetime);
                }
            }
        }

        private void FireRocket(Vector3 position, Vector2 direction, WeaponData weapon)
        {
            if (weapon.projectilePrefab != null)
            {
                GameObject rocketObj = Instantiate(weapon.projectilePrefab, position, Quaternion.identity);
                PlayerRocket rocket = rocketObj.GetComponent<PlayerRocket>();
                if (rocket != null)
                {
                    rocket.Init(direction, weapon.damage, weapon.bulletSpeed, weapon.bulletLifetime, weapon.explosionRadius, explosionPrefab);
                }
            }
            else
            {
                CreateFallbackRocket(position, direction, weapon.damage, weapon.bulletSpeed, weapon.bulletLifetime, weapon.explosionRadius);
            }
        }

        private void CreateFallbackBullet(Vector3 position, Vector2 direction, int damage, float speed, float lifetime)
        {
            GameObject bulletObj = new GameObject("PlayerBullet_Runtime");
            bulletObj.transform.position = position;
            CircleCollider2D col = bulletObj.AddComponent<CircleCollider2D>();
            col.radius = 0.15f;
            col.isTrigger = true;
            PlayerBullet bullet = bulletObj.AddComponent<PlayerBullet>();
            bullet.Init(direction, damage, speed, lifetime);
        }

        private void CreateFallbackRocket(Vector3 position, Vector2 direction, int damage, float speed, float lifetime, float radius)
        {
            GameObject rocketObj = new GameObject("PlayerRocket_Runtime");
            rocketObj.transform.position = position;
            CircleCollider2D col = rocketObj.AddComponent<CircleCollider2D>();
            col.radius = 0.2f;
            col.isTrigger = true;
            PlayerRocket rocket = rocketObj.AddComponent<PlayerRocket>();
            rocket.Init(direction, damage, speed, lifetime, radius, explosionPrefab);
        }

        private void HandleReloadInput()
        {
            if (inputHandler.ReloadPressed && !isReloading)
            {
                WeaponSlot slot = CurrentWeaponSlot;
                if (slot != null && !slot.weaponData.isInfiniteAmmo && slot.currentClip < slot.weaponData.magazineSize && slot.reserveAmmo > 0)
                {
                    StartReload();
                }
            }
        }

        private void StartReload()
        {
            if (isReloading) return;
            WeaponSlot slot = CurrentWeaponSlot;
            if (slot == null || slot.weaponData.isInfiniteAmmo || slot.reserveAmmo <= 0) return;

            if (reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
            }
            reloadCoroutine = StartCoroutine(ReloadRoutine(slot));
        }

        private IEnumerator ReloadRoutine(WeaponSlot slot)
        {
            isReloading = true;
            PlayerEvents.OnReloadingChanged?.Invoke(true);

            yield return new WaitForSeconds(slot.weaponData.reloadTime);

            int needed = slot.weaponData.magazineSize - slot.currentClip;
            int transfer = Mathf.Min(needed, slot.reserveAmmo);
            slot.currentClip += transfer;
            slot.reserveAmmo -= transfer;

            isReloading = false;
            PlayerEvents.OnReloadingChanged?.Invoke(false);
            PlayerEvents.OnAmmoChanged?.Invoke(slot.currentClip, slot.reserveAmmo);

            reloadCoroutine = null;
        }

        private void HandleGrenade()
        {
            if (inputHandler.GrenadePressed && currentGrenades > 0 && playerController.CanShoot)
            {
                currentGrenades--;
                PlayerEvents.OnGrenadeChanged?.Invoke(currentGrenades, maxGrenades);
                playerAnimation?.PlayGrenadeAnimation();

                Transform spawn = firePointForward != null ? firePointForward : transform;
                Vector2 throwVelocity = new Vector2(playerController.FacingDirection * grenadeThrowForceX, grenadeThrowForceY);

                if (grenadePrefab != null)
                {
                    GameObject grenadeObj = Instantiate(grenadePrefab, spawn.position, Quaternion.identity);
                    PlayerGrenade grenade = grenadeObj.GetComponent<PlayerGrenade>();
                    if (grenade != null)
                    {
                        grenade.Init(throwVelocity, 30, 3f, 3f, explosionPrefab, grenadeSprite);
                    }
                }
                else
                {
                    // Fallback tạo grenade runtime
                    GameObject grenadeObj = new GameObject("PlayerGrenade_Runtime");
                    grenadeObj.transform.position = spawn.position;
                    CircleCollider2D col = grenadeObj.AddComponent<CircleCollider2D>();
                    col.radius = 0.2f;
                    Rigidbody2D rb = grenadeObj.AddComponent<Rigidbody2D>();
                    rb.gravityScale = 2.5f;
                    PlayerGrenade grenade = grenadeObj.AddComponent<PlayerGrenade>();
                    grenade.Init(throwVelocity, 30, 3f, 3f, explosionPrefab, grenadeSprite);
                }
            }
        }

        private void HandleWeaponSwitch()
        {
            int slotSelected = inputHandler.WeaponSlotSelected;
            if (slotSelected >= 0 && slotSelected < weaponSlots.Count)
            {
                EquipWeaponByIndex(slotSelected);
            }
            else if (inputHandler.NextWeaponPressed)
            {
                CycleWeapon(1);
            }
            else if (inputHandler.PrevWeaponPressed)
            {
                CycleWeapon(-1);
            }
        }

        private void CycleWeapon(int direction)
        {
            int count = weaponSlots.Count;
            if (count <= 1) return;

            int nextIndex = currentWeaponIndex;
            for (int i = 0; i < count; i++)
            {
                nextIndex = (nextIndex + direction + count) % count;
                WeaponSlot slot = weaponSlots[nextIndex];
                if (slot.isUnlocked && (slot.weaponData.isInfiniteAmmo || (slot.currentClip + slot.reserveAmmo) > 0))
                {
                    EquipWeaponByIndex(nextIndex);
                    return;
                }
            }
        }

        public void EquipWeaponByIndex(int index)
        {
            if (index < 0 || index >= weaponSlots.Count) return;

            WeaponSlot slot = weaponSlots[index];
            if (!slot.isUnlocked && !slot.weaponData.isInfiniteAmmo) return;

            // Hủy nạp đạn của vũ khí trước
            if (isReloading)
            {
                if (reloadCoroutine != null) StopCoroutine(reloadCoroutine);
                isReloading = false;
                PlayerEvents.OnReloadingChanged?.Invoke(false);
            }

            currentWeaponIndex = index;

            int displayClip = slot.weaponData.isInfiniteAmmo ? -1 : slot.currentClip;
            int displayReserve = slot.weaponData.isInfiniteAmmo ? -1 : slot.reserveAmmo;

            PlayerEvents.OnWeaponChanged?.Invoke(slot.weaponData.weaponName, displayClip);
            PlayerEvents.OnAmmoChanged?.Invoke(displayClip, displayReserve);
        }

        public bool TryAddAmmo(WeaponType weaponType, int amount)
        {
            foreach (var slot in weaponSlots)
            {
                if (slot.weaponData.weaponType == weaponType)
                {
                    slot.isUnlocked = true;
                    int maxReserve = slot.weaponData.maxAmmo;
                    int currentTotal = slot.currentClip + slot.reserveAmmo;
                    if (currentTotal >= maxReserve) return false;

                    slot.reserveAmmo = Mathf.Min(slot.reserveAmmo + amount, maxReserve - slot.currentClip);

                    // Nếu băng hiện tại trống và chưa cầm, nạp ngay vào băng
                    if (slot.currentClip == 0 && slot.reserveAmmo > 0)
                    {
                        int load = Mathf.Min(slot.weaponData.magazineSize, slot.reserveAmmo);
                        slot.currentClip += load;
                        slot.reserveAmmo -= load;
                    }

                    // Tự động chuyển sang vũ khí vừa nhặt đạn nếu đang dùng pistol
                    if (currentWeaponIndex == 0)
                    {
                        EquipWeaponByIndex(weaponSlots.IndexOf(slot));
                    }
                    else if (CurrentWeaponSlot == slot)
                    {
                        PlayerEvents.OnAmmoChanged?.Invoke(slot.currentClip, slot.reserveAmmo);
                    }

                    return true;
                }
            }
            return false;
        }

        public bool TryAddWeapon(WeaponData newWeapon, int initialAmmo)
        {
            if (newWeapon == null) return false;

            for (int i = 0; i < weaponSlots.Count; i++)
            {
                if (weaponSlots[i].weaponData.weaponType == newWeapon.weaponType)
                {
                    weaponSlots[i].isUnlocked = true;
                    weaponSlots[i].reserveAmmo = Mathf.Min(weaponSlots[i].reserveAmmo + initialAmmo, newWeapon.maxAmmo);
                    if (weaponSlots[i].currentClip == 0)
                    {
                        int load = Mathf.Min(newWeapon.magazineSize, weaponSlots[i].reserveAmmo);
                        weaponSlots[i].currentClip += load;
                        weaponSlots[i].reserveAmmo -= load;
                    }
                    EquipWeaponByIndex(i);
                    return true;
                }
            }

            // Nếu là vũ khí hoàn toàn mới chưa có trong list
            WeaponSlot newSlot = new WeaponSlot(newWeapon, true, 0, initialAmmo);
            int clipLoad = Mathf.Min(newWeapon.magazineSize, newSlot.reserveAmmo);
            newSlot.currentClip = clipLoad;
            newSlot.reserveAmmo -= clipLoad;
            weaponSlots.Add(newSlot);
            EquipWeaponByIndex(weaponSlots.Count - 1);
            return true;
        }

        public bool TryAddGrenades(int amount)
        {
            if (currentGrenades >= maxGrenades) return false;

            currentGrenades = Mathf.Min(currentGrenades + amount, maxGrenades);
            PlayerEvents.OnGrenadeChanged?.Invoke(currentGrenades, maxGrenades);
            return true;
        }

        public Vector2 GetAimDirection()
        {
            // Khi đang ngồi: bắn thấp về phía trước
            if (playerController.IsCrouching)
            {
                return new Vector2(playerController.FacingDirection, 0f).normalized;
            }

            // Khi đang ở trên không (nhảy/rơi)
            if (!playerController.IsGrounded)
            {
                if (inputHandler.IsAimingDown)
                {
                    return Vector2.down;
                }
                if (inputHandler.IsAimingUp)
                {
                    return Vector2.up;
                }
                return new Vector2(playerController.FacingDirection, 0f).normalized;
            }

            // Khi đang đứng / chạy trên mặt đất
            if (inputHandler.IsAimingUp)
            {
                return Vector2.up;
            }
            if (inputHandler.IsAimingDown)
            {
                return Vector2.down;
            }

            return new Vector2(playerController.FacingDirection, 0f).normalized;
        }

        private Transform GetSpawnPoint(Vector2 direction)
        {
            if (playerController.IsCrouching && firePointCrouch != null)
            {
                return firePointCrouch;
            }

            if (direction == Vector2.up && firePointUp != null)
            {
                return firePointUp;
            }

            if (direction == Vector2.down && firePointDown != null)
            {
                return firePointDown;
            }

            return firePointForward != null ? firePointForward : transform;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            if (firePointForward != null) Gizmos.DrawWireSphere(firePointForward.position, 0.1f);
            if (firePointUp != null) Gizmos.DrawWireSphere(firePointUp.position, 0.1f);
            if (firePointDown != null) Gizmos.DrawWireSphere(firePointDown.position, 0.1f);
            if (firePointCrouch != null) Gizmos.DrawWireSphere(firePointCrouch.position, 0.1f);
        }
    }
}
