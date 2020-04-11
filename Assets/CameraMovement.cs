using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float edge = 10f;
    public float speed = 1f;
      // Update is called once per frame
    void Update()
    {
        if (RayCast.IsPointerOverUIObject()) return;
        var mp = Input.mousePosition;
        //if (mp.y > Screen.height || mp.y < 0 || mp.x > Screen.width || mp.x < 0) return;
        if (mp.x > Screen.width - edge)
        {
            transform.position += (mp.x - (Screen.width - edge)) / (edge) * Vector3.right * Time.deltaTime * speed;
        }
        if (mp.x < edge)
        {
            transform.position += (edge - mp.x) / (edge) * Vector3.left * Time.deltaTime * speed;
        }

        if (mp.y > Screen.height - edge)
        {
            transform.position += (mp.y - (Screen.height - edge)) / (edge) * Vector3.forward * Time.deltaTime * speed;
        }
        if (mp.y < edge)
        {
            transform.position += (edge - mp.y) / (edge) * Vector3.back * Time.deltaTime * speed;
        }
    }
}
