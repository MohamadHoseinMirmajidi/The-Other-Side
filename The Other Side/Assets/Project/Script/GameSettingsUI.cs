using UnityEngine;
using UnityEngine.UI;

public class GameSettingsUI : MonoBehaviour
{
    private const string AudioEnabledKey = "Settings.AudioEnabled";
    private const string VolumeKey = "Settings.Volume";

    [Header("View References")]
    [SerializeField] private Button audioOnOffButton;
    [SerializeField] private Slider volumeSlider;

    [Header("First Launch Defaults")]
    [SerializeField] private bool defaultAudioEnabled = true;
    [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.5f;

    [Header("Simple On/Off Visual")]
    [SerializeField] private Color audioOnColor = Color.white;
    [SerializeField] private Color audioOffColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    private bool audioEnabled;
    private float volume;

    private void Awake()
    {
        LoadSavedValues();

        if (audioOnOffButton != null)
            audioOnOffButton.onClick.AddListener(ToggleAudio);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(SetVolumeFromView);
    }

    private void OnDestroy()
    {
        if (audioOnOffButton != null)
            audioOnOffButton.onClick.RemoveListener(ToggleAudio);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(SetVolumeFromView);
    }

    public void ToggleAudio()
    {
        audioEnabled = !audioEnabled;
        UpdateAudioButtonView();
        SaveValues();

        // VIEW -> DATA: later, send the audio enabled value to the real settings data here.
        ApplySettingsToAudioSystem();
    }

    private void SetVolumeFromView(float newVolume)
    {
        volume = newVolume;
        SaveValues();

        // VIEW -> DATA: later, send the slider value to the real settings data here.
        ApplySettingsToAudioSystem();
    }

    private void LoadSavedValues()
    {
        audioEnabled = PlayerPrefs.GetInt(AudioEnabledKey, defaultAudioEnabled ? 1 : 0) == 1;
        volume = PlayerPrefs.GetFloat(VolumeKey, defaultVolume);

        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(volume);

        UpdateAudioButtonView();

        // DATA -> VIEW/AUDIO: saved values are restored here when the game opens.
        ApplySettingsToAudioSystem();
    }

    private void SaveValues()
    {
        PlayerPrefs.SetInt(AudioEnabledKey, audioEnabled ? 1 : 0);
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    private void UpdateAudioButtonView()
    {
        if (audioOnOffButton == null || audioOnOffButton.targetGraphic == null)
            return;

        audioOnOffButton.targetGraphic.color = audioEnabled ? audioOnColor : audioOffColor;
    }

    private void ApplySettingsToAudioSystem()
    {
        GameAudioManager.Instance?.ApplySettings(audioEnabled, volume);
    }
}
