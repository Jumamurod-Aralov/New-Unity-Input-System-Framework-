using System;
using UnityEngine;
using Cinemachine;

namespace Game.Scripts.LiveObjects
{
    public class Forklift : MonoBehaviour
    {
        [SerializeField]
        private GameObject _lift, _steeringWheel, _leftWheel, _rightWheel, _rearWheels;
        [SerializeField]
        private Vector3 _liftLowerLimit, _liftUpperLimit;
        [SerializeField]
        private float _speed = 5f, _liftSpeed = 1f;
        [SerializeField]
        private CinemachineVirtualCamera _forkliftCam;
        [SerializeField]
        private GameObject _driverModel;
        private bool _inDriveMode = false;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onDriveModeEntered;
        public static event Action onDriveModeExited;

        // NEW INPUT SYSTEM
        private PlayerInputActions _input;
        private Vector2 _move;

        private void Awake()
        {
            _input = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _input.Player.Enable();
            InteractableZone.onZoneInteractionComplete += EnterDriveMode;
        }

        private void EnterDriveMode(InteractableZone zone)
        {
            if (_inDriveMode !=true && zone.GetZoneID() == 5) //Enter ForkLift
            {
                _inDriveMode = true;
                _forkliftCam.Priority = 11;
                onDriveModeEntered?.Invoke();
                _driverModel.SetActive(true);
                _interactableZone.CompleteTask(5);
            }
        }

        private void ExitDriveMode()
        {
            _inDriveMode = false;
            _forkliftCam.Priority = 9;            
            _driverModel.SetActive(false);
            onDriveModeExited?.Invoke();
            
        }

        private void Update()
        {
            if (_inDriveMode == true)
            {
                _move = _input.Player.Movement.ReadValue<Vector2>();
                LiftControls();
                CalcutateMovement();

                //if (Input.GetKeyDown(KeyCode.Escape))
                if (_input.Player.Exit.triggered)
                    ExitDriveMode();
            }

        }

        private void CalcutateMovement()
        {
            // LEGACY INPUT SYSTEM
            //float h = Input.GetAxisRaw("Horizontal");
            //float v = Input.GetAxisRaw("Vertical");
            //var direction = new Vector3(0, 0, v);
            //var direction = new Vector3(0, 0, _move.y
            //var velocity = direction * _speed;

            //transform.Translate(velocity * Time.deltaTime);

            //if (Mathf.Abs(v) > 0)
            //{
            //    var tempRot = transform.rotation.eulerAngles;
            //    tempRot.y += h * _speed / 2;
            //    transform.rotation = Quaternion.Euler(tempRot);
            //}

            // NEW INPUT SYSTEM
            // Forward/backward movement
            var direction = new Vector3(0, 0, _move.y);
            var velocity = direction * _speed;

            transform.Translate(velocity * Time.deltaTime);

            // Apply rotation if moving forward/backward
            if (Mathf.Abs(_move.y) > 0)
            {
                var tempRot = transform.rotation.eulerAngles;
                tempRot.y += _move.x * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }
        }

        private void LiftControls()
        {
            //if (Input.GetKey(KeyCode.R))
            //    LiftUpRoutine();
            //else if (Input.GetKey(KeyCode.T))
            //    LiftDownRoutine();

            // NEW INPUT SYSTEM
            if (_input.Player.LiftUp.IsPressed())
                LiftUpRoutine();
            else if (_input.Player.LiftDown.IsPressed())
                LiftDownRoutine();
        }

        private void LiftUpRoutine()
        {
            if (_lift.transform.localPosition.y < _liftUpperLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y += Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y >= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftUpperLimit;
        }

        private void LiftDownRoutine()
        {
            if (_lift.transform.localPosition.y > _liftLowerLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y -= Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y <= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftLowerLimit;
        }

        private void OnDisable()
        {
            _input.Player.Disable();
            InteractableZone.onZoneInteractionComplete -= EnterDriveMode;
        }

    }
}