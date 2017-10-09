namespace IceClimber.Tools
{
    using Base;

    public class IcePick : ICHangableObject
    {
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        protected override void HangEnd(object sender)
        {
            base.HangEnd(this);
        }
    }
}