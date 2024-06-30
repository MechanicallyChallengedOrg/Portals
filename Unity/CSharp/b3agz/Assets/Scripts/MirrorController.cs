using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace b3agz.Portals {

    /// <summary>
    /// Attached to a "mirror" object which will then mirror its parent's positions.
    /// </summary>
    public class MirrorController : MonoBehaviour {

        private Teleportable _teleportable;
        private List<Transform> children = new();

        public void Init(Teleportable teleportable) {

            _teleportable = teleportable;
            Sanitise();

            foreach (Transform child in _teleportable.Transform) {
                children.Add(child);
            }

        }

        private void LateUpdate() {

            UpdateChildren();
            Utilities.MirrorTransformToPortal(_teleportable.Transform, transform, _teleportable.PortalTransform, _teleportable.LinkedPortal.LinkedPortal.transform);

        }

        private void UpdateChildren() {

            if (children.Count < 1) return;

            int index = 0;
            foreach (Transform child in transform) {
                child.SetLocalPositionAndRotation(children[index].localPosition, children[index].localRotation);
                index++;
            }

        }

        /// <summary>
        /// Function for clearing out any components we do not want on our mirrored objects
        /// (such as movement controllers, etc).
        /// </summary>
        public void Sanitise() {

            // Destroy all component that you do not want on a "mirror" object.
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) {
                Destroy(rb);
            }

            Collider collider = GetComponent<Collider>();
            if (collider != null) {
                Destroy(collider);
            }

            Player player = GetComponent<Player>();
            if (player != null) {
                Destroy(player);
            }

            PortalGun gun = GetComponent<PortalGun>();
            if (gun != null) {
                Destroy(gun);
            }

            foreach (Transform child in transform) {
                if (child.CompareTag("MainCamera")) {
                    Destroy(child.GetComponent<UniversalAdditionalCameraData>());
                    Destroy(child.GetComponent<AudioListener>());
                    Destroy(child.GetComponent<Camera>());
                }
            }

        }

        public void Delete() {
            Debug.Log($"Delete called on {transform.name}");
            Destroy(gameObject);
        }
    }

}
