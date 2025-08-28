using UnityEngine;
using System; // cần để dùng Guid

public static class Global_Helper
{
    public static string GenerateUniqueID(GameObject obj)
    {
        // Generate a unique ID using System.Guid
        return $"{obj.scene.name}_{obj.transform.position.x}_{obj.transform.position.y}_{Guid.NewGuid()}";
    }
}
