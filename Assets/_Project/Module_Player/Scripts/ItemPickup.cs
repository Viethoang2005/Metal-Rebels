using MetalRebels.Core.Data;
using UnityEngine;

namespace MetalRebels.Player
{
    public enum PickupType
    {
        Health,             // Hồi 30 HP
        ShotgunAmmo,        // +20 viên shotgun
        AssaultRifleAmmo,   // +30 viên súng trường
        RocketWeapon,       // Mở khóa Rocket + 1-2 viên
        RocketAmmo,         // +2 viên rocket
        Grenade             // +2 quả lựu đạn
    }

    [RequireComponent(typeof(Collider2D))]
    public class ItemPickup : MonoBehaviour
    {
        private static Sprite fallbackPickupSprite;

        [Header("Loại vật phẩm & Giá trị")]
        [SerializeField] private PickupType pickupType = PickupType.Health;
        [SerializeField] private int amount = 30;
        [SerializeField] private WeaponData weaponDataReference; // Dành cho RocketWeapon nếu mở khóa vũ khí mới

        [Header("Hiệu ứng khi nhặt")]
        [SerializeField] private GameObject pickupEffectPrefab;
        [SerializeField] private AudioClip pickupSound;

        [Header("Hiệu ứng trôi nổi (Hover)")]
        [SerializeField] private bool enableHover = true;
        [SerializeField] private float hoverAmplitude = 0.15f;
        [SerializeField] private float hoverSpeed = 3f;

        private Vector3 startPosition;

        private void Awake()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite == null)
            {
                if (fallbackPickupSprite == null)
                {
                    fallbackPickupSprite = Sprite.Create(
                        Texture2D.whiteTexture,
                        new Rect(0f, 0f, 1f, 1f),
                        new Vector2(0.5f, 0.5f),
                        4f);
                }

                spriteRenderer.sprite = fallbackPickupSprite;
            }
        }

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            if (enableHover)
            {
                float newY = startPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
                transform.position = new Vector3(startPosition.x, newY, startPosition.z);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandlePickup(collision.gameObject);
        }

        private void HandlePickup(GameObject target)
        {
            PlayerController controller = target.GetComponent<PlayerController>() ?? target.GetComponentInParent<PlayerController>();
            PlayerCombat combat = target.GetComponent<PlayerCombat>() ?? target.GetComponentInParent<PlayerCombat>();

            if (controller == null && combat == null) return;

            bool success = false;

            switch (pickupType)
            {
                case PickupType.Health:
                    if (controller != null)
                    {
                        success = controller.TryHeal(amount > 0 ? amount : 30);
                    }
                    break;

                case PickupType.ShotgunAmmo:
                    if (combat != null)
                    {
                        success = combat.TryAddAmmo(WeaponType.Shotgun, amount > 0 ? amount : 20);
                    }
                    break;

                case PickupType.AssaultRifleAmmo:
                    if (combat != null)
                    {
                        success = combat.TryAddAmmo(WeaponType.AssaultRifle, amount > 0 ? amount : 30);
                    }
                    break;

                case PickupType.RocketWeapon:
                    if (combat != null)
                    {
                        if (weaponDataReference != null)
                        {
                            success = combat.TryAddWeapon(weaponDataReference, amount > 0 ? amount : 1);
                        }
                        else
                        {
                            success = combat.TryAddAmmo(WeaponType.RocketLauncher, amount > 0 ? amount : 1);
                        }
                    }
                    break;

                case PickupType.RocketAmmo:
                    if (combat != null)
                    {
                        success = combat.TryAddAmmo(WeaponType.RocketLauncher, amount > 0 ? amount : 2);
                    }
                    break;

                case PickupType.Grenade:
                    if (combat != null)
                    {
                        success = combat.TryAddGrenades(amount > 0 ? amount : 2);
                    }
                    break;
            }

            // Chỉ thu hồi item nếu nhặt thành công theo hợp đồng trong WORKFLOW.md
            if (success)
            {
                if (pickupEffectPrefab != null)
                {
                    Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                }
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                Destroy(gameObject);
            }
        }
    }
}
