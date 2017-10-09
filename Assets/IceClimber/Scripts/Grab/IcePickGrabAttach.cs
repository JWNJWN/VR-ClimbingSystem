namespace IceClimber.Grab
{
    using System;
    using UnityEngine;
    using VRTK.GrabAttachMechanics;

    public class IcePickGrabAttach : VRTK_BaseGrabAttach
    {
        public float detachDistance = 1f;

        public bool isInsideWall = false;

        public override void StopGrab(bool applyGrabbingObjectVelocity)
        {
            ReleaseObject(applyGrabbingObjectVelocity);
            base.StopGrab(applyGrabbingObjectVelocity);
        }

        public override Transform CreateTrackPoint(Transform controllerPoint, GameObject currentGrabbedObject, GameObject currentGrabbingObject, ref bool customTrackPoint)
        {
            Transform trackPoint = null;
            if (precisionGrab)
            {
                trackPoint = new GameObject(string.Format("[{0}]TrackObject_PrecisionSanp_AttachPoint", currentGrabbedObject.name)).transform;
                trackPoint.parent = currentGrabbingObject.transform;
                SetTrackPointOrientation(ref trackPoint, currentGrabbingObject.transform, controllerPoint);
                customTrackPoint = true;
            }
            else
            {
                trackPoint = base.CreateTrackPoint(controllerPoint, currentGrabbedObject, currentGrabbingObject, ref customTrackPoint);
            }

            return trackPoint;
        }

        public override void ProcessUpdate()
        {
            if (trackPoint && grabbedObjectScript.IsDroppable())
            {
                float distance = Vector3.Distance(trackPoint.position, initialAttachPoint.position);
                if (distance > detachDistance)
                    ForceReleaseGrab();
            }
        }

        public override void ProcessFixedUpdate()
        {
            if (isInsideWall)
                grabbedObjectRigidBody.useGravity = false;
            else
                grabbedObjectRigidBody.useGravity = true;

            float maxDistanceDelta = 10f;
            float angle;
            Vector3 axis;
            Vector3 positionDelta;
            Quaternion rotationDelta;

            if (grabbedSnapHandle != null)
            {
                rotationDelta = trackPoint.rotation * Quaternion.Inverse(grabbedSnapHandle.rotation);
                positionDelta = trackPoint.position - grabbedSnapHandle.position;
            }
            else
            {
                rotationDelta = trackPoint.rotation * Quaternion.Inverse(grabbedObject.transform.rotation);
                positionDelta = trackPoint.position - grabbedObject.transform.position;
            }

            rotationDelta.ToAngleAxis(out angle, out axis);

            angle = ((angle > 180) ? angle -= 360 : angle);


            if (angle != 0)
            {
                Vector3 angularTarget = angle * axis;

                if (isInsideWall)
                {
                    Vector3 angularVelocity = transform.InverseTransformVector(angularTarget);
                    angularVelocity.y = 0;
                    angularVelocity.z = 0;
                    if (angularVelocity.x > 0)
                        angularVelocity.x = 0;
                    angularTarget = transform.TransformVector(angularVelocity);
                }
                grabbedObjectRigidBody.angularVelocity = Vector3.MoveTowards(grabbedObjectRigidBody.angularVelocity, angularTarget, maxDistanceDelta);
            }

            Vector3 velocityTarget = positionDelta / Time.fixedDeltaTime;
            if (isInsideWall)
            {
                Vector3 localVelocity = transform.InverseTransformVector(velocityTarget);
                localVelocity.x = 0;
                localVelocity.z = 0;
                if (localVelocity.y < 0)
                    localVelocity.y = 0;
                velocityTarget = transform.TransformVector(localVelocity);
            }

            grabbedObjectRigidBody.velocity = Vector3.MoveTowards(grabbedObjectRigidBody.velocity, velocityTarget, maxDistanceDelta);
        }

        protected override void Initialise()
        {
            tracked = true;
            climbable = false;
            kinematic = false;
        }

        protected virtual void SetTrackPointOrientation(ref Transform trackPoint, Transform currentGrabbedObject, Transform controllerPoint)
        {
            trackPoint.position = currentGrabbedObject.position;
            trackPoint.rotation = currentGrabbedObject.rotation;
        }
    }
}