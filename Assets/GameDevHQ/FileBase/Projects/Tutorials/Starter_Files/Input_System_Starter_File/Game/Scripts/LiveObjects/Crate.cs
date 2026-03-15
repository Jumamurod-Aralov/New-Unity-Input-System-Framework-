using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private bool _isReadyToBreak = false;

        private List<Rigidbody> _brakeOff = new List<Rigidbody>();

        private PlayerInputActions _input;

        private void Awake()
        {
            _input = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _input.Player.Enable();
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            
            if (_isReadyToBreak == false && _brakeOff.Count >0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }

            if (_isReadyToBreak && zone.GetZoneID() == 6) //Crate zone            
            {
                if (_brakeOff.Count > 0)
                {
                    //BreakPart();

                    float force = _input.Player.Interact.IsPressed() ? 15f : 5f;
                    BreakPart(force);

                    StartCoroutine(PunchDelay());
                }
                else if(_brakeOff.Count == 0)
                {
                    _isReadyToBreak = false;
                    _crateCollider.enabled = false;
                    _interactableZone.CompleteTask(6);
                    Debug.Log("Completely Busted");
                }
            }
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);
            
        }



        public void BreakPart(float forceMultiplier)
        {
            //int rng = Random.Range(0, _brakeOff.Count);
            //_brakeOff[rng].constraints = RigidbodyConstraints.None;
            //_brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
            //_brakeOff.Remove(_brakeOff[rng]);
            
            int rng = Random.Range(0, _brakeOff.Count);

            Rigidbody piece = _brakeOff[rng];
            piece.constraints = RigidbodyConstraints.None;

            Vector3 force = new Vector3(1f, 1f, 1f) * forceMultiplier;
            piece.AddForce(force, ForceMode.Impulse);

            _brakeOff.Remove(piece);
        }

        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }

        private void OnDisable()
        {
            _input.Player.Disable();
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
        }
    }
}
