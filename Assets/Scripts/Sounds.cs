using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Sounds : MonoBehaviour
{
    public AudioClip[] sounds;

    // We use [SerializeField] so you can see the slot in the Inspector 
    // even if the variable is private.
    [SerializeField] private AudioSource _audioSrc;

    void Awake()
    {
        // Force the script to find the component on this object
        if (_audioSrc == null)
        {
            _audioSrc = GetComponent<AudioSource>();
        }
    }

    public void Playsound(AudioClip clip, float volume = 0.2f, bool destroyed = false, float p1 = 0.85f, float p2 = 1.2f)
    {
        // Final safety check
        if (_audioSrc == null)
        {
            _audioSrc = GetComponent<AudioSource>();
        }

        if (_audioSrc != null)
        {
            if (clip == null) return;
            _audioSrc.pitch = Random.Range(p1, p2);
            _audioSrc.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning($"CRITICAL: No AudioSource found on {gameObject.name} even after retry!");
        }
    }
}