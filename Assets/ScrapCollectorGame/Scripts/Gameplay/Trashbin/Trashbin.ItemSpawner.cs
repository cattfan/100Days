using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Trashbin
{
    public partial class Trashbin : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private LayerMask blockedLayers; // layer bị cấm spawn (Tilemap, tường...)

        private void SpawnItemsWithItemData()
        {
            List<ItemData> shuffledList = itemDataList.OrderBy(x => Random.value).ToList();
            int itemCount = Random.Range(minItems, Mathf.Min(maxItems + 1, shuffledList.Count));
            int actualSpawnedCount = 0;

            for (int i = 0; i < itemCount; i++)
            {
                ItemData selectedItemData = shuffledList[i];
                if (selectedItemData != null)
                {
                    Vector3 targetPosition = GetValidSpawnPosition();
                    if (targetPosition == Vector3.zero) continue; // không tìm được chỗ hợp lệ thì bỏ qua

                    GameObject droppedItem = ItemDropFactory.CreateDrop(selectedItemData, transform.position, itemPickupPrefab);
                    if (droppedItem != null)
                    {
                        actualSpawnedCount++;
                        StartCoroutine(ItemFlyOutAnimation(droppedItem, targetPosition, actualSpawnedCount * 0.1f));
                    }
                }
            }

            if (actualSpawnedCount == 0)
                ShowFailIcon();
        }

        private Vector3 GetValidSpawnPosition()
        {
            int maxAttempts = 100;     // số lần thử tối đa
            float checkRadius = 0.3f; // bán kính kiểm tra
            string[] blockedTags = { "Wall","Tilemap" }; // danh sách tag cấm

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
                Vector3 candidatePos = transform.position + spawnOffset + new Vector3(randomOffset.x, randomOffset.y, 0);

                // Lấy tất cả collider trong vùng kiểm tra
                Collider2D[] hits = Physics2D.OverlapCircleAll(candidatePos, checkRadius);
                bool blocked = false;

                foreach (var hit in hits)
                {
                    if (hit != null && blockedTags.Any(tag => hit.CompareTag(tag)))
                    {
                        blocked = true; // trúng đối tượng bị cấm
                        break;
                    }
                }

                if (!blocked)
                {
                    return candidatePos; // vị trí hợp lệ
                }
            }

            return Vector3.zero; // không tìm được chỗ hợp lệ
        }

        // Vẽ gizmo debug để thấy vùng kiểm tra
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + spawnOffset, spawnRadius);
        }
    }
}
