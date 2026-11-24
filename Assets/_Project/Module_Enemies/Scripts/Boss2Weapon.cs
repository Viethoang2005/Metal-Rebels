using UnityEngine;

// Thả Pod. Boss2AI gọi trực tiếp, không cần Animation Event.
[DisallowMultipleComponent]
[RequireComponent(typeof(Boss2AI))]
public class Boss2Weapon : MonoBehaviour
{
    [Header("Pod")]
    public Boss2Pod podPrefab;
    public Transform podSpawnPoint;
    [Min(0f)] public float podSpacing = 1f;

    public void SpawnPods(int count)
    {
        if (podPrefab == null) return;
        Vector3 origin = podSpawnPoint != null ? podSpawnPoint.position : transform.position;
        for (int i = 0; i < count; i++)
        {
            float offset = (i - (count - 1) * 0.5f) * Mathf.Max(0f, podSpacing);
            Instantiate(podPrefab, origin + Vector3.right * offset, Quaternion.identity);
        }
    }
}
