using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class SoundEntry
    {
        public string id;
        public AudioClip clip;
    }

    public SoundEntry[] sounds;
    private AudioSource sfxSource;
    private AudioSource musicSource;
    private Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    void Awake()
    {
        Instance = this;
        var sources = GetComponents<AudioSource>();
        musicSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        sfxSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();

        foreach (var s in sounds)
            if (s.clip != null) clips[s.id] = s.clip;
    }

    public void Play(string id)
    {
        if (clips.TryGetValue(id, out var clip))
            sfxSource.PlayOneShot(clip);
    }

    public void PlayMusic(string id)
    {
        if (clips.TryGetValue(id, out var clip))
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }
}