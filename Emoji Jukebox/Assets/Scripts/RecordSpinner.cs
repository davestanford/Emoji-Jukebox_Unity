using UnityEngine;

public class RecordSpinner : MonoBehaviour
{
    public float targetSpeed = 180f;
    public float acceleration = 200f;

    private float currentSpeed = 0f;
    public bool isSpinning = false;

    void Update()
    {
        float desiredSpeed = isSpinning ? targetSpeed : 0f;
        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * Time.deltaTime);

        transform.Rotate(0f, 0f, currentSpeed * Time.deltaTime);
    }
}