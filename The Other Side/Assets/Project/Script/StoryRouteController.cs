using System.Collections;
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
    [SerializeField] private Transform startEntryFromKeyRoom;
    [SerializeField] private Transform endingWalkTarget;

    [Header("Room 02 Design Points")]
    [SerializeField] private Transform keyRoomEntryFromStart;
    [SerializeField] private Transform keyRoomEntryFromFuse;
    [SerializeField] private Transform boxClimbGroundPoint;
    [SerializeField] private Transform boxClimbTopPoint;
    [SerializeField] private Transform keyPickupPoint;
    [SerializeField] private Transform keyShadowCameraPoint;

    [Header("Room 03 Design Points")]
    [SerializeField] private Transform fuseRoomEntryFromKey;
    [SerializeField] private Transform fuseRoomEntryFromBackstage;
    [SerializeField] private Transform fuseEscapeTarget;

    [Header("Room 04 Design Points")]
    [SerializeField] private Transform backstageEntryFromFuse;
    [SerializeField] private Transform backstageEntryFromUpper;

    [Header("Room 05 Design Points")]
    [SerializeField] private Transform upperEntryFromBackstage;

    [Header("Scene UI")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private CanvasGroup storyCanvas;
    [SerializeField] private Image storyImage;
    [SerializeField] private TMP_Text storyLabel;
    [SerializeField] private GameObject creditPanel;

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
    [SerializeField] private float endingWalkDuration = 2.4f;
    [SerializeField] private float shortCutsceneDuration = 1.4f;
    [SerializeField] private float creditMinimumDuration = 10f;
    [SerializeField] private float creditMaximumDuration = 60f;

    [Header("Cutscene Presentation")]
    [SerializeField] private float cutsceneTransitionDuration = 0.2f;
    [SerializeField] private float cutsceneStartScale = 0.82f;
    [SerializeField] private float keyShadowCameraMoveDuration = 0.35f;
    [SerializeField, Range(0.5f, 1f)] private float keyShadowCameraZoom = 0.82f;
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
    private bool waitingForKeyPickup;
    private bool introFinished;
    private bool cameraFocusActive;
    private Vector3 storyImageBaseScale = Vector3.one;

    private static readonly string[] IntroLines =
    {
        "آفرین پسرم، خیلی خوب نمایش اجرا کردی.",
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
        if (creditPanel != null)
            creditPanel.SetActive(false);
        if (keyRoomShadow != null &&
            !gameProgress.HasCompleted(GameProgressEvent.KeyCollected))
        {
            keyRoomShadow.ResetHidden();
        }
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
            Say("باید یه چیزی پیدا کنم بزارم زیر پام که قدم برسه.");
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
                    Say("میخوام اول اون برگه رو بردارم.");
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
                    Say("میخوام بفهمم اون سایه چیه؟");
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
                else
                {
                    GameAudioManager.Instance?.PlayDoorOpen();
                    yield return ChangeRoom(0, startEntryFromKeyRoom);
                }
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
                Say("باید وسیله سنگین رو پیدا کنم.");
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
        yield return ShowStoryImageAnimated(
            tutorialPaperImage,
            new Color(0.86f, 0.79f, 0.62f, 1f),
            "TUTORIAL PAPER PLACEHOLDER");

        yield return WaitForCutsceneDismiss();

        yield return HideStoryImageAnimated();
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

        IsCutsceneOpen = true;
        cameraFocusActive = true;
        Coroutine keyGrabAnimation = StartPlayerAnimation("Key Grab Animation");
        yield return MoveCameraToKeyShadow();
        if (keyGrabAnimation != null)
            yield return keyGrabAnimation;
        GameAudioManager.Instance?.PlayScare();
        yield return PlayPlayerAnimation("Scared Animation");
        playerAnimator?.SetPersistentState("Scared Idle Animation");
        Say("این دیگه چیه؟");
        yield return new WaitForSeconds(2.2f);
        Say("باید زودتر از اینجا برم.");
        yield return new WaitForSeconds(2.2f);

        yield return WaitForCutsceneDismiss();

        gameProgress.TryComplete(GameProgressEvent.KeyCollected);
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

    private IEnumerator MoveCameraTo(
        Vector3 targetPosition,
        float targetSize,
        float duration)
    {
        if (routeCamera == null)
            yield break;

        Vector3 startPosition = routeCamera.transform.position;
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
        Say("ممکنه داخل این جعبه ابزار چیزی که لازمه رو پیدا کنم.");
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

        playerShadow.SetActive(true);
        yield return ShowStoryImageAnimated(
            ownShadowImage,
            new Color(0.05f, 0.05f, 0.08f, 0.92f),
            "PLAYER SHADOW SCARE PLACEHOLDER");
        GameAudioManager.Instance?.PlayScare();
        yield return PlayPlayerAnimation("Scared Animation");
        playerAnimator?.SetPersistentState("Scared Idle Animation");
        Say("اون سایه دوباره پیداش شد!");
        yield return WaitForCutsceneDismiss();
        yield return HideStoryImageAnimated();

        Say("داره دنبالم میاد... باید فرار کنم!");
        yield return new WaitForSeconds(escapeShockDuration);

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

        yield return ShowStoryImageAnimated(
            decorationRevealImage,
            new Color(0.26f, 0.22f, 0.2f, 1f),
            "STAGE DECORATION REVEAL PLACEHOLDER");
        Say("پس سایه دکور بود که اینهمه ازش ترسیدم!");
        yield return WaitForCutsceneDismiss();
        yield return HideStoryImageAnimated();

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
        yield return ShowStoryImageAnimated(
            ownShadowImage,
            new Color(0.09f, 0.09f, 0.12f, 0.9f),
            "OWN SHADOW REVEAL PLACEHOLDER");
        Say("یعنی من از سایه خودم انقدر ترسیدم؟");
        yield return WaitForCutsceneDismiss();
        yield return HideStoryImageAnimated();
        SetPlayerControl(true);
        isBusy = false;
    }

    private IEnumerator ReturnKeySequence()
    {
        isBusy = true;
        SetPlayerControl(false);
        Say("نکنه سایه ای که اینجا دیده بودم هم واقعا موجود ترسناکی نبوده!");
        yield return new WaitForSeconds(2.8f);

        yield return MovePlayerTo(boxClimbGroundPoint, walkToPointSpeed);
        playerMovement?.FaceDirection(1f);
        GameAudioManager.Instance?.PlayClimb();
        yield return MovePlayerToWithAnimation(
            boxClimbTopPoint,
            GetClimbState(true),
            true);

        yield return ShowStoryImageAnimated(
            costumeRevealImage,
            new Color(0.32f, 0.27f, 0.34f, 1f),
            "COSTUME SHADOW REVEAL PLACEHOLDER");
        Say("پس هیچ هیولایی در کار نبود!");
        yield return WaitForCutsceneDismiss();
        yield return HideStoryImageAnimated();
        keyRoomShadow?.Hide();

        playerMovement?.FaceDirection(1f);
        GameAudioManager.Instance?.PlayClimb();
        yield return MovePlayerToWithAnimation(
            boxClimbGroundPoint,
            GetClimbState(false),
            false);
        Say("باید برم به سمت حیاط، مادرم منتظرمه.");
        SetPlayerControl(true);
        isBusy = false;
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
                StartCoroutine(ReturnKeySequence());
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
        Say("اون سایه روی پرده چیه؟");
        yield return new WaitForSeconds(2.4f);
        Say("باید بفهمم پشت این پرده چی قایم شده.");
        yield return new WaitForSeconds(2.4f);
        Say("باید یه جوری طناب پرده رو بکشم پایین تا پرده بره کنار.");
        backstageIntroComplete = true;
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
            // The climb-up clips call OnClimbHeightMoment on the exact frame
            // where the player's root should move onto the chair.
            float elapsed = 0f;
            const float missingEventFallback = 0.75f;
            while (playerAnimator != null &&
                   !playerAnimator.ClimbHeightMomentReached &&
                   elapsed < missingEventFallback)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            SetPlayerPosition(targetPosition);
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

    private IEnumerator WaitForCutsceneDismiss()
    {
        while (IsDismissKeyHeld())
            yield return null;

        while (!IsDismissKeyPressed())
            yield return null;
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
        if (keyShadowCameraPoint == null)
        {
            Transform[] sceneTransforms = FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < sceneTransforms.Length; i++)
            {
                if (sceneTransforms[i].name == "CAMERA POINT - CUT SCENE")
                {
                    keyShadowCameraPoint = sceneTransforms[i];
                    break;
                }
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

        SetActive(keyRoomDoor, currentRoom == 1 && !bagCollected);
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
            lowerWeightedRopeVisual.SetActive(currentRoom == 3 && curtainOpened);

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
