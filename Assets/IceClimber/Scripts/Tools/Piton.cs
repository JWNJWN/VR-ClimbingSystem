namespace IceClimber.Tools
{
    using UnityEngine;
    using IceClimber.Base;
    using VRTK;

    public class Piton : ICHangableObject
    {
        private VRTK_BodyPhysics playerPhysics;
        
        protected override void Awake()
        {
            base.Awake();
            playerPhysics = FindObjectOfType<VRTK_BodyPhysics>();
        }

        protected override void HangStart(object sender)
        {
            base.HangStart(this);
            TetherPlayer(true);
        }

        protected override void HangEnd(object sender)
        {
            base.HangEnd(this);
            TetherPlayer(false);
        }

        void TetherPlayer(bool state)
        {
            playerPhysics.enabled = !state;
        }
    }
}