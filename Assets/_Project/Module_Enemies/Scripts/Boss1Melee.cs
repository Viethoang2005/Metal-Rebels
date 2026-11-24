using System.Collections;
using UnityEngine;
using MetalRebels.Core.Interfaces;

// Attach to the same object as Animator. PunchHit is an Animation Event.
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator), typeof(EnemyHealth))]
public class Boss1Melee : MonoBehaviour
{
    public Transform punchPoint;
    [Min(0f)] public float punchRange = 2.5f;
    [Min(0.01f)] public float hitRadius = 0.6f;
    [Min(0)] public int damage = 25;
    [Min(0.01f)] public float cooldown = 2.5f;
    [Min(0f)] public float recoveryTime = 0.6f;

    [Header("Hiệu ứng khi đấm trúng")]
    public GameObject punchHitEffectPrefab;
    [Tooltip("Chỉ dùng nếu prefab chưa có AutoDestroyFX.")]
    [Min(0.01f)] public float hitEffectLifetime = 0.5f;
    public bool IsBusy { get; private set; }

    private Animator anim;
    private EnemyHealth health;
    private Transform victim;
    private bool hitConsumed;
    private bool hasTrigger;
    private float nextPunchTime;
    private static readonly int Punch = Animator.StringToHash("Punch");

    void Awake()
    {
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        if (anim.runtimeAnimatorController == null) return;
        foreach (var parameter in anim.parameters)
            if (parameter.nameHash == Punch && parameter.type == AnimatorControllerParameterType.Trigger)
                hasTrigger = true;
        if (!hasTrigger) Debug.LogWarning("Boss1Melee: add the Punch trigger to Animator.", this);
    }

    public bool TryBeginPunch(Transform target)
    {
        if (!isActiveAndEnabled || !anim.isActiveAndEnabled || !hasTrigger || IsBusy ||
            health.currentHP <= 0 || punchPoint == null || target == null ||
            Time.time < nextPunchTime ||
            Vector2.Distance(transform.position, target.position) > punchRange) return false;

        victim = target;
        hitConsumed = false;
        IsBusy = true;
        nextPunchTime = Time.time + cooldown;
        anim.SetTrigger(Punch);
        StartCoroutine(WaitForPunch());
        return true;
    }

    IEnumerator WaitForPunch()
    {
        float deadline = Time.time + 1f;
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Boss1_Punch") && Time.time < deadline)
            yield return null;

        bool entered = anim.GetCurrentAnimatorStateInfo(0).IsName("Boss1_Punch");
        if (!entered)
            Debug.LogWarning("Boss1Melee: Animator did not enter Boss1_Punch. Check the Punch transition.", this);

        // Safety timeout prevents a broken/looping Animator state locking the AI forever.
        deadline = Time.time + 10f;
        while (entered && anim.GetCurrentAnimatorStateInfo(0).IsName("Boss1_Punch") && Time.time < deadline)
            yield return null;

        hitConsumed = true;
        anim.ResetTrigger(Punch);
        yield return new WaitForSeconds(recoveryTime);
        victim = null;
        IsBusy = false;
    }

    public void PunchHit()
    {
        if (!isActiveAndEnabled || !IsBusy || hitConsumed || health.currentHP <= 0 ||
            !anim.GetCurrentAnimatorStateInfo(0).IsName("Boss1_Punch")) return;
        hitConsumed = true;
        if (victim == null || !victim.gameObject.activeInHierarchy || punchPoint == null) return;

        // Only the selected player can be hit; multiple player colliders still take one hit.
        IDamageable receiver = victim.GetComponentInParent<IDamageable>();
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(punchPoint.position, hitRadius))
        {
            if (hit.transform.IsChildOf(transform)) continue;
            bool belongsToVictim = hit.transform == victim || hit.transform.IsChildOf(victim) ||
                (receiver != null && ReferenceEquals(hit.GetComponentInParent<IDamageable>(), receiver));
            if (!belongsToVictim) continue;
            // A physical hit can show FX even on the test player without health.
            SpawnHitEffect(hit);
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null) damageable.TakeDamage(damage);
            else Debug.LogWarning("Punch hit Player, but no IDamageable health component was found.", hit);
            return;
        }
    }

    void SpawnHitEffect(Collider2D hit)
    {
        if (punchHitEffectPrefab == null) return;
        Vector2 contact = hit.ClosestPoint(punchPoint.position);
        Vector3 position = new Vector3(contact.x, contact.y, punchPoint.position.z);
        GameObject effect = Instantiate(punchHitEffectPrefab, position, punchHitEffectPrefab.transform.rotation);
        if (effect.GetComponent<AutoDestroyFX>() == null)
            Destroy(effect, Mathf.Max(0.01f, hitEffectLifetime));
    }

    void OnDisable()
    {
        StopAllCoroutines();
        IsBusy = false;
        hitConsumed = true;
        victim = null;
        if (anim != null && hasTrigger) anim.ResetTrigger(Punch);
    }

    void OnDrawGizmosSelected()
    {
        if (punchPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(punchPoint.position, hitRadius);
    }
}
