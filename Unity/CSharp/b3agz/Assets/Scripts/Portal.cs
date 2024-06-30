using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace b3agz.Portals {

    public class Portal : MonoBehaviour {

        /// <summary>
        /// The partner portal. When a Teleportable object passes through this portal, it will appear at the _partner portal.
        /// </summary>
        [SerializeField] private Portal _partner;
        public Portal LinkedPortal => _partner;

        [field: SerializeField] public Color Colour { get; private set; }

        private Collider _boundaryTriggerCollider;

        [SerializeField] private Collider _edgeProtectionCollider;

        /// <summary>
        /// This camera serves as the viewpoint. If you want to view the portals from a camera other than the player's,
        /// you would need to set it here.
        /// </summary>
        private Camera _playerCamera;

        /// <summary>
        /// The RenderTexture we create to display the portal's "interior" on.
        /// </summary>
        private RenderTexture _portalTexture;

        /// <summary>
        /// The MeshRenderer of this portal, where we assign _portalTexture and make any adjustments to the material.
        /// </summary>
        [SerializeField] private MeshRenderer _portalRenderer;

        /// <summary>
        /// This portal's "view" camera, used to show the view from this portal for pasting onto the _partner portal.
        /// </summary>
        [SerializeField] private Camera _camera;

        /// <summary>
        /// For the maths to work, one of the portals needs to be "flipped". This bool tells us whether to
        /// flip the direction of forward when setting the portal position.
        /// </summary>
        [SerializeField] private bool _flipped;

        /// <summary>
        /// The collider that this portal is currently sitting on.
        /// </summary>
        private Collider _attachedCollider;

        /// <summary>
        /// Public access to the portal's view camera.
        /// </summary>
        public Camera Cam => _camera;

        /// <summary>
        /// Public access to the portal's view camera transform.
        /// </summary>
        public Transform CamTransform => Cam.transform;

        /// <summary>
        /// Returns true if the portal is currently closed.
        /// </summary>
        public bool IsClosed => _portalRenderer.material.GetFloat("_PortalSize") == 0;

        public bool Active { get; private set; } = false;

        /// <summary>
        /// Any object that can be teleported through a portal must inherit from the Teleportable class.
        /// This list keeps track of any teleportables that have collided with the portal so that we can
        /// check if the teleportable has crossed through the portal's boundary.
        /// </summary>
        [SerializeField] private List<Teleportable> _teleportables = new();

        private void Awake() {
            _boundaryTriggerCollider = GetComponent<Collider>();
        }

        private void Start() {

            _playerCamera = Camera.main;
            SetSize();
            SetColour(Colour);
            UpdateViewTexture();

            // By default at the start of the game our portals are inactive.
            _portalRenderer.material.SetFloat("_PortalSize", 0f);
            Deactivate();
            ProtectAgainstClipping();

        }

        private void FixedUpdate() {
            
        }

        private void LateUpdate() {

            if (!Active || !_partner.Active) return;

            UpdateTeleportables();
            UpdateViewTexture();
            UpdateViewCamera();
            ProtectAgainstClipping();

            // Force camera to render at this point to ensure visuals don't lag.
            Cam.Render();

        }

        private void OnTriggerStay(Collider other) {

            if (!Active || !_partner.Active) return;

            if (other.CompareTag("Player") || other.CompareTag("Teleportable")) {
                //Debug.Log($"{transform.name} has started tracking {other.name}");
                NoDuplicateAddToTeleportables(new Teleportable(other.transform, this));
            }

        }

        private void OnTriggerExit(Collider other) {

            if (!Active || !_partner.Active) return;

            if (other.CompareTag("Player") || other.CompareTag("Teleportable")) {
                //Debug.Log($"{transform.name} has stopped tracking {other.name}");
                RemoveTeleportable(new Teleportable(other.transform, this));
            }
        }

        private void SetSize() {
            transform.localScale = new Vector3(Globals.PortalWidth, Globals.PortalHeight, 1f);
        }

        /// <summary>
        /// Checks to see if any of the currently tracked Teleportables have crossed the
        /// boundary of the portal. If they have, we move them to the corresponding position
        /// at the other portal.
        /// </summary>
        private void UpdateTeleportables() {

            // Loop through each teleportable currently being tracked by the portal. We loop backwards because
            // we may need to remove items from the list and going backwards ensures the any changes don't affect
            // the our iterative check.
            for (int i = _teleportables.Count - 1; i >= 0; i--) {

                // Check to see if this Teleportable's side has changed since the last time we checked. If it has,
                // this teleportable has moved through the portal.
                if (_teleportables[i].SideChanged) {

                    CharacterController cc = _teleportables[i].Transform.GetComponent<CharacterController>();
                    if (cc != null) {
                        cc.enabled = false;
                    }

                    // Update the Teleportable's rotation and teleport to new location.
                    Utilities.InvertAndTeleportTransform(_teleportables[i].Transform, transform, _partner.transform);

                    // If the Teleportable has a Rigidbody attached and that Rigidbody has velocity, invert the velocity.
                    if (_teleportables[i].Rigidbody != null && _teleportables[i].Rigidbody.velocity.sqrMagnitude > 0.001f) {
                        _teleportables[i].Rigidbody.velocity = Utilities.InvertRelativeDirection(_teleportables[i].Rigidbody.velocity, transform, _partner.transform);
                    }

                    // Since we can't rely on OnTriggerExit() to register our Teleportable leaving when we move the transform
                    // manually, we remove it from teh list of teleportables here.
                    RemoveTeleportable(_teleportables[i]);

                }
            }
        }

        /// <summary>
        /// Updates the position of the portal's camera based on the main camera's position.
        /// </summary>
        private void UpdateViewCamera() {
            
            _partner.CamTransform.SetPositionAndRotation(_playerCamera.transform.position, _playerCamera.transform.rotation);

            // Get a Vector3 that represents the player camera's relative position from this portal.
            Vector3 relativePos = transform.InverseTransformPoint(_playerCamera.transform.position);

            // Adjust the Vector3 so it's position is relative to the other portal, not this one.
            relativePos = _partner.transform.TransformPoint(relativePos);

            Quaternion relativeRot = Quaternion.Inverse(transform.rotation) * _partner.CamTransform.rotation;
            relativeRot = _partner.transform.rotation * relativeRot;

            _partner.CamTransform.SetPositionAndRotation(relativePos, relativeRot);

        }

        /// <summary>
        /// Checks to see if the screen dimensions have changed and adjusted the viewtexture
        /// resolution accordingly.
        /// </summary>
        private void UpdateViewTexture() {

            // If we already have a RenderTexture and that texture is the correct width and height for the screen,
            // we don't need to update it. Just return.
            if (_portalTexture != null && _portalTexture.width == Screen.width && _portalTexture.height == Screen.height) {
                return;
            }

            // Create a new RenderTexture based on screen size. If not, the perspective will be wrong.
            _portalTexture = new RenderTexture(Screen.width, Screen.height, 0);

            // Set the RenderTexture as the target of the OTHER portal's view camera.
            _partner.Cam.targetTexture = _portalTexture;

            // Set the RenderTexture as the main texture of THIS portal's renderer.
            _portalRenderer.material.SetTexture("_MainTex", _portalTexture);

        }

        /// <summary>
        /// Credit: Sebastian Lague. This code resizes the portal depth on the fly so that it always
        /// matches the cross section of the camera. This prevents the player from clipping and seeing
        /// halfway through the 
        /// </summary>
        private void ProtectAgainstClipping() {
            float halfHeight = _playerCamera.nearClipPlane * Mathf.Tan(_playerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * _playerCamera.aspect;
            float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, _playerCamera.nearClipPlane).magnitude;
            float screenThickness = dstToNearClipPlaneCorner;

            Transform screenT = _portalRenderer.transform;
            bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - _playerCamera.transform.position) > 0;
            screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
            screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
        }

        /// <summary>
        /// Adds a Teleportable to the _teleportables list after checking to make sure it is not already
        /// in the list.
        /// </summary>
        /// <param name="t">The Teleportable being added.</param>
        public void NoDuplicateAddToTeleportables(Teleportable t) {

            // Loop through each of the currently tracked teleportables and make sure the one we're trying to
            // add isn't already there.
            foreach (Teleportable teleportable in _teleportables) {
                if (teleportable.Equals(t)) {
                    return;
                }
            }

            if (t.Collider != null && _attachedCollider != null) {
                //Debug.Log($"Disabling collisions between {t.Transform.name} and {_attachedCollider.name}");
                Physics.IgnoreCollision(t.Collider, _attachedCollider);
            }

            

            GameObject mirror = t.CreateMirror();
            if (mirror != null) {
                _partner.SetSliceMaterial(mirror, true);
            }
            SetSliceMaterial(t.Transform.gameObject, true);
            _teleportables.Add(t);

        }

        /// <summary>
        /// Sets the colour of this portal.
        /// </summary>
        /// <param name="colour">The Color of the portal should be set to.</param>
        public void SetColour(Color colour) {
            _portalRenderer.material.SetColor("_Colour", colour);
        }

        /// <summary>
        /// Searches the currently tracked teleportables list for t and, if found, removes it.
        /// </summary>
        /// <param name="t">The teleportable to be removed.</param>
        private void RemoveTeleportable(Teleportable t) {

            // Loop through currently tracked teleportables.
            for (int i = 0; i < _teleportables.Count; i++) {

                if (_teleportables[i].Equals(t)) {
                    // If the Teleportable has a collider and the portal is attached to a collider, set them to
                    // register collisions with each other again.
                    if (_teleportables[i].Collider != null && _attachedCollider != null) {
                        //Debug.Log($"Enabling collisions between {t.Transform.name} and {_attachedCollider.name}");
                        Physics.IgnoreCollision(_teleportables[i].Collider, _attachedCollider, false);
                    }
                    Debug.Log($"Attempting to remove {_teleportables[i].Transform.name}. Name of mirror object: {_teleportables[i].MirrorTransform.name}");
                    SetSliceMaterial(_teleportables[i].Transform.gameObject, false);
                    _teleportables[i].RemoveMirror();
                    _teleportables.RemoveAt(i);
                    return;

                }
            }
        }

        public void Activate() {
            Active = true;
            _edgeProtectionCollider.enabled = true;
            _portalRenderer.material.SetFloat("_Active", 1f);
        }

        public void Deactivate() {
            Active = false;
            _edgeProtectionCollider.enabled = false;
            _portalRenderer.material.SetFloat("_Active", 0f);
        }

        /// <summary>
        /// Sets the forward direction of this portal factoring in whether this is a flipped portal or not.
        /// </summary>
        /// <param name="direction">Vector3 of the direction we want forward to be.</param>
        public void SetForward(Vector3 direction) {

            if (_flipped) {
                transform.forward = -direction;
            } else {
                transform.forward = direction;
            }

        }

        public void SetSliceMaterial(GameObject teleportable, bool enable) {

            MeshRenderer[] renderers = teleportable.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers) {
                    AssignSliceInfo(renderer, enable);
            }

        }

        private void AssignSliceInfo(MeshRenderer mR, bool enable) {

            List<Material> materials = new();
            mR.GetMaterials(materials);

            if (materials.Count < 1) return;

            foreach (Material material in materials) {

                if (!enable) {
                    material.SetFloat("_Active", 0f);
                } else {

                    Vector3 forward = _flipped ? transform.forward : -transform.forward;

                    material.SetFloat("_Active", 1f);
                    material.SetVector("_SliceCentre", transform.position - (forward * 0.5f));
                    material.SetVector("_SliceNormal", forward);
                }

            }

            mR.SetMaterials(materials);

        }

        public void MovePortal(Vector3 position, Vector3 normal, Collider collider) {
            StartCoroutine(movePortal(position, normal, collider));
        }

        private IEnumerator movePortal(Vector3 position, Vector3 normal, Collider collider) {

            // Loop through each teleportable we currently have and remove it. We use RemoveTeleportable
            // to ensure any disabled collisions are re-established, but this needs improving as it's
            // currently very wasteful.
            for (int i =  _teleportables.Count - 1; i >= 0; i--) {
                RemoveTeleportable(_teleportables[i]);
            }

            // Attach the new collider to the portal so we know what to exclude in our teleportable interactions.
            _attachedCollider = collider;

            // Get the current size of the portal from the shader.
            float value = _portalRenderer.material.GetFloat("_PortalSize");

            // If portal is currently open, shrink it first. If not, we can skip this part.
            if (value > 0f) {
                while (value != 0f) {

                    value = Mathf.MoveTowards(value, 0f, Time.deltaTime * Globals.PortalBloopSpeed);
                    _portalRenderer.material.SetFloat("_PortalSize", value);
                    yield return null;

                }
            }

            // Set the position and direction of the portal.
            transform.position = (position + (normal * 0.01f));
            _partner.Activate();
            //transform.position = position;
            SetForward(normal);

            // Open the portal.
            while (value != 1f) {

                value = Mathf.MoveTowards(value, 1f, Time.deltaTime * Globals.PortalBloopSpeed);
                _portalRenderer.material.SetFloat("_PortalSize", value);
                yield return null;

            }

        }

    }
}