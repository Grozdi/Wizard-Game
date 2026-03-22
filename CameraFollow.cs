using UnityEngine;

/*
    CameraFollow.cs

    Attach this to the Camera.
    Assign the player transform in the Inspector.
    The camera will smoothly follow the player and always look at them.

    NOTE
    - Use this instead of parenting the camera under the player if you want a stable follow camera.
    - Avoid attaching additional scripts that also rotate the same camera every frame.
*/
public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 2f, -5f);
    public float smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        Vector3 desiredPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(player);
    }
}
