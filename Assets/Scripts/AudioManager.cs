using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] List<AudioClip> soundTracks;
    private List<AudioClip> shuffledTracks;
    private Toggle MuteToggle;
    public AudioSource MainAudioSource, SFXAudioSource;
    private int currentTrackIndex = 0;

    void Awake()
    {
        Instance = this;
        shuffledTracks = new List<AudioClip>(soundTracks);
    }

    void Start()
    {
        ShuffleTracks();
        if (shuffledTracks.Count > 0)
        {
            PlayNextTrack();
        }
    }

    void Update()
    {
        if (!MainAudioSource.isPlaying)
        {
            PlayNextTrack();
        }
    }

    public void PlayNextTrack()
    {
        // play the track
        MainAudioSource.clip = shuffledTracks[currentTrackIndex];
        MainAudioSource.Play();

        currentTrackIndex++;
        if (currentTrackIndex >= shuffledTracks.Count)
        {
            // When all tracks have been played, reshuffle and start from the beginning
            currentTrackIndex = 0;
            ShuffleTracks();
        }
    }

    // Shuffle the tracks using Fisher-Yates shuffle
    private void ShuffleTracks()
    {
        for (int i = shuffledTracks.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            // swap values
            var temp = shuffledTracks[i];
            shuffledTracks[i] = shuffledTracks[j];
            shuffledTracks[j] = temp;
        }
    }
}

