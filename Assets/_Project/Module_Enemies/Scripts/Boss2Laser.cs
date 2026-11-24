using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Laser phase 2: vòng nạp báo trước tại X của Marco, sau đó tia laser đánh dọc xuống.
[DisallowMultipleComponent]
[RequireComponent(typeof(Boss2AI))]
public class Boss2Laser : MonoBehaviour
{
    [Header("Vòng nạp báo trước")]
    public GameObject warningPrefab;
    [Min(0.1f)] public float warningDuration = 1.2f;

    [Header("Tụ tia tại lõi boss (chạy suốt thời gian nạp)")]
    [Tooltip("Prefab FX tụ (các vòng co vào lõi). Độ dài clip nên bằng warningDuration.")]
    public GameObject chargeEffectPrefab;
    [Tooltip("Điểm đặt FX tụ. Trống = vị trí boss.")]
    public Transform chargePoint;

    [Header("Tia laser")]
    [Tooltip("Prefab cần Collider2D (Is Trigger) và script Boss2LaserBeam.")]
    public Boss2LaserBeam beamPrefab;
    [Min(0.1f)] public float beamDuration = 1f;
    [Min(0)] public int damage = 45;
    [Tooltip("Điểm tia chui ra: đặt ngay cửa cannon. Trống = vị trí boss.")]
    public Transform beamOrigin;
    [Tooltip("Bật: tia bắn thẳng xuống từ beamOrigin. Tắt: tia rơi từ mép trên camera xuống chỗ Marco.")]
    public bool fireFromOrigin = true;
    [Tooltip("Chỉ dùng khi tắt Fire From Origin: khoảng nhô lên khỏi mép trên camera.")]
    [Min(0f)] public float beamTopMargin = 2f;
    [Tooltip("Layer của mặt đất, để đặt vòng cảnh báo đúng chỗ tia chạm sàn.")]
    public LayerMask groundMask = ~0;

    private readonly List<GameObject> spawned = new List<GameObject>();

    // Boss2AI chờ coroutine này xong mới chọn đòn tiếp theo.
    public IEnumerator FireRoutine(Transform target)
    {
        if (beamPrefab == null)
        {
            Debug.LogWarning("Boss2Laser chưa gán Beam Prefab nên không bắn được tia.", this);
            yield break;
        }
        if (target == null) yield break;

        Vector3 origin = beamOrigin != null ? beamOrigin.position : transform.position;

        // Khóa vị trí ngay lúc bắt đầu nạp để người chơi có thể chạy khỏi.
        float x = fireFromOrigin ? origin.x : target.position.x;
        Vector2 groundPoint = fireFromOrigin ? FindGroundBelow(origin, target.position.y) : (Vector2)target.position;

        if (warningPrefab != null)
        {
            GameObject warning = Instantiate(warningPrefab, groundPoint, Quaternion.identity);
            spawned.Add(warning);
            if (warning.GetComponent<AutoDestroyFX>() == null)
                Destroy(warning, warningDuration);
        }

        // FX tụ chạy đúng bằng thời gian nạp, rồi biến mất khi tia bắn ra.
        if (chargeEffectPrefab != null)
        {
            Vector3 chargePos = chargePoint != null ? chargePoint.position : origin;
            GameObject charge = Instantiate(chargeEffectPrefab, chargePos, Quaternion.identity);
            spawned.Add(charge);
            if (charge.GetComponent<AutoDestroyFX>() == null)
                Destroy(charge, warningDuration);
        }

        yield return new WaitForSeconds(warningDuration);

        float topY = fireFromOrigin ? origin.y : ResolveTopY();
        Boss2LaserBeam beam = Instantiate(beamPrefab, new Vector3(x, topY, 0f), Quaternion.identity);
        beam.damage = damage;
        spawned.Add(beam.gameObject);
        Destroy(beam.gameObject, beamDuration);
        yield return new WaitForSeconds(beamDuration);
    }

    // Bắn tia xuống từ origin để tìm sàn. Không trúng gì thì lấy cao độ của Marco.
    Vector2 FindGroundBelow(Vector3 origin, float fallbackY)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, 50f, groundMask);
        foreach (RaycastHit2D hit in hits)
        {
            for (Transform t = hit.collider.transform; t != null; t = t.parent)
                if (t.CompareTag("Ground")) return hit.point;
        }
        return new Vector2(origin.x, fallbackY);
    }

    float ResolveTopY()
    {
        Camera cam = Camera.main;
        if (cam != null && cam.orthographic)
            return cam.transform.position.y + cam.orthographicSize + Mathf.Max(0f, beamTopMargin);
        return transform.position.y + 6f;
    }

    // Dùng khi boss đổi phase để xóa laser đang chờ.
    public void CancelAll()
    {
        StopAllCoroutines();
        foreach (GameObject go in spawned)
            if (go != null) Destroy(go);
        spawned.Clear();
    }
}
