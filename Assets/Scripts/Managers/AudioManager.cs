using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] List<AudioClip> soundTracks;
    private List<AudioClip> shuffledTracks;
    public AudioSource MainAudioSource, SFXAudioSource;
    private int currentTrackIndex = 0;

    PlayerData _data;

    void Awake()
    {
        Instance = this;
        shuffledTracks = new List<AudioClip>(soundTracks);
        _data = SaveSystem.LoadPlayerData();
    }

    void Start()
    {
        if (_data.TOTAL_XP <= 150)
        {
            // It's the first launch, play the specific song
            Debug.Log("First: " + _data.TOTAL_XP);
            MainAudioSource.clip = soundTracks[7];
            MainAudioSource.Play();
        }
        else
        {
            ShuffleTracks();
            if (shuffledTracks.Count > 0)
            {
                PlayNextTrack();
            }
        }
    }

    void Update()
    {
        var _data = GameManager.Instance.PlayerData;
        if (!MainAudioSource.isPlaying)
        {
            PlayNextTrack();

        }
    }

    public void PlayNextTrack()
    {
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
