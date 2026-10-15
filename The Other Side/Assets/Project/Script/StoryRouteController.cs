using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class StoryRouteController : MonoBehaviour
{
    private const float RoomHalfWidth = 7.4f;
    private const float PlayerY = -2.6f;

    [Header("References")]
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private DialogueHUD dialogueHUD;
    [SerializeField] private Light2D globalLight;

    [Header("Placeholder cutscene images")]
    [SerializeField] private Sprite tutorialPaperImage;
    [SerializeField] private Sprite firstShadowImage;
    [SerializeField] private Sprite curtainOpeningImage;
    [SerializeField] private Sprite decorationRevealImage;
    [SerializeField] private Sprite ownShadowImage;
    [SerializeField] private Sprite costumeRevealImage;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float autoRunSpeed = 6f;
    [SerializeField] private float shortCutsceneDuration = 1.4f;

    private readonly float[] roomCenters = { 0f, 18f, 36f, 54f, 72f };

    private Rigidbody2D playerBody;
    private Camera routeCamera;
    private CanvasGroup fadeCanvas;
    private CanvasGroup storyCanvas;
    private Image storyImage;
    private TMP_Text storyLabel;
    private Transform routeRoot;
    private float normalLightIntensity = 1f;
    private int currentRoom;
    private bool isBusy;
    private bool keyDistanceDialogueShown;
    private bool backstageIntroShown;

    private StoryInteractable tutorialItem;
    private StoryInteractable hallwayDoor;
    private StoryInteractable courtyardDoor;
    private StoryInteractable keyRoomDoor;
    private StoryInteractable fuseBox;
    private StoryInteractable toolbox;
    private StoryInteractable weight;
    private StoryInteractable lockedStageDoor;
    private StoryInteractable curtainRope;
    private StoryInteractable backpack;
    private StoryInteractable backstageReturnDoor;
    private StoryInteractable fuseReturnDoor;
    private StoryInteractable keyRoomReturnDoor;
    private StoryPushBoxInteractable pushBox;
    private GameObject highKey;
    private GameObject backstageStairs;
    private GameObject upperStairs;
    private GameObject curtainShadow;
    private GameObject revealedDecoration;
    private GameObject playerShadow;
    private SpriteRenderer fuseRenderer;
    private Light2D farLight;

    public bool CanUsePushBox =>
        currentRoom == 1 &&
        gameProgress.HasCompleted(GameProgressEvent.EnteredKeyRoom) &&
        !gameProgress.HasCompleted(GameProgressEvent.BoxPlaced) &&
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
        DisableLegacyGameplay();
        CreateFadeCanvas();
        CreateStoryCanvas();
        CreateRouteObjects();
        SetCameraToRoom(0);
        UpdateRouteObjects();
        ApplyRoomLighting();
    }

    private void Update()
    {
        if (player == null || isBusy)
            return;

        KeepPlayerInsideCurrentRoom();

        if (currentRoom == 1 &&
            !keyDistanceDialogueShown &&
            !gameProgress.HasCompleted(GameProgressEvent.BoxPlaced) &&
            Mathf.Abs(player.position.x - highKey.transform.position.x) < 2.25f)
        {
            keyDistanceDialogueShown = true;
            Say("باید یه چیزی پیدا کنم بزارم زیر پام که قدم برسه.");
        }
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
                       !gameProgress.HasCompleted(GameProgressEvent.FusePartCollected);
            case StoryInteractionType.FuseBox:
                return currentRoom == 2 &&
                       !gameProgress.HasCompleted(GameProgressEvent.FuseRepaired);
            case StoryInteractionType.Weight:
                return currentRoom == 3 &&
                       !gameProgress.HasCompleted(GameProgressEvent.WeightCollected);
            case StoryInteractionType.LockedStageDoor:
                return currentRoom == 3;
            case StoryInteractionType.CurtainRope:
                return currentRoom == 4 &&
                       gameProgress.HasCompleted(GameProgressEvent.WeightCollected) &&
                       !gameProgress.HasCompleted(GameProgressEvent.CurtainOpened);
            case StoryInteractionType.Backpack:
                return currentRoom == 3 &&
                       gameProgress.HasCompleted(GameProgressEvent.DecorationRevealed) &&
                       !gameProgress.HasCompleted(GameProgressEvent.BagCollected);
            case StoryInteractionType.BackstageReturnDoor:
                return currentRoom == 3 &&
                       gameProgress.HasCompleted(GameProgressEvent.BagCollected);
            case StoryInteractionType.FuseReturnDoor:
                return currentRoom == 2 &&
                       gameProgress.HasCompleted(GameProgressEvent.BagCollected);
            case StoryInteractionType.KeyRoomReturnDoor:
                return currentRoom == 1 &&
                       gameProgress.HasCompleted(GameProgressEvent.BagCollected);
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
                    Say("اول باید اون برگه رو بردارم.");
                else
                    yield return ChangeRoom(1, 12.2f);
                break;
            case StoryInteractionType.CourtyardDoor:
                if (!gameProgress.HasCompleted(GameProgressEvent.BagCollected))
                    Say("باید از پشت صحنه کوله پشتیمو بردارم.");
                break;
            case StoryInteractionType.KeyRoomDoor:
                if (!gameProgress.HasCompleted(GameProgressEvent.KeyCollected))
                    Say("در قفله، باید دنبال کلید بگردم.");
                else
                    yield return ChangeRoom(2, 30.2f);
                break;
            case StoryInteractionType.Toolbox:
                yield return ToolboxSequence(interactedObject);
                break;
            case StoryInteractionType.FuseBox:
                if (!gameProgress.HasCompleted(GameProgressEvent.FusePartCollected))
                    Say("یه قطعه از جعبه فیوز کمه. باید پیداش کنم.");
                else
                    yield return FuseSequence();
                break;
            case StoryInteractionType.Weight:
                interactedObject.SetActive(false);
                gameProgress.TryComplete(GameProgressEvent.WeightCollected);
                Say("باید یه راهی پیدا کنم برم بالا، کنار طناب پرده نمایش.");
                UpdateRouteObjects();
                break;
            case StoryInteractionType.LockedStageDoor:
                Say("در قفله، باید یه راه دیگه وجود داشته باشه.");
                break;
            case StoryInteractionType.CurtainRope:
                yield return CurtainSequence();
                break;
            case StoryInteractionType.Backpack:
                interactedObject.SetActive(false);
                gameProgress.TryComplete(GameProgressEvent.BagCollected);
                Say("باید برم سمت حیاط.");
                UpdateRouteObjects();
                break;
            case StoryInteractionType.BackstageReturnDoor:
                yield return ChangeRoom(2, 42.2f);
                break;
            case StoryInteractionType.FuseReturnDoor:
                yield return ChangeRoom(1, 24.2f);
                break;
            case StoryInteractionType.KeyRoomReturnDoor:
                yield return ChangeRoom(0, 6.2f);
                break;
        }
    }

    public void NotifyBoxPlaced()
    {
        if (isBusy || !gameProgress.TryComplete(GameProgressEvent.BoxPlaced))
            return;

        StartCoroutine(KeySequence());
    }

    public void EnterArea(StoryAreaType type)
    {
        if (isBusy)
            return;

        if (type == StoryAreaType.BackstageStairsUp && currentRoom == 3)
        {
            if (!gameProgress.HasCompleted(GameProgressEvent.WeightCollected))
            {
                Say("اول باید وسیله سنگین رو پیدا کنم.");
                return;
            }

            StartCoroutine(ChangeRoom(4, 67.5f));
        }
        else if (type == StoryAreaType.UpperStairsDown &&
                 currentRoom == 4 &&
                 gameProgress.HasCompleted(GameProgressEvent.CurtainOpened))
        {
            StartCoroutine(ChangeRoom(3, 57.2f));
        }
    }

    private IEnumerator TutorialSequence(GameObject item)
    {
        isBusy = true;
        SetPlayerControl(false);
        item.SetActive(false);
        ShowStoryImage(
            tutorialPaperImage,
            new Color(0.86f, 0.79f, 0.62f, 1f),
            "TUTORIAL PAPER PLACEHOLDER");

        yield return WaitForFreshEPress();

        HideStoryImage();
        gameProgress.TryComplete(GameProgressEvent.TutorialItemRead);
        SetPlayerControl(true);
        isBusy = false;
        UpdateRouteObjects();
    }

    private IEnumerator KeySequence()
    {
        isBusy = true;
        SetPlayerControl(false);

        Vector3 groundPosition = player.position;
        yield return MovePlayerTo(new Vector3(21f, PlayerY, groundPosition.z), 3.5f);
        yield return MovePlayerTo(new Vector3(21f, PlayerY + 1.25f, groundPosition.z), 2.5f);

        ShowStoryImage(
            firstShadowImage,
            new Color(0.08f, 0.08f, 0.12f, 1f),
            "FIRST SCARY SHADOW PLACEHOLDER");
        Say("این دیگه چیه؟");
        yield return new WaitForSeconds(2.2f);
        Say("باید زودتر از اینجا برم.");
        yield return new WaitForSeconds(2.2f);

        yield return WaitForFreshEPress();

        gameProgress.TryComplete(GameProgressEvent.KeyCollected);
        highKey.SetActive(false);
        HideStoryImage();
        yield return MovePlayerTo(groundPosition, 2.5f);

        SetPlayerControl(true);
        isBusy = false;
        UpdateRouteObjects();
    }

    private IEnumerator ToolboxSequence(GameObject toolboxObject)
    {
        isBusy = true;
        SetPlayerControl(false);
        Say("ممکنه داخل این جعبه ابزار چیزی که لازمه رو پیدا کنم.");
        yield return new WaitForSeconds(2.2f);

        toolboxObject.SetActive(false);
        gameProgress.TryComplete(GameProgressEvent.FusePartCollected);
        Say("قطعه رو پیدا کردم. باید بزارمش داخل جعبه فیوز.");

        SetPlayerControl(true);
        isBusy = false;
        UpdateRouteObjects();
    }

    private IEnumerator FuseSequence()
    {
        isBusy = true;
        SetPlayerControl(false);
        gameProgress.TryComplete(GameProgressEvent.FuseRepaired);

        if (fuseRenderer != null)
            fuseRenderer.color = new Color(0.22f, 0.8f, 0.28f);
        if (farLight != null)
            farLight.intensity = 1.3f;
        if (globalLight != null)
            globalLight.intensity = normalLightIntensity;

        playerShadow.SetActive(true);
        ShowStoryImage(
            ownShadowImage,
            new Color(0.05f, 0.05f, 0.08f, 0.92f),
            "PLAYER SHADOW SCARE PLACEHOLDER");
        Say("اون سایه دوباره پیداش شد!");
        yield return new WaitForSeconds(shortCutsceneDuration);
        HideStoryImage();

        yield return AutoRunTo(42.3f);
        yield return ChangeRoom(3, 48.5f, false, false);

        isBusy = false;
        HandleRoomEntered(3);
    }

    private IEnumerator CurtainSequence()
    {
        isBusy = true;
        SetPlayerControl(false);
        Say("باید این وزنه رو به طناب پرده گره بزنم.");
        yield return new WaitForSeconds(2f);

        ShowStoryImage(
            curtainOpeningImage,
            new Color(0.3f, 0.12f, 0.18f, 1f),
            "CURTAIN OPENING PLACEHOLDER");
        yield return new WaitForSeconds(shortCutsceneDuration);
        gameProgress.TryComplete(GameProgressEvent.CurtainOpened);
        HideStoryImage();

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

        ShowStoryImage(
            decorationRevealImage,
            new Color(0.26f, 0.22f, 0.2f, 1f),
            "STAGE DECORATION REVEAL PLACEHOLDER");
        Say("پس سایه دکور بود که اینهمه ازش ترسیدم!");
        yield return new WaitForSeconds(2.8f);
        HideStoryImage();

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
        ShowStoryImage(
            ownShadowImage,
            new Color(0.09f, 0.09f, 0.12f, 0.9f),
            "OWN SHADOW REVEAL PLACEHOLDER");
        Say("یعنی من از سایه خودم انقدر ترسیدم؟");
        yield return new WaitForSeconds(2.8f);
        HideStoryImage();
        SetPlayerControl(true);
        isBusy = false;
    }

    private IEnumerator ReturnKeySequence()
    {
        isBusy = true;
        SetPlayerControl(false);
        Say("نکنه سایه ای که اینجا دیده بودم هم واقعا موجود ترسناکی نبوده!");
        yield return new WaitForSeconds(2.8f);

        Vector3 groundPosition = player.position;
        yield return MovePlayerTo(new Vector3(21f, PlayerY, groundPosition.z), 3.5f);
        yield return MovePlayerTo(new Vector3(21f, PlayerY + 1.25f, groundPosition.z), 2.5f);

        ShowStoryImage(
            costumeRevealImage,
            new Color(0.32f, 0.27f, 0.34f, 1f),
            "COSTUME SHADOW REVEAL PLACEHOLDER");
        Say("پس هیچ هیولایی در کار نبود!");
        yield return new WaitForSeconds(2.8f);
        HideStoryImage();

        yield return MovePlayerTo(groundPosition, 2.5f);
        Say("باید برم به سمت حیاط، مادرم منتظرمه.");
        SetPlayerControl(true);
        isBusy = false;
    }

    private IEnumerator EndingSequence()
    {
        isBusy = true;
        SetPlayerControl(false);
        gameProgress.TryComplete(GameProgressEvent.EnteredCourtyard);

        float targetX = -5.8f;
        if (playerBody != null)
            playerBody.linearVelocity = new Vector2(-2f, 0f);

        float elapsed = 0f;
        Vector3 startPosition = player.position;
        while (elapsed < 2.4f)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / 2.4f);
            Vector3 position = Vector3.Lerp(
                startPosition,
                new Vector3(targetX, PlayerY, startPosition.z),
                progress);
            player.position = position;
            fadeCanvas.alpha = progress;
            yield return null;
        }

        if (playerBody != null)
            playerBody.linearVelocity = Vector2.zero;

        fadeCanvas.alpha = 1f;
        gameProgress.TryComplete(GameProgressEvent.GameFinished);
    }

    private IEnumerator ChangeRoom(
        int targetRoom,
        float spawnX,
        bool manageBusy = true,
        bool runEntrySequence = true)
    {
        if (manageBusy)
        {
            isBusy = true;
            SetPlayerControl(false);
        }

        yield return Fade(0f, 1f);
        currentRoom = targetRoom;
        player.position = new Vector3(spawnX, PlayerY, player.position.z);
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
            Say("در قفله، باید دنبال کلید بگردم.");
        }
        else if (room == 1 && gameProgress.HasCompleted(GameProgressEvent.BagCollected))
        {
            if (gameProgress.TryComplete(GameProgressEvent.ReturnedToKeyRoom))
                StartCoroutine(ReturnKeySequence());
        }
        else if (room == 2 && !gameProgress.HasCompleted(GameProgressEvent.EnteredFuseRoom))
        {
            gameProgress.TryComplete(GameProgressEvent.EnteredFuseRoom);
            Say("اینجا خیلی تاریکه. باید قطعه گمشده جعبه فیوز رو پیدا کنم.");
        }
        else if (room == 2 && gameProgress.HasCompleted(GameProgressEvent.BagCollected))
        {
            if (gameProgress.TryComplete(GameProgressEvent.ReturnedToFuseRoom))
                StartCoroutine(ReturnFuseSequence());
        }
        else if (room == 3 && !gameProgress.HasCompleted(GameProgressEvent.EnteredBackstage))
        {
            gameProgress.TryComplete(GameProgressEvent.EnteredBackstage);
            StartCoroutine(BackstageIntroSequence());
        }
        else if (room == 3 &&
                 gameProgress.HasCompleted(GameProgressEvent.CurtainOpened) &&
                 !gameProgress.HasCompleted(GameProgressEvent.DecorationRevealed))
        {
            StartCoroutine(BackstageRevealSequence());
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
        Say("اون سایه روی پرده چیه؟");
        yield return new WaitForSeconds(2.4f);
        Say("باید بفهمم پشت این پرده چی قایم شده.");
        yield return new WaitForSeconds(2.4f);
        Say("باید یه جوری طناب پرده رو بکشم پایین تا پرده بره کنار.");
        SetPlayerControl(true);
        isBusy = false;
    }

    private IEnumerator AutoRunTo(float targetX)
    {
        if (playerBody == null)
        {
            yield return MovePlayerTo(
                new Vector3(targetX, PlayerY, player.position.z),
                autoRunSpeed);
            yield break;
        }

        float direction = Mathf.Sign(targetX - player.position.x);
        while (Mathf.Abs(player.position.x - targetX) > 0.12f)
        {
            playerBody.linearVelocity = new Vector2(direction * autoRunSpeed, 0f);
            yield return null;
        }

        player.position = new Vector3(targetX, PlayerY, player.position.z);
        playerBody.linearVelocity = Vector2.zero;
    }

    private IEnumerator MovePlayerTo(Vector3 target, float speed)
    {
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

    private IEnumerator WaitForFreshEPress()
    {
        while (Keyboard.current != null && Keyboard.current.eKey.isPressed)
            yield return null;

        while (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
            yield return null;
    }

    private void KeepPlayerInsideCurrentRoom()
    {
        float center = roomCenters[currentRoom];
        float clampedX = Mathf.Clamp(
            player.position.x,
            center - RoomHalfWidth,
            center + RoomHalfWidth);

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
        if (player == null && playerMovement != null)
            player = playerMovement.transform;
        if (dialogueHUD == null)
            dialogueHUD = FindAnyObjectByType<DialogueHUD>();
        if (player != null)
            playerBody = player.GetComponent<Rigidbody2D>();

        routeCamera = Camera.main;
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

    private void DisableLegacyGameplay()
    {
        foreach (Interactable interactable in
                 FindObjectsByType<Interactable>(FindObjectsInactive.Include))
        {
            interactable.gameObject.SetActive(false);
        }

        foreach (ProgressTrigger trigger in
                 FindObjectsByType<ProgressTrigger>(FindObjectsInactive.Include))
        {
            trigger.gameObject.SetActive(false);
        }

        foreach (ControlledNarrativeShadow shadow in
                 FindObjectsByType<ControlledNarrativeShadow>(FindObjectsInactive.Include))
        {
            shadow.gameObject.SetActive(false);
        }

        foreach (CartController cart in
                 FindObjectsByType<CartController>(FindObjectsInactive.Include))
        {
            cart.gameObject.SetActive(false);
        }

        GreyboxFlowController greybox = FindAnyObjectByType<GreyboxFlowController>();
        if (greybox != null)
            greybox.enabled = false;
    }

    private void CreateRouteObjects()
    {
        routeRoot = new GameObject("Reworked Story Route - Replace Placeholders").transform;

        tutorialItem = CreateInteractive(
            "Tutorial Item (Replace Art)", new Vector2(0f, -2.75f),
            new Vector2(0.75f, 0.35f), new Color(0.84f, 0.76f, 0.42f),
            StoryInteractionType.TutorialPaper);
        hallwayDoor = CreateDoor(
            "Right Hallway Door (Replace Art)", new Vector2(5.8f, -1.75f),
            StoryInteractionType.HallwayDoor, new Color(0.35f, 0.28f, 0.2f));
        courtyardDoor = CreateDoor(
            "Left Courtyard Door (Replace Art)", new Vector2(-5.8f, -1.75f),
            StoryInteractionType.CourtyardDoor, new Color(0.22f, 0.45f, 0.28f));

        keyRoomDoor = CreateDoor(
            "Locked Key Room Door (Replace Art)", new Vector2(23.8f, -1.75f),
            StoryInteractionType.KeyRoomDoor, new Color(0.42f, 0.25f, 0.18f));
        keyRoomReturnDoor = CreateDoor(
            "Key Room Return Door (Replace Art)", new Vector2(11.6f, -1.75f),
            StoryInteractionType.KeyRoomReturnDoor, new Color(0.25f, 0.34f, 0.4f));
        highKey = CreateBlock(
            "High Key (Replace Art)", new Vector2(21f, 0.35f),
            new Vector2(0.45f, 0.22f), new Color(0.92f, 0.72f, 0.18f), 7);
        pushBox = CreatePushBox();

        fuseBox = CreateInteractive(
            "Fuse Box (Replace Art)", new Vector2(32f, -1.3f),
            new Vector2(0.75f, 1.15f), new Color(0.68f, 0.12f, 0.12f),
            StoryInteractionType.FuseBox);
        fuseRenderer = fuseBox.GetComponent<SpriteRenderer>();
        toolbox = CreateInteractive(
            "Toolbox (Replace Art)", new Vector2(38.5f, -2.65f),
            new Vector2(1.2f, 0.55f), new Color(0.55f, 0.34f, 0.12f),
            StoryInteractionType.Toolbox);
        fuseReturnDoor = CreateDoor(
            "Fuse Room Return Door (Replace Art)", new Vector2(29.7f, -1.75f),
            StoryInteractionType.FuseReturnDoor, new Color(0.25f, 0.34f, 0.4f));
        CreateFarLightAndShadow();

        weight = CreateInteractive(
            "Heavy Weight (Replace Art)", new Vector2(51f, -2.55f),
            new Vector2(0.85f, 0.75f), new Color(0.28f, 0.3f, 0.34f),
            StoryInteractionType.Weight);
        lockedStageDoor = CreateDoor(
            "Permanently Locked Stage Door (Replace Art)", new Vector2(59.6f, -1.75f),
            StoryInteractionType.LockedStageDoor, new Color(0.5f, 0.18f, 0.18f));
        backstageReturnDoor = CreateDoor(
            "Backstage Return Door (Replace Art)", new Vector2(47.7f, -1.75f),
            StoryInteractionType.BackstageReturnDoor, new Color(0.25f, 0.34f, 0.4f));
        backstageStairs = CreateArea(
            "Stairs To Upper Backstage (Replace Art)", new Vector2(57.4f, -1.65f),
            new Vector2(1.5f, 3f), StoryAreaType.BackstageStairsUp,
            new Color(0.42f, 0.38f, 0.3f));
        curtainShadow = CreateBlock(
            "Unknown Curtain Shadow (Replace Art)", new Vector2(54.5f, -0.2f),
            new Vector2(3.2f, 4.8f), new Color(0.05f, 0.05f, 0.08f, 0.78f), 3);
        revealedDecoration = CreateBlock(
            "Revealed Stage Decoration (Replace Art)", new Vector2(54.5f, -0.5f),
            new Vector2(2.6f, 4.1f), new Color(0.45f, 0.34f, 0.26f), 3);
        backpack = CreateInteractive(
            "Backpack (Replace Art)", new Vector2(53f, -2.55f),
            new Vector2(0.8f, 0.9f), new Color(0.22f, 0.36f, 0.5f),
            StoryInteractionType.Backpack);

        CreateUpperSectionPlaceholder();
        curtainRope = CreateInteractive(
            "Upper Curtain Rope (Replace Art)", new Vector2(76f, -0.5f),
            new Vector2(0.22f, 4.5f), new Color(0.68f, 0.54f, 0.24f),
            StoryInteractionType.CurtainRope);
        upperStairs = CreateArea(
            "Stairs Down (Replace Art)", new Vector2(65.6f, -1.65f),
            new Vector2(1.5f, 3f), StoryAreaType.UpperStairsDown,
            new Color(0.42f, 0.38f, 0.3f));
    }

    private StoryPushBoxInteractable CreatePushBox()
    {
        GameObject box = CreateBlock(
            "Push Box (Replace Art)", new Vector2(15.2f, -2.55f),
            new Vector2(1.45f, 1.45f), new Color(0.48f, 0.3f, 0.14f), 6);
        BoxCollider2D collider = box.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        StoryPushBoxInteractable component = box.AddComponent<StoryPushBoxInteractable>();
        component.Initialize(this, player, 14f, 21f);
        return component;
    }

    private StoryInteractable CreateDoor(
        string objectName,
        Vector2 position,
        StoryInteractionType type,
        Color color)
    {
        return CreateInteractive(
            objectName, position, new Vector2(1.45f, 3.8f), color, type);
    }

    private StoryInteractable CreateInteractive(
        string objectName,
        Vector2 position,
        Vector2 size,
        Color color,
        StoryInteractionType type)
    {
        GameObject item = CreateBlock(objectName, position, size, color, 6);
        BoxCollider2D collider = item.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        StoryInteractable interactable = item.AddComponent<StoryInteractable>();
        interactable.Initialize(this, type);
        return interactable;
    }

    private GameObject CreateArea(
        string objectName,
        Vector2 position,
        Vector2 size,
        StoryAreaType type,
        Color color)
    {
        GameObject area = CreateBlock(objectName, position, size, color, 2);
        BoxCollider2D collider = area.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        StoryAreaTrigger trigger = area.AddComponent<StoryAreaTrigger>();
        trigger.Initialize(this, type);
        return area;
    }

    private GameObject CreateBlock(
        string objectName,
        Vector2 position,
        Vector2 size,
        Color color,
        int sortingOrder)
    {
        GameObject block = new GameObject(objectName);
        block.transform.SetParent(routeRoot, false);
        block.transform.position = position;
        block.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            new Vector2(0.5f, 0.5f),
            Texture2D.whiteTexture.width);
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return block;
    }

    private void CreateFarLightAndShadow()
    {
        GameObject lightObject = new GameObject("Far Light (Replace Art)");
        lightObject.transform.SetParent(routeRoot, false);
        lightObject.transform.position = new Vector3(42f, 0.8f, 0f);
        farLight = lightObject.AddComponent<Light2D>();
        farLight.lightType = Light2D.LightType.Point;
        farLight.pointLightOuterRadius = 5f;
        farLight.intensity = 0f;

        playerShadow = CreateBlock(
            "Player Shadow (Replace Art)", new Vector2(39.8f, -0.5f),
            new Vector2(1.25f, 4f), new Color(0.04f, 0.04f, 0.06f, 0.72f), 5);
        playerShadow.SetActive(false);
    }

    private void CreateUpperSectionPlaceholder()
    {
        CreateBlock(
            "Upper Backstage Background (Replace Art)", new Vector2(72f, 0f),
            new Vector2(15.5f, 8.5f), new Color(0.13f, 0.12f, 0.16f), -20);
        CreateBlock(
            "Upper Backstage Floor (Replace Art)", new Vector2(72f, -3.45f),
            new Vector2(15.5f, 0.6f), new Color(0.25f, 0.22f, 0.2f), 1);
    }

    private void UpdateRouteObjects()
    {
        bool bagCollected = gameProgress.HasCompleted(GameProgressEvent.BagCollected);
        bool decorationRevealed = gameProgress.HasCompleted(GameProgressEvent.DecorationRevealed);

        SetActive(tutorialItem, currentRoom == 0 &&
            !gameProgress.HasCompleted(GameProgressEvent.TutorialItemRead));
        SetActive(hallwayDoor, currentRoom == 0 && !bagCollected);
        SetActive(courtyardDoor, currentRoom == 0);

        SetActive(keyRoomDoor, currentRoom == 1 && !bagCollected);
        SetActive(keyRoomReturnDoor, currentRoom == 1 && bagCollected);
        if (pushBox != null)
            pushBox.gameObject.SetActive(currentRoom == 1);
        if (highKey != null)
            highKey.SetActive(currentRoom == 1 &&
                !gameProgress.HasCompleted(GameProgressEvent.KeyCollected));

        SetActive(fuseBox, currentRoom == 2);
        SetActive(toolbox, currentRoom == 2 &&
            !gameProgress.HasCompleted(GameProgressEvent.FusePartCollected));
        SetActive(fuseReturnDoor, currentRoom == 2 && bagCollected);
        if (playerShadow != null)
            playerShadow.SetActive(currentRoom == 2 &&
                gameProgress.HasCompleted(GameProgressEvent.FuseRepaired));

        SetActive(weight, currentRoom == 3 &&
            !gameProgress.HasCompleted(GameProgressEvent.WeightCollected));
        SetActive(lockedStageDoor, currentRoom == 3);
        SetActive(backstageReturnDoor, currentRoom == 3 && bagCollected);
        if (backstageStairs != null)
            backstageStairs.SetActive(currentRoom == 3);
        SetActive(backpack, currentRoom == 3 && decorationRevealed && !bagCollected);
        if (curtainShadow != null)
            curtainShadow.SetActive(currentRoom == 3 && !decorationRevealed);
        if (revealedDecoration != null)
            revealedDecoration.SetActive(currentRoom == 3 && decorationRevealed);

        SetActive(curtainRope, currentRoom == 4);
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
        if (globalLight == null)
            return;

        bool darkFuseRoom = currentRoom == 2 &&
                            !gameProgress.HasCompleted(GameProgressEvent.FuseRepaired);
        globalLight.intensity = darkFuseRoom
            ? Mathf.Min(normalLightIntensity, 0.22f)
            : normalLightIntensity;
    }

    private void SetCameraToRoom(int room)
    {
        if (routeCamera == null)
            return;

        Vector3 position = routeCamera.transform.position;
        position.x = roomCenters[room];
        routeCamera.transform.position = position;
    }

    private void Say(string message)
    {
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

    private void ShowStoryImage(Sprite sprite, Color fallbackColor, string label)
    {
        storyImage.sprite = sprite;
        storyImage.color = sprite == null ? fallbackColor : Color.white;
        storyImage.preserveAspect = true;
        storyLabel.text = sprite == null ? label : string.Empty;
        storyCanvas.alpha = 1f;
        storyCanvas.blocksRaycasts = true;
    }

    private void HideStoryImage()
    {
        storyCanvas.alpha = 0f;
        storyCanvas.blocksRaycasts = false;
    }

    private void CreateFadeCanvas()
    {
        GameObject fadeObject = new GameObject("Story Fade");
        Canvas canvas = fadeObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        fadeCanvas = fadeObject.AddComponent<CanvasGroup>();
        fadeCanvas.alpha = 0f;
        fadeCanvas.blocksRaycasts = false;

        GameObject imageObject = new GameObject("Fade Image");
        imageObject.transform.SetParent(fadeObject.transform, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = Color.black;
        Stretch(image.rectTransform);
    }

    private void CreateStoryCanvas()
    {
        GameObject canvasObject = new GameObject("Story Image Placeholders");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 250;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        storyCanvas = canvasObject.AddComponent<CanvasGroup>();
        storyCanvas.alpha = 0f;
        storyCanvas.blocksRaycasts = false;

        GameObject imageObject = new GameObject("Replace With Cutscene Image");
        imageObject.transform.SetParent(canvasObject.transform, false);
        storyImage = imageObject.AddComponent<Image>();
        RectTransform imageRect = storyImage.rectTransform;
        imageRect.anchorMin = new Vector2(0.08f, 0.08f);
        imageRect.anchorMax = new Vector2(0.92f, 0.92f);
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        GameObject labelObject = new GameObject("Placeholder Label");
        labelObject.transform.SetParent(imageObject.transform, false);
        storyLabel = labelObject.AddComponent<TextMeshProUGUI>();
        storyLabel.alignment = TextAlignmentOptions.Center;
        storyLabel.fontSize = 34f;
        storyLabel.color = Color.white;
        Stretch(storyLabel.rectTransform);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
