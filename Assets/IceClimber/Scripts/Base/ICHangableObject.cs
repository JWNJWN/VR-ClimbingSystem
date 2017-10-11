namespace IceClimber.Base
{
    using System.Collections.Generic;
    using UnityEngine;
    using VRTK;

    public delegate void HangEventHandler(object sender);

    public class ICHangableObject : VRTK_InteractableObject
    {
        public event HangEventHandler OnHangStart;
        public event HangEventHandler OnHangEnd;
        
        public bool IsInsideWall
        {
            get { return EnteredMaterials.Count > 0; }
        }

        public Transform HangPoint;

        protected Vector3 velocity;
        private Vector3 previousPosition;

        public List<ICHangableMaterial> EnteredMaterials;

        protected virtual float velocityForce
        {
            get { return (velocity.magnitude * interactableRigidbody.mass) / Time.fixedDeltaTime; }
        }

        protected override void Awake()
        {
            base.Awake();

            EnteredMaterials = new List<ICHangableMaterial>();
        }

        public override void Ungrabbed(VRTK_InteractGrab previousGrabbingObject = null)
        {
            OnHangEnd(this);
            base.Ungrabbed(previousGrabbingObject);
        }

        protected virtual void OnTriggerEnter(Collider collider)
        {
            if(IsGrabbed() && !IsInsideWall)
            {
                ICHangableMaterial mat = collider.GetComponent<ICHangableMaterial>();
                if (mat != null && velocityForce >= mat.EnterSpeed)
                {
                    if (!EnteredMaterials.Contains(mat))
                    {
                        EnteredMaterials.Add(mat);
                        HangStart(this);
                    }
                }
            }
        }   

        protected virtual void OnTriggerExit(Collider collider)
        {
            if(IsGrabbed() && IsInsideWall)
            {
                ICHangableMaterial mat = collider.GetComponent<ICHangableMaterial>();
                if (mat != null)
                {
                    if (EnteredMaterials.Contains(mat))
                    {
                        EnteredMaterials.Remove(mat);
                        HangEnd(this);
                    }
                }
            }
        }

        public virtual void HangStart(object sender)
        {
            OnHangStart(sender);
        }
        public virtual void HangEnd(object sender)
        {
            OnHangEnd(sender);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (IsInsideWall)
                interactableRigidbody.detectCollisions = false;
            else
                interactableRigidbody.detectCollisions = true;

            if(HangPoint)
            {
                velocity = HangPoint.position - previousPosition;
                previousPosition = HangPoint.position;
            }
            else
            {
                velocity = interactableRigidbody.velocity;
            }
        }
    }
}