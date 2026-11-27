using UnityEngine;

namespace MetalRebels.Player
{
    public class HazardTrigger : MonoBehaviour
    {
        public int damage = 20;
        private float lastDamageTime;

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (Time.time - lastDamageTime < 1.0f) return;

            PlayerController player = collision.GetComponent<PlayerController>()
                ?? collision.GetComponentInParent<PlayerController>();
            if (player != null && !player.IsInvincible)
            {
                lastDamageTime = Time.time;
                player.TakeDamage(damage);
            }
        }
    }
}
