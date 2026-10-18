using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Central audio point for the game. Assign clips in the Inspector; gameplay
/// code only asks for a named sound and does not know where the file lives.
/// </summary>
public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Audio Sources (optional - created automatically if empty)")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource breathingSource;

    [Header("Shared Mixer")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioMixerGroup masterGroup;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip endingMusic;

    [Header("UI SFX")]
    [SerializeField] private AudioClip uiConfirm;
    [SerializeField] private AudioClip uiBack;

    [Header("Story SFX")]
    [SerializeField] private AudioClip dialogue;
    [SerializeField] private AudioClip paperPickup;
    [SerializeField] private AudioClip paperOpen;
    [SerializeField] private AudioClip doorOpen;
    [SerializeField] private AudioClip doorLocked;
    [SerializeField] private AudioClip boxPushLoop;
    [SerializeField] private AudioClip climb;
    [SerializeField] private AudioClip keyPickup;
    [SerializeField] private AudioClip toolboxSearch;
    [SerializeField] private AudioClip fuseRepair;
    [SerializeField] private AudioClip projectorPowerOn;
    [SerializeField] private AudioClip scare;
    [SerializeField] private AudioClip weightPickup;
    [SerializeField] private AudioClip ropeTie;
    [SerializeField] private AudioClip ropePull;
    [SerializeField] private AudioClip curtainOpen;
    [SerializeField] private AudioClip backpackPickup;

    [Header("Character SFX")]
    [SerializeField] private AudioClip walkFootstep;
    [SerializeField] private AudioClip runFootstep;
    [SerializeField] private AudioClip panting;
    [SerializeField] private AudioClip reliefBreath;

    [Header("Mix")]
    [SerializeField, Range(0f, 1f)] private float musicLevel = 0.75f;
    [SerializeField, Range(0f, 1f)] private float sfxLevel = 1f;
    [SerializeField] private float musicFadeDuration = 0.35f;

    private bool audioEnabled = true;
    private float masterVolume = 0.5f;
    private Coroutine musicRoutine;

    private void Awake()
    {
        Instance = this;
        PrepareSources();
        audioEnabled = PlayerPrefs.GetInt("Settings.AudioEnabled", 1) == 1;
        masterVolume = PlayerPrefs.GetFloat("Settings.Volume", 0.5f);
        ApplyVolumes();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ApplySettings(bool enabled, float master)
    {
        audioEnabled = enabled;
        masterVolume = Mathf.Clamp01(master);
        ApplyVolumes();
    }

    public void PlayMenuMusic() => PlayMusic(menuMusic);
    public void PlayGameplayMusic() => PlayMusic(gameplayMusic);
    public void PlayEndingMusic() => PlayMusic(endingMusic);

    public void PlayUIConfirm() => PlayUIOneShot(uiConfirm);
    public void PlayUIBack() => PlayUIOneShot(uiBack);
    public void PlayDialogue() => PlayOneShot(dialogue);
    public void PlayPaperPickup() => PlayOneShot(paperPickup);
    public void PlayPaperOpen() => PlayOneShot(paperOpen);
    public void PlayDoorOpen() => PlayOneShot(doorOpen);
    public void PlayDoorLocked() => PlayOneShot(doorLocked);
    public void PlayClimb() => PlayOneShot(climb);
    public void PlayKeyPickup() => PlayOneShot(keyPickup);
    public void PlayToolboxSearch() => PlayOneShot(toolboxSearch);
    public void PlayFuseRepair() => PlayOneShot(fuseRepair);
    public void PlayProjectorPowerOn() => PlayOneShot(projectorPowerOn);
    public void PlayScare() => PlayOneShot(scare);
    public void PlayWeightPickup() => PlayOneShot(weightPickup);
    public void PlayRopeTie() => PlayOneShot(ropeTie);
    public void PlayRopePull() => PlayOneShot(ropePull);
    public void PlayCurtainOpen() => PlayOneShot(curtainOpen);
    public void PlayBackpackPickup() => PlayOneShot(backpackPickup);
    public void PlayWalkFootstep() => PlayOneShot(walkFootstep);
    public void PlayRunFootstep() => PlayOneShot(runFootstep);
    public void PlayPanting() => PlayOneShot(panting);
    public void PlayReliefBreath() => PlayOneShot(reliefBreath);

    public void StartPanting()
    {
        if (breathingSource == null || panting == null || !audioEnabled)
            return;

        breathingSource.clip = panting;
        breathingSource.loop = true;
        breathingSource.Play();
    }

    public void StopPanting()
    {
        if (breathingSource == null)
            return;

        breathingSource.Stop();
        breathingSource.clip = null;
    }

    public void StartBoxPush()
    {
        if (sfxSource == null || boxPushLoop == null || sfxSource.clip == boxPushLoop)
            return;

        sfxSource.clip = boxPushLoop;
        sfxSource.loop = true;
        sfxSource.Play();
    }

    public void StopBoxPush()
    {
        if (sfxSource == null || sfxSource.clip != boxPushLoop)
            return;

        sfxSource.Stop();
        sfxSource.clip = null;
        sfxSource.loop = false;
    }

    private void PrepareSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (musicSource == null && sources.Length > 0)
            musicSource = sources[0];
        if (sfxSource == null && sources.Length > 1)
            sfxSource = sources[1];
        if (uiSource == null && sources.Length > 2)
            uiSource = sources[2];
        if (breathingSource == null && sources.Length > 3)
            breathingSource = sources[3];

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
        if (uiSource == null)
            uiSource = gameObject.AddComponent<AudioSource>();
        if (breathingSource == null)
            breathingSource = gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.ignoreListenerPause = true;
        musicSource.outputAudioMixerGroup = masterGroup;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.ignoreListenerPause = false;
        sfxSource.outputAudioMixerGroup = masterGroup;

        uiSource.playOnAwake = false;
        uiSource.loop = false;
        uiSource.spatialBlend = 0f;
        uiSource.ignoreListenerPause = true;
        uiSource.outputAudioMixerGroup = masterGroup;

        breathingSource.playOnAwake = false;
        breathingSource.loop = true;
        breathingSource.spatialBlend = 0f;
        breathingSource.ignoreListenerPause = false;
        breathingSource.outputAudioMixerGroup = masterGroup;
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null || musicSource.clip == clip)
            return;

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);
        musicRoutine = StartCoroutine(ChangeMusic(clip));
    }

    private IEnumerator ChangeMusic(AudioClip clip)
    {
        float duration = Mathf.Max(0f, musicFadeDuration);
        float startVolume = musicSource.volume;
        float elapsed = 0f;
        while (musicSource.isPlaying && elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        musicSource.clip = clip;
        musicSource.Play();

        elapsed = 0f;
        float targetVolume = GetMusicVolume();
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        musicRoutine = null;
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (sfxSource != null && clip != null && audioEnabled)
            sfxSource.PlayOneShot(clip);
    }

    private void PlayUIOneShot(AudioClip clip)
    {
        if (uiSource != null && clip != null && audioEnabled)
            uiSource.PlayOneShot(clip);
    }

    private void ApplyVolumes()
    {
        if (masterMixer != null && !string.IsNullOrWhiteSpace(masterVolumeParameter))
        {
            float mixerVolume = audioEnabled
                ? Mathf.Log10(Mathf.Max(0.0001f, masterVolume)) * 20f
                : -80f;
            masterMixer.SetFloat(masterVolumeParameter, mixerVolume);
        }

        bool mixerControlsMaster = masterMixer != null;
        float fallbackMaster = audioEnabled && !mixerControlsMaster
            ? masterVolume
            : audioEnabled ? 1f : 0f;
        if (musicSource != null)
            musicSource.volume = fallbackMaster * musicLevel;
        if (sfxSource != null)
            sfxSource.volume = fallbackMaster * sfxLevel;
        if (uiSource != null)
            uiSource.volume = fallbackMaster * sfxLevel;
        if (breathingSource != null)
            breathingSource.volume = fallbackMaster * sfxLevel;
    }

    private float GetMusicVolume()
    {
        return audioEnabled
            ? (masterMixer != null ? 1f : masterVolume) * musicLevel
            : 0f;
    }
}
