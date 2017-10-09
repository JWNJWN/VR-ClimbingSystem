namespace IceClimber.Base
{
    using UnityEngine;
    using VRTK.GrabAttachMechanics;

    public class ICHangableGrabAttach : VRTK_BaseGrabAttach
    {
        public float detachDistance = 1f;

        private ICHangableObject GrabbedObjectScript
        {
            get { return (ICHangableObject)grabbedObjectScript; }
        }

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
            base.ProcessFixedUpdate();

            grabbedObjectRigidBody.useGravity = !((ICHangableObject)grabbedObjectScript).isInsideWall;

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
                if (GrabbedObjectScript.isInsideWall)
                    angularTarget = transform.TransformVector(RestrictRotation(transform.InverseTransformVector(angularTarget)));

                grabbedObjectRigidBody.angularVelocity = Vector3.MoveTowards(grabbedObjectRigidBody.angularVelocity, angularTarget, maxDistanceDelta);
            }

            Vector3 velocityTarget = positionDelta / Time.fixedDeltaTime;
            if (GrabbedObjectScript.isInsideWall)
                velocityTarget = transform.TransformVector(RestrictMovement(transform.InverseTransformVector(velocityTarget)));

            grabbedObjectRigidBody.velocity = Vector3.MoveTowards(grabbedObjectRigidBody.velocity, velocityTarget, maxDistanceDelta);
        }
        
        protected virtual Vector3 RestrictMovement(Vector3 localVelocity)
        {
            localVelocity.x = 0;
            localVelocity.z = 0;
            if (localVelocity.y < 0)
                localVelocity.y = 0;
            return localVelocity;
        }

        protected virtual Vector3 RestrictRotation(Vector3 angularVelocity)
        {
            angularVelocity.y = 0;
            angularVelocity.z = 0;
            if (angularVelocity.x > 0)
                angularVelocity.x = 0;
            return angularVelocity;
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
