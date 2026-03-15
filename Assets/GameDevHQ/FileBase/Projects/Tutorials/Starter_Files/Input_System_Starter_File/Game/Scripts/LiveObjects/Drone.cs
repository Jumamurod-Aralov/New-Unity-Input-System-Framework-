using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;
// NEW INPUT SYSTEM
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField] private Animator _propAnim;
        [SerializeField] private CinemachineVirtualCamera _droneCam;
        [SerializeField] private InteractableZone _interactableZone;

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        // NEW INPUT SYSTEM
        private PlayerInputActions _input;
        private Vector2 _move;
        private bool _thrustUp, _thrustDown, _exitDrone;

        private void Awake()
        {
            _input = new PlayerInputActions();

            // Map Input Events for Drone
            _input.Drone.Movement.performed += ctx => _move = ctx.ReadValue<Vector2>();
            _input.Drone.Movement.canceled += ctx => _move = Vector2.zero;

            _input.Drone.ThrustUp.performed += ctx => _thrustUp = true;
            _input.Drone.ThrustUp.canceled += ctx => _thrustUp = false;

            _input.Drone.ThrustDown.performed += ctx => _thrustDown = true;
            _input.Drone.ThrustDown.canceled += ctx => _thrustDown = false;

            _input.Drone.Exit.performed += ctx => _exitDrone = true;
        }

        private void OnEnable()
        {
            _input.Drone.Enable();
            // _input.Player.Enable(); // legacy input
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (!_inFlightMode && zone.GetZoneID() == 4) // Drone Scene
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                // Use Drone Action Map movement
                //_move = _input.Player.Movement.ReadValue<Vector2>(); // old

                CalculateTilt();
                CalculateMovementUpdate();

                // LEGACY INPUT
                //if (Input.GetKeyDown(KeyCode.Escape))
                //{
                //    _inFlightMode = false;
                //    onExitFlightmode?.Invoke();
                //    ExitFlightMode();
                //}

                // NEW INPUT SYSTEM
                if (_input.Drone.Exit.triggered || _exitDrone)
                {
                    _inFlightMode = false;
                    onExitFlightmode?.Invoke();
                    ExitFlightMode();
                    _exitDrone = false;
                }
            }
        }

        private void FixedUpdate()
        {
            // Gravity compensation / hovering
            _rigidbody.AddForce(transform.up * 9.81f, ForceMode.Acceleration);

            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {
            // Rotation Yaw using WASD X-axis or arrow keys
            // Legacy arrow keys commented
            //if (Input.GetKey(KeyCode.LeftArrow))
            //{ var tempRot = transform.localRotation.eulerAngles; tempRot.y -= _speed/3; transform.localRotation = Quaternion.Euler(tempRot); }
            //if (Input.GetKey(KeyCode.RightArrow))
            //{ var tempRot = transform.localRotation.eulerAngles; tempRot.y += _speed/3; transform.localRotation = Quaternion.Euler(tempRot); }

            // Using WASD X-axis
            var yawChange = _move.x * _speed * Time.deltaTime;
            _rigidbody.MoveRotation(Quaternion.Euler(
                _rigidbody.rotation.eulerAngles.x,
                _rigidbody.rotation.eulerAngles.y + yawChange,
                _rigidbody.rotation.eulerAngles.z));
        }

        private void CalculateMovementFixedUpdate()
        {
            // Vertical Movement
            //_rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration); // old Space
            //_rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration); // old V

            if (_thrustUp) _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
            if (_thrustDown) _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);

            // Forward/back movement
            var forward = transform.forward * _move.y * _speed;
            _rigidbody.MovePosition(_rigidbody.position + forward * Time.fixedDeltaTime);
        }

        private void CalculateTilt()
        {
            // Old legacy checks commented
            //if (Input.GetKey(KeyCode.A)) ...
            float roll = 0f;
            float pitch = 0f;

            if (_move.x < 0) roll = 30f;
            else if (_move.x > 0) roll = -30f;

            if (_move.y > 0) pitch = 30f;
            else if (_move.y < 0) pitch = -30f;

            float yaw = transform.localRotation.eulerAngles.y;
            _rigidbody.MoveRotation(Quaternion.Euler(pitch, yaw, roll));
        }

        private void OnDisable()
        {
            _input.Drone.Disable();
            // _input.Player.Disable(); // legacy input
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
        }
    }
}