using UnityEngine;

namespace b3agz.Portals {

    /// <summary>
    /// A class used to track objects that can be teleported. When the object is within range of a portal,
    /// this class can determine whether the object has moved through the portal.
    /// </summary>
    public class Teleportable {

        /// <summary>
        /// The transform of the Teleportable.
        /// </summary>
        public Transform Transform { get; private set; }

        /// <summary>
        /// The rigidbody attached to this teleportable (if it has one).
        /// </summary>
        public Rigidbody Rigidbody { get; private set; }

        /// <summary>
        /// The collider attached to this teleportable (if it has one).
        /// </summary>
        public Collider Collider { get; private set; }

        /// <summary>
        /// The transform of the portal currently tracking this Teleportable.
        /// </summary>
        public Transform PortalTransform => LinkedPortal.transform;

        public Portal LinkedPortal;

        public MirrorController MController { get; private set; }
        public Transform MirrorTransform => MController.transform;

        /// <summary>
        /// The sign of the previous side, used to compare against the current side so we can see if it has changed.
        /// </summary>
        private int _previousSide;

        /// <summary>
        /// Returns true if the teleportable is no longer on the same side of the portal as it was last time
        /// the side was updated and sets previous side to current side so the check is accurate next time.
        /// </summary>
        public bool SideChanged {
            get {
                bool val = (_previousSide != CurrentSide);
                _previousSide = CurrentSide;
                return val;
            }
        }

        /// <summary>
        /// Returns the current offset of this Teleportable from the portal it is overlapping.
        /// </summary>
        private Vector3 CurrentOffset => Transform.position - PortalTransform.position;

        /// <summary>
        /// Returns the current side of the portal as an int. 1 is in front, -1 is behind.
        /// </summary>
        private int CurrentSide => System.Math.Sign(Vector3.Dot(CurrentOffset, PortalTransform.forward));

        /// <summary>
        /// Compares this Teleportable to another Teleportable to see if they contain the same references.
        /// </summary>
        /// <param name="obj">The Teleportable being compared.</param>
        /// <returns>True if both Teleportables contain the same references.</returns>
        public override bool Equals(object obj) {

            Teleportable item = obj as Teleportable;
            if (item == null) return false;
            if (item.Transform != Transform) return false;
            if (item.PortalTransform != PortalTransform) return false;
            return true;

        }

        /// <summary>
        /// Overriding Equals() without overriding GetHashCode is a big no no.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return base.GetHashCode();
        }

        /// <summary>
        /// Create a mirrored version of this teleportable.
        /// </summary>
        /// <param name="obj">The GameObject that is the teleportable.</param>
        public GameObject CreateMirror() {

            // If we're trying to create a mirror when we already have one... don't.
            if (MController != null || Transform == null) {
                return null;
            }

            GameObject obj = Transform.gameObject;

            // Instantiate a copy of the teleportable.
            GameObject mirrorObject = Object.Instantiate(obj);

            // Add a MirrorController to the mirrored object so we can manipulate it later.
            MController = mirrorObject.AddComponent<MirrorController>();

            // Initialise MirrorController.
            MController.Init(this);

            return mirrorObject;

        }

        /// <summary>
        /// Deletes the attached mirror object from the scene.
        /// </summary>
        public void RemoveMirror() {
            Debug.Log($"RemoveMirror called on {MirrorTransform.name} teleportable.");
            MController.Delete();
        }

        /// <summary>
        /// Initialises a Teleportable instance, assigning the associated transforms.
        /// </summary>
        /// <param name="teleportable">The Transform of the teleportable being created.</param>
        /// <param name="portal">The transform of the portal this Teleportable is overlapping.</param>
        public Teleportable(Transform teleportable, Portal portal) {

            Transform = teleportable;
            Collider = teleportable.GetComponent<Collider>();
            Rigidbody = teleportable.GetComponent<Rigidbody>();
            LinkedPortal = portal;

            // Set the previousSide value so we don't immediately return true from SideChanged.
            _previousSide = CurrentSide;

        }

    }

}
