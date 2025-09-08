// ItemDropFactory.cs: Xử lý việc tạo item drops
using UnityEngine;

public static class ItemDropFactory
{
    public static GameObject CreateDrop(ItemData data, Vector3 position, GameObject itemPrefab = null)
    {
        if (!data.ShouldDrop())
        {
            return null;
        }

        GameObject droppedItem = CreateDroppedGameObject(data, position, itemPrefab);
        ConfigureDroppedItem(droppedItem, data);

        return droppedItem;
    }

    private static GameObject CreateDroppedGameObject(ItemData data, Vector3 position, GameObject itemPrefab)
    {
        GameObject droppedItem;

        if (itemPrefab != null)
        {
            droppedItem = Object.Instantiate(itemPrefab, position, Quaternion.identity);
        }
        else
        {
            droppedItem = new GameObject($"Item_{data.itemName}");
            droppedItem.transform.position = position;
            droppedItem.AddComponent<SpriteRenderer>();
            droppedItem.AddComponent<CircleCollider2D>();
            droppedItem.AddComponent<ItemPickup>();
            droppedItem.AddComponent<ItemPickupVisuals>();
            droppedItem.AddComponent<ItemPickupAudio>();
        }

        return droppedItem;
    }

    private static void ConfigureDroppedItem(GameObject droppedItem, ItemData data)
    {
        ItemPickup itemComponent = droppedItem.GetComponent<ItemPickup>();
        if (itemComponent != null)
        {
            itemComponent.itemData = data;

            if (data.isStackable)
            {
                itemComponent.currentAmount = data.GetRandomDropAmount();
            }
            else
            {
                itemComponent.currentAmount = 1;
            }

            Debug.Log($"Đã drop {data.itemName} x{itemComponent.currentAmount}");
        }
    }
}