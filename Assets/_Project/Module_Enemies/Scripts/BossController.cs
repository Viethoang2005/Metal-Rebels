using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
   [RequireComponent(typeof(EnemyHealth))]
public class BossController : MonoBehaviour
{
    public bool isPhase2 = false;
    public float attackInterval = 2.5f;

    [Header("Vị trí nòng súng & Vũ khí")]
    public Transform machineGunPoint;
    public Transform cannonPoint;
    public GameObject normalBulletPrefab;
    public GameObject heavyMissilePrefab;
    public GameObject laserPrefab;

    [Header("Gọi đệ tử (Phase 2)")]
    public GameObject minionPrefab;
    public Transform spawnMinionPoint;

    private EnemyHealth bossHealth;
    private Transform player;

    void Start()
    {
        bossHealth = GetComponent<EnemyHealth>();
        bossHealth.OnHealthPercentChanged += CheckPhaseTransition;
        bossHealth.OnEnemyDied += () => Debug.Log("BOSS BỊ TIÊU DIỆT! QUA MÀN!");

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        StartCoroutine(BossAttackRoutine());
    }

    void CheckPhaseTransition(float healthPercent)
    {
        if (!isPhase2 && healthPercent <= 0.5f)
        {
            isPhase2 = true;
            attackInterval *= 0.6f;
            GetComponentInChildren<SpriteRenderer>().color = new Color(1f, 0.7f, 0.7f);
            Debug.Log("BOSS NỔI GIẬN - KÍCH HOẠT PHASE 2!");
        }
    }

    IEnumerator BossAttackRoutine()
    {
        while (bossHealth.currentHP > 0)
        {
            yield return new WaitForSeconds(attackInterval);
            if (player == null) continue;

            if (!isPhase2)
            {
                if (Random.Range(0, 2) == 0) yield return StartCoroutine(BurstMachineGun(3));
                else FireHeavyCannon();
            }
            else
            {
                int r = Random.Range(0, 3);
                if (r == 0) yield return StartCoroutine(BurstMachineGun(5));
                else if (r == 1) FireLaser();
                else SpawnMinion();
            }
        }
    }

    IEnumerator BurstMachineGun(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (normalBulletPrefab != null && machineGunPoint != null && player != null)
            {
                GameObject b = Instantiate(normalBulletPrefab, machineGunPoint.position, Quaternion.identity);
                b.GetComponent<EnemyBullet>().SetDirection(player.position - machineGunPoint.position);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    void FireHeavyCannon()
    {
        if (heavyMissilePrefab != null && cannonPoint != null && player != null)
        {
            GameObject m = Instantiate(heavyMissilePrefab, cannonPoint.position, Quaternion.identity);
            m.GetComponent<EnemyBullet>().SetDirection(player.position - cannonPoint.position);
        }
    }

    void FireLaser()
    {
        if (laserPrefab != null && cannonPoint != null)
        {
            GameObject l = Instantiate(laserPrefab, cannonPoint.position, Quaternion.identity);
            l.GetComponent<EnemyBullet>().SetDirection(Vector2.left);
        }
    }

    void SpawnMinion()
    {
        if (minionPrefab != null && spawnMinionPoint != null)
            Instantiate(minionPrefab, spawnMinionPoint.position, Quaternion.identity);
    }
}
