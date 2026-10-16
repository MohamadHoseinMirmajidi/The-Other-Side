using UnityEngine;

public class StorySectionLayout : MonoBehaviour
{
    [Header("Player Limits")]
    [SerializeField] private Transform playerLeftLimit;
    [SerializeField] private Transform playerRightLimit;

    [Header("Camera")]
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private Transform cameraLeftLimit;
    [SerializeField] private Transform cameraRightLimit;

    public void Configure(
        Transform leftPlayerLimit,
        Transform rightPlayerLimit,
        Transform anchor,
        Transform leftCameraLimit,
        Transform rightCameraLimit)
    {
        playerLeftLimit = leftPlayerLimit;
        playerRightLimit = rightPlayerLimit;
        cameraAnchor = anchor;
        cameraLeftLimit = leftCameraLimit;
        cameraRightLimit = rightCameraLimit;
    }

    public float ClampPlayerX(float playerX)
    {
        if (playerLeftLimit == null || playerRightLimit == null)
            return playerX;

        float minimum = Mathf.Min(playerLeftLimit.position.x, playerRightLimit.position.x);
        float maximum = Mathf.Max(playerLeftLimit.position.x, playerRightLimit.position.x);
        return Mathf.Clamp(playerX, minimum, maximum);
    }

    public float GetHorizontalProgress(float playerX)
    {
        if (playerLeftLimit == null || playerRightLimit == null)
            return 0f;

        float minimum = Mathf.Min(playerLeftLimit.position.x, playerRightLimit.position.x);
        float maximum = Mathf.Max(playerLeftLimit.position.x, playerRightLimit.position.x);
        if (Mathf.Approximately(minimum, maximum))
            return 0f;

        return Mathf.InverseLerp(minimum, maximum, playerX);
    }

    public Vector3 GetCameraPosition(Vector3 currentPosition, Vector3 playerPosition)
    {
        Vector3 result = currentPosition;

        if (cameraAnchor != null)
        {
            result.y = cameraAnchor.position.y;
            result.z = cameraAnchor.position.z;
        }

        if (cameraLeftLimit != null && cameraRightLimit != null)
        {
            float minimum = Mathf.Min(cameraLeftLimit.position.x, cameraRightLimit.position.x);
            float maximum = Mathf.Max(cameraLeftLimit.position.x, cameraRightLimit.position.x);
            result.x = Mathf.Clamp(playerPosition.x, minimum, maximum);
        }
        else if (cameraAnchor != null)
        {
            result.x = cameraAnchor.position.x;
        }

        return result;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.25f, 0.9f, 0.35f, 1f);
        DrawLine(playerLeftLimit, playerRightLimit);

        Gizmos.color = new Color(0.25f, 0.65f, 1f, 1f);
        DrawLine(cameraLeftLimit, cameraRightLimit);

        if (cameraAnchor != null)
        {
            Gizmos.DrawWireSphere(cameraAnchor.position, 0.25f);
            Gizmos.DrawLine(cameraAnchor.position + Vector3.left * 0.35f,
                cameraAnchor.position + Vector3.right * 0.35f);
            Gizmos.DrawLine(cameraAnchor.position + Vector3.down * 0.35f,
                cameraAnchor.position + Vector3.up * 0.35f);
        }
    }

    private static void DrawLine(Transform first, Transform second)
    {
        if (first == null || second == null)
            return;

        Gizmos.DrawLine(first.position, second.position);
        Gizmos.DrawWireSphere(first.position, 0.18f);
        Gizmos.DrawWireSphere(second.position, 0.18f);
    }
}
