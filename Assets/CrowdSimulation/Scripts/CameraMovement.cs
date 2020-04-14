using Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public float edge = 10f;
    public float speed = 1f;
    public bool mouse = false;
    private Vector3 right;
    private Vector3 forward;

    private Vector3 speedDirection;
    private float scroll;

    public void OnMovement(InputAction.CallbackContext context)
    {
        var v = context.ReadValue<Vector2>();
        right = transform.right;
        forward = transform.forward;
        right.y = 0;
        forward.y = 0;
        right.Normalize();
        forward.Normalize();
        speedDirection = forward * v.y + right * v.x;
    }
    public void OnZoom(InputAction.CallbackContext context)
    {
        scroll = context.ReadValue<float>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += speedDirection * Time.deltaTime * speed;
        if (!Camera.main.orthographic)
        {
            transform.position += transform.forward * scroll * speed * Time.deltaTime;
        }
        else
        {
            Camera.main.orthographicSize -= scroll * speed * Time.deltaTime;
        }

        if (RayCast.IsPointerOverUIObject()) return;

        var mp = Input.mousePosition;
        //if (mp.y > Screen.height || mp.y < 0 || mp.x > Screen.width || mp.x < 0) return;
        if (mouse && mp.x > Screen.width - edge)
        {
            transform.position += (mp.x - (Screen.width - edge)) / (edge) * right * Time.deltaTime * speed;
        }
        if (mouse && mp.x < edge)
        {
            transform.position += (edge - mp.x) / (edge) * -right * Time.deltaTime * speed;
        }

        if (mouse && mp.y > Screen.height - edge)
        {
            transform.position += (mp.y - (Screen.height - edge)) / (edge) * forward * Time.deltaTime * speed;
        }
        if (mouse && mp.y < edge)
        {
            transform.position += (edge - mp.y) / (edge) * -forward * Time.deltaTime * speed;
        }
    }
}
