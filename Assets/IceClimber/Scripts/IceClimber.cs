namespace IceClimber.Climbing
{
    using UnityEngine;
    using Tools;
    using Base;
    using VRTK;
    using System.Collections.Generic;

    public class IceClimber : MonoBehaviour
    {
        public bool usePlayerScale = true;

        private Transform playArea;
        private VRTK_BodyPhysics bodyPhysics;
        private bool isClimbing = false;

        private Dictionary<GameObject, Vector3> playAreaStartLocations = new Dictionary<GameObject, Vector3>();
        private Dictionary<GameObject, Vector3> controllerStartLocations = new Dictionary<GameObject, Vector3>();
        private List<GameObject> controllers = new List<GameObject>();

        private void Awake()
        {
            playArea = VRTK_DeviceFinder.PlayAreaTransform();
            bodyPhysics = GetComponent<VRTK_BodyPhysics>();
        }

        private void OnEnable()
        {
            InitListeners(true);
        }

        private void OnDisable()
        {
            InitListeners(false);
        }

        private void HangStart(object sender)
        {
            IcePick IP = (IcePick)sender;
            var controller = VRTK_DeviceFinder.GetActualController(IP.GetGrabbingObject());

            bodyPhysics.TogglePreventSnapToFloor(true);
            bodyPhysics.enableBodyCollisions = false;
            bodyPhysics.ToggleOnGround(false);

            isClimbing = true;

            controllers.Add(controller);
            controllerStartLocations.Add(controller, GetPosition(controller.transform));
            playAreaStartLocations.Add(controller, playArea.position);
        }

        private void HangEnd(object sender)
        {
            IcePick IP = (IcePick)sender;
            var controller = VRTK_DeviceFinder.GetActualController(IP.GetGrabbingObject());

            controllers.Remove(controller);
            controllerStartLocations.Remove(controller);
            playAreaStartLocations.Remove(controller);

            if (controllers.Count == 0)
            {
                bodyPhysics.TogglePreventSnapToFloor(false);
                bodyPhysics.enableBodyCollisions = true;

                isClimbing = false;
            }
        }

        private Vector3 GetPosition(Transform objTransform)
        {
            if (usePlayerScale)
                return playArea.localRotation * Vector3.Scale(objTransform.localPosition, playArea.localScale);

            return playArea.localRotation * objTransform.localPosition;
        }

        private void FixedUpdate()
        {
            if (isClimbing)
            {
                GameObject mostRecentController = controllers[controllers.Count - 1];

                Vector3 controllerDelta = GetPosition(mostRecentController.transform) - controllerStartLocations[mostRecentController];
                Vector3 startPos = playAreaStartLocations[mostRecentController];

                playArea.position = startPos - controllerDelta;
            }
        }

        private void InitListeners(bool state)
        {
            foreach (IcePick IP in FindObjectsOfType<IcePick>())
            {
                IP.OnHangStart += new HangEventHandler(HangStart);
                IP.OnHangEnd += new HangEventHandler(HangEnd);
            }
        }
    }
}