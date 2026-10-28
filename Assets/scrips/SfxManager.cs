using UnityEngine;

/// <summary>
/// Put this on an empty GameObject called "SfxManager".
/// Other scripts call, for example: SfxManager.Instance.PlaySwing(position);
/// It plays the sound at that position with a small random pitch change.
/// </summary>
public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [Header("Sound clips (drag your sound files here)")]
    [SerializeField] private AudioClip swingClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip alertClip;
    [SerializeField] private AudioClip keyClip;

    [Header("Volumes (0 = silent, 1 = full)")]
    [SerializeField, Range(0f, 1f)] private float swingVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float hitVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float alertVolume = 0.9f;
    [SerializeField, Range(0f, 1f)] private float keyVolume = 0.8f;

    [Header("Variety")]
    [SerializeField] private float pitchVariation = 0.08f;   // 0.08 = pitch changes randomly between 92% and 108%

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySwing(Vector3 position) { Play(swingClip, position, swingVolume); }
    public void PlayHit(Vector3 position)   { Play(hitClip, position, hitVolume); }
    public void PlayAlert(Vector3 position) { Play(alertClip, position, alertVolume); }
    public void PlayKey(Vector3 position)   { Play(keyClip, position, keyVolume); }

    private void Play(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;   // no clip assigned yet: do nothing, no error

        // Make a temporary object that plays the sound and then removes itself.
        GameObject temp = new GameObject("TempSfx");
        temp.transform.position = position;

        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 1f;   // 1 = 3D sound: louder when close, quieter when far
        source.minDistance = 3f;
        source.maxDistance = 30f;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.Play();

        Destroy(temp, clip.length / source.pitch + 0.1f);
    }
}