using UnityEngine;

namespace b3agz.Portals {

    /// <summary>
    /// Script for controlling the projectile that is fired from the portal gun. When it hits
    /// a valid object, it moves the associated portal to that position.
    /// </summary>
    public class PortalBall : MonoBehaviour {

        /// <summary>
        /// The portal that will be spawned at this position.
        /// </summary>
        [SerializeField] private Portal _portal;

        /// <summary>
        /// Layermask to ensure we can ignore certain objects.
        /// </summary>
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private MeshRenderer _meshRenderer;

        Vector3 _previousPosition;

        private Rigidbody _rigidbody;
        private float _timer;

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start() {
            _previousPosition = transform.position;
            _meshRenderer.material.SetColor("_Colour", _portal.Colour);
        }

        private void Update() {

            // Increment our timer value and compare it to our maximum portal ball lifetime. If timer exceed
            // that time, destroy the ball.
            _timer += Time.deltaTime;
            if (_timer > Globals.PortalBallLifetime) {
                DestroyBall();
            }

            //ValidatePosition(transform.position, transform.forward, null);

        }

        void FixedUpdate() {

            // Move the portal ball in a straight line by our global portal ball speed.
            _rigidbody.MovePosition(transform.position + transform.forward * Globals.PortalGunSpeed * Time.deltaTime);

            // Perform a LineCast to see if the ball has encountered an object along its path.
            RaycastHit hit;
            if (Physics.Linecast(_previousPosition, _rigidbody.position, out hit, _layerMask, QueryTriggerInteraction.Ignore)) {

                // Make sure the ball has hit a valid surface. If not, destroy and return.
                if (!hit.transform.CompareTag("PortalSurface")) {
                    DestroyBall();
                    return;
                }

                ValidatePosition(hit.point, hit.normal, hit.collider);
                // Give the portal it's new position and forward direction and the collider it is attached to.
                //_portal.MovePortal(hit.point, hit.normal, hit.collider);

                // Don't forget to destroy the ball.
                //DestroyBall();
                return;

            }
            
            // Store the current position as the previous position for comparing next frame.
            _previousPosition = _rigidbody.position;

        }

        private void ValidatePosition(Vector3 position, Vector3 normal, Collider collider) {

            position += (normal * 0.025f);

            float angleStep = 180f / Globals.PortalSpaceCheckFidelity;

            for (int i = 0; i < Globals.PortalSpaceCheckFidelity; i++) {

                float angle = i * angleStep;
                Vector3 direction = AngleToVector3(angle, normal);
                direction = AdjustDirectionForPortalShape(direction, normal);
                float rayLength = Vector3.Distance(position, position + direction);

                // Cast A, if needs to move, adjust cast B to compensate.
                float a = PortalLineCast(position, direction, normal, Color.green);
                float b = PortalLineCast(position, -direction, normal, Color.green);

                // Compare our first cast to the distance we intended. If it is not equal, adjust the opposing raycast.
                if (Mathf.Abs(rayLength - a) > 0.001f) {

                    // Cast the opposing ray from the adjusted position.
                    b = PortalLineCast(position - (direction.normalized * (rayLength - a)), -direction, normal, Color.green);
                    Debug.Log($"A length: {rayLength - a} ({rayLength}, {a})");
                    // If the second raycast from the adjusted position fails, there isn't enough room for a portal.
                    if (Mathf.Abs(rayLength - b) > 0.001f) {
                        DestroyBall();
                        return;

                    // If it doesn't fail, adjust the position.
                    } else {
                        position -= direction.normalized * (rayLength - a);
                    }

                // Else we just cast the second cast and check to see if THAT needs moving.
                } else if (Mathf.Abs(rayLength - b) > 0.001f) {

                    // Recast cast A adjusted for the position shift from cast B
                    a = PortalLineCast(position + (direction.normalized * (rayLength - b)), direction, normal, Color.blue);
                    Debug.Log($"B length: {rayLength - b} ({rayLength}, {b})");
                    // If the new cast returns shorter than ray length, we do not have enough room for the
                    // portal at this position. Destroy the portal ball and return.
                    if (Mathf.Abs(rayLength - a) > 0.001f) {
                        DestroyBall();
                        return;
                        
                    // If the A recast is not shortened (meaning there is enough space for the portal),
                    // then reposition the portal, moving it up and away from the obstacle that b encountered.
                    } else {
                        position += direction.normalized * (rayLength - b);
                    }

                }

            }

            position -= (normal * 0.025f);

            // Give the portal it's new position and forward direction and the collider it is attached to.
            _portal.MovePortal(position, normal, collider);

            // Don't forget to destroy the ball.
            DestroyBall();

        }

        Vector3 AngleToVector3(float angle, Vector3 forward) {
            float radian = angle * Mathf.Deg2Rad;
            Vector3 up = CalculateUpDirection(forward);
            Vector3 right = Vector3.Cross(up, forward);
            return (Mathf.Cos(radian) * right + Mathf.Sin(radian) * up).normalized;
        }

        public static Vector3 CalculateUpDirection(Vector3 normal) {
            // Choose an arbitrary vector that is not parallel to the normal
            Vector3 arbitrary = (Mathf.Abs(normal.y) < 0.9f) ? Vector3.up : Vector3.forward;

            // Calculate the right direction using cross product
            Vector3 right = Vector3.Cross(arbitrary, normal).normalized;

            // Calculate the up direction using cross product
            Vector3 up = Vector3.Cross(normal, right).normalized;
            return up;
        }

        private Vector3 AdjustDirectionForPortalShape(Vector3 direction, Vector3 normal) {

            Vector3 up = CalculateUpDirection(normal);
            Vector3 right = Vector3.Cross(up, normal);

            float horizontalComponent = Vector3.Dot(direction, right) * Globals.PortalWidthRadius;
            float verticalComponent = Vector3.Dot(direction, up) * Globals.PortalHeightRadius;

            return (horizontalComponent * right + verticalComponent * up);

        }

        public float PortalLineCast(Vector3 position, Vector3 direction, Vector3 forward, Color color) {

            // Step 1: Cast a ray in the current direction, cache the distance that we travelled until we hit something.
            // Step 2: Cast a ray backwards register true or false is something there.
            // Step 3: If false, cast a ray downwards, cache the distance until we hit something.
            // Step 4: Compare the first distance and the second distance and use the shortest distance.

            float rayLength = Vector3.Distance(position, position + direction);
            Ray ray = new (position, direction);
            float currentLength = rayLength;

            // Cast a ray in the direction we have been given at the desired length.
            if (Physics.Raycast(ray, out RaycastHit hit, rayLength, _layerMask)) {
                // If we hit something here, we've encountered an obstacle and need to adjust our ray length.
                currentLength = hit.distance;
                Debug.DrawLine(position, hit.point, Color.red, 7.5f);
            } else {
                // If we didn't hit anything, we can continue the checks with the desired length.
                Debug.DrawLine(position, position + direction, color, 7.5f);
            }

            Vector3 nextPosition = position + direction.normalized * currentLength;
            ray = new(nextPosition, -forward);

            // Cast another ray backwards from the point we reached.
            if (Physics.Raycast(ray, 0.15f, _layerMask)) {
                // If we hit something here, we're on a solid surface and we can return the current length.
                return currentLength;
            } else {
                // If we didn't hit something, our portal is hanging out in space somewhere and we need to do
                // more checks to see where it needs to move to.
                Debug.DrawLine(nextPosition, nextPosition - (forward * 0.15f), Color.red, 7.5f);
            }

            Vector3 lastPosition = nextPosition - (forward * 0.15f);
            ray = new(lastPosition, -direction);

            // Cast a final ray from the point of our last check back along the direction we originally travelled.
            if (Physics.Raycast(ray, out hit, rayLength, _layerMask)) {
                // If we hit something, we've encountered some solid surface and we can return a length that reflects that.
                Debug.DrawLine(lastPosition, lastPosition - (direction.normalized * hit.distance), color, 7.5f);
                return rayLength - hit.distance;
            } else {
                // As we're comparing two opposite PortalLineCasts to ensure they add up to at least the diameter of the portal
                // returning a minus number is just a way of ensuring this pair of casts fails.
                return 0;
            }
        }

        /// <summary>
        /// Sets the attached portal of this portal ball. This is the portal that will be position where the ball
        /// impacts something.
        /// </summary>
        /// <param name="portal">The Portal class that we are attached to.</param>
        public void SetPortal(Portal portal) {
            _portal = portal;
        }

        /// <summary>
        /// Destroys this portal ball.
        /// </summary>
        private void DestroyBall() {
            Destroy(gameObject);
        }

    }

}
