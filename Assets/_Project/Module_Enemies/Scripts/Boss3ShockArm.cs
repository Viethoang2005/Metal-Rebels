using System.Collections;
using System.Collections.Generic;
using MetalRebels.Core.Interfaces;
using UnityEngine;

// Chiêu 3 - Xung Điện (Anti-Melee). Chỉ dùng khi Marco đứng quá sát (Boss3AI.antiMeleeRange).
// Báo hiệu: cánh tay co lại tích điện xanh trong chargeTime (0.5s).
// Ra đòn: luồng sét ngang tầm thấp phía trước boss. Lùi ra ngoài tầm hoặc nhảy cao để né.
[DisallowMultipleComponent]
[RequireComponent(typeof(Boss3AI))]
public class Boss3ShockArm : MonoBehaviour
{
    [Header("Điểm phát")]
    [Tooltip("Gốc luồng sét (đặt ở cánh tay, sát mặt đất). Child của boss để lật theo hướng mặt.")]
    public Transform armPoint;

    [Header("Báo hiệu")]
    [Min(0.1f)] public float chargeTime = 0.5f;
    [Tooltip("FX tích điện xanh tại cánh tay.")]
    public GameObject chargeEffectPrefab;

    [Header("Luồng sét")]
    [Tooltip("Prefab hiển thị luồng sét, vẽ hướng theo mặt gốc của boss. Sinh làm con của armPoint nên tự lật.")]
    public GameObject lightningPrefab;
    [Min(0.05f)] public float activeTime = 0.35f;
    [Tooltip("Tầm với phía trước armPoint.")]
    [Min(0.1f)] public float reach = 4.5f;
    [Tooltip("Chiều cao vùng sát thương. Giữ thấp để nhảy cao là né được.")]
    [Min(0.05f)] public float height = 0.9f;
    [Tooltip("Dời vùng sát thương lên/xuống so với armPoint.")]
    public float verticalOffset = 0f;
    [Tooltip("Lùi vùng sát thương ra sau armPoint (để trúng cả Marco đứng sát thân).")]
    [Min(0f)] public float backReach = 0.5f;
    [Min(0)] public int damage = 40;

    [Header("Hồi chiêu")]
    [Tooltip("Thời gian tối thiểu giữa hai lần Xung Điện, tránh khóa người chơi liên tục.")]
    [Min(0f)] public float cooldown = 3f;
    [Min(0f)] public float recoveryTime = 0.5f;

    public bool IsReady { get { return Time.time >= nextReadyTime; } }
    public bool IsActive { get; private set; }

    private Animator anim;
    private float nextReadyTime;
    private int facing = 1;
    private readonly List<GameObject> spawned = new List<GameObject>();
    private readonly HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();

    // Animator (tùy chọn): Bool isShockCharging, Trigger Shock
    private static readonly int ChargingParam = Animator.StringToHash("isShockCharging");
    private static readonly int ShockParam = Animator.StringToHash("Shock");
    private bool hasCharging, hasShock;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        hasCharging = Boss3AI.HasParam(anim, ChargingParam, AnimatorControllerParameterType.Bool);
        hasShock = Boss3AI.HasParam(anim, ShockParam, AnimatorControllerParameterType.Trigger);
        if (armPoint == null)
            Debug.LogWarning("Boss3ShockArm chưa gán Arm Point: luồng sét sẽ phát từ gốc boss.", this);
    }

    public IEnumerator FireRoutine(Boss3AI ai)
    {
        float speed = ai.ActionSpeed;
        facing = ai.FacingSign;
        nextReadyTime = Time.time + cooldown;
        alreadyHit.Clear();

        // --- Báo hiệu ---
        if (anim != null && hasCharging) anim.SetBool(ChargingParam, true);
        GameObject charge = Boss3AI.SpawnFx(chargeEffectPrefab, ArmPosition(), armPoint, 0f);
        if (charge != null) spawned.Add(charge);
        yield return new WaitForSeconds(Mathf.Max(0.1f, chargeTime) / speed);
        if (charge != null) { spawned.Remove(charge); Destroy(charge); }
        if (anim != null && hasCharging) anim.SetBool(ChargingParam, false);

        // --- Ra đòn ---
        IsActive = true;
        if (anim != null && hasShock) anim.SetTrigger(ShockParam);
        if (lightningPrefab != null)
        {
            Transform parent = armPoint != null ? armPoint : transform;
            GameObject bolt = Instantiate(lightningPrefab, parent);
            bolt.transform.localPosition = lightningPrefab.transform.localPosition;
            bolt.transform.localRotation = lightningPrefab.transform.localRotation;
            spawned.Add(bolt);
        }
        for (float t = 0f; t < activeTime; t += Time.deltaTime)
        {
            DamageZone();
            yield return null;
        }
        IsActive = false;
        ClearSpawned();

        // --- Hồi chiêu ---
        if (recoveryTime > 0f) yield return new WaitForSeconds(recoveryTime / speed);
        nextReadyTime = Mathf.Max(nextReadyTime, Time.time + cooldown * 0.5f);
    }

    void DamageZone()
    {
        GetZone(out Vector2 center, out Vector2 size);
        foreach (Collider2D col in Physics2D.OverlapBoxAll(center, size, 0f))
        {
            if (!Boss3AI.IsTagged(col.transform, "Player")) continue;
            IDamageable receiver = col.GetComponentInParent<IDamageable>();
            if (receiver != null && alreadyHit.Add(receiver)) receiver.TakeDamage(Mathf.Max(0, damage));
        }
    }

    void GetZone(out Vector2 center, out Vector2 size)
    {
        Vector2 origin = ArmPosition();
        float back = Mathf.Max(0f, backReach);
        float length = Mathf.Max(0.1f, reach) + back;
        size = new Vector2(length, Mathf.Max(0.05f, height));
        center = origin + new Vector2(facing * (length * 0.5f - back), verticalOffset);
    }

    Vector2 ArmPosition()
    {
        return armPoint != null ? (Vector2)armPoint.position : (Vector2)transform.position;
    }

    void ClearSpawned()
    {
        foreach (GameObject go in spawned)
            if (go != null) Destroy(go);
        spawned.Clear();
    }

    public void CancelAll()
    {
        IsActive = false;
        ClearSpawned();
        alreadyHit.Clear();
        if (anim != null && hasCharging) anim.SetBool(ChargingParam, false);
        if (anim != null && hasShock) anim.ResetTrigger(ShockParam);
    }

    void OnDisable()
    {
        CancelAll();
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Boss3AI ai = GetComponent<Boss3AI>();
            bool right = (transform.TransformVector(Vector3.right).x >= 0f) != (ai != null && ai.spriteFacesLeft);
            facing = right ? 1 : -1;
        }
        GetZone(out Vector2 center, out Vector2 size);
        Gizmos.color = IsActive ? Color.red : new Color(0.3f, 0.6f, 1f);
        Gizmos.DrawWireCube(center, size);
    }
}
