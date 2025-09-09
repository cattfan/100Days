using UnityEngine;
using System.Linq;

public class TrashbinSpawner : MonoBehaviour
{
    [Header("Trashbin Prefab")]
    public GameObject trashbinPrefab;
    public int trashbinCount = 10; // số lượng muốn spawn

    [Header("Spawn Area")]
    public Vector2 areaSize = new Vector2(20f, 20f); // vùng spawn hình chữ nhật
    public Vector3 areaCenter = Vector3.zero;        // tâm vùng spawn
    public float checkRadius = 0.5f;                 // bán kính kiểm tra trùng

    [Header("Blocked Settings")]
    public string[] blockedTags = { "Wall", "Obstacle", "Tilemap" };
    public int maxAttemptsPerTrashbin = 20;

    private void Start()
    {
        SpawnTrashbins();
    }

    private void SpawnTrashbins()
    {
        int spawned = 0;

        for (int i = 0; i < trashbinCount; i++)
        {
            Vector3 spawnPos = GetValidSpawnPosition();
            if (spawnPos != Vector3.zero)
            {
                Instantiate(trashbinPrefab, spawnPos, Quaternion.identity);
                spawned++;
            }
        }

        Debug.Log($"Spawned {spawned}/{trashbinCount} trashbins.");
    }

    private Vector3 GetValidSpawnPosition()
    {
        for (int attempt = 0; attempt < maxAttemptsPerTrashbin; attempt++)
        {
            // random trong vùng chữ nhật
            float randX = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
            float randY = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
            Vector3 candidatePos = areaCenter + new Vector3(randX, randY, 0);

            Collider2D[] hits = Physics2D.OverlapCircleAll(candidatePos, checkRadius);
            bool blocked = false;

            foreach (var hit in hits)
            {
                if (hit != null && blockedTags.Any(tag => hit.CompareTag(tag)))
                {
                    blocked = true;
                    break;
                }
            }

            if (!blocked)
                return candidatePos; // tìm được chỗ hợp lệ
        }

        return Vector3.zero; // không tìm được
    }

    // Vẽ gizmo để debug vùng spawn
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(areaCenter, new Vector3(areaSize.x, areaSize.y, 0.1f));
    }
}
