using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    public float radius;
    public float speedAdjustment;

    private float circumference;
    private Vector3 forwardSpeed;
    private Vector3 lastPos;

    void Awake()
    {
        circumference = 2 * Mathf.PI * radius;
        lastPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (lastPos != transform.position)
        {
            forwardSpeed = transform.position - lastPos;
            forwardSpeed /= Time.deltaTime;
            lastPos = transform.position;

            float rotationSpeed = forwardSpeed.magnitude / circumference;
            transform.Rotate(rotationSpeed * speedAdjustment, 0f, 0f);
        }
    }
}
