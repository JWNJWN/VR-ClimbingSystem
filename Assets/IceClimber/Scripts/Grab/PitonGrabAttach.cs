namespace IceClimber.Grab
{
    using System;
    using UnityEngine;
    using VRTK.GrabAttachMechanics;
    using Base;

    public class PitonGrabAttach : ICHangableGrabAttach
    {
        protected override Vector3 RestrictRotation(Vector3 angularVelocity)
        {
            angularVelocity.x = 0;
            angularVelocity.y = 0;
            angularVelocity.z = 0;

            return angularVelocity;
        }
    }
}