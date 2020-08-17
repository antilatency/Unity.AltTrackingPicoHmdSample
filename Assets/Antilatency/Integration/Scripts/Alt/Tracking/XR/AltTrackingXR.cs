using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Antilatency;
using Antilatency.Integration;
using Antilatency.DeviceNetwork;

public class AltTrackingXR : AltTracking {
    public Camera XRCamera;
    /// <summary>
    /// If false, use alt position data. If true, use alt position as a corrention to XR position.
    /// </summary>
	public bool LerpPosition = false;

    private Antilatency.TrackingAlignment.ILibrary _alignmentLibrary;
    private Antilatency.TrackingAlignment.ITrackingAlignment _alignment;

    protected override NodeHandle GetAvailableTrackingNode() {
        return GetUsbConnectedFirstIdleTrackerNode();
    }

    protected override Pose GetPlacement() {
        var result = Pose.identity;

        using (var localStorage = Antilatency.Integration.StorageClient.GetLocalStorage()) {

            if (localStorage == null) {
                return result;
            }

            var placementCode = localStorage.read("placement", "default");

            if (string.IsNullOrEmpty(placementCode)) {
                Debug.LogError("Failed to get placement code");
                result = Pose.identity;
            } else {
                result = _trackingLibrary.createPlacement(placementCode);
            }

            return result;
        }
    }

    protected override void Awake() {
        base.Awake();

        _alignmentLibrary = Antilatency.TrackingAlignment.Library.load();

        var placement = GetPlacement();
        _alignment = _alignmentLibrary.createTrackingAlignment(Antilatency.Math.doubleQ.FromQuaternion(placement.rotation), ExtrapolationTime);

        if (XRCamera == null) {
            XRCamera = GetComponentInChildren<Camera>();
            if (XRCamera == null) {
                Debug.LogError("XR Camera is not setted and no cameras has been found in children gameobjects");
                enabled = false;
                return;
            } else {
                Debug.LogWarning("XR Camera: " + XRCamera.gameObject.name);
            }
        }

        UnityEngine.XR.InputTracking.disablePositionalTracking = !LerpPosition;
    }

    protected override void Update() {
        base.Update();

        bool altTrackingActive;
        Antilatency.Alt.Tracking.State trackingState;

        bool xrRotationRecieved = false;
        bool xrPositionRecieved = false;

        var xrPosition = Vector3.zero;
        var xrRotation = Quaternion.identity;

        if (UnityEngine.XR.XRDevice.isPresent) {
			var states = new List<UnityEngine.XR.XRNodeState>();
			UnityEngine.XR.InputTracking.GetNodeStates(states);

			if (states.Exists(v => v.nodeType == UnityEngine.XR.XRNode.CenterEye)) {
				var centerEyeState = states.First(v => v.nodeType == UnityEngine.XR.XRNode.CenterEye);
				if (centerEyeState.tracked) {
					xrRotationRecieved = centerEyeState.TryGetRotation(out xrRotation);
					xrPositionRecieved = centerEyeState.TryGetPosition(out xrPosition);
				}
			}

			if (!xrRotationRecieved) {
				if (states.Exists(v => v.nodeType == UnityEngine.XR.XRNode.LeftEye) && states.Exists(v => v.nodeType == UnityEngine.XR.XRNode.RightEye)) {
					var leftEyeState = states.First(v => v.nodeType == UnityEngine.XR.XRNode.LeftEye);
					var rightEyeState = states.First(v => v.nodeType == UnityEngine.XR.XRNode.RightEye);

					var leftEyePos = Vector3.zero;
					var rightEyePos = Vector3.zero;
					var leftEyeRot = Quaternion.identity;
					var rightEyeRot = Quaternion.identity;

					var leftEyePosRecieved = leftEyeState.TryGetPosition(out leftEyePos);
					var rightEyePosRecieved = rightEyeState.TryGetPosition(out rightEyePos);
					var leftEyeRotRecieved = leftEyeState.TryGetRotation(out leftEyeRot);
					var rightEyeRotRecieved = rightEyeState.TryGetRotation(out rightEyeRot);

					xrRotationRecieved = leftEyeRotRecieved && rightEyeRotRecieved;
					xrPositionRecieved = leftEyePosRecieved && rightEyePosRecieved;

					if (xrPositionRecieved) {
						xrPosition = Vector3.Lerp(leftEyePos, rightEyePos, 0.5f);
					}

					if (xrRotationRecieved) {
						xrRotation = Quaternion.Lerp(leftEyeRot, rightEyeRot, 0.5f);
					}
				}
			}

			altTrackingActive = GetRawTrackingState(out trackingState);

			if (altTrackingActive && trackingState.stability.stage == Antilatency.Alt.Tracking.Stage.Tracking6Dof && xrRotationRecieved) {
				var result = _alignment.update(
					Antilatency.Math.doubleQ.FromQuaternion(trackingState.pose.rotation), 
					Antilatency.Math.doubleQ.FromQuaternion(xrRotation), 
					Time.realtimeSinceStartup);

				ExtrapolationTime = (float)result.timeBAheadOfA;
				_placement.rotation = result.rotationARelativeToB.ToQuaternion();
				transform.localRotation = result.rotationBSpace.ToQuaternion();
			}

            altTrackingActive = GetTrackingState(out trackingState);
            if (!altTrackingActive || trackingState.stability.stage == Antilatency.Alt.Tracking.Stage.InertialDataInitialization) {
                return;
            }

            if (!xrRotationRecieved) {
                transform.localRotation = trackingState.pose.rotation;
                XRCamera.transform.localRotation = Quaternion.identity;
            }
            
            if (LerpPosition && xrPositionRecieved) {
                if (trackingState.stability.stage == Antilatency.Alt.Tracking.Stage.Tracking6Dof || trackingState.stability.stage == Antilatency.Alt.Tracking.Stage.TrackingBlind6Dof) {
                    var altEnvSpacePosition = transform.parent.TransformPoint(trackingState.pose.position);
                    var altToXr = altEnvSpacePosition - transform.TransformPoint(xrPosition);
                    var posDiff = Vector3.Lerp(Vector3.zero, altToXr, 1.0f - Fx(altToXr.magnitude, 0.75f));

                    transform.position = transform.position + posDiff;
                }
            } else {
                transform.localPosition = trackingState.pose.position;
                XRCamera.transform.localPosition = Vector3.zero;
            }
        }
    }

    private float Fx(float x, float k) {
        var xDk = x / k;
        return 1.0f / (xDk * xDk + 1);
    }
}
