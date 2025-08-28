using UnityEngine;

public class AudioManagement : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource MusicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("Audio Clips")]
    public AudioClip Background;
    public AudioClip PickupItem;
    public AudioClip SuccessTrashbinInteract;
    public AudioClip FailTrashbinInteract;
    public AudioClip Walking;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MusicSource.clip = Background;
        MusicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
