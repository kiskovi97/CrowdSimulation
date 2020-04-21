using Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CameraMovement : MonoBehaviour
{

    public float speed = 1f;
    public float zoomSpeed = 1f;
    public float movementTime = 1f;
    public float rotationAmount = 1f;

    public float maxDistance = 300f;
    public float minDistance = 5f;


    private Transform cameraTransform;
    private Vector3 speedDirection;
    private float zoom;
    private float rotate;
    private Vector3 cameraPos;

    private Vector3 newPosition;
    private Quaternion newRotation;

    public void OnMovement(InputValue value)
    {
        var v = value.Get<Vector2>();
        speedDirection = (Vector3.forward * v.y + Vector3.right * v.x) * speed;
    }
    public void OnZoom(InputValue value)
    {
        zoom = value.Get<float>() / 120;
    }

    public void OnRotate(InputValue value)
    {
        rotate = value.Get<float>();
    }

    private void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        cameraTransform = Camera.main.transform;
        cameraPos = cameraTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        var speedMulti = 1f;
        if (!Camera.main.orthographic)
        {
            var tmp = cameraPos;
            cameraPos += transform.InverseTransformDirection(cameraTransform.forward) * zoom * zoomSpeed * Time.deltaTime;
            if ((cameraPos.magnitude > maxDistance) || (cameraPos.magnitude < minDistance) || (cameraPos.y < 0))
            {
                cameraPos = tmp;
            }
            else
            {
                cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, cameraPos, Time.deltaTime * movementTime);
                speedMulti = Mathf.Max(1f, cameraPos.magnitude / 40f);
            }
        }
        else
        {
            Camera.main.orthographicSize -= zoom * speed * Time.deltaTime;
        }

        newPosition += transform.rotation * speedDirection * Time.deltaTime * speedMulti;
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime * speedMulti);


        newRotation *= Quaternion.Euler(Vector3.up * rotationAmount * rotate * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);

        //if (RayCast.IsPointerOverUIObject()) return;

    }
}
