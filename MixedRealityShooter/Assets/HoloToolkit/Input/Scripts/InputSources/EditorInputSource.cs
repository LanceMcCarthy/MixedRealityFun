﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// Input source for fake input source information, which gives details about current source state and position.
    /// </summary>
    [RequireComponent(typeof(EditorInputControl))]
    public class EditorInputSource : BaseInputSource
    {
        // TODO: add thumbstick, touchpad, and trigger axis support.

        public class ButtonStates
        {
            public ButtonStates()
            {
                IsSelectButtonDown = false;
                SelectButtonStateChanged = false;

                IsMenuButtonDown = false;
                MenuButtonStateChanged = false;

                IsGrasped = false;
                GraspStateChanged = false;

                ManipulationInProgress = false;
                HoldInProgress = false;
                CumulativeDelta = Vector3.zero;
            }

            public bool IsSelectButtonDown;
            public bool SelectButtonStateChanged;
            public float SelectDownStartTime;

            public bool IsMenuButtonDown;
            public bool MenuButtonStateChanged;

            public bool IsGrasped;
            public bool GraspStateChanged;

            public bool ManipulationInProgress;
            public bool HoldInProgress;
            public Vector3 CumulativeDelta;
        }

        public bool SupportsPosition;
        public bool SupportsRotation;
        public bool SupportsRay;
        public bool SupportsMenuButton;
        public bool SupportsGrasp;
        public bool RaiseEventsBasedOnVisibility;
        public InteractionSourceKind sourceKind;

        public Vector3 ControllerPosition;
        public Quaternion ControllerRotation;

        public Ray? PointingRay;

        public ButtonStates buttonStates;

        private uint controllerId;

        private EditorInputControl manualController;

        private bool CurrentlyVisible;
        private bool VisibilityChanged;

        /// <summary>
        /// The maximum interval between button down and button up that will result in a clicked event.
        /// </summary>
        private const float MaxClickDuration = 0.5f;

        [SerializeField] 
        [Tooltip("The total amount of input source movement that needs to happen to signal intent to start a manipulation. This is a distance, but not a distance in any one direction.")]
        private float manipulationStartMovementThreshold = 0.03f;

        public override SupportedInputInfo GetSupportedInputInfo(uint sourceId)
        {
            Debug.Assert(sourceId == controllerId, "Controller data requested for a mismatched source ID.");

            SupportedInputInfo supportedInputInfo = SupportedInputInfo.None;

            if (SupportsPosition)
            {
                supportedInputInfo |= SupportedInputInfo.Position;
            }

            if (SupportsRotation)
            {
                supportedInputInfo |= SupportedInputInfo.Rotation;
            }

            if (SupportsRay)
            {
                supportedInputInfo |= SupportedInputInfo.Ray;
            }

            if (SupportsMenuButton)
            {
                supportedInputInfo |= SupportedInputInfo.Menu;
            }

            if (SupportsGrasp)
            {
                supportedInputInfo |= SupportedInputInfo.Grasp;
            }

            return supportedInputInfo;
        }

        public override bool TryGetSourceKind(uint sourceId, out InteractionSourceKind sourceKind)
        {
            Debug.Assert(sourceId == controllerId, "Controller data requested for a mismatched source ID.");

            sourceKind = this.sourceKind;
            return true;
        }

        public override bool TryGetPointerPosition(uint sourceId, out Vector3 position)
        {
            Debug.Assert(sourceId == controllerId, "Controller data requested for a mismatched source ID.");

            if (SupportsPosition)
            {
                position = ControllerPosition;
                return true;
            }

            position = Vector3.zero;
            return false;
        }

        public override bool TryGetPointerRotation(uint sourceId, out Quaternion rotation)
        {
            Debug.Assert(sourceId == controllerId, "Controller data requested for a mismatched source ID.");

            if (SupportsRotation)
            {
                rotation = ControllerRotation;
                return true;
            }

            rotation = Quaternion.identity;
            return false;
        }

        public override bool TryGetPointerRay(uint sourceId, out Ray pointingRay)
        {
            Debug.Assert(sourceId == controllerId, "Controller data requested for a mismatched source ID.");

            if (SupportsRay && (PointingRay != null))
            {
                pointingRay = (Ray)PointingRay;
                return true;
            }

            pointingRay = default(Ray);
            return false;
        }

        public override bool TryGetGripPosition(uint sourceId, out Vector3 position)
        {
            Debug.Assert(sourceId == controllerId, "Controller data requested for a mismatched source ID.");

            if (SupportsPosition)
            {
                position = ControllerPosition;
                return true;
            }

            position = Vector3.zero;
            return false;
        }

        public override bool TryGetGripRotation(uint sourceId, out Quaternion rotation)
        {
            Debug.Assert(sourceId == controllerId, "Controller data requested for a mismatched source ID.");

            if (SupportsRotation)
            {
                rotation = ControllerRotation;
                return true;
            }

            rotation = Quaternion.identity;
            return false;
        }

        public override bool TryGetGripRay(uint sourceId, out Ray pointingRay)
        {
            Debug.Assert(sourceId == controllerId, "Controller data requested for a mismatched source ID.");

            if (SupportsRay && (PointingRay != null))
            {
                pointingRay = (Ray)PointingRay;
                return true;
            }

            pointingRay = default(Ray);
            return false;
        }

        public override bool TryGetThumbstick(uint sourceId, out bool isPressed, out Vector2 position)
        {
            Debug.Assert(sourceId == controllerId, "Controller data requested for a mismatched source ID.");

            isPressed = false;
            position = Vector2.zero;
            return false;
        }

        public override bool TryGetTouchpad(uint sourceId, out bool isPressed, out bool isTouched, out Vector2 position)
        {
            Debug.Assert(sourceId == controllerId, "Controller data requested for a mismatched source ID.");

            isPressed = false;
            isTouched = false;
            position = Vector2.zero;
            return false;
        }

        public override bool TryGetSelect(uint sourceId, out bool isPressed, out double pressedAmount)
        {
            Debug.Assert(sourceId == controllerId, "Controller data requested for a mismatched source ID.");

            isPressed = false;
            pressedAmount = 0;
            return false;
        }

        public override bool TryGetGrasp(uint sourceId, out bool isPressed)
        {
            Debug.Assert(sourceId == controllerId, "Controller data requested for a mismatched source ID.");

            if (SupportsGrasp)
            {
                isPressed = buttonStates.IsGrasped;
                return true;
            }

            isPressed = false;
            return false;
        }

        public override bool TryGetMenu(uint sourceId, out bool isPressed)
        {
            Debug.Assert(sourceId == controllerId, "Controller data requested for a mismatched source ID.");

            if (SupportsMenuButton)
            {
                isPressed = buttonStates.IsMenuButtonDown;
                return true;
            }

            isPressed = false;
            return false;
        }

        private void Awake()
        {
#if !UNITY_EDITOR
            Destroy(gameObject);
#else
            manualController = GetComponent<EditorInputControl>();

            buttonStates = new ButtonStates();
            CurrentlyVisible = false;
            VisibilityChanged = false;
            controllerId = (uint)Random.value;
#endif
        }

#if UNITY_EDITOR
        private void Update()
        {
            UpdateControllerData();
            SendControllerVisibilityEvents();
        }

        protected override void OnEnableAfterStart()
        {
            base.OnEnableAfterStart();

            ConnectController();
        }

        protected override void OnDisableAfterStart()
        {
            DisconnectController();

            base.OnDisableAfterStart();
        }
#endif

        private void ConnectController()
        {
            if (!RaiseEventsBasedOnVisibility)
            {
                InputManager.Instance.RaiseSourceDetected(this, controllerId);
            }
        }

        private void DisconnectController()
        {
            if (!RaiseEventsBasedOnVisibility)
            {
                InputManager.Instance.RaiseSourceLost(this, controllerId);
            }
        }

        /// <summary>
        /// Update the controller data for the currently detected controllers.
        /// </summary>
        private void UpdateControllerData()
        {
            bool doUpdateState = !RaiseEventsBasedOnVisibility;

            if (manualController.ControllerInView)
            {
                if (!CurrentlyVisible)
                {
                    VisibilityChanged = true;
                }

                CurrentlyVisible = true;
                doUpdateState = true;
            }
            else
            {
                if (CurrentlyVisible)
                {
                    VisibilityChanged = true;
                }

                CurrentlyVisible = false;
            }

            if (doUpdateState)
            {
                UpdateControllerState(manualController.ControllerSourceState);
            }
        }

        /// <summary>
        /// Updates the controller state information.
        /// </summary>
        /// <param name="source">Input source to use to update the position.</param>
        private void UpdateControllerState(DebugInteractionSourceState source)
        {
            float time;

            if (manualController.UseUnscaledTime)
            {
                time = Time.unscaledTime;
            }
            else
            {
                time = Time.time;
            }

            buttonStates.SelectButtonStateChanged = (buttonStates.IsSelectButtonDown != source.selectPressed);
            buttonStates.IsSelectButtonDown = source.selectPressed;

            if (buttonStates.SelectButtonStateChanged && source.selectPressed)
            {
                buttonStates.SelectDownStartTime = time;
                buttonStates.CumulativeDelta = Vector3.zero;
            }

            if (SupportsPosition)
            {
                Vector3 controllerPosition;
                if (source.sourcePose.TryGetPosition(out controllerPosition))
                {
                    buttonStates.CumulativeDelta += controllerPosition - ControllerPosition;
                    ControllerPosition = controllerPosition;
                }
            }

            if (SupportsRotation)
            {
                Quaternion controllerRotation;
                if (source.sourcePose.TryGetRotation(out controllerRotation))
                {
                    ControllerRotation = controllerRotation;
                }
            }

            if (SupportsRay)
            {
                PointingRay = source.sourcePose.PointerRay;
            }

            if (SupportsMenuButton)
            {
                buttonStates.MenuButtonStateChanged = (buttonStates.IsMenuButtonDown != source.menuPressed);
                buttonStates.IsMenuButtonDown = source.menuPressed;
            }

            if (SupportsGrasp)
            {
                buttonStates.GraspStateChanged = (buttonStates.IsGrasped != source.grasped);
                buttonStates.IsGrasped = source.grasped;
            }

            SendControllerStateEvents(time);
        }

        /// <summary>
        /// Sends the events for controller state changes.
        /// </summary>
        private void SendControllerStateEvents(float time)
        {
            // TODO: Send other new input manager events relating to source updates.

            if (buttonStates.SelectButtonStateChanged)
            {
                if (buttonStates.IsSelectButtonDown)
                {
                    InputManager.Instance.RaiseSourceDown(this, controllerId, InteractionSourcePressType.Select);
                }
                // New up presses require sending different events depending on whether it's also a click, hold, or manipulation.
                else
                {
                    // A gesture is always either a click, a hold or a manipulation.
                    if (buttonStates.ManipulationInProgress)
                    {
                        InputManager.Instance.RaiseManipulationCompleted(this, controllerId, buttonStates.CumulativeDelta);
                        buttonStates.ManipulationInProgress = false;
                    }
                    // Clicks and holds are based on time, and both are overruled by manipulations.
                    else if (buttonStates.HoldInProgress)
                    {
                        InputManager.Instance.RaiseHoldCompleted(this, controllerId);
                        buttonStates.HoldInProgress = false;
                    }
                    else
                    {
                        // We currently only support single taps in editor.
                        InputManager.Instance.RaiseInputClicked(this, controllerId, InteractionSourcePressType.Select, 1);
                    }
                    InputManager.Instance.RaiseSourceUp(this, controllerId, InteractionSourcePressType.Select);
                }
            }
            // If the select state hasn't changed, but it's down, that means it might
            // trigger a hold or a manipulation (or a hold and then a manipulation).
            else if (buttonStates.IsSelectButtonDown)
            {
                if (!buttonStates.ManipulationInProgress)
                {
                    // Manipulations are triggered by the amount of movement since select was pressed down.
                    if (buttonStates.CumulativeDelta.magnitude > manipulationStartMovementThreshold)
                    {
                        // Starting a manipulation will cancel an existing hold.
                        if (buttonStates.HoldInProgress)
                        {
                            InputManager.Instance.RaiseHoldCanceled(this, controllerId);
                            buttonStates.HoldInProgress = false;
                        }

                        InputManager.Instance.RaiseManipulationStarted(this, controllerId, buttonStates.CumulativeDelta);
                        buttonStates.ManipulationInProgress = true;
                    }
                    // Holds are triggered by time.
                    else if (!buttonStates.HoldInProgress && (time - buttonStates.SelectDownStartTime >= MaxClickDuration))
                    {
                        InputManager.Instance.RaiseHoldStarted(this, controllerId);
                        buttonStates.HoldInProgress = true;
                    }
                }
                else
                {
                    InputManager.Instance.RaiseManipulationUpdated(this, controllerId, buttonStates.CumulativeDelta);
                }
            }

            if (buttonStates.MenuButtonStateChanged)
            {
                if (buttonStates.IsMenuButtonDown)
                {
                    InputManager.Instance.RaiseSourceDown(this, controllerId, InteractionSourcePressType.Menu);
                }
                else
                {
                    InputManager.Instance.RaiseSourceUp(this, controllerId, InteractionSourcePressType.Menu);
                }
            }

            if (buttonStates.GraspStateChanged)
            {
                if (buttonStates.IsGrasped)
                {
                    InputManager.Instance.RaiseSourceDown(this, controllerId, InteractionSourcePressType.Grasp);
                }
                else
                {
                    InputManager.Instance.RaiseSourceUp(this, controllerId, InteractionSourcePressType.Grasp);
                }
            }
        }

        /// <summary>
        /// Sends the events for hand visibility changes & controller connect/disocnnect.
        /// </summary>
        private void SendControllerVisibilityEvents()
        {
            // Send event for new hands that were added
            if (RaiseEventsBasedOnVisibility && VisibilityChanged)
            {
                if (CurrentlyVisible)
                {
                    InputManager.Instance.RaiseSourceDetected(this, controllerId);
                }
                else
                {
                    InputManager.Instance.RaiseSourceLost(this, controllerId);
                }

                VisibilityChanged = false;
            }
        }
    }
}