using UnityEngine;

namespace Trashbin
{
    public partial class Trashbin : MonoBehaviour, IInteractable
    {
        public bool isChecked { get; private set; }
        public string TrashbinName { get; private set; }
        public GameObject FailInteractIcon;

        [Header("Stamina Settings")]
        public float staminaCost = 10f;
        [HideInInspector] public ThanhTheLucplayer playerStamina;

        [Header("Item System")]
        public ItemData[] itemDataList;
        public GameObject itemPickupPrefab;

        [Header("Spawn Settings")]
        public float spawnChance = 1f;
        public int minItems = 3;
        public int maxItems = 5;
        public float spawnRadius = 1.5f;
        public Vector3 spawnOffset = Vector3.down;

        [Header("Visual Settings")]
        public Sprite CheckedBin;
        public Sprite UncheckedBin;

        [Header("Reset Settings")]
        public float resetTime = 60f;
        public bool showResetTimer = true;

        [Header("Music")]
        public AudioManagement audioManagement;

        [HideInInspector] public Sprite originalSprite;
        [HideInInspector] public Coroutine resetCoroutine;

        private void Awake()
        {
            audioManagement = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManagement>();
        }

        void Start()
        {
            TrashbinName ??= Global_Helper.GenerateUniqueID(gameObject);

            FailInteractIcon.SetActive(false);
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                originalSprite = spriteRenderer.sprite;

            if (UncheckedBin == null)
                UncheckedBin = originalSprite;

            playerStamina = FindFirstObjectByType<ThanhTheLucplayer>();

            ValidateItemSetup();
        }

        private void ValidateItemSetup()
        {
            if (itemDataList == null || itemDataList.Length == 0)
            {
                Debug.LogError($"Trashbin '{TrashbinName}': No ItemData configured!");
            }
        }

        public void SetChecked(bool value)
        {
            isChecked = value;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                if (isChecked && CheckedBin != null)
                    spriteRenderer.sprite = CheckedBin;
                else if (!isChecked && UncheckedBin != null)
                    spriteRenderer.sprite = UncheckedBin;
            }
        }
    }
}
