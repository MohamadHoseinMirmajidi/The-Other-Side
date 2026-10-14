using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class StoryRouteController : MonoBehaviour
{
    private const float RoomHalfWidth = 7.4f;

    [Header("References")]
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Light2D globalLight;

    [Header("Transition")]
    [SerializeField] private float fadeDuration = 0.35f;

    private readonly float[] roomCenters = { 0f, 18f, 36f, 54f };
    private Rigidbody2D playerBody;
    private CanvasGroup fadeCanvas;
    private int currentRoom;
    private bool isTransitioning;

    private RouteDoorInteractable firstDoor;
    private RouteDoorInteractable highHandleDoor;
    private RouteDoorInteractable cartDoor;
    private RouteDoorInteractable bagRoomReturnDoor;
    private RouteDoorInteractable cartRoomReturnDoor;
    private RouteDoorInteractable shadowRoomReturnDoor;
    private RouteDoorInteractable lockedStageDoor;
    private RouteDoorInteractable courtyardDoor;
    private PushableBox pushableBox;

    private void Awake()
    {
        if (gameProgress == null)
            gameProgress = FindAnyObjectByType<GameProgress>();

        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        if (player == null && playerMovement != null)
            player = playerMovement.transform;

        if (player != null)
            playerBody = player.GetComponent<Rigidbody2D>();

        CreateFadeCanvas();
        CreateRouteObjects();
        UpdateDoorVisibility();
    }

    private void Update()
    {
        if (player == null || isTransitioning)
            return;

        KeepPlayerInsideCurrentRoom();
        UpdateDoorVisibility();
    }

    public bool CanUseDoor(RouteDoorRole role)
    {
        switch (role)
        {
            case RouteDoorRole.FirstDoor:
                return currentRoom == 0 &&
                       !gameProgress.HasCompleted(GameProgressEvent.ReturnedToFirstRoom);

            case RouteDoorRole.HighHandleDoor:
                return currentRoom == 1 &&
                       gameProgress.HasCompleted(GameProgressEvent.BoxPlaced);

            case RouteDoorRole.CartDoor:
                return currentRoom == 2 &&
                       gameProgress.HasCompleted(GameProgressEvent.CartMovedAway);

            case RouteDoorRole.LockedStageDoor:
                return currentRoom == 3 &&
                       gameProgress.HasCompleted(GameProgressEvent.HallwayLightSelected) &&
                       !gameProgress.HasCompleted(GameProgressEvent.StageDoorChecked);

            case RouteDoorRole.ReturnFromBagRoom:
                return currentRoom == 3 &&
                       gameProgress.HasCompleted(GameProgressEvent.StageDoorChecked);

            case RouteDoorRole.ReturnFromCartRoom:
                return currentRoom == 2 &&
                       gameProgress.HasCompleted(GameProgressEvent.StageDoorChecked);

            case RouteDoorRole.ReturnFromShadowRoom:
                return currentRoom == 1 &&
                       gameProgress.HasCompleted(GameProgressEvent.StageDoorChecked);

            case RouteDoorRole.CourtyardDoor:
                return currentRoom == 0 &&
                       gameProgress.HasCompleted(GameProgressEvent.ReturnedToFirstRoom);

            default:
                return false;
        }
    }

    public IEnumerator UseDoor(RouteDoorInteractable door)
    {
        switch (door.Role)
        {
            case RouteDoorRole.FirstDoor:
                if (!door.IsOpen)
                {
                    door.Open();
                    gameProgress.TryComplete(GameProgressEvent.FirstDoorOpened);
                }
                else
                {
                    yield return ChangeRoom(1, 11f);
                    gameProgress.TryComplete(GameProgressEvent.EnteredBackstage);
                }
                break;

            case RouteDoorRole.HighHandleDoor:
                if (!door.IsOpen)
                {
                    yield return ReachHighHandle();
                    door.Open();
                    gameProgress.TryComplete(GameProgressEvent.HighDoorOpened);
                }
                else
                {
                    yield return ChangeRoom(2, 29f);
                    gameProgress.TryComplete(GameProgressEvent.EnteredCartRoom);
                }
                break;

            case RouteDoorRole.CartDoor:
                if (!door.IsOpen)
                {
                    door.Open();
                    gameProgress.TryComplete(GameProgressEvent.CartDoorOpened);
                }
                else
                {
                    yield return ChangeRoom(3, 47f);
                    gameProgress.TryComplete(GameProgressEvent.EnteredBagRoom);
                }
                break;

            case RouteDoorRole.LockedStageDoor:
                gameProgress.TryComplete(GameProgressEvent.StageDoorChecked);
                break;

            case RouteDoorRole.ReturnFromBagRoom:
                yield return ChangeRoom(2, 43f);
                break;

            case RouteDoorRole.ReturnFromCartRoom:
                yield return ChangeRoom(1, 25f);
                break;

            case RouteDoorRole.ReturnFromShadowRoom:
                yield return ChangeRoom(0, 7f);
                gameProgress.TryComplete(GameProgressEvent.ReturnedToFirstRoom);
                break;

            case RouteDoorRole.CourtyardDoor:
                if (!door.IsOpen)
                {
                    door.Open();
                    gameProgress.TryComplete(GameProgressEvent.EnteredCourtyard);
                }
                else
                {
                    gameProgress.TryComplete(GameProgressEvent.ExitOpened);
                }
                break;
        }

        UpdateDoorVisibility();
    }

    public void NotifyBoxPlaced()
    {
        gameProgress.TryComplete(GameProgressEvent.BoxPlaced);
    }

    private IEnumerator ReachHighHandle()
    {
        SetPlayerControl(false);
        Vector3 startPosition = player.position;
        Vector3 boxPosition = pushableBox.transform.position;
        player.position = new Vector3(boxPosition.x, startPosition.y + 0.9f, startPosition.z);

        yield return new WaitForSeconds(0.3f);

        player.position = new Vector3(boxPosition.x - 0.8f, startPosition.y, startPosition.z);
        SetPlayerControl(true);
    }

    private IEnumerator ChangeRoom(int targetRoom, float spawnX)
    {
        isTransitioning = true;
        SetPlayerControl(false);
        yield return Fade(0f, 1f);

        currentRoom = targetRoom;
        player.position = new Vector3(spawnX, -2.6f, player.position.z);

        if (playerBody != null)
            playerBody.linearVelocity = Vector2.zero;

        UpdateDoorVisibility();
        yield return Fade(1f, 0f);
        SetPlayerControl(true);
        isTransitioning = false;
    }

    private void KeepPlayerInsideCurrentRoom()
    {
        float center = roomCenters[currentRoom];
        float minimumX = center - RoomHalfWidth;
        if (currentRoom == 0 &&
            !gameProgress.HasCompleted(GameProgressEvent.ReturnedToFirstRoom))
        {
            minimumX = -2.7f;
        }

        float clampedX = Mathf.Clamp(
            player.position.x,
            minimumX,
            center + RoomHalfWidth);

        if (Mathf.Approximately(clampedX, player.position.x))
            return;

        Vector3 position = player.position;
        position.x = clampedX;
        player.position = position;

        if (playerBody != null)
            playerBody.linearVelocity = Vector2.zero;
    }

    private void UpdateDoorVisibility()
    {
        bool returning = gameProgress.HasCompleted(GameProgressEvent.StageDoorChecked);
        bool returnedToFirstRoom = gameProgress.HasCompleted(GameProgressEvent.ReturnedToFirstRoom);

        firstDoor.gameObject.SetActive(currentRoom == 0 && !returnedToFirstRoom);
        courtyardDoor.gameObject.SetActive(currentRoom == 0 && returnedToFirstRoom);
        highHandleDoor.gameObject.SetActive(currentRoom == 1 && !returning);
        shadowRoomReturnDoor.gameObject.SetActive(currentRoom == 1 && returning);
        cartDoor.gameObject.SetActive(currentRoom == 2 && !returning);
        cartRoomReturnDoor.gameObject.SetActive(currentRoom == 2 && returning);
        lockedStageDoor.gameObject.SetActive(currentRoom == 3);
        bagRoomReturnDoor.gameObject.SetActive(currentRoom == 3 && returning);
        pushableBox.gameObject.SetActive(currentRoom == 1);
    }

    private void CreateRouteObjects()
    {
        firstDoor = CreateDoor("First Door", new Vector2(5.5f, -1.8f), RouteDoorRole.FirstDoor, new Color(0.35f, 0.3f, 0.25f));
        courtyardDoor = CreateDoor("Courtyard Door", new Vector2(-5.4f, -1.8f), RouteDoorRole.CourtyardDoor, new Color(0.22f, 0.48f, 0.3f));
        highHandleDoor = CreateDoor("High Handle Door", new Vector2(22f, -1.8f), RouteDoorRole.HighHandleDoor, new Color(0.36f, 0.28f, 0.22f));
        shadowRoomReturnDoor = CreateDoor("Shadow Room Return Door", new Vector2(11f, -1.8f), RouteDoorRole.ReturnFromShadowRoom, new Color(0.28f, 0.34f, 0.4f));
        cartDoor = CreateDoor("Door Behind Cart", new Vector2(36.2f, -1.8f), RouteDoorRole.CartDoor, new Color(0.36f, 0.28f, 0.22f));
        cartRoomReturnDoor = CreateDoor("Cart Room Return Door", new Vector2(29f, -1.8f), RouteDoorRole.ReturnFromCartRoom, new Color(0.28f, 0.34f, 0.4f));
        bagRoomReturnDoor = CreateDoor("Bag Room Return Door", new Vector2(47f, -1.8f), RouteDoorRole.ReturnFromBagRoom, new Color(0.28f, 0.34f, 0.4f));
        lockedStageDoor = CreateDoor("Locked Stage Door", new Vector2(59.5f, -1.8f), RouteDoorRole.LockedStageDoor, new Color(0.5f, 0.2f, 0.2f));
        pushableBox = CreatePushableBox();
    }

    private RouteDoorInteractable CreateDoor(
        string doorName,
        Vector2 position,
        RouteDoorRole role,
        Color color)
    {
        GameObject doorObject = CreateBlock(doorName, position, new Vector2(1.5f, 3.8f), color, 3);
        BoxCollider2D interactionArea = doorObject.AddComponent<BoxCollider2D>();
        interactionArea.isTrigger = true;
        interactionArea.size = new Vector2(1.5f, 1.2f);

        RouteDoorInteractable door = doorObject.AddComponent<RouteDoorInteractable>();
        door.Initialize(this, role, doorObject.GetComponent<SpriteRenderer>());
        return door;
    }

    private PushableBox CreatePushableBox()
    {
        GameObject boxObject = CreateBlock(
            "Pushable Box",
            new Vector2(14.5f, -2.9f),
            new Vector2(1.5f, 1.5f),
            new Color(0.48f, 0.32f, 0.18f),
            4);

        BoxCollider2D boxCollider = boxObject.AddComponent<BoxCollider2D>();
        boxCollider.size = Vector2.one;

        Rigidbody2D boxBody = boxObject.AddComponent<Rigidbody2D>();
        boxBody.gravityScale = 0f;
        boxBody.mass = 2.5f;
        boxBody.constraints = RigidbodyConstraints2D.FreezePositionY |
                              RigidbodyConstraints2D.FreezeRotation;

        PushableBox box = boxObject.AddComponent<PushableBox>();
        box.Initialize(this, 13f, 20.4f);
        return box;
    }

    private static GameObject CreateBlock(
        string objectName,
        Vector2 position,
        Vector2 size,
        Color color,
        int sortingOrder)
    {
        GameObject block = new GameObject(objectName);
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

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        fadeCanvas.alpha = from;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = fadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeDuration);
            fadeCanvas.alpha = Mathf.Lerp(from, to, progress);
            yield return null;
        }

        fadeCanvas.alpha = to;
    }

    private void SetPlayerControl(bool value)
    {
        if (playerMovement == null)
            return;

        playerMovement.ResetMotion();
        playerMovement.enabled = value;
    }

    private void CreateFadeCanvas()
    {
        GameObject fadeObject = new GameObject("Route Fade");
        Canvas canvas = fadeObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        fadeCanvas = fadeObject.AddComponent<CanvasGroup>();
        fadeCanvas.alpha = 0f;
        fadeCanvas.blocksRaycasts = false;

        GameObject imageObject = new GameObject("Fade Image");
        imageObject.transform.SetParent(fadeObject.transform, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = Color.black;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
