using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace b3agz.Portals {

    /// <summary>
    /// Some useful reusable functions.
    /// </summary>
    public static class Utilities {

        /// <summary>
        /// Takes in two portal transforms and an target, calculates the relative position and rotation of the
        /// target from the origin portal to the destination portal and moves it.
        /// </summary>
        public static void InvertAndTeleportTransform(Transform target, Transform originPortal, Transform destinationPortal) {

            // Initialise our rotationDifference quaternion with no rotation. We need to make sure the portals are not
            // facing the same direction before calculating the difference.
            Quaternion rotationDifference = Quaternion.identity;

            if (originPortal.rotation != destinationPortal.rotation) {
                // Calculate the new rotation of the teleportable on the other side of the portal by getting the
                // inverse of the teleportables current rotation and multiplying it by the destination portal's
                // rotation.
                rotationDifference = Quaternion.Inverse(originPortal.rotation) * destinationPortal.rotation;
            }

            // Calculate the teleportable's position relative to this portal.
            Vector3 relativePosition = originPortal.InverseTransformPoint(target.position);

            // Adjust the relative position based on the rotation differences between this portal and it's partner.
            relativePosition = destinationPortal.TransformPoint(relativePosition);

            // Set the position and rotation using the new values.
            target.SetPositionAndRotation(relativePosition, rotationDifference * target.rotation);

        }

        /// <summary>
        /// Inverts the relative direction (direction) from originPortal to destinationPortal.
        /// </summary>
        /// <param name="direction">The direction being transformed.</param>
        /// <param name="originPortal">The portal being entered.</param>
        /// <param name="destinationPortal">The portal being exited.</param>
        /// <returns></returns>
        public static Vector3 InvertRelativeDirection (Vector3 direction, Transform originPortal, Transform destinationPortal) {

            // Calculate the local direction relative to origin portal.
            direction = originPortal.InverseTransformDirection(direction);

            // Calculate the global direction relative to the destination portal.
            direction = destinationPortal.transform.TransformDirection(direction);
            return direction;

        }

        public static void MirrorTransformToPortal(Transform source, Transform target, Transform originPortal, Transform destinationPortal) {

            // Initialise our rotationDifference quaternion with no rotation. We need to make sure the portals are not
            // facing the same direction before calculating the difference.
            Quaternion rotationDifference = Quaternion.identity;

            if (originPortal.rotation != destinationPortal.rotation) {
                // Calculate the new rotation of the teleportable on the other side of the portal by getting the
                // inverse of the teleportables current rotation and multiplying it by the destination portal's
                // rotation.
                rotationDifference = Quaternion.Inverse(originPortal.rotation) * destinationPortal.rotation;
            }

            // Calculate the teleportable's position relative to this portal.
            Vector3 relativePosition = originPortal.InverseTransformPoint(source.position);

            // Adjust the relative position based on the rotation differences between this portal and it's partner.
            relativePosition = destinationPortal.TransformPoint(relativePosition);

            // Set the position and rotation using the new values.
            target.SetPositionAndRotation(relativePosition, rotationDifference * source.rotation);

        }

    }

}
