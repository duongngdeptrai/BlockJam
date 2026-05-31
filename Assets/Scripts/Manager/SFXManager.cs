using System.Collections.Generic;
using UnityEngine;

public class SFXManager : Singleton<SFXManager>
{
    [Header("SFX List")]
    public List<SFXData> sfxList;

    private AudioSource oneShotSource;
    private AudioSource loopSource;
    private Dictionary<string, AudioClip> sfxDictionary;

    private void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        DontDestroyOnLoad(gameObject);

        if (sfxList == null)
        {
            Debug.LogError("SFXManager: sfxList is null! Assign SFX entries in Inspector.", this);
        }

        oneShotSource = gameObject.AddComponent<AudioSource>();
        oneShotSource.playOnAwake = false;
        oneShotSource.loop = false;

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.loop = false;

        sfxDictionary = new Dictionary<string, AudioClip>();
        if (sfxList != null)
        {
            foreach (var sfx in sfxList)
            {
                if (sfx == null)
                {
                    Debug.LogWarning("SFXManager: Skipping null SFXData entry in sfxList.", this);
                    continue;
                }
                if (!sfxDictionary.ContainsKey(sfx.name))
                {
                    sfxDictionary.Add(sfx.name, sfx.clip);
                }
            }
        }

        HandleStateChanged(GameStateMachine.CurrentState);
    }

    private void OnEnable()
    {
        GameStateMachine.StateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateMachine.StateChanged -= HandleStateChanged;
    }

    public void Play(string soundName, bool isLoop = false, float volume = 1f)
    {
        if (!TryPlay(soundName, isLoop, volume))
        {
            Debug.LogWarning($"SFXManager: SFX not found: '{soundName}'. Add it to sfxList in Inspector.", this);
        }
    }

    public void PlayOneShot(string soundName, float volume = 1f)
    {
        Play(soundName, false, volume);
    }

    public void PlayLoop(string soundName, float volume = 1f)
    {
        Play(soundName, true, volume);
    }

    public void StopLoop()
    {
        if (loopSource != null)
        {
            loopSource.Stop();
        }
        else
        {
            Debug.LogWarning("SFXManager: loopSource is null! Cannot stop loop.", this);
        }
    }

    private void HandleStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Home:
                Play(AudioKeys.MusicHome, true);
                break;
            case GameState.Playing:
                if (!TryPlay(AudioKeys.MusicPlaying, true))
                {
                    Play(AudioKeys.MusicPlay, true);
                }
                break;
            case GameState.Win:
                StopLoop();
                Play(AudioKeys.WinGame, false);
                break;
            case GameState.Lose:
                StopLoop();
                Play(AudioKeys.LoseGame, false);
                break;
        }
    }

    private bool TryPlay(string soundName, bool isLoop = false, float volume = 1f)
    {
        if (sfxDictionary == null)
        {
            Debug.LogError("SFXManager: sfxDictionary is null! Cannot play sound.", this);
            return false;
        }

        if (!sfxDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            return false;
        }

        if (isLoop)
        {
            if (loopSource == null)
            {
                Debug.LogError("SFXManager: loopSource is null! Cannot play loop.", this);
                return false;
            }

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
            if (oneShotSource == null)
            {
                Debug.LogError("SFXManager: oneShotSource is null! Cannot play one-shot.", this);
                return false;
            }
            oneShotSource.PlayOneShot(clip, volume);
        }

        return true;
    }
}
