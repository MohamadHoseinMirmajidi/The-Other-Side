using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("First Selected Buttons")]
    [SerializeField] private Button mainMenuFirstButton;
    [SerializeField] private Button settingsFirstButton;
    [SerializeField] private Button pauseFirstButton;

    [Header("Panel Animation")]
    [SerializeField] private float transitionDuration = 0.2f;
    [SerializeField] private float hiddenScale = 0.94f;

    [Header("Button Selection")]
    [SerializeField] private Color normalButtonColor = new Color(1f, 1f, 1f, 0.72f);
    [SerializeField] private Color selectedButtonColor = new Color(1f, 0.78f, 0.28f, 1f);
    [SerializeField] private Color pressedButtonColor = new Color(0.82f, 0.58f, 0.18f, 1f);

    private readonly Dictionary<GameObject, CanvasGroup> canvasGroups =
        new Dictionary<GameObject, CanvasGroup>();

    private readonly Dictionary<GameObject, Vector3> panelScales =
        new Dictionary<GameObject, Vector3>();

    private readonly Dictionary<GameObject, Coroutine> panelAnimations =
        new Dictionary<GameObject, Coroutine>();

    private GameObject settingsReturnPanel;
    private GameObject activePanel;
    private Button activeFirstButton;
    private bool gameStarted;
    private bool isPaused;
    private bool settingsOpen;

    private void Awake()
    {
        if (gameProgress == null)
            gameProgress = FindAnyObjectByType<GameProgress>();

        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        PreparePanel(mainMenuPanel);
        PreparePanel(settingsPanel);
        PreparePanel(pausePanel);

        ConfigureButtons(mainMenuPanel);
        ConfigureButtons(settingsPanel);
        ConfigureButtons(pausePanel);

        HideImmediate(settingsPanel);
        HideImmediate(pausePanel);

        if (GameProgress.ConsumeStartGameplayAfterReload())
        {
            HideImmediate(mainMenuPanel);
            gameStarted = true;
            SetGameplayPaused(false);
            activePanel = null;
            activeFirstButton = null;
        }
        else
        {
            ShowImmediate(mainMenuPanel);
            SetGameplayPaused(true);
            activePanel = mainMenuPanel;
            activeFirstButton = mainMenuFirstButton;
        }
    }

    private void Start()
    {
        if (!gameStarted)
            SelectButton(mainMenuFirstButton);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.escapeKey.wasPressedThisFrame)
            HandleCancel();

        if (!UIButtonSelectionSync.IsPointerOverButton)
        {
            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
                MoveSelection(-1);

            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
                MoveSelection(1);
        }

        if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
            ChangeSelectedSlider(-1f);

        if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
            ChangeSelectedSlider(1f);

        if (keyboard.enterKey.wasPressedThisFrame ||
            keyboard.numpadEnterKey.wasPressedThisFrame ||
            keyboard.spaceKey.wasPressedThisFrame)
        {
            SubmitSelection();
        }
    }

    public void StartGame()
    {
        if (gameStarted)
            return;

        gameStarted = true;
        settingsOpen = false;
        HidePanel(settingsPanel);
        HidePanel(mainMenuPanel);
        SetGameplayPaused(false);
        activePanel = null;
        activeFirstButton = null;
        ClearSelection();
    }

    public void PauseGame()
    {
        if (!gameStarted || isPaused)
            return;

        isPaused = true;
        settingsOpen = false;
        SetGameplayPaused(true);
        ShowPanel(pausePanel, pauseFirstButton);
    }

    public void ResumeGame()
    {
        if (!gameStarted || !isPaused)
            return;

        isPaused = false;
        settingsOpen = false;
        HidePanel(settingsPanel);
        HidePanel(pausePanel);
        SetGameplayPaused(false);
        activePanel = null;
        activeFirstButton = null;
        ClearSelection();
    }

    public void OpenSettings()
    {
        if (settingsOpen)
            return;

        settingsOpen = true;
        settingsReturnPanel = isPaused ? pausePanel : mainMenuPanel;
        HidePanel(settingsReturnPanel);
        ShowPanel(settingsPanel, settingsFirstButton);
    }

    public void CloseSettings()
    {
        if (!settingsOpen)
            return;

        settingsOpen = false;
        HidePanel(settingsPanel);

        if (settingsReturnPanel == pausePanel)
            ShowPanel(pausePanel, pauseFirstButton);
        else
            ShowPanel(mainMenuPanel, mainMenuFirstButton);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        gameProgress.RestartGame();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        gameProgress.ReturnToMainMenu();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetGameplayPaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;

        if (playerMovement == null)
            return;

        playerMovement.ResetMotion();
        playerMovement.enabled = !paused;
    }

    private void PreparePanel(GameObject panel)
    {
        if (panel == null)
            return;

        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();

        canvasGroups[panel] = canvasGroup;
        panelScales[panel] = panel.transform.localScale;
    }

    private void ConfigureButtons(GameObject panel)
    {
        if (panel == null)
            return;

        Selectable[] selectables = panel.GetComponentsInChildren<Selectable>(true);
        foreach (Selectable selectable in selectables)
        {
            Navigation navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.Automatic;
            selectable.navigation = navigation;
        }

        Button[] buttons = panel.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.GetComponent<UIButtonSelectionSync>() == null)
                button.gameObject.AddComponent<UIButtonSelectionSync>();

            ColorBlock colors = button.colors;
            colors.normalColor = normalButtonColor;
            colors.highlightedColor = selectedButtonColor;
            colors.selectedColor = selectedButtonColor;
            colors.pressedColor = pressedButtonColor;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }
    }

    private void ShowPanel(GameObject panel, Button firstButton)
    {
        if (panel == null)
            return;

        StartPanelAnimation(panel, true);
        activePanel = panel;
        activeFirstButton = firstButton;
        StartCoroutine(SelectNextFrame(firstButton));
    }

    private void HidePanel(GameObject panel)
    {
        if (panel != null && panel.activeSelf)
            StartPanelAnimation(panel, false);
    }

    private void ShowImmediate(GameObject panel)
    {
        if (panel == null)
            return;

        panel.SetActive(true);
        CanvasGroup canvasGroup = canvasGroups[panel];
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        panel.transform.localScale = panelScales[panel];
    }

    private void HideImmediate(GameObject panel)
    {
        if (panel == null)
            return;

        CanvasGroup canvasGroup = canvasGroups[panel];
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        panel.transform.localScale = panelScales[panel] * hiddenScale;
        panel.SetActive(false);
    }

    private void StartPanelAnimation(GameObject panel, bool show)
    {
        if (panelAnimations.TryGetValue(panel, out Coroutine runningAnimation) &&
            runningAnimation != null)
        {
            StopCoroutine(runningAnimation);
        }

        panelAnimations[panel] = StartCoroutine(AnimatePanel(panel, show));
    }

    private IEnumerator AnimatePanel(GameObject panel, bool show)
    {
        CanvasGroup canvasGroup = canvasGroups[panel];
        Vector3 visibleScale = panelScales[panel];
        Vector3 smallScale = visibleScale * hiddenScale;

        if (show)
            panel.SetActive(true);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float startAlpha = canvasGroup.alpha;
        float targetAlpha = show ? 1f : 0f;
        Vector3 startScale = panel.transform.localScale;
        Vector3 targetScale = show ? visibleScale : smallScale;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = transitionDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / transitionDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothProgress);
            panel.transform.localScale = Vector3.Lerp(startScale, targetScale, smoothProgress);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        panel.transform.localScale = targetScale;
        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = show;

        if (!show)
            panel.SetActive(false);

        panelAnimations[panel] = null;
    }

    private IEnumerator SelectNextFrame(Button button)
    {
        yield return null;
        SelectButton(button);
    }

    private void HandleCancel()
    {
        if (settingsOpen)
            CloseSettings();
        else if (gameStarted && isPaused)
            ResumeGame();
        else if (gameStarted)
            PauseGame();
    }

    private void SubmitSelection()
    {
        if (EventSystem.current == null)
            return;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject != null && selectedObject.TryGetComponent(out Button button))
            button.onClick.Invoke();
    }

    private void MoveSelection(int direction)
    {
        if (activePanel == null || EventSystem.current == null)
            return;

        Selectable[] selectables = activePanel.GetComponentsInChildren<Selectable>(false);
        if (selectables.Length == 0)
            return;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        int currentIndex = -1;

        for (int index = 0; index < selectables.Length; index++)
        {
            if (selectables[index].gameObject == selectedObject)
            {
                currentIndex = index;
                break;
            }
        }

        for (int step = 1; step <= selectables.Length; step++)
        {
            int nextIndex = currentIndex < 0
                ? (direction > 0 ? 0 : selectables.Length - 1)
                : (currentIndex + direction * step + selectables.Length) % selectables.Length;

            Selectable next = selectables[nextIndex];
            if (next.IsActive() && next.IsInteractable())
            {
                SelectSelectable(next);
                return;
            }
        }
    }

    private static void ChangeSelectedSlider(float direction)
    {
        if (EventSystem.current == null)
            return;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject == null || !selectedObject.TryGetComponent(out Slider slider))
            return;

        float step = (slider.maxValue - slider.minValue) * 0.05f;
        slider.value = Mathf.Clamp(slider.value + step * direction, slider.minValue, slider.maxValue);
    }

    private static void SelectButton(Button button)
    {
        SelectSelectable(button);
    }

    private static void SelectSelectable(Selectable selectable)
    {
        if (selectable == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectable.gameObject);
    }

    private static void ClearSelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
