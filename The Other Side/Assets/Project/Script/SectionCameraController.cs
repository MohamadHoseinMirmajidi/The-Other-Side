using System;
using UnityEngine;

public class SectionCameraController : MonoBehaviour
{
    [Serializable]
    private class CameraSection
    {
        public Transform cameraPoint;
        public float minPlayerX;
        public float maxPlayerX;
    }

    private const float TargetAspectRatio = 16f / 9f;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private CameraSection[] sections;

    [Header("Movement")]
    [SerializeField] private float horizontalFollowRange = 0.75f;
    [SerializeField] private float smoothTime = 0.35f;

    private Vector3 moveVelocity;
    private int activeSection;
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        activeSection = FindSection(player.position.x);
        transform.position = GetTargetPosition();
        ApplyAspectRatio();
    }

    private void LateUpdate()
    {
        if (player == null || sections == null || sections.Length == 0)
            return;

        activeSection = FindSection(player.position.x);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            GetTargetPosition(),
            ref moveVelocity,
            smoothTime);

        if (Screen.width != lastScreenWidth ||
            Screen.height != lastScreenHeight)
        {
            ApplyAspectRatio();
        }
    }

    private int FindSection(float playerX)
    {
        if (sections == null || sections.Length == 0)
            return 0;

        for (int i = 0; i < sections.Length; i++)
        {
            if (playerX >= sections[i].minPlayerX &&
                playerX < sections[i].maxPlayerX)
            {
                return i;
            }
        }

        int closestSection = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < sections.Length; i++)
        {
            float distance = Mathf.Abs(
                playerX - sections[i].cameraPoint.position.x);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSection = i;
            }
        }

        return closestSection;
    }

    private Vector3 GetTargetPosition()
    {
        Transform cameraPoint = sections[activeSection].cameraPoint;
        float playerOffset = Mathf.Clamp(
            player.position.x - cameraPoint.position.x,
            -horizontalFollowRange,
            horizontalFollowRange);

        return new Vector3(
            cameraPoint.position.x + playerOffset,
            cameraPoint.position.y,
            transform.position.z);
    }

    private void ApplyAspectRatio()
    {
        if (targetCamera == null || Screen.height == 0)
            return;

        float screenAspect = (float)Screen.width / Screen.height;
        float heightScale = screenAspect / TargetAspectRatio;
        Rect cameraRect = new Rect(0f, 0f, 1f, 1f);

        if (heightScale < 1f)
        {
            cameraRect.height = heightScale;
            cameraRect.y = (1f - heightScale) * 0.5f;
        }
        else
        {
            float widthScale = 1f / heightScale;
            cameraRect.width = widthScale;
            cameraRect.x = (1f - widthScale) * 0.5f;
        }

        targetCamera.rect = cameraRect;
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }
}
