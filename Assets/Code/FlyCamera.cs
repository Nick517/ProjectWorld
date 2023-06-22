using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FlyCamera : MonoBehaviour
{
    public float acceleration = 50;
    public float sprintMultiplier = 4;
    public float cameraSensitivity = 1;
    public float damping = 5;
    public bool focusOnEnable = true;

    private Vector3 _velocity;

    private static bool Focused
    {
        get => Cursor.lockState == CursorLockMode.Locked;
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = value == false;
        }
    }

    private void OnEnable()
    {
        if (focusOnEnable)
        {
            Focused = true;
        }
    }

    private void OnDisable()
    {
        Focused = false;
    }

    private void Update()
    {
        if (Focused)
        {
            UpdateInput();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Focused = true;
        }

        _velocity = Vector3.Lerp(_velocity, Vector3.zero, damping * Time.deltaTime);
        transform.position += _velocity * Time.deltaTime;
    }

    private void UpdateInput()
    {
        _velocity += GetAccelerationVector() * Time.deltaTime;

        Vector2 mouseDelta = cameraSensitivity * new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
        Quaternion rotation = transform.rotation;
        Quaternion horizontal = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
        Quaternion vertical = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);
        transform.rotation = horizontal * rotation * vertical;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Focused = false;
        }
    }

    private Vector3 GetAccelerationVector()
    {
        Vector3 moveInput = default;

        void AddMovement(KeyCode key, Vector3 dir)
        {
            if (Input.GetKey(key))
            {
                moveInput += dir;
            }
        }

        AddMovement(KeyCode.W, Vector3.forward);
        AddMovement(KeyCode.S, Vector3.back);
        AddMovement(KeyCode.D, Vector3.right);
        AddMovement(KeyCode.A, Vector3.left);
        AddMovement(KeyCode.Space, transform.InverseTransformDirection(Vector3.up));
        AddMovement(KeyCode.LeftControl, transform.InverseTransformDirection(Vector3.down));
        Vector3 direction = transform.TransformVector(moveInput.normalized);

        return Input.GetKey(KeyCode.LeftShift) ? direction * (acceleration * sprintMultiplier) : direction * acceleration;
    }
}
