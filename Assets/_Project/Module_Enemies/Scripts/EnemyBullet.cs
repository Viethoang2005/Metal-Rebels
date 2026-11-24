using MetalRebels.Core.Interfaces;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 20;
    public float lifeTime = 4f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 dir)
    {
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            IDamageable receiver = collision.GetComponentInParent<IDamageable>();
            if (receiver != null)
            {
                receiver.TakeDamage(damage);
                Debug.Log("Marco trúng đạn! Mất " + damage + " HP");
            }
            else
                Debug.LogWarning("Player cần component triển khai IDamageable để nhận sát thương.", collision);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
