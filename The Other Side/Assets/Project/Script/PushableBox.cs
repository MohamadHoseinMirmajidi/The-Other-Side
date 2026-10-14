using UnityEngine;

public class PushableBox : MonoBehaviour
{
    private StoryRouteController routeController;
    private Rigidbody2D boxBody;
    private float minX;
    private float targetX;
    private bool isPlaced;

    public void Initialize(
        StoryRouteController controller,
        float railMinimum,
        float placementPosition)
    {
        routeController = controller;
        minX = railMinimum;
        targetX = placementPosition;
        boxBody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (boxBody == null || isPlaced)
            return;

        Vector2 position = boxBody.position;
        position.x = Mathf.Clamp(position.x, minX, targetX);
        boxBody.position = position;

        if (position.x < targetX - 0.05f)
            return;

        isPlaced = true;
        boxBody.linearVelocity = Vector2.zero;
        boxBody.constraints = RigidbodyConstraints2D.FreezeAll;
        routeController.NotifyBoxPlaced();
    }
}
