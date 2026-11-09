using System.Collections;
using System.Collections.Generic;
using MetalRebels.Core.Data;
using MetalRebels.Core.Events;
using MetalRebels.Core.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("firePointForward")]
        [SerializeField] private Transform firePointForward1;
        [SerializeField] private Transform firePointUp;
        [SerializeField] private Transform firePointDown;
        [SerializeField] private Transform firePointCrouch;
        [FormerlySerializedAs("firePointShotgunForward")]
        [SerializeField] private Transform firePointForward2;

        [Header("Cấu hình Dữ liệu Vũ khí")]
        [SerializeField] private WeaponData pistolData;
        [SerializeField] private WeaponData shotgunData;
        [SerializeField] private WeaponData assaultRifleData;
        [SerializeField] private WeaponData rocketLauncherData;

        [Header("Lựu đạn")]
        [SerializeField] private GameObject grenadePrefab;
        [SerializeField] private Sprite grenadeSprite;
        [SerializeField] private int maxGrenades = 5;
        [SerializeField] private float grenadeThrowForceX = 5.5f;
        [SerializeField] private float grenadeThrowForceY = 5f;

        [Header("Hiệu ứng nổ dùng chung")]
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private Sprite[] explosionSprites;
        [SerializeField] private Sprite[] grenadeExplosionSprites;
        [SerializeField] private Sprite[] shotgunExplosionSprites;
        [SerializeField] private Texture2D grenadeExplosionSheet;
        [SerializeField] private Texture2D shotgunExplosionSheet;

        [Header("Hiệu ứng đạn bay")]
        [SerializeField] private Sprite bulletSprite;

        private PlayerController playerController;
        private PlayerInputHandler inputHandler;
        private PlayerAnimation playerAnimation;

        private List<WeaponSlot> weaponSlots = new List<WeaponSlot>();
        private int currentWeaponIndex = -1;
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

            if (firePointForward2 == null)
            {
                firePointForward2 = transform.Find("Firepoint_Forward_2");
                if (firePointForward2 == null)
                {
                    GameObject point = new GameObject("Firepoint_Forward_2");
                    firePointForward2 = point.transform;
                    firePointForward2.SetParent(transform, false);
                    Vector3 pistolPointPosition = firePointForward1 != null
                        ? firePointForward1.localPosition
                        : new Vector3(0.55f, 0.65f, 0f);
                    firePointForward2.localPosition = new Vector3(pistolPointPosition.x, 0.07f, pistolPointPosition.z);
                }
            }
            else if (firePointForward1 != null)
            {
                firePointForward2.localPosition = new Vector3(
                    firePointForward1.localPosition.x,
                    0.07f,
                    firePointForward1.localPosition.z);
            }
            if (firePointForward1 != null && firePointForward1.name == "Firepoint_Forward")
            {
                firePointForward1.name = "Firepoint_Forward_1";
            }

            if (grenadeExplosionSheet != null)
            {
                grenadeExplosionSprites = CreateSheetFrames(
                    grenadeExplosionSheet, 1621, 129, 72, 0, 129, 23, new Vector2(0.5f, 0f));
            }
            if (shotgunExplosionSheet != null)
            {
                shotgunExplosionSprites = CreateSheetFrames(
                    shotgunExplosionSheet, 2400, 1018, 200, 400, 190, 12, new Vector2(1f, 0.5f));
            }

            InitWeapons();
        }

        private static Sprite[] CreateSheetFrames(
            Texture2D sheet,
            int sourceWidth,
            int sourceHeight,
            int frameWidth,
            int frameY,
            int frameHeight,
            int frameCount,
            Vector2 framePivot)
        {
            float scaleX = sheet.width / (float)sourceWidth;
            float scaleY = sheet.height / (float)sourceHeight;
            Sprite[] frames = new Sprite[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                float x = i * frameWidth * scaleX;
                float y = frameY * scaleY;
                float width = Mathf.Min(frameWidth * scaleX, sheet.width - x);
                float height = Mathf.Min(frameHeight * scaleY, sheet.height - y);
                frames[i] = Sprite.Create(
                    sheet,
                    new Rect(x, y, width, height),
                    framePivot,
                    32f,
                    0,
                    SpriteMeshType.FullRect);
                frames[i].name = $"{sheet.name}_{i}";
            }

            return frames;
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
            Transform spawnPoint = GetSpawnPoint(aimDirection, weapon.weaponType == WeaponType.Shotgun);
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

            playerAnimation?.PlayShootAnimation();

            // Khởi tạo projectile
            if (weapon.weaponType == WeaponType.Shotgun)
            {
                FireShotgunExplosion(spawnPos, aimDirection, weapon);
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
                    bullet.Init(direction, weapon.damage, weapon.bulletSpeed, weapon.bulletLifetime, default, bulletSprite, 0.55f);
                }
            }
            else
            {
                // Fallback đạn mặc định
                CreateFallbackBullet(position, direction, weapon.damage, weapon.bulletSpeed, weapon.bulletLifetime, 0.55f);
            }
        }

        private void FireShotgunExplosion(Vector3 position, Vector2 direction, WeaponData weapon)
        {
            Vector2 forward = direction.normalized;
            int totalDamage = weapon.damage > 0 ? weapon.damage : 50;
            float rotation = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            SpawnShotgunBlastExplosion(position, Quaternion.Euler(0f, 0f, rotation), 1.5f, totalDamage);
        }

        private void SpawnShotgunBlastExplosion(
            Vector3 position,
            Quaternion rotation,
            float radius,
            int damage)
        {
            if (explosionPrefab != null)
            {
                GameObject expObj = Instantiate(explosionPrefab, position, rotation);
                ExplosionEffect eff = expObj.GetComponent<ExplosionEffect>();
                if (eff != null)
                {
                    eff.Init(
                        damage,
                        radius,
                        default,
                        visualSprites: shotgunExplosionSprites,
                        visualScale: 0.16f,
                        visualDuration: 0.24f,
                        visualDirection: Quaternion.Euler(0f, 0f, rotation.eulerAngles.z) * Vector2.right,
                        visualDistance: 3f);
                }
            }
            else
            {
                GameObject expObj = new GameObject("ShotgunBlast_Runtime");
                expObj.transform.position = position;
                expObj.transform.rotation = rotation;
                SpriteRenderer spriteRenderer = expObj.AddComponent<SpriteRenderer>();
                spriteRenderer.sortingOrder = 10;
                if (shotgunExplosionSprites != null && shotgunExplosionSprites.Length > 0)
                {
                    spriteRenderer.sprite = shotgunExplosionSprites[0];
                    spriteRenderer.color = Color.white;
                }

                ExplosionEffect eff = expObj.AddComponent<ExplosionEffect>();
                eff.Init(
                    damage,
                    radius,
                    default,
                    visualSprites: shotgunExplosionSprites,
                    visualScale: 0.16f,
                    visualDuration: 0.24f,
                    visualDirection: Quaternion.Euler(0f, 0f, rotation.eulerAngles.z) * Vector2.right,
                    visualDistance: 3f);
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

        private void CreateFallbackBullet(Vector3 position, Vector2 direction, int damage, float speed, float lifetime, float visualScale = 1f)
        {
            GameObject bulletObj = new GameObject("PlayerBullet_Runtime");
            bulletObj.transform.position = position;
            CircleCollider2D col = bulletObj.AddComponent<CircleCollider2D>();
            col.radius = 0.15f;
            col.isTrigger = true;
            SpriteRenderer sr = bulletObj.AddComponent<SpriteRenderer>();
            sr.sprite = bulletSprite;
            sr.color = Color.white;
            sr.sortingOrder = 5;
            PlayerBullet bullet = bulletObj.AddComponent<PlayerBullet>();
            bullet.Init(direction, damage, speed, lifetime, default, bulletSprite, visualScale);
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

            if (slot.weaponData.weaponType == WeaponType.Shotgun)
            {
                playerAnimation?.PlayChargeAnimation();
            }

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

                Transform spawn = firePointForward1 != null ? firePointForward1 : transform;
                Vector2 throwVelocity = new Vector2(playerController.FacingDirection * grenadeThrowForceX, grenadeThrowForceY);

                if (grenadePrefab != null)
                {
                    GameObject grenadeObj = Instantiate(grenadePrefab, spawn.position, Quaternion.identity);
                    PlayerGrenade grenade = grenadeObj.GetComponent<PlayerGrenade>();
                    if (grenade != null)
                    {
                        grenade.Init(throwVelocity, 30, 3f, 3f, explosionPrefab, grenadeSprite, grenadeExplosionSprites, 0.22f);
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
                    grenade.Init(throwVelocity, 30, 3f, 3f, explosionPrefab, grenadeSprite, grenadeExplosionSprites, 0.22f);
                }
            }
        }

        private void HandleWeaponSwitch()
        {
            int slotSelected = inputHandler.WeaponSlotSelected;
            if (slotSelected >= 0 && slotSelected <= (int)WeaponType.RocketLauncher)
            {
                WeaponType weaponType = (WeaponType)slotSelected;
                for (int i = 0; i < weaponSlots.Count; i++)
                {
                    if (weaponSlots[i].weaponData != null && weaponSlots[i].weaponData.weaponType == weaponType)
                    {
                        EquipWeaponByIndex(i);
                        return;
                    }
                }
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
                if (CanEquipWeapon(slot))
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
            if (!CanEquipWeapon(slot)) return;
            if (currentWeaponIndex == index)
            {
                PublishWeaponState(slot);
                return;
            }

            // Hủy nạp đạn của vũ khí trước
            if (isReloading)
            {
                if (reloadCoroutine != null) StopCoroutine(reloadCoroutine);
                isReloading = false;
                PlayerEvents.OnReloadingChanged?.Invoke(false);
            }

            currentWeaponIndex = index;
            PublishWeaponState(slot);

            if (slot.weaponData != null && slot.weaponData.weaponType == WeaponType.Shotgun)
            {
                playerAnimation?.PlayChargeAnimation();
            }
        }

        private static void PublishWeaponState(WeaponSlot slot)
        {
            int displayClip = slot.weaponData.isInfiniteAmmo ? -1 : slot.currentClip;
            int displayReserve = slot.weaponData.isInfiniteAmmo ? -1 : slot.reserveAmmo;

            PlayerEvents.OnWeaponChanged?.Invoke(slot.weaponData.weaponName, displayClip);
            PlayerEvents.OnAmmoChanged?.Invoke(displayClip, displayReserve);
        }

        private static bool CanEquipWeapon(WeaponSlot slot)
        {
            return slot != null
                && slot.weaponData != null
                && slot.isUnlocked
                && (slot.weaponData.isInfiniteAmmo || slot.currentClip + slot.reserveAmmo > 0);
        }

        public bool TryAddAmmo(WeaponType weaponType, int amount)
        {
            if (amount <= 0) return false;

            foreach (var slot in weaponSlots)
            {
                if (slot.weaponData == null) continue;
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
                if (weaponSlots[i].weaponData != null && weaponSlots[i].weaponData.weaponType == newWeapon.weaponType)
                {
                    WeaponSlot slot = weaponSlots[i];
                    slot.isUnlocked = true;
                    slot.reserveAmmo = Mathf.Min(
                        slot.reserveAmmo + Mathf.Max(0, initialAmmo),
                        Mathf.Max(0, newWeapon.maxAmmo - slot.currentClip));
                    if (slot.currentClip == 0 && slot.reserveAmmo > 0)
                    {
                        int load = Mathf.Min(newWeapon.magazineSize, weaponSlots[i].reserveAmmo);
                        slot.currentClip += load;
                        slot.reserveAmmo -= load;
                    }
                    EquipWeaponByIndex(i);
                    return true;
                }
            }

            // Nếu là vũ khí hoàn toàn mới chưa có trong list
            WeaponSlot newSlot = new WeaponSlot(newWeapon, true, 0, Mathf.Max(0, initialAmmo));
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

        private Transform GetSpawnPoint(Vector2 direction, bool isShotgun = false)
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

            if (isShotgun && firePointForward2 != null)
            {
                return firePointForward2;
            }

            return firePointForward1 != null ? firePointForward1 : transform;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            if (firePointForward1 != null) Gizmos.DrawWireSphere(firePointForward1.position, 0.1f);
            if (firePointForward2 != null) Gizmos.DrawWireSphere(firePointForward2.position, 0.1f);
            if (firePointUp != null) Gizmos.DrawWireSphere(firePointUp.position, 0.1f);
            if (firePointDown != null) Gizmos.DrawWireSphere(firePointDown.position, 0.1f);
            if (firePointCrouch != null) Gizmos.DrawWireSphere(firePointCrouch.position, 0.1f);
        }
    }
}
