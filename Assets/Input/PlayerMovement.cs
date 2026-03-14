using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInputActions _input;
    private Vector2 _move;
    public float speed = 5f;

    void Awake()
    {
        _input = new PlayerInputActions();
    }

    void OnEnable()
    {
        _input.Player.Enable();
    }

    void OnDisable()
    {
        _input.Player.Disable();
    }

    void Update()
    {
        _move = _input.Player.Movement.ReadValue<Vector2>();

        Vector3 direction = new Vector3(_move.x, 0, _move.y);
        transform.Translate(direction * speed * Time.deltaTime);
    }
}