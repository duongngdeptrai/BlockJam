using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SFXData
{
    public string name;
    public AudioClip clip;
}

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("SFX List")]
    public List<SFXData> sfxList;

    private AudioSource oneShotSource;
    private AudioSource loopSource;
    private Dictionary<string, AudioClip> sfxDictionary;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Tạo AudioSources
        oneShotSource = gameObject.AddComponent<AudioSource>();
        oneShotSource.playOnAwake = false;
        oneShotSource.loop = false;

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.loop = false;

        // Tạo dictionary
        sfxDictionary = new Dictionary<string, AudioClip>();

        foreach (var sfx in sfxList)
        {
            if (!sfxDictionary.ContainsKey(sfx.name))
            {
                sfxDictionary.Add(sfx.name, sfx.clip);
            }
        }

        HandleStateChanged(StateManager.CurrentState);
    }

    private void OnEnable()
    {
        StateManager.StateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        StateManager.StateChanged -= HandleStateChanged;
    }

    /// <summary>
    /// Play SFX based on sound name and loop setting
    /// </summary>
    public void Play(string soundName, bool isLoop = false, float volume = 1f)
    {
        if (!TryPlay(soundName, isLoop, volume))
        {
            Debug.LogWarning("Không tìm thấy SFX: " + soundName);
        }
    }

    /// <summary>
    /// Play One Shot (Deprecated: Use Play instead)
    /// </summary>
    public void PlayOneShot(string soundName, float volume = 1f)
    {
        Play(soundName, false, volume);
    }

    /// <summary>
    /// Play Loop (Deprecated: Use Play instead)
    /// </summary>
    public void PlayLoop(string soundName, float volume = 1f)
    {
        Play(soundName, true, volume);
    }

    /// <summary>
    /// Stop Loop
    /// </summary>
    public void StopLoop()
    {
        loopSource.Stop();
    }

    private void HandleStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Home:
                Play("music_home", true);
                break;
            case GameState.Playing:
                if (!TryPlay("music_playing", true))
                {
                    Play("music_play", true);
                }
                break;
            case GameState.Win:
                StopLoop();
                Play("win_game", false);
                break;
            case GameState.Lose:
                StopLoop();
                Play("lose_game", false);
                break;
        }
    }

    private bool TryPlay(string soundName, bool isLoop = false, float volume = 1f)
    {
        if (!sfxDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            return false;
        }

        if (isLoop)
        {
            // Nếu đang phát đúng clip thì bỏ qua
            if (loopSource.clip == clip && loopSource.isPlaying)
                return true;

            loopSource.Stop();
            loopSource.clip = clip;
            loopSource.volume = volume;
            loopSource.loop = true;
            loopSource.Play();
        }
        else
        {
            // Play one shot
            oneShotSource.PlayOneShot(clip, volume);
        }

        return true;
    }
}