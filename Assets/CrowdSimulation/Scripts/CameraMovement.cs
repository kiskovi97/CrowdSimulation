using Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CameraMovement : MonoBehaviour
{
    public float speed = 1f;
    public float movementTime = 1f;
    public float rotationAmount = 1f;

    private Vector3 speedDirection;
    private float zoom;
    private float rotate;

    private Vector3 newPosition;
    private Quaternion newRotation;

    public void OnMovement(InputValue value)
    {
        var v = value.Get<Vector2>();
        speedDirection = (Vector3.forward * v.y + Vector3.right * v.x) * speed;
    }
    public void OnZoom(InputValue value)
    {
        zoom = value.Get<float>();
    }

    public void OnRotate(InputValue value)
    {
        rotate = value.Get<float>();
    }

    private void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        newPosition += transform.rotation * speedDirection * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);

        newRotation *= Quaternion.Euler(Vector3.up * rotationAmount * rotate * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);

        if (!Camera.main.orthographic)
        {
            transform.position += transform.forward * zoom * speed * Time.deltaTime * 5f;
        }
        else
        {
            Camera.main.orthographicSize -= zoom * speed * Time.deltaTime;
        }

        //if (RayCast.IsPointerOverUIObject()) return;
        
    }
}
