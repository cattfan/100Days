// ItemPickupAudio.cs: Xử lý âm thanh
using UnityEngine;

public class ItemPickupAudio : MonoBehaviour
{
    private AudioManagement audioManagement;

    private void Awake()
    {
        FindAudioManager();
    }

    private void FindAudioManager()
    {
        GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioObject != null)
        {
            audioManagement = audioObject.GetComponent<AudioManagement>();
        }
    }

    public void PlayPickupSound()
    {
        if (audioManagement != null)
        {
            audioManagement.PlaySFX(audioManagement.PickupItem);
        }
    }

    public void PlayCannotPickupSound()
    {
        audioManagement?.PlaySFX(audioManagement.CannotPickup);
    }
}