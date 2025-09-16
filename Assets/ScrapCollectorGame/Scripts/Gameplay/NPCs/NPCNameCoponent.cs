using UnityEngine;
using TMPro;

[System.Serializable]
public class NPCNameComponent : MonoBehaviour
{
    [Header("NPC Information")]
    [SerializeField] public string npcName = "Unknown NPC";

    [Header("Name Display")]
    public TMP_Text nameDisplayText; // Kéo TextMeshPro component vào đây
    public bool showNameOnStart = false;
    public bool hideNameWhenFarAway = true;
    public float hideDistance = 2f;

    [Header("Optional Settings")]
    [TextArea(2, 4)]
    public string npcDescription = "";
    public Sprite npcPortrait;

    private Transform playerTransform;

    void Start()
    {
        // Nếu không có tên, sử dụng tên GameObject
        if (string.IsNullOrEmpty(npcName))
        {
            npcName = gameObject.name;
        }

        // Tìm player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // Setup name display
        if (nameDisplayText != null)
        {
            nameDisplayText.text = npcName;
            SetNameVisibility(showNameOnStart);
        }
    }

    void Update()
    {
        // Ẩn/hiện tên dựa trên khoảng cách
        if (hideNameWhenFarAway && playerTransform != null && nameDisplayText != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool shouldShow = distance <= hideDistance;

            // Chỉ thay đổi khi cần thiết để tránh lag
            if (nameDisplayText.gameObject.activeInHierarchy != shouldShow)
            {
                SetNameVisibility(shouldShow);
            }
        }
    }

    public void SetNameVisibility(bool visible)
    {
        if (nameDisplayText != null)
        {
            nameDisplayText.gameObject.SetActive(visible);
        }
    }

    public void ShowName()
    {
        SetNameVisibility(true);
    }

    public void HideName()
    {
        SetNameVisibility(false);
    }

    public void UpdateDisplayName()
    {
        if (nameDisplayText != null)
        {
            nameDisplayText.text = npcName;
        }
    }

    // Method để thay đổi tên runtime (nếu cần)
    public void SetNPCName(string newName)
    {
        npcName = newName;
        UpdateDisplayName();
    }

    // Getter methods
    public string GetNPCName()
    {
        return npcName;
    }

    public string GetNPCDescription()
    {
        return npcDescription;
    }

    public Sprite GetNPCPortrait()
    {
        return npcPortrait;
    }

    // Method để kiểm tra xem có text component không
    public bool HasNameDisplay()
    {
        return nameDisplayText != null;
    }
}