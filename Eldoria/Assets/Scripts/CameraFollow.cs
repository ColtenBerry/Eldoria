using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform player;
    public float deadZoneWidth = 2f;
    public float deadZoneHeight = 2f;
    public float easing = 5f;

    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position;
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 delta = player.position - targetPosition;

        if (Mathf.Abs(delta.x) > deadZoneWidth)
            targetPosition.x += delta.x - Mathf.Sign(delta.x) * deadZoneWidth;

        if (Mathf.Abs(delta.y) > deadZoneHeight)
            targetPosition.y += delta.y - Mathf.Sign(delta.y) * deadZoneHeight;

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPosition, easing * Time.deltaTime);
    }
}
