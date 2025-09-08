// ItemPickupConfig.cs: Cấu hình cho item pickup system
using UnityEngine;

[CreateAssetMenu(fileName = "ItemPickupConfig", menuName = "Game/Item Pickup Config")]
public class ItemPickupConfig : ScriptableObject
{
    [Header("Visual Settings")]
    public float blinkSpeed = 8f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1.0f;

    [Header("Ready Effect")]
    public float readyEffectDuration = 0.3f;
    public float brightnesMultiplier = 1.3f;

    [Header("Physics")]
    public float defaultColliderRadius = 0.5f;

    [Header("Audio")]
    public bool playPickupSound = true;
    public bool playCannotPickupSound = true;
}