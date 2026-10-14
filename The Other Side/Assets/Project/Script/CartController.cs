using UnityEngine;
using UnityEngine.InputSystem;

public class CartController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D playerBody;
    [SerializeField] private Transform[] wheelVisuals;
    [SerializeField] private Transform branchShadow;
    [SerializeField] private AudioSource rollingAudio;

    [Header("Rail")]
    [SerializeField] private float railMinX;
    [SerializeField] private float railMaxX;
    [SerializeField] private float moveSpeed = 0.75f;
    [SerializeField] private float handleDistance = 2.1f;
    [SerializeField] private float wheelRadius = 0.33f;

    private bool brakeReleased;
    private bool isAttached;
    private bool isParked;
    private int attachedFrame;
    private AudioClip rollingClip;
    private AudioClip lockedClip;

    public bool IsBrakeReleased => brakeReleased;
    public bool IsAttached => isAttached;
    public bool IsParked => isParked;

    private void Awake()
    {
        if (gameProgress == null)
            gameProgress = FindAnyObjectByType<GameProgress>();

        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        if (playerBody == null && playerMovement != null)
            playerBody = playerMovement.GetComponent<Rigidbody2D>();

        brakeReleased = gameProgress.HasCompleted(GameProgressEvent.CartBrakeReleased);
        isParked = gameProgress.HasCompleted(GameProgressEvent.CartMovedAway);

        rollingClip = CreateToneClip("Cart Rolling", 95f, 0.2f, 0.04f);
        lockedClip = CreateToneClip("Cart Locked", 180f, 0.08f, 0.08f);

        if (rollingAudio != null)
        {
            rollingAudio.clip = rollingClip;
            rollingAudio.loop = true;
            rollingAudio.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (!isAttached || Time.frameCount == attachedFrame || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            StopPushPull();
        }
    }

    private void FixedUpdate()
    {
        if (!isAttached || Keyboard.current == null)
        {
            SetRollingSound(false);
            return;
        }

        float input = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            input -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            input += 1f;

        float currentX = transform.position.x;
        float nextX = Mathf.Clamp(
            currentX + input * moveSpeed * Time.fixedDeltaTime,
            railMinX,
            railMaxX);
        float movedDistance = nextX - currentX;

        if (Mathf.Abs(movedDistance) > 0.0001f)
        {
            transform.position += Vector3.right * movedDistance;
            playerBody.MovePosition(playerBody.position + Vector2.right * movedDistance);
            RotateWheels(movedDistance);
        }

        SetRollingSound(Mathf.Abs(movedDistance) > 0.0001f);

        if (nextX >= railMaxX - 0.01f)
            ParkCart();
    }

    public void ReleaseBrake()
    {
        brakeReleased = true;
    }

    public void StartPushPull()
    {
        if (!brakeReleased || isAttached || isParked)
            return;

        float side = playerBody.position.x <= transform.position.x ? -1f : 1f;
        Vector2 playerPosition = playerBody.position;
        playerPosition.x = transform.position.x + side * handleDistance;
        playerBody.position = playerPosition;

        playerMovement.ResetMotion();
        playerMovement.enabled = false;
        isAttached = true;
        attachedFrame = Time.frameCount;
    }

    public void StopPushPull()
    {
        if (!isAttached)
            return;

        isAttached = false;
        SetRollingSound(false);
        playerMovement.ResetMotion();
        playerMovement.enabled = true;
    }

    public void PlayLockedFeedback()
    {
        if (rollingAudio != null && lockedClip != null)
            rollingAudio.PlayOneShot(lockedClip);
    }

    private void ParkCart()
    {
        if (isParked)
            return;

        isParked = true;
        gameProgress.TryComplete(GameProgressEvent.CartMovedAway);
        StopPushPull();
    }

    private void RotateWheels(float movedDistance)
    {
        float angle = -(movedDistance / wheelRadius) * Mathf.Rad2Deg;
        foreach (Transform wheel in wheelVisuals)
        {
            if (wheel != null)
                wheel.Rotate(0f, 0f, angle);
        }

        if (branchShadow != null)
            branchShadow.Rotate(0f, 0f, angle * 0.08f);
    }

    private void SetRollingSound(bool shouldPlay)
    {
        if (rollingAudio == null)
            return;

        if (shouldPlay && !rollingAudio.isPlaying)
            rollingAudio.Play();
        else if (!shouldPlay && rollingAudio.isPlaying)
            rollingAudio.Stop();
    }

    private static AudioClip CreateToneClip(
        string clipName,
        float frequency,
        float duration,
        float volume)
    {
        const int sampleRate = 22050;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int index = 0; index < sampleCount; index++)
        {
            float time = index / (float)sampleRate;
            samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * time) * volume;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
