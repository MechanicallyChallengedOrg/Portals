using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace b3agz.Portals {

    public class Player : MonoBehaviour {

        /// <summary>
        /// How fast the player moves.
        /// </summary>
        [Tooltip("How fast the player moves.")]
        [SerializeField] private float _speed = 75f;

        /// <summary>
        /// How quickly the player rotates to look around.
        /// </summary>
        [Tooltip("How quickly the player rotates to look around.")]
        [SerializeField] private float _lookSpeed;

        /// <summary>
        /// The amount of upward force applied when the player jumps.
        /// </summary>
        [Tooltip("The amount of upward force applied when the player jumps.")]
        [SerializeField] private float _jumpForce = 10f;

        /// <summary>
        /// How quickly the player rotates to being the right way up when they come through a portal.
        /// </summary>
        [Tooltip("How quickly the player rotates to being the right way up when they come through a portal.")]
        [SerializeField] private float _uprightSpeed = 15f;

        /// <summary>
        /// How fast the player comes to a stop on the ground when no force is being applied.
        /// </summary>
        [Tooltip("How fast the player comes to a stop on the ground when no force is being applied.")]
        [SerializeField] private float _friction = 0.5f;

        /// <summary>
        /// Reference to the main look camera.
        /// </summary>
        [Tooltip("Reference to the main look camera.")]
        [SerializeField] private Transform _camera;

        /// <summary>
        /// Layermask to dictate what the player considers the ground.
        /// </summary>
        [Tooltip("Layermask to dictate what the player considers the ground.")]
        [SerializeField] private LayerMask _groundLayerMask;


        private Rigidbody _rigidBody;
        private Collider _collider;
        [SerializeField] private bool _grounded;
        private Vector3 _input;
        private bool _jump;

        private void Awake() {

            _rigidBody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();

        }

        void Update() {

            _camera.Rotate(new Vector3(-Input.GetAxis("Mouse Y") * Time.deltaTime * _lookSpeed, 0f, 0f));
            _input = Vector3.zero;
            _input += transform.forward * Input.GetAxis("Vertical");
            _input += transform.right * Input.GetAxis("Horizontal");
            _input.Normalize();

            if (_grounded) {

                if (Input.GetButtonDown("Jump")) {
                    _jump = true;
                }

            }

            // If we are not upright (our up is global up), then rotate towards global up.
            if (Vector3.Dot(transform.up, Vector3.up) != 1f) {
                transform.up = Vector3.Lerp(transform.up, Vector3.up, Time.deltaTime * _uprightSpeed);
            }

            // Glitch protection. If we somehow break out of the level and fall into oblivion,
            // reload the scene.
            if (transform.position.y < -4f) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                Application.Quit();
            }

        }

        private void FixedUpdate() {

            Move(_input);

            if (_jump) {

                _jump = false;
                _rigidBody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);

            }

            GroundCheck();

        }

        private void Move(Vector3 delta) {

            // We don't want physics affecting the player's rotation, so reset any angular velocity we have picked up.
            _rigidBody.angularVelocity = Vector3.zero;

            // Rotated around the Y axis based on player input (turning).
            _rigidBody.MoveRotation(_rigidBody.rotation * Quaternion.Euler(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * _lookSpeed));

            if (_input.magnitude > 0.1f) {

                // Apply input velocity as a force.
                _rigidBody.AddForce(delta * _speed);

            } else {

                _rigidBody.velocity = new Vector3(_rigidBody.velocity.x * _friction, _rigidBody.velocity.y, _rigidBody.velocity.z * _friction);

            }

        }

        private void GroundCheck() {

            Ray ray = new Ray(_collider.bounds.center, Vector3.down);
            _grounded = Physics.Raycast(ray, _collider.bounds.extents.y + 0.1f, _groundLayerMask, QueryTriggerInteraction.Ignore);

        }

    }
}
