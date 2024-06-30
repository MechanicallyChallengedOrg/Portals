using UnityEngine;

namespace b3agz.Portals {

    /// <summary>
    /// Attached to a test balls for firing through portals and whatnot.
    /// </summary>
    public class Ball : MonoBehaviour {

        [SerializeField] private float _force;
        private Rigidbody _rigidbody;

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
            if ( _rigidbody != null ) {
                Destroy(this);
            }
        }

        private void OnEnable() {
            _rigidbody.AddForce(transform.forward * _force, ForceMode.Impulse);
        }

    }
}