using System.Collections;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class StoryRouteController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private GameUIController gameUIController;
    [SerializeField] private DialogueHUD dialogueHUD;
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Camera routeCamera;

    [Header("Scene Sections")]
    [SerializeField] private StorySectionLayout[] sections;

    [Header("Room 01 Design Points")]
    [SerializeField] private Transform gameStartPoint;
    [SerializeField] private Transform startEntryFromKeyRoom;
    [SerializeField] private Transform endingWalkTarget;

    [Header("Room 02 Design Points")]
    [SerializeField] private Transform keyRoomEntryFromStart;
    [SerializeField] private Transform keyRoomEntryFromFuse;
    [SerializeField] private Transform boxClimbGroundPoint;
    [SerializeField] private Transform boxClimbTopPoint;
    [SerializeField] private Transform keyPickupPoint;
    [SerializeField] private Transform keyShadowCameraPoint;
    [SerializeField] private Transform keyRoomPlayerRightLimitAfterKey;
    [SerializeField] private Transform keyRoomCameraRightLimitAfterKey;

    [Header("Room 03 Design Points")]
    [SerializeField] private Transform fuseRoomEntryFromKey;
    [SerializeField] private Transform fuseRoomEntryFromBackstage;
    [SerializeField] private Transform fuseEscapeTarget;
    [SerializeField] private Transform fuseShadowCameraPoint;

    [Header("Room 04 Design Points")]
    [SerializeField] private Transform backstageEntryFromFuse;
    [SerializeField] private Transform backstageEntryFromUpper;
    [SerializeField] private Transform backstageCurtainCameraPoint;

    [Header("Room 05 Design Points")]
    [SerializeField] private Transform upperEntryFromBackstage;

    [Header("Scene UI")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private CanvasGroup storyCanvas;
    [SerializeField] private Image storyImage;
    [SerializeField] private TMP_Text storyLabel;
    [SerializeField] private GameObject creditPanel;
    [SerializeField] private RectTransform tutorialHUDPanel;

    [Header("Room 01 - Start")]
    [SerializeField] private StoryInteractable tutorialItem;
    [SerializeField] private StoryInteractable hallwayDoor;
    [SerializeField] private StoryInteractable courtyardDoor;

    [Header("Room 02 - Key")]
    [SerializeField] private StoryInteractable keyRoomDoor;
    [SerializeField] private StoryInteractable keyRoomReturnDoor;
    [SerializeField] private StoryPushBoxInteractable pushBox;
    [SerializeField] private GameObject highKey;
    [SerializeField] private StoryShadowReveal keyRoomShadow;
    [SerializeField] private StoryInteractable returnCurtain;
    [SerializeField] private Transform returnCurtainVisual;
    [SerializeField] private GameObject customRackShadow;
    [SerializeField] private GameObject customRackReal;

    [Header("Room 03 - Fuse")]
    [SerializeField] private StoryInteractable fuseBox;
    [SerializeField] private SpriteRenderer fuseRenderer;
    [SerializeField] private StoryInteractable toolbox;
    [SerializeField] private StoryInteractable fuseReturnDoor;
    [SerializeField] private Light2D farLight;
    [SerializeField] private GameObject playerShadow;

    [Header("Room 04 - Backstage")]
    [SerializeField] private StoryInteractable weight;
    [SerializeField] private StoryInteractable lockedStageDoor;
    [SerializeField] private StoryInteractable backpack;
    [SerializeField] private StoryInteractable backstageReturnDoor;
    [SerializeField] private GameObject backstageStairs;
    [SerializeField] private GameObject curtainShadow;
    [SerializeField] private GameObject revealedDecoration;
    [SerializeField] private GameObject curtainClosedVisual;
    [SerializeField] private GameObject curtainOpenVisual;
    [SerializeField] private GameObject lowerWeightedRopeVisual;

    [Header("Room 05 - Upper Backstage")]
    [SerializeField] private StoryInteractable curtainRope;
    [SerializeField] private GameObject upperStairs;
    [SerializeField] private CurtainWeightPullController curtainWeightPull;

    [Header("Placeholder cutscene images")]
    [SerializeField] private Sprite tutorialPaperImage;
    [SerializeField] private Sprite firstShadowImage;
    [SerializeField] private Sprite curtainOpeningImage;
    [SerializeField] private Sprite decorationRevealImage;
    [SerializeField] private Sprite ownShadowImage;
    [SerializeField] private Sprite costumeRevealImage;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float introLineDuration = 3.2f;
    [SerializeField] private float curtainVisualSwapDelay = 0.45f;
    [SerializeField] private float escapeShockDuration = 1.25f;
    [SerializeField] private float autoRunSpeed = 6f;
    [SerializeField] private float walkToPointSpeed = 3.5f;
    [SerializeField] private float keyPickupCameraDelay = 0.4f;
    [SerializeField] private float keyShadowCameraMaximumDuration = 6f;
    [SerializeField] private float returnCurtainDuration = 0.9f;
    [SerializeField] private float returnCurtainNormalXScale = 0.44f;
    [SerializeField] private float returnCurtainRetractedXScale = 0.15f;
    [SerializeField] private float endingWalkDuration = 2.4f;
    [SerializeField] private float shortCutsceneDuration = 1.4f;
    [SerializeField] private float creditMinimumDuration = 10f;
    [SerializeField] private float creditMaximumDuration = 60f;

    [Header("Cutscene Presentation")]
    [SerializeField] private float cutsceneTransitionDuration = 0.2f;
    [SerializeField] private float cutsceneStartScale = 0.82f;
    [SerializeField] private float tutorialHUDTransitionDuration = 0.25f;
    [SerializeField, Range(0.5f, 1f)] private float tutorialHUDStartScale = 0.85f;
    [SerializeField] private float keyShadowCameraMoveDuration = 0.35f;
    [SerializeField, Range(0.5f, 1f)] private float keyShadowCameraZoom = 0.82f;
    [SerializeField] private float roomFocusCameraMoveDuration = 0.35f;
    [SerializeField, Range(0.5f, 1f)] private float roomFocusCameraZoom = 0.82f;
    [SerializeField] private AnimationCurve cutsceneEase =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Rigidbody2D playerBody;
    private float normalLightIntensity = 1f;
    private int currentRoom;
    private bool isBusy;
    private bool keyDistanceDialogueShown;
    private bool backstageIntroShown;
    private bool backstageIntroComplete;
    private bool fuseInspected;
    private bool keyDoorInspected;
    private bool waitingForKeyPickup;
    private bool introFinished;
    private bool cameraFocusActive;
    private Coroutine pantingRoutine;
    private Vector3 storyImageBaseScale = Vector3.one;
    private Vector3 tutorialHUDBaseScale = Vector3.one;
    private CanvasGroup tutorialHUDCanvas;

    private static readonly string[] IntroLines =
    {
        "آفرین پسرم، نمایش خیلی خوبی اجرا کردی.",
        "من میرم توی حیاط و منتظرت میمونم.",
        "تو هم برو کوله پشتیت رو از پشت صحنه بردار و بیا."
    };

    public static bool IsCutsceneOpen { get; private set; }

    public bool CanUsePushBox =>
        currentRoom == 1 &&
        gameProgress.HasCompleted(GameProgressEvent.EnteredKeyRoom) &&
        !gameProgress.HasCompleted(GameProgressEvent.BoxPlaced) &&
        !isBusy;

    public bool CanClimbPlacedBox =>
        currentRoom == 1 &&
        gameProgress.HasCompleted(GameProgressEvent.BoxPlaced) &&
        !gameProgress.HasCompleted(GameProgressEvent.KeyCollected) &&
        !waitingForKeyPickup &&
        !isBusy;

    public string GetCurrentHint()
    {
        if (gameProgress == null)
            return string.Empty;

        bool bagCollected = gameProgress.HasCompleted(
            GameProgressEvent.BagCollected);

        switch (currentRoom)
        {
            case 0:
                if (!gameProgress.HasCompleted(GameProgressEvent.TutorialItemRead))
                    return "برگه روی زمین را بردار.";
                return bagCollected
                    ? "از درب حیاط خارج شو."
                    : "از درب راهرو وارد مسیر پشت صحنه شو.";

            case 1:
                if (bagCollected)
                {
                    return gameProgress.HasCompleted(
                        GameProgressEvent.ReturnCurtainRevealed)
                        ? "به سمت حیاط برگرد."
                        : "پرده را بررسی کن و ببین پشت آن چه بوده.";
                }

                if (!keyDoorInspected)
                    return "درب انتهای مسیر را بررسی کن.";
                if (!gameProgress.HasCompleted(GameProgressEvent.BoxPlaced))
                    return "یک وسیله پیدا کن و آن را زیر محل کلید ببر.";
                if (!gameProgress.HasCompleted(GameProgressEvent.KeyCollected))
                    return "روی صندلی برو و کلید را بردار.";
                return "درب قفل شده را با کلید باز کن و مسیر را ادامه بده.";

            case 2:
                if (bagCollected)
                    return "به سمت حیاط برگرد.";
                if (!fuseInspected)
                    return "جعبه فیوز را بررسی کن.";
                if (!gameProgress.HasCompleted(GameProgressEvent.FusePartCollected))
                    return "محیط را بگرد و قطعه گمشده جعبه فیوز را پیدا کن.";
                if (!gameProgress.HasCompleted(GameProgressEvent.FuseRepaired))
                    return "قطعه را داخل جعبه فیوز قرار بده.";
                return "به سمت پشت صحنه برو.";

            case 3:
                if (bagCollected)
                    return "به سمت حیاط برگرد.";
                if (!backstageIntroComplete)
                    return "دنبال کوله پشتی بگرد.";
                if (!gameProgress.HasCompleted(GameProgressEvent.WeightCollected))
                    return "یک وسیله سنگین پیدا کن.";
                if (!gameProgress.HasCompleted(GameProgressEvent.CurtainOpened))
                    return "وسیله سنگین را به طبقه بالا ببر.";
                if (!gameProgress.HasCompleted(GameProgressEvent.DecorationRevealed))
                    return "ببین سایه روی پرده متعلق به چه چیزی بوده.";
                return "کوله پشتی را بردار.";

            case 4:
                return gameProgress.HasCompleted(GameProgressEvent.CurtainOpened)
                    ? "به طبقه پایین برگرد."
                    : "وزنه را به طناب پرده اصلی گره بزن.";

            default:
                return string.Empty;
        }
    }

    // Old door components are disabled at startup. These two methods only keep
    // those serialized scene components compatible while the route is replaced.
    public bool CanUseDoor(RouteDoorRole role)
    {
        return false;
    }

    public IEnumerator UseDoor(RouteDoorInteractable door)
    {
        yield break;
    }

    private void Awake()
    {
        FindReferences();
        if (gameStartPoint != null && player != null)
        {
            SetPlayerPosition(GetPointPosition(gameStartPoint, player.position));
            playerMovement?.FaceDirection(1f);
        }
        if (gameProgress != null)
            gameProgress.ProgressChanged += HandleProgressChanged;

        IsCutsceneOpen = false;
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
        }
        if (storyCanvas != null)
        {
            storyCanvas.alpha = 0f;
            storyCanvas.blocksRaycasts = false;
        }
        if (storyImage != null)
            storyImageBaseScale = storyImage.rectTransform.localScale;
        PrepareTutorialHUD();
        if (creditPanel != null)
            creditPanel.SetActive(false);
        if (keyRoomShadow != null &&
            !gameProgress.HasCompleted(GameProgressEvent.KeyCollected))
        {
            keyRoomShadow.ResetHidden();
        }
        if (returnCurtainVisual != null &&
            !gameProgress.HasCompleted(GameProgressEvent.ReturnCurtainRevealed))
        {
            Vector3 curtainScale = returnCurtainVisual.localScale;
            curtainScale.x = returnCurtainNormalXScale;
            returnCurtainVisual.localScale = curtainScale;
        }
        bool returnCurtainRevealed = gameProgress.HasCompleted(
            GameProgressEvent.ReturnCurtainRevealed);
        if (customRackShadow != null)
            customRackShadow.SetActive(!returnCurtainRevealed);
        if (customRackReal != null)
            customRackReal.SetActive(returnCurtainRevealed);
        if (gameProgress.HasCompleted(GameProgressEvent.KeyCollected))
            ApplyKeyRoomExpandedLimits();
        SetCameraToRoom(0);
        UpdateRouteObjects();
        ApplyRoomLighting();
    }

    private void Start()
    {
        StartCoroutine(IntroSequence());
    }

    private void Update()
    {
        if (waitingForKeyPickup && IsInteractionPressed())
        {
            waitingForKeyPickup = false;
            StartCoroutine(KeyPickupSequence());
            return;
        }

        if (player == null || isBusy)
            return;

        KeepPlayerInsideCurrentRoom();

        if (currentRoom == 1 &&
            !keyDistanceDialogueShown &&
            !gameProgress.HasCompleted(GameProgressEvent.BoxPlaced) &&
            highKey != null &&
            Mathf.Abs(player.position.x - highKey.transform.position.x) < 2.25f)
        {
            keyDistanceDialogueShown = true;
            Say("قدم بهش نمیرسه.");
        }
    }

    private void LateUpdate()
    {
        if (!cameraFocusActive)
            UpdateCameraPosition();
    }

    private void OnDestroy()
    {
        if (gameProgress != null)
            gameProgress.ProgressChanged -= HandleProgressChanged;

        IsCutsceneOpen = false;
    }

    public bool CanInteract(StoryInteractionType type)
    {
        if (isBusy)
            return false;

        switch (type)
        {
            case StoryInteractionType.TutorialPaper:
                return currentRoom == 0 &&
                       !gameProgress.HasCompleted(GameProgressEvent.TutorialItemRead);
            case StoryInteractionType.HallwayDoor:
                return currentRoom == 0 &&
                       !gameProgress.HasCompleted(GameProgressEvent.BagCollected);
            case StoryInteractionType.CourtyardDoor:
                return currentRoom == 0;
            case StoryInteractionType.KeyRoomDoor:
                return currentRoom == 1 &&
                       !gameProgress.HasCompleted(GameProgressEvent.EnteredFuseRoom);
            case StoryInteractionType.Toolbox:
                return currentRoom == 2 &&
                       fuseInspected &&
                       !gameProgress.HasCompleted(GameProgressEvent.FusePartCollected);
            case StoryInteractionType.FuseBox:
                return currentRoom == 2 &&
                       !gameProgress.HasCompleted(GameProgressEvent.FuseRepaired);
            case StoryInteractionType.Weight:
                return currentRoom == 3 &&
                       backstageIntroComplete &&
                       !gameProgress.HasCompleted(GameProgressEvent.WeightCollected);
            case StoryInteractionType.LockedStageDoor:
                return currentRoom == 3 && backstageIntroComplete;
            case StoryInteractionType.CurtainRope:
                return currentRoom == 4 &&
                       gameProgress.HasCompleted(GameProgressEvent.WeightCollected) &&
                       !gameProgress.HasCompleted(GameProgressEvent.CurtainOpened);
            case StoryInteractionType.Backpack:
                return currentRoom == 3 &&
                       backstageIntroComplete &&
                       !gameProgress.HasCompleted(GameProgressEvent.BagCollected);
            case StoryInteractionType.BackstageReturnDoor:
                return currentRoom == 3 && backstageIntroComplete;
            case StoryInteractionType.FuseReturnDoor:
                return currentRoom == 2;
            case StoryInteractionType.KeyRoomReturnDoor:
                return currentRoom == 1;
            case StoryInteractionType.ReturnCurtain:
                return currentRoom == 1 &&
                       gameProgress.HasCompleted(GameProgressEvent.ReturnedToKeyRoom) &&
                       !gameProgress.HasCompleted(GameProgressEvent.ReturnCurtainRevealed);
            default:
                return false;
        }
    }

    public IEnumerator HandleInteraction(
        StoryInteractionType type,
        GameObject interactedObject)
    {
        switch (type)
        {
            case StoryInteractionType.TutorialPaper:
                yield return TutorialSequence(interactedObject);
                break;
            case StoryInteractionType.HallwayDoor:
                if (!gameProgress.HasCompleted(GameProgressEvent.TutorialItemRead))
                    Say("باید اون برگه رو بردارم.");
                else
                {
                    GameAudioManager.Instance?.PlayDoorOpen();
                    yield return ChangeRoom(1, keyRoomEntryFromStart);
                }
                break;
            case StoryInteractionType.CourtyardDoor:
                if (!gameProgress.HasCompleted(GameProgressEvent.BagCollected))
                {
                    GameAudioManager.Instance?.PlayDoorLocked();
                    Say("باید از پشت صحنه کوله پشتیمو بردارم.");
                }
                break;
            case StoryInteractionType.KeyRoomDoor:
                if (!gameProgress.HasCompleted(GameProgressEvent.KeyCollected))
                {
                    keyDoorInspected = true;
                    GameAudioManager.Instance?.PlayDoorLocked();
                    Say("در قفله، باید دنبال کلید بگردم.");
                }
                else
                {
                    GameAudioManager.Instance?.PlayDoorOpen();
                    yield return ChangeRoom(2, fuseRoomEntryFromKey);
                }
                break;
            case StoryInteractionType.Toolbox:
                yield return ToolboxSequence(interactedObject);
                break;
            case StoryInteractionType.FuseBox:
                if (!gameProgress.HasCompleted(GameProgressEvent.FusePartCollected))
                {
                    fuseInspected = true;
                    Say("یه قطعه از جعبه فیوز کمه. باید پیداش کنم.");
                }
                else
                    yield return FuseSequence();
                break;
            case StoryInteractionType.Weight:
                isBusy = true;
                SetPlayerControl(false);
                GameAudioManager.Instance?.PlayWeightPickup();
                yield return PlayPlayerAnimation("Weight Grab Animation");
                interactedObject.SetActive(false);
                gameProgress.TryComplete(GameProgressEvent.WeightCollected);
                playerAnimator?.SetPersistentState("Weight Carry Animation");
                Say("باید یه راهی پیدا کنم برم بالا، کنار طناب پرده نمایش.");
                UpdateRouteObjects();
                SetPlayerControl(true);
                isBusy = false;
                break;
            case StoryInteractionType.LockedStageDoor:
                GameAudioManager.Instance?.PlayDoorLocked();
                Say("در قفله، باید یه راه دیگه وجود داشته باشه.");
                break;
            case StoryInteractionType.CurtainRope:
                yield return CurtainSequence();
                break;
            case StoryInteractionType.Backpack:
                if (!gameProgress.HasCompleted(GameProgressEvent.DecorationRevealed))
                {
                    Say("باید اول بفهمم اون سایه چیه؟");
                    break;
                }

                isBusy = true;
                SetPlayerControl(false);
                GameAudioManager.Instance?.PlayBackpackPickup();
                yield return PlayPlayerAnimation("Backpack Grab Animation");
                interactedObject.SetActive(false);
                gameProgress.TryComplete(GameProgressEvent.BagCollected);
                Say("باید برم سمت حیاط.");
                UpdateRouteObjects();
                SetPlayerControl(true);
                isBusy = false;
                break;
            case StoryInteractionType.BackstageReturnDoor:
                if (!gameProgress.HasCompleted(GameProgressEvent.BagCollected))
                    Say("باید اول کوله پشتیمو بردارم.");
                else
                {
                    GameAudioManager.Instance?.PlayDoorOpen();
                    yield return ChangeRoom(2, fuseRoomEntryFromBackstage);
                }
                break;
            case StoryInteractionType.FuseReturnDoor:
                if (!gameProgress.HasCompleted(GameProgressEvent.BagCollected))
                    Say("باید اول کوله پشتیمو بردارم.");
                else
                {
                    GameAudioManager.Instance?.PlayDoorOpen();
                    yield return ChangeRoom(1, keyRoomEntryFromFuse);
                }
                break;
            case StoryInteractionType.KeyRoomReturnDoor:
                if (!gameProgress.HasCompleted(GameProgressEvent.BagCollected))
                    Say("باید اول کوله پشتیمو بردارم.");
                else if (!gameProgress.HasCompleted(
                    GameProgressEvent.ReturnCurtainRevealed))
                {
                    Say("میخوام بدونم پشت اون پرده چی بوده.");
                }
                else
                {
                    GameAudioManager.Instance?.PlayDoorOpen();
                    yield return ChangeRoom(0, startEntryFromKeyRoom);
                }
                break;
            case StoryInteractionType.ReturnCurtain:
                yield return ReturnCurtainSequence();
                break;
        }
    }

    public void EnterNarrativeArea(StoryNarrativeAreaType areaType)
    {
        if (isBusy)
            return;

        if (areaType == StoryNarrativeAreaType.BackstageCurtainIntro)
        {
            if (currentRoom == 3 &&
                gameProgress.HasCompleted(GameProgressEvent.EnteredBackstage) &&
                !gameProgress.HasCompleted(GameProgressEvent.CurtainOpened) &&
                !backstageIntroShown)
            {
                StartCoroutine(BackstageIntroSequence());
            }
            else if (currentRoom == 3 &&
                     gameProgress.HasCompleted(GameProgressEvent.CurtainOpened) &&
                     !gameProgress.HasCompleted(GameProgressEvent.DecorationRevealed))
            {
                StartCoroutine(BackstageRevealSequence());
            }

            return;
        }

        if (areaType == StoryNarrativeAreaType.ReturnFuseShadow &&
            currentRoom == 2 &&
            gameProgress.HasCompleted(GameProgressEvent.BagCollected) &&
            gameProgress.TryComplete(GameProgressEvent.ReturnedToFuseRoom))
        {
            StartCoroutine(ReturnFuseSequence());
        }
    }

    public void NotifyBoxPlaced()
    {
        if (isBusy)
            return;

        gameProgress.TryComplete(GameProgressEvent.BoxPlaced);
    }

    public bool CanUseStairs(StoryAreaType type)
    {
        if (isBusy)
            return false;

        if (type == StoryAreaType.BackstageStairsUp)
            return currentRoom == 3 && backstageIntroComplete;

        return type == StoryAreaType.UpperStairsDown &&
               currentRoom == 4 &&
               gameProgress.HasCompleted(GameProgressEvent.CurtainOpened);
    }

    public IEnumerator UseStairs(StoryAreaType type)
    {
        if (!CanUseStairs(type))
            yield break;

        if (type == StoryAreaType.BackstageStairsUp && currentRoom == 3)
        {
            if (!gameProgress.HasCompleted(GameProgressEvent.WeightCollected))
            {
                Say("اول باید وسیله سنگین رو پیدا کنم.");
                yield break;
            }

            isBusy = true;
            SetPlayerControl(false);
            yield return ChangeRoom(4, upperEntryFromBackstage, false);
            SetPlayerControl(true);
            isBusy = false;
        }
        else if (type == StoryAreaType.UpperStairsDown &&
                 currentRoom == 4 &&
                 gameProgress.HasCompleted(GameProgressEvent.CurtainOpened))
        {
            isBusy = true;
            SetPlayerControl(false);
            yield return ChangeRoom(3, backstageEntryFromUpper, false);
            SetPlayerControl(true);
            isBusy = false;
        }
    }

    public IEnumerator ClimbPlacedBox()
    {
        if (!CanClimbPlacedBox)
            yield break;

        isBusy = true;
        SetPlayerControl(false);
        yield return MovePlayerTo(boxClimbGroundPoint, walkToPointSpeed);
        playerMovement?.FaceDirection(1f);
        GameAudioManager.Instance?.PlayClimb();
        ApplyKeyRoomExpandedLimits();

        Coroutine shadowReveal = keyRoomShadow != null
            ? StartCoroutine(keyRoomShadow.Reveal())
            : null;
        yield return MovePlayerToWithAnimation(
            boxClimbTopPoint,
            GetClimbState(true),
            true);
        if (shadowReveal != null)
            yield return shadowReveal;

        isBusy = false;
        waitingForKeyPickup = true;
    }

    private IEnumerator TutorialSequence(GameObject item)
    {
        isBusy = true;
        SetPlayerControl(false);
        GameAudioManager.Instance?.PlayPaperPickup();
        yield return PlayPlayerAnimation("Grab Animation");
        item.SetActive(false);
        GameAudioManager.Instance?.PlayPaperOpen();
        yield return ShowTutorialHUD();

        yield return WaitForTutorialDismiss();

        yield return HideTutorialHUD();
        gameProgress.TryComplete(GameProgressEvent.TutorialItemRead);
        SetPlayerControl(true);
        isBusy = false;
        UpdateRouteObjects();
    }

    private IEnumerator IntroSequence()
    {
        if (introFinished)
            yield break;

        isBusy = true;
        while (Time.timeScale <= 0f)
            yield return null;

        if (fadeCanvas != null)
            fadeCanvas.alpha = 1f;
        IsCutsceneOpen = true;
        SetPlayerControl(false);
        foreach (string line in IntroLines)
        {
            Say(line);
            yield return new WaitForSecondsRealtime(introLineDuration);
        }

        dialogueHUD?.HideDialogue();
        yield return Fade(1f, 0f);
        introFinished = true;
        IsCutsceneOpen = false;
        SetPlayerControl(true);
        isBusy = false;
    }

    private IEnumerator KeyPickupSequence()
    {
        isBusy = true;
        SetPlayerControl(false);

        if (keyPickupPoint != null)
            yield return MovePlayerTo(keyPickupPoint, walkToPointSpeed, false);

        playerMovement?.FaceDirection(1f);
        GameAudioManager.Instance?.PlayKeyPickup();
        Vector3 cameraReturnPosition = routeCamera != null
            ? routeCamera.transform.position
            : Vector3.zero;
        float cameraReturnSize = routeCamera != null
            ? routeCamera.orthographicSize
            : 0f;

        yield return PlayPlayerAnimation("Key Grab Animation");
        yield return new WaitForSeconds(keyPickupCameraDelay);

        IsCutsceneOpen = true;
        cameraFocusActive = true;
        yield return MoveCameraToKeyShadow();
        float cameraFocusStartedAt = Time.unscaledTime;
        yield return PlayScareReaction();
        Say("این دیگه چیه؟");
        yield return new WaitForSeconds(2.2f);
        Say("باید زودتر از اینجا برم.");
        yield return new WaitForSeconds(2.2f);

        float remainingCameraFocusTime = Mathf.Max(
            0f,
            keyShadowCameraMaximumDuration -
            (Time.unscaledTime - cameraFocusStartedAt));
        yield return WaitForCutsceneDismissOrTimeout(remainingCameraFocusTime);

        gameProgress.TryComplete(GameProgressEvent.KeyCollected);
        ApplyKeyRoomExpandedLimits();
        playerAnimator?.ClearPersistentState();
        SetInteractionVisual(highKey, true);
        dialogueHUD?.HideDialogue();
        yield return MoveCameraTo(
            cameraReturnPosition,
            cameraReturnSize,
            keyShadowCameraMoveDuration);
        cameraFocusActive = false;
        IsCutsceneOpen = false;
        yield return new WaitForSeconds(0.15f);
        playerMovement?.FaceDirection(1f);
        GameAudioManager.Instance?.PlayClimb();
        yield return MovePlayerToWithAnimation(
            boxClimbGroundPoint,
            GetClimbState(false),
            false);

        SetPlayerControl(true);
        isBusy = false;
        UpdateRouteObjects();
    }

    private IEnumerator MoveCameraToKeyShadow()
    {
        if (routeCamera == null || keyShadowCameraPoint == null)
            yield break;

        Vector3 targetPosition = keyShadowCameraPoint.position;
        targetPosition.z = routeCamera.transform.position.z;
        float targetSize = routeCamera.orthographicSize * keyShadowCameraZoom;
        yield return MoveCameraTo(
            targetPosition,
            targetSize,
            keyShadowCameraMoveDuration);
    }

    private IEnumerator MoveCameraToFocusPoint(Transform focusPoint)
    {
        if (routeCamera == null || focusPoint == null)
            yield break;

        Vector3 targetPosition = focusPoint.position;
        targetPosition.z = routeCamera.transform.position.z;
        float targetSize = routeCamera.orthographicSize * roomFocusCameraZoom;
        yield return MoveCameraTo(
            targetPosition,
            targetSize,
            roomFocusCameraMoveDuration);
    }

    private IEnumerator MoveCameraTo(
        Vector3 targetPosition,
        float targetSize,
        float duration)
    {
        if (routeCamera == null)
            yield break;

        Vector3 startPosition = routeCamera.transform.position;
        startPosition.y = 0f;
        targetPosition.y = 0f;
        StorySectionLayout currentSection = GetCurrentSection();
        if (currentSection != null)
        {
            startPosition.x = currentSection.ClampCameraX(startPosition.x);
            targetPosition.x = currentSection.ClampCameraX(targetPosition.x);
        }
        routeCamera.transform.position = startPosition;
        float startSize = routeCamera.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = duration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / duration);
            float eased = cutsceneEase.Evaluate(progress);
            routeCamera.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                eased);
            routeCamera.orthographicSize = Mathf.Lerp(
                startSize,
                targetSize,
                eased);
            yield return null;
        }

        routeCamera.transform.position = targetPosition;
        routeCamera.orthographicSize = targetSize;
    }

    private IEnumerator ToolboxSequence(GameObject toolboxObject)
    {
        isBusy = true;
        SetPlayerControl(false);
        Say("ممکنه داخل این جعبه چیزی که لازمه رو پیدا کنم.");
        yield return new WaitForSeconds(2.2f);
        GameAudioManager.Instance?.PlayToolboxSearch();
        yield return PlayPlayerAnimation("Ground Search Animation");

        gameProgress.TryComplete(GameProgressEvent.FusePartCollected);
        SetInteractionVisual(toolboxObject, true);
        Say("قطعه رو پیدا کردم. باید بزارمش داخل جعبه فیوز.");

        SetPlayerControl(true);
        isBusy = false;
        UpdateRouteObjects();
    }

    private IEnumerator FuseSequence()
    {
        isBusy = true;
        SetPlayerControl(false);
        GameAudioManager.Instance?.PlayFuseRepair();
        yield return PlayPlayerAnimation("Key Grab Animation");
        gameProgress.TryComplete(GameProgressEvent.FuseRepaired);

        if (fuseRenderer != null)
            fuseRenderer.color = new Color(0.22f, 0.8f, 0.28f);
        SetInteractionVisual(fuseBox != null ? fuseBox.gameObject : null, true);

        ApplyRoomLighting();
        GameAudioManager.Instance?.PlayProjectorPowerOn();

        playerShadow.SetActive(true);
        Vector3 cameraReturnPosition = routeCamera != null
            ? routeCamera.transform.position
            : Vector3.zero;
        float cameraReturnSize = routeCamera != null
            ? routeCamera.orthographicSize
            : 0f;
        cameraFocusActive = true;
        yield return MoveCameraToFocusPoint(fuseShadowCameraPoint);

        yield return PlayScareReaction();
        Say("اون سایه دوباره پیداش شد!");
        yield return new WaitForSeconds(2.2f);

        Say("داره دنبالم میاد... باید فرار کنم!");
        yield return new WaitForSeconds(escapeShockDuration);
        dialogueHUD?.HideDialogue();
        yield return MoveCameraTo(
            cameraReturnPosition,
            cameraReturnSize,
            roomFocusCameraMoveDuration);
        cameraFocusActive = false;

        playerAnimator?.ClearPersistentState();
        playerAnimator?.SetPersistentState("Run Animation");
        yield return AutoRunTo(fuseEscapeTarget);
        playerAnimator?.ClearPersistentState();
        GameAudioManager.Instance?.PlayDoorOpen();
        yield return ChangeRoom(3, backstageEntryFromFuse, false, false);

        SetPlayerControl(true);
        isBusy = false;
        HandleRoomEntered(3);
    }

    private IEnumerator CurtainSequence()
    {
        isBusy = true;
        SetPlayerControl(false);
        Say("باید این وزنه رو به طناب پرده گره بزنم.");
        playerAnimator?.ClearPersistentState();
        GameAudioManager.Instance?.PlayRopeTie();
        yield return PlayPlayerAnimation("Backpack Grab Animation");

        Coroutine ropePull = null;
        if (curtainWeightPull != null)
        {
            GameAudioManager.Instance?.PlayRopePull();
            ropePull = StartCoroutine(curtainWeightPull.PullDown());
        }

        yield return new WaitForSeconds(curtainVisualSwapDelay);
        GameAudioManager.Instance?.PlayCurtainOpen();
        if (curtainClosedVisual != null)
            curtainClosedVisual.SetActive(false);
        if (curtainOpenVisual != null)
            curtainOpenVisual.SetActive(true);
        if (curtainShadow != null)
            curtainShadow.SetActive(false);
        if (revealedDecoration != null)
            revealedDecoration.SetActive(true);

        if (ropePull != null)
            yield return ropePull;

        gameProgress.TryComplete(GameProgressEvent.CurtainOpened);

        SetPlayerControl(true);
        isBusy = false;
        UpdateRouteObjects();
    }

    private IEnumerator BackstageRevealSequence()
    {
        isBusy = true;
        SetPlayerControl(false);
        curtainShadow.SetActive(false);
        revealedDecoration.SetActive(true);

        Vector3 cameraReturnPosition = routeCamera != null
            ? routeCamera.transform.position
            : Vector3.zero;
        float cameraReturnSize = routeCamera != null
            ? routeCamera.orthographicSize
            : 0f;
        cameraFocusActive = true;
        yield return MoveCameraToFocusPoint(backstageCurtainCameraPoint);

        Say("پس سایه دکور بود که اینهمه ازش ترسیدم!");
        yield return new WaitForSeconds(2.4f);
        dialogueHUD?.HideDialogue();
        yield return MoveCameraTo(
            cameraReturnPosition,
            cameraReturnSize,
            roomFocusCameraMoveDuration);
        cameraFocusActive = false;
        GameAudioManager.Instance?.PlayReliefBreath();

        gameProgress.TryComplete(GameProgressEvent.DecorationRevealed);
        Say("باید زودتر کوله پشتیمو بردارم و از اینجا برم.");
        SetPlayerControl(true);
        isBusy = false;
        UpdateRouteObjects();
    }

    private IEnumerator ReturnFuseSequence()
    {
        isBusy = true;
        SetPlayerControl(false);
        playerShadow.SetActive(true);

        Vector3 cameraReturnPosition = routeCamera != null
            ? routeCamera.transform.position
            : Vector3.zero;
        float cameraReturnSize = routeCamera != null
            ? routeCamera.orthographicSize
            : 0f;
        cameraFocusActive = true;
        yield return MoveCameraToFocusPoint(fuseShadowCameraPoint);

        Say("اینکه سایه خودمه.");
        yield return new WaitForSeconds(2.4f);
        Say("یعنی از سایه خودم انقدر ترسیدم؟");
        yield return new WaitForSeconds(2.4f);
        dialogueHUD?.HideDialogue();
        yield return MoveCameraTo(
            cameraReturnPosition,
            cameraReturnSize,
            roomFocusCameraMoveDuration);
        cameraFocusActive = false;
        GameAudioManager.Instance?.PlayReliefBreath();
        SetPlayerControl(true);
        isBusy = false;
    }

    private IEnumerator ReturnCurtainSequence()
    {
        isBusy = true;
        SetPlayerControl(false);

        if (customRackShadow != null)
            customRackShadow.SetActive(false);
        if (customRackReal != null)
            customRackReal.SetActive(true);

        if (returnCurtainVisual != null)
        {
            GameAudioManager.Instance?.PlayCurtainOpen();
            Vector3 startScale = returnCurtainVisual.localScale;
            startScale.x = returnCurtainNormalXScale;
            returnCurtainVisual.localScale = startScale;

            Vector3 targetScale = startScale;
            targetScale.x = returnCurtainRetractedXScale;
            float elapsed = 0f;
            while (elapsed < returnCurtainDuration)
            {
                elapsed += Time.deltaTime;
                float progress = returnCurtainDuration <= 0f
                    ? 1f
                    : Mathf.Clamp01(elapsed / returnCurtainDuration);
                float eased = Mathf.SmoothStep(0f, 1f, progress);
                returnCurtainVisual.localScale = Vector3.Lerp(
                    startScale,
                    targetScale,
                    eased);
                yield return null;
            }

            returnCurtainVisual.localScale = targetScale;
        }

        keyRoomShadow?.Hide();
        gameProgress.TryComplete(GameProgressEvent.ReturnCurtainRevealed);
        Say("پس هیچ هیولایی در کار نبود!");
        GameAudioManager.Instance?.PlayReliefBreath();
        SetPlayerControl(true);
        isBusy = false;
        UpdateRouteObjects();
    }

    private IEnumerator EndingSequence()
    {
        isBusy = true;
        SetPlayerControl(false);
        GameAudioManager.Instance?.PlayEndingMusic();
        gameProgress.TryComplete(GameProgressEvent.EnteredCourtyard);

        float elapsed = 0f;
        Vector3 startPosition = player.position;
        Vector3 targetPosition = GetPointPosition(endingWalkTarget, startPosition);
        playerAnimator?.SetPersistentState(GetWalkState());
        while (elapsed < endingWalkDuration)
        {
            elapsed += Time.deltaTime;
            float progress = endingWalkDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / endingWalkDuration);
            Vector3 position = Vector3.Lerp(startPosition, targetPosition, progress);
            player.position = position;
            fadeCanvas.alpha = progress;
            yield return null;
        }

        if (playerBody != null)
            playerBody.linearVelocity = Vector2.zero;
        playerAnimator?.ClearPersistentState();

        fadeCanvas.alpha = 1f;
        gameProgress.TryComplete(GameProgressEvent.GameFinished);
        yield return CreditSequence();
    }

    private IEnumerator ChangeRoom(
        int targetRoom,
        Transform spawnPoint,
        bool manageBusy = true,
        bool runEntrySequence = true)
    {
        if (manageBusy)
        {
            isBusy = true;
            SetPlayerControl(false);
        }

        dialogueHUD?.HideDialogue();
        yield return Fade(0f, 1f);
        float entryDirection = targetRoom >= currentRoom ? 1f : -1f;
        currentRoom = targetRoom;
        player.position = GetPointPosition(spawnPoint, player.position);
        playerMovement?.FaceDirection(entryDirection);
        if (playerBody != null)
            playerBody.linearVelocity = Vector2.zero;

        SetCameraToRoom(targetRoom);
        UpdateRouteObjects();
        ApplyRoomLighting();
        yield return Fade(1f, 0f);

        if (manageBusy)
        {
            SetPlayerControl(true);
            isBusy = false;
        }

        if (runEntrySequence)
            HandleRoomEntered(targetRoom);
    }

    private void HandleRoomEntered(int room)
    {
        if (room == 1 && !gameProgress.HasCompleted(GameProgressEvent.EnteredKeyRoom))
        {
            gameProgress.TryComplete(GameProgressEvent.EnteredKeyRoom);
        }
        else if (room == 1 && gameProgress.HasCompleted(GameProgressEvent.BagCollected))
        {
            if (gameProgress.TryComplete(GameProgressEvent.ReturnedToKeyRoom))
                Say("نکنه سایه ای که اینجا دیدم هم هیولا نبوده؟");
        }
        else if (room == 2 && !gameProgress.HasCompleted(GameProgressEvent.EnteredFuseRoom))
        {
            gameProgress.TryComplete(GameProgressEvent.EnteredFuseRoom);
            Say("اینجا خیلی تاریکه، باید چراغ رو روشن کنم.");
        }
        else if (room == 2 && gameProgress.HasCompleted(GameProgressEvent.BagCollected))
        {
            // The return reveal starts from a movable scene trigger after the
            // player has walked a few steps into the room.
        }
        else if (room == 3 && !gameProgress.HasCompleted(GameProgressEvent.EnteredBackstage))
        {
            gameProgress.TryComplete(GameProgressEvent.EnteredBackstage);
            Say("باید کیفم همینجاها باشه!");
            StartBackstagePanting();
        }
        else if (room == 4)
        {
            gameProgress.TryComplete(GameProgressEvent.EnteredUpperBackstage);
            Say("باید این وزنه رو به طناب پرده گره بزنم.");
        }
        else if (room == 0 && gameProgress.HasCompleted(GameProgressEvent.BagCollected))
        {
            if (gameProgress.TryComplete(GameProgressEvent.ReturnedToFirstRoom))
                StartCoroutine(EndingSequence());
        }
    }

    private IEnumerator BackstageIntroSequence()
    {
        if (backstageIntroShown)
            yield break;

        backstageIntroShown = true;
        isBusy = true;
        SetPlayerControl(false);

        Vector3 cameraReturnPosition = routeCamera != null
            ? routeCamera.transform.position
            : Vector3.zero;
        float cameraReturnSize = routeCamera != null
            ? routeCamera.orthographicSize
            : 0f;
        cameraFocusActive = true;
        yield return MoveCameraToFocusPoint(backstageCurtainCameraPoint);

        yield return PlayScareReaction();

        Say("اون سایه روی پرده چیه؟");
        yield return new WaitForSeconds(2.4f);
        Say("باید بفهمم پشت این پرده چی قایم شده.");
        yield return new WaitForSeconds(2.4f);
        Say("باید یه جوری طناب پرده رو بکشم پایین تا پرده بره کنار.");
        yield return new WaitForSeconds(2.4f);
        dialogueHUD?.HideDialogue();
        playerAnimator?.ClearPersistentState();
        yield return MoveCameraTo(
            cameraReturnPosition,
            cameraReturnSize,
            roomFocusCameraMoveDuration);
        cameraFocusActive = false;

        backstageIntroComplete = true;
        SetPlayerControl(true);
        isBusy = false;
    }

    private IEnumerator PlayScareReaction()
    {
        playerAnimator?.ClearPersistentState();
        GameAudioManager.Instance?.PlayScare();
        yield return PlayPlayerAnimation("Scared Animation");
        playerAnimator?.SetPersistentState("Scared Idle Animation");
    }

    private void StartBackstagePanting()
    {
        if (pantingRoutine != null)
            StopCoroutine(pantingRoutine);

        GameAudioManager.Instance?.StartPanting();
        pantingRoutine = StartCoroutine(StopPantingAfterMovementOrDelay());
    }

    private IEnumerator StopPantingAfterMovementOrDelay()
    {
        float elapsed = 0f;
        while (elapsed < 6f)
        {
            if (playerMovement != null && playerMovement.IsMoving)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        GameAudioManager.Instance?.StopPanting();
        pantingRoutine = null;
    }

    private IEnumerator CreditSequence()
    {
        IsCutsceneOpen = true;
        gameUIController?.ShowCursorForUI();
        if (creditPanel != null)
            creditPanel.SetActive(true);

        float elapsed = 0f;
        while (elapsed < creditMaximumDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (elapsed >= creditMinimumDuration && IsDismissKeyPressed())
                break;

            yield return null;
        }

        if (creditPanel != null)
            creditPanel.SetActive(false);
        IsCutsceneOpen = false;
        gameProgress.ReturnToMainMenu();
    }

    private IEnumerator AutoRunTo(Transform targetPoint)
    {
        Vector3 target = GetPointPosition(targetPoint, player.position);
        if (playerBody == null)
        {
            yield return MovePlayerTo(targetPoint, autoRunSpeed);
            yield break;
        }

        while (Vector2.Distance(player.position, target) > 0.12f)
        {
            Vector2 direction = (target - player.position).normalized;
            playerMovement?.FaceDirection(direction.x);
            playerBody.linearVelocity = direction * autoRunSpeed;
            yield return null;
        }

        player.position = target;
        playerBody.linearVelocity = Vector2.zero;
    }

    private IEnumerator MovePlayerTo(
        Transform targetPoint,
        float speed,
        bool updateFacing = true)
    {
        Vector3 target = GetPointPosition(targetPoint, player.position);
        if (updateFacing)
            playerMovement?.FaceDirection(target.x - player.position.x);
        while (Vector3.Distance(player.position, target) > 0.03f)
        {
            player.position = Vector3.MoveTowards(
                player.position,
                target,
                speed * Time.deltaTime);
            yield return null;
        }

        player.position = target;
    }

    private IEnumerator MovePlayerToWithAnimation(
        Transform targetPoint,
        string stateName,
        bool moveAtAnimationEvent)
    {
        Vector3 targetPosition = GetPointPosition(targetPoint, player.position);
        playerMovement?.FaceDirection(1f);
        playerAnimator?.PrepareClimbHeightMoment();

        // On descent the root returns to ground immediately. The downward clip
        // starts with the body visually raised, then brings it down to the root.
        if (!moveAtAnimationEvent)
            SetPlayerPosition(targetPosition);

        Coroutine animation = StartPlayerAnimation(stateName);

        if (moveAtAnimationEvent)
        {
            if (animation != null)
                yield return animation;
            SetPlayerPosition(targetPosition);
            yield break;
        }

        if (animation != null)
            yield return animation;
    }

    private void SetPlayerPosition(Vector3 targetPosition)
    {
        player.position = targetPosition;
        if (playerBody == null)
            return;

        playerBody.position = targetPosition;
        playerBody.linearVelocity = Vector2.zero;
    }

    private string GetClimbState(bool climbingUp)
    {
        bool hasBackpack = gameProgress != null &&
            gameProgress.HasCompleted(GameProgressEvent.BagCollected);
        if (hasBackpack)
            return climbingUp ? "Climb Up BK Animation" : "Climb Down BK Animation";

        return climbingUp ? "Climb Up Animation" : "Climb Down Animation";
    }

    private string GetWalkState()
    {
        return gameProgress != null &&
               gameProgress.HasCompleted(GameProgressEvent.BagCollected)
            ? "Walk BK Animation"
            : "Walk Animation";
    }

    private IEnumerator PlayPlayerAnimation(string stateName)
    {
        if (playerAnimator != null)
            yield return playerAnimator.PlayAction(stateName);
    }

    private Coroutine StartPlayerAnimation(string stateName)
    {
        return playerAnimator != null
            ? StartCoroutine(playerAnimator.PlayAction(stateName))
            : null;
    }

    private static Vector3 GetPointPosition(Transform point, Vector3 fallback)
    {
        if (point == null)
            return fallback;

        Vector3 result = point.position;
        result.z = fallback.z;
        return result;
    }

    private void ApplyKeyRoomExpandedLimits()
    {
        if (sections == null || sections.Length <= 1 || sections[1] == null)
            return;

        sections[1].SetRightLimits(
            keyRoomPlayerRightLimitAfterKey,
            keyRoomCameraRightLimitAfterKey);
    }

    private IEnumerator WaitForCutsceneDismiss()
    {
        while (IsDismissKeyHeld())
            yield return null;

        while (!IsDismissKeyPressed())
            yield return null;
    }

    private IEnumerator WaitForTutorialDismiss()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            yield break;

        while (keyboard.eKey.isPressed)
            yield return null;

        while (!keyboard.eKey.wasPressedThisFrame)
            yield return null;
    }

    private void PrepareTutorialHUD()
    {
        if (tutorialHUDPanel == null)
            tutorialHUDPanel = FindTutorialHUDPanel();

        if (tutorialHUDPanel == null)
            return;

        tutorialHUDBaseScale = tutorialHUDPanel.localScale;
        tutorialHUDCanvas = tutorialHUDPanel.GetComponent<CanvasGroup>();
        if (tutorialHUDCanvas == null)
            tutorialHUDCanvas = tutorialHUDPanel.gameObject.AddComponent<CanvasGroup>();

        tutorialHUDCanvas.alpha = 0f;
        tutorialHUDCanvas.blocksRaycasts = false;
        tutorialHUDCanvas.interactable = false;
        tutorialHUDPanel.gameObject.SetActive(false);
    }

    private RectTransform FindTutorialHUDPanel()
    {
        RectTransform[] rectTransforms =
            Resources.FindObjectsOfTypeAll<RectTransform>();
        RectTransform hudRoot = null;

        for (int i = 0; i < rectTransforms.Length; i++)
        {
            RectTransform candidate = rectTransforms[i];
            if (!candidate.gameObject.scene.IsValid())
                continue;

            if (string.Equals(
                candidate.name,
                "HUD",
                StringComparison.OrdinalIgnoreCase))
            {
                hudRoot = candidate;
            }

            if (!HasParentNamed(candidate, "HUD"))
            {
                continue;
            }

            string objectName = candidate.name;
            if (objectName.IndexOf("tutorial", StringComparison.OrdinalIgnoreCase) >= 0 ||
                objectName.Contains("توتوریال") ||
                objectName.Contains("آموزش"))
            {
                return candidate;
            }
        }

        if (hudRoot != null)
        {
            for (int i = 0; i < hudRoot.childCount; i++)
            {
                RectTransform child = hudRoot.GetChild(i) as RectTransform;
                if (child != null &&
                    !string.Equals(
                        child.name,
                        "Text Box",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return child;
                }
            }
        }

        return null;
    }

    private static bool HasParentNamed(Transform target, string parentName)
    {
        Transform current = target.parent;
        while (current != null)
        {
            if (string.Equals(
                current.name,
                parentName,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private IEnumerator ShowTutorialHUD()
    {
        if (tutorialHUDPanel == null)
            PrepareTutorialHUD();

        if (tutorialHUDPanel == null || tutorialHUDCanvas == null)
            yield break;

        IsCutsceneOpen = true;
        tutorialHUDPanel.gameObject.SetActive(true);
        tutorialHUDCanvas.alpha = 0f;
        tutorialHUDCanvas.blocksRaycasts = true;
        tutorialHUDPanel.localScale =
            tutorialHUDBaseScale * tutorialHUDStartScale;

        float elapsed = 0f;
        while (elapsed < tutorialHUDTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = tutorialHUDTransitionDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / tutorialHUDTransitionDuration);
            float eased = cutsceneEase.Evaluate(progress);
            tutorialHUDCanvas.alpha = eased;
            tutorialHUDPanel.localScale = Vector3.Lerp(
                tutorialHUDBaseScale * tutorialHUDStartScale,
                tutorialHUDBaseScale,
                eased);
            yield return null;
        }

        tutorialHUDCanvas.alpha = 1f;
        tutorialHUDPanel.localScale = tutorialHUDBaseScale;
    }

    private IEnumerator HideTutorialHUD()
    {
        if (tutorialHUDPanel == null || tutorialHUDCanvas == null)
        {
            IsCutsceneOpen = false;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < tutorialHUDTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = tutorialHUDTransitionDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / tutorialHUDTransitionDuration);
            float eased = cutsceneEase.Evaluate(progress);
            tutorialHUDCanvas.alpha = 1f - eased;
            tutorialHUDPanel.localScale = Vector3.Lerp(
                tutorialHUDBaseScale,
                tutorialHUDBaseScale * tutorialHUDStartScale,
                eased);
            yield return null;
        }

        tutorialHUDCanvas.alpha = 0f;
        tutorialHUDCanvas.blocksRaycasts = false;
        tutorialHUDPanel.localScale = tutorialHUDBaseScale;
        tutorialHUDPanel.gameObject.SetActive(false);
        IsCutsceneOpen = false;
    }

    private IEnumerator WaitForCutsceneDismissOrTimeout(float duration)
    {
        float elapsed = 0f;

        while (IsDismissKeyHeld() && elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        while (!IsDismissKeyPressed() && elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private static bool IsInteractionPressed()
    {
        return Keyboard.current != null &&
               Keyboard.current.eKey.wasPressedThisFrame;
    }

    private static bool IsDismissKeyHeld()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null &&
               (keyboard.eKey.isPressed ||
                keyboard.spaceKey.isPressed ||
                keyboard.enterKey.isPressed ||
                keyboard.numpadEnterKey.isPressed ||
                keyboard.escapeKey.isPressed);
    }

    private static bool IsDismissKeyPressed()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null &&
               (keyboard.eKey.wasPressedThisFrame ||
                keyboard.spaceKey.wasPressedThisFrame ||
                keyboard.enterKey.wasPressedThisFrame ||
                keyboard.numpadEnterKey.wasPressedThisFrame ||
                keyboard.escapeKey.wasPressedThisFrame);
    }

    private void KeepPlayerInsideCurrentRoom()
    {
        StorySectionLayout section = GetCurrentSection();
        if (section == null)
            return;

        float clampedX = section.ClampPlayerX(player.position.x);

        if (Mathf.Approximately(clampedX, player.position.x))
            return;

        player.position = new Vector3(clampedX, player.position.y, player.position.z);
        if (playerBody != null)
            playerBody.linearVelocity = Vector2.zero;
    }

    private void FindReferences()
    {
        if (gameProgress == null)
            gameProgress = FindAnyObjectByType<GameProgress>();
        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (playerAnimator == null)
            playerAnimator = FindAnyObjectByType<PlayerAnimator>();
        if (gameUIController == null)
            gameUIController = FindAnyObjectByType<GameUIController>();
        if (player == null && playerMovement != null)
            player = playerMovement.transform;
        if (dialogueHUD == null)
            dialogueHUD = FindAnyObjectByType<DialogueHUD>();
        if (player != null)
            playerBody = player.GetComponent<Rigidbody2D>();

        if (routeCamera == null)
            routeCamera = Camera.main;
        if (keyShadowCameraPoint == null ||
            gameStartPoint == null ||
            fuseShadowCameraPoint == null ||
            backstageCurtainCameraPoint == null ||
            keyRoomPlayerRightLimitAfterKey == null ||
            keyRoomCameraRightLimitAfterKey == null ||
            returnCurtainVisual == null ||
            returnCurtain == null ||
            customRackShadow == null ||
            customRackReal == null)
        {
            Transform[] sceneTransforms = FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < sceneTransforms.Length; i++)
            {
                if (keyShadowCameraPoint == null &&
                    sceneTransforms[i].name == "CAMERA POINT - CUT SCENE")
                    keyShadowCameraPoint = sceneTransforms[i];

                if (gameStartPoint == null &&
                    sceneTransforms[i].name == "SPAWN - GAME START")
                    gameStartPoint = sceneTransforms[i];

                if (fuseShadowCameraPoint == null &&
                    sceneTransforms[i].name == "CAMERA POINT - FUSE SHADOW")
                    fuseShadowCameraPoint = sceneTransforms[i];

                if (backstageCurtainCameraPoint == null &&
                    sceneTransforms[i].name == "CAMERA POINT - BACKSTAGE CURTAIN")
                    backstageCurtainCameraPoint = sceneTransforms[i];

                if (keyRoomPlayerRightLimitAfterKey == null &&
                    sceneTransforms[i].name == "PLAYER LIMIT - RIGHT - AFTER KEY")
                    keyRoomPlayerRightLimitAfterKey = sceneTransforms[i];

                if (keyRoomCameraRightLimitAfterKey == null &&
                    sceneTransforms[i].name == "CAMERA LIMIT - RIGHT - AFTER KEY")
                    keyRoomCameraRightLimitAfterKey = sceneTransforms[i];

                if ((returnCurtainVisual == null || returnCurtain == null) &&
                    sceneTransforms[i].name == "White Curtain")
                {
                    returnCurtainVisual = sceneTransforms[i];
                    returnCurtain = sceneTransforms[i]
                        .GetComponent<StoryInteractable>();
                }

                if (customRackShadow == null &&
                    sceneTransforms[i].name == "Custom rack Shadow")
                    customRackShadow = sceneTransforms[i].gameObject;

                if (customRackReal == null &&
                    sceneTransforms[i].name == "Custom rack Real")
                    customRackReal = sceneTransforms[i].gameObject;

                if (keyShadowCameraPoint != null &&
                    gameStartPoint != null &&
                    fuseShadowCameraPoint != null &&
                    backstageCurtainCameraPoint != null &&
                    keyRoomPlayerRightLimitAfterKey != null &&
                    keyRoomCameraRightLimitAfterKey != null &&
                    returnCurtainVisual != null &&
                    returnCurtain != null &&
                    customRackShadow != null &&
                    customRackReal != null)
                    break;
            }
        }
        if (routeCamera != null)
        {
            SectionCameraController sectionCamera =
                routeCamera.GetComponent<SectionCameraController>();
            if (sectionCamera != null)
                sectionCamera.enabled = false;
        }

        if (globalLight != null)
            normalLightIntensity = globalLight.intensity;
    }

    private void UpdateRouteObjects()
    {
        bool bagCollected = gameProgress.HasCompleted(GameProgressEvent.BagCollected);
        bool decorationRevealed = gameProgress.HasCompleted(GameProgressEvent.DecorationRevealed);
        bool curtainOpened = gameProgress.HasCompleted(GameProgressEvent.CurtainOpened);

        playerAnimator?.SetBackpackEquipped(bagCollected);

        if (sections != null)
        {
            for (int i = 0; i < sections.Length; i++)
            {
                if (sections[i] != null)
                    sections[i].gameObject.SetActive(i == currentRoom);
            }
        }

        SetActive(tutorialItem, currentRoom == 0 &&
            !gameProgress.HasCompleted(GameProgressEvent.TutorialItemRead));
        SetActive(hallwayDoor, currentRoom == 0 && !bagCollected);
        SetActive(courtyardDoor, currentRoom == 0);

        SetActive(keyRoomDoor, currentRoom == 1);
        SetActive(keyRoomReturnDoor, currentRoom == 1);
        if (pushBox != null)
            pushBox.gameObject.SetActive(currentRoom == 1);
        if (highKey != null)
        {
            highKey.SetActive(currentRoom == 1);
            SetInteractionVisual(
                highKey,
                gameProgress.HasCompleted(GameProgressEvent.KeyCollected));
        }

        SetActive(fuseBox, currentRoom == 2);
        SetActive(toolbox, currentRoom == 2);
        if (toolbox != null)
        {
            SetInteractionVisual(
                toolbox.gameObject,
                gameProgress.HasCompleted(GameProgressEvent.FusePartCollected));
        }
        if (fuseBox != null)
        {
            SetInteractionVisual(
                fuseBox.gameObject,
                gameProgress.HasCompleted(GameProgressEvent.FuseRepaired));
        }
        SetActive(fuseReturnDoor, currentRoom == 2);
        if (playerShadow != null)
            playerShadow.SetActive(currentRoom == 2 &&
                gameProgress.HasCompleted(GameProgressEvent.FuseRepaired));

        SetActive(weight, currentRoom == 3 &&
            !gameProgress.HasCompleted(GameProgressEvent.WeightCollected));
        SetActive(lockedStageDoor, currentRoom == 3);
        SetActive(backstageReturnDoor, currentRoom == 3);
        if (backstageStairs != null)
            backstageStairs.SetActive(currentRoom == 3);
        SetActive(backpack, currentRoom == 3 && !bagCollected);
        if (curtainShadow != null)
            curtainShadow.SetActive(currentRoom == 3 && !curtainOpened && !decorationRevealed);
        if (revealedDecoration != null)
            revealedDecoration.SetActive(currentRoom == 3 &&
                (curtainOpened || decorationRevealed));
        if (curtainClosedVisual != null)
            curtainClosedVisual.SetActive(currentRoom == 3 && !curtainOpened);
        if (curtainOpenVisual != null)
            curtainOpenVisual.SetActive(currentRoom == 3 && curtainOpened);
        if (lowerWeightedRopeVisual != null)
            lowerWeightedRopeVisual.SetActive(false);

        SetActive(curtainRope, currentRoom == 4);
        if (curtainWeightPull != null)
            curtainWeightPull.ApplyState(curtainOpened);
        if (upperStairs != null)
            upperStairs.SetActive(currentRoom == 4 &&
                gameProgress.HasCompleted(GameProgressEvent.CurtainOpened));
    }

    private static void SetActive(Component component, bool value)
    {
        if (component != null)
            component.gameObject.SetActive(value);
    }

    private void ApplyRoomLighting()
    {
        bool fuseRepaired =
            gameProgress.HasCompleted(GameProgressEvent.FuseRepaired);

        if (farLight != null)
        {
            farLight.enabled = fuseRepaired;
            if (fuseRepaired)
                farLight.intensity = 1.3f;
        }

        if (globalLight == null)
            return;

        bool darkFuseRoom = currentRoom == 2 &&
                            !fuseRepaired;
        globalLight.intensity = darkFuseRoom
            ? Mathf.Min(normalLightIntensity, 0.22f)
            : normalLightIntensity;
    }

    private void SetCameraToRoom(int room)
    {
        if (routeCamera == null || sections == null || room < 0 || room >= sections.Length)
            return;

        StorySectionLayout section = sections[room];
        if (section != null)
            routeCamera.transform.position = section.GetCameraPosition(
                routeCamera.transform.position,
                player != null ? player.position : routeCamera.transform.position);
    }

    private void UpdateCameraPosition()
    {
        if (routeCamera == null || player == null)
            return;

        StorySectionLayout section = GetCurrentSection();
        if (section != null)
            routeCamera.transform.position = section.GetCameraPosition(
                routeCamera.transform.position,
                player.position);
    }

    private StorySectionLayout GetCurrentSection()
    {
        if (sections == null || currentRoom < 0 || currentRoom >= sections.Length)
            return null;

        return sections[currentRoom];
    }

    private void Say(string message)
    {
        GameAudioManager.Instance?.PlayDialogue();
        dialogueHUD?.ShowDialogue(message);
    }

    private void SetPlayerControl(bool value)
    {
        if (playerMovement == null)
            return;

        playerMovement.ResetMotion();
        playerMovement.enabled = value;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeCanvas == null)
            yield break;

        float elapsed = 0f;
        fadeCanvas.alpha = from;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = fadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / fadeDuration);
            fadeCanvas.alpha = Mathf.Lerp(from, to, progress);
            yield return null;
        }

        fadeCanvas.alpha = to;
    }

    private IEnumerator ShowStoryImageAnimated(
        Sprite sprite,
        Color fallbackColor,
        string label)
    {
        if (storyCanvas == null || storyImage == null)
            yield break;

        IsCutsceneOpen = true;
        storyImage.sprite = sprite;
        storyImage.color = sprite == null ? fallbackColor : Color.white;
        storyImage.preserveAspect = true;
        if (storyLabel != null)
            storyLabel.text = sprite == null ? label : string.Empty;

        storyCanvas.alpha = 0f;
        storyCanvas.blocksRaycasts = true;
        storyImage.rectTransform.localScale = storyImageBaseScale * cutsceneStartScale;

        float elapsed = 0f;
        while (elapsed < cutsceneTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = cutsceneTransitionDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / cutsceneTransitionDuration);
            float eased = cutsceneEase.Evaluate(progress);
            storyCanvas.alpha = eased;
            storyImage.rectTransform.localScale = Vector3.Lerp(
                storyImageBaseScale * cutsceneStartScale,
                storyImageBaseScale,
                eased);
            yield return null;
        }

        storyCanvas.alpha = 1f;
        storyImage.rectTransform.localScale = storyImageBaseScale;
    }

    private IEnumerator HideStoryImageAnimated()
    {
        if (storyCanvas == null)
        {
            IsCutsceneOpen = false;
            yield break;
        }

        float elapsed = 0f;
        Vector3 startScale = storyImage != null
            ? storyImage.rectTransform.localScale
            : storyImageBaseScale;
        while (elapsed < cutsceneTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = cutsceneTransitionDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / cutsceneTransitionDuration);
            float eased = cutsceneEase.Evaluate(progress);
            storyCanvas.alpha = 1f - eased;
            if (storyImage != null)
            {
                storyImage.rectTransform.localScale = Vector3.Lerp(
                    startScale,
                    storyImageBaseScale * cutsceneStartScale,
                    eased);
            }
            yield return null;
        }

        storyCanvas.alpha = 0f;
        storyCanvas.blocksRaycasts = false;
        if (storyImage != null)
            storyImage.rectTransform.localScale = storyImageBaseScale;
        IsCutsceneOpen = false;
    }

    private void HandleProgressChanged(GameProgressEvent progressEvent)
    {
        dialogueHUD?.HideDialogue();
    }

    private static void SetInteractionVisual(GameObject item, bool showAfter)
    {
        if (item == null)
            return;

        InteractionVisualState visualState =
            item.GetComponent<InteractionVisualState>();
        if (visualState == null)
            return;

        if (showAfter)
            visualState.ShowAfterInteraction();
        else
            visualState.ShowBeforeInteraction();
    }
}
