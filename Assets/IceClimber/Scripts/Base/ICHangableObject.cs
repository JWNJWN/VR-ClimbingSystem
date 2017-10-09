namespace IceClimber.Base
{
    using UnityEngine;
    using VRTK;

    public delegate void HangEventHandler(object sender);

    public class ICHangableObject : VRTK_InteractableObject
    {
        public event HangEventHandler OnHangStart;
        public event HangEventHandler OnHangEnd;
        
        public bool isInsideWall;

        public Transform hangPoint;

        protected Vector3 velocity;
        private Vector3 previousPosition;
        
        protected virtual float velocityForce
        {
            get { return (velocity.magnitude * interactableRigidbody.mass) / Time.fixedDeltaTime; }
        }

        protected override void Awake()
        {
            base.Awake();
            OnHangStart += new HangEventHandler(HangStart);
            OnHangEnd += new HangEventHandler(HangEnd);
        }

        public override void Grabbed(VRTK_InteractGrab currentGrabbingObject = null)
        {
            base.Grabbed(currentGrabbingObject);
        }

        public override void Ungrabbed(VRTK_InteractGrab previousGrabbingObject = null)
        {
            OnHangEnd(this);
            base.Ungrabbed(previousGrabbingObject);
        }

        protected virtual void OnTriggerEnter(Collider collider)
        {
            if(IsGrabbed() && !isInsideWall)
            {
                ICHangableMaterial mat = collider.GetComponent<ICHangableMaterial>();
                if (mat != null && velocityForce >= mat.EnterForce)
                {
                    OnHangStart(this);
                }
            }
        }   

        protected virtual void OnTriggerExit(Collider collider)
        {
            if(IsGrabbed() && isInsideWall)
            {
                ICHangableMaterial mat = collider.GetComponent<ICHangableMaterial>();
                if (mat != null && velocityForce >= mat.ExitForce)
                    OnHangEnd(this);
            }
        }

        protected virtual void HangStart(object sender)
        {
            isInsideWall = true;
        }
        protected virtual void HangEnd(object sender)
        {
            isInsideWall = false;
        }


        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if(hangPoint)
            {
                velocity = hangPoint.position - previousPosition;
                previousPosition = hangPoint.position;
            }
            else
            {
                velocity = interactableRigidbody.velocity;
            }
        }
    }
}