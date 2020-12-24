using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentCameraController : MonoBehaviour
{
    public float rotationSpeed = 200f;
    public float zoomSpeed = 10f;

    private Camera cam;
    private GameObject agent;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        agent = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (cam.enabled)
        {

            var rightClicked = false;

            rightClicked = Input.GetMouseButton(1);

            float rotationVertical = 0f;
            float rotationHorizontal = 0f;

            if (rightClicked)
            {
                rotationHorizontal = -Input.GetAxis("Mouse X");
                rotationVertical = -Input.GetAxis("Mouse Y");
            }
            
            transform.RotateAround(agent.transform.position, Vector3.up, rotationHorizontal * rotationSpeed * Time.deltaTime);
            if (transform.rotation.eulerAngles.x > 55f && transform.rotation.eulerAngles.x < 60f && rotationVertical > 0f)
                rotationVertical = 0f;
            else if (transform.rotation.eulerAngles.x < 345f && transform.rotation.eulerAngles.x > 330f  && rotationVertical < 0f)
                rotationVertical = 0f;
            transform.RotateAround(agent.transform.position, transform.right, rotationVertical * rotationSpeed * Time.deltaTime);
        
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift) &&
                !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
            {
                float forwardInput = Input.GetAxis("Mouse ScrollWheel");
                transform.position = Vector3.MoveTowards(transform.position, agent.transform.position, zoomSpeed * forwardInput * Time.deltaTime);
            }
        }
    }
}
