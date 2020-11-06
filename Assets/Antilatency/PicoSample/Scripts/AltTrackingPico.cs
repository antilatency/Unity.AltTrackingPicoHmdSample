// Copyright 2020, ALT LLC. All Rights Reserved.
// This file is part of Antilatency SDK.
// It is subject to the license terms in the LICENSE file found in the top-level directory
// of this distribution and at http://www.antilatency.com/eula
// You may not use this file except in compliance with the License.
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Antilatency.Integration;
using Antilatency.DeviceNetwork;

/// <summary>
/// %Antilatency %Alt tracking components and scripts specific for %Pico HMD devices.
/// </summary>
namespace Antilatency.IntegrationPico {
    /// <summary>
    /// %Antilatency %Alt tracking sample implementation for %Pico headsets.
    /// </summary>
    public class AltTrackingPico : AltTracking {
        public Pvr_UnitySDKManager PvrSdkManager;
        public Pvr_UnitySDKHeadTrack PvrHeadTrack;

        private Antilatency.TrackingAlignment.ILibrary _alignmentLibrary;
        private Antilatency.TrackingAlignment.ITrackingAlignment _alignment;

        private IEnumerator _framesSkip = null;
        private const int _framesToSkipAtInit = 10;
        private bool _hmd6Dof = false;

        private bool _altInitialPositionApplied = false;
        private const float _bQuality = 0.125f;

        private Transform _aSpace;
        private Transform _bSpace;
        private Transform _b;

        protected override void OnEnable() {
            base.OnEnable();

            //var nativeHeadTrack = GetComponentInChildren<Pvr_UnitySDKHeadTrack>();
            //if (nativeHeadTrack != null) {
            //    nativeHeadTrack.trackPosition = false;
            //}

            ////////For PVR SDK v2.7.8 or earlier
            //Pvr_UnitySDKManager.PVRNeck = false;

            ////////For PVR SDK v2.7.9 or later
            if (PvrSdkManager == null) {
                PvrSdkManager = Pvr_UnitySDKManager.SDK;
            }
            PvrSdkManager.PVRNeck = false;
        }

        protected override void Awake() {
            base.Awake();

            _alignmentLibrary = Antilatency.TrackingAlignment.Library.load();

            InitializeTrackingAlignment();
        }

        protected virtual void Start() {
            var hmd6DofSupport = 0;
            Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig((int)Pvr_UnitySDKAPI.GlobalIntConfigs.ABILITY6DOF, ref hmd6DofSupport);
            if (hmd6DofSupport == 1) {
                _hmd6Dof = !PvrSdkManager.HmdOnlyrot && PvrHeadTrack.trackPosition;
            } else {
                _hmd6Dof = false;
            }

            _aSpace = PvrSdkManager.transform.parent;
            _bSpace = PvrSdkManager.transform;
            _b = PvrHeadTrack.transform;
        }

        private void OnApplicationPause(bool pause) {
            FocusAlignment(!pause);
        }

        private void OnApplicationFocus(bool focus) {
            FocusAlignment(focus);
        }

        private void FocusAlignment(bool focus) {
            if (focus) {
                InitializeTrackingAlignment();
            } else {
                StopTrackingAlignment();
            }
        }

        private void InitializeTrackingAlignment()
        {
            if (_framesSkip != null)
            {
                StopCoroutine(_framesSkip);
                _framesSkip = null;
            }

            //Do not add samples for 10 frames due to headsets's incorrect rotation values received after focus has been restored
            _framesSkip = FramesSkip(_framesToSkipAtInit);
            StartCoroutine(_framesSkip);
        }

        private IEnumerator FramesSkip(uint frameCount) {
            var counter = 0u;
            while (frameCount >= counter) {
                yield return new WaitForEndOfFrame();
                counter++;
            }
            StartTrackingAlignment();
        }

        private void StartTrackingAlignment() {
            if (_alignment != null) {
                StopTrackingAlignment();
            }

            var placement = GetPlacement();
            _alignment = _alignmentLibrary.createTrackingAlignment(Antilatency.Math.doubleQ.FromQuaternion(placement.rotation), ExtrapolationTime);
        }

        private void StopTrackingAlignment() {
            if (_alignment == null) {
                return;
            }

            _alignment.Dispose();
            _alignment = null;
        }

        /// <summary>
        /// Applies tracking data. We used %Pico native rotation as base and then smoothly correct it with our tracking data 
        /// to avoid glitches that can be seen because %Pico timewarp system uses only native rotation data provided by headset.
        /// </summary>
        protected override void Update() {
            base.Update();

            ApplyTrackingData();
        }

        protected override NodeHandle GetAvailableTrackingNode() {
            return GetUsbConnectedFirstIdleTrackerNode();
        }

        protected virtual void ApplyTrackingData() {
            Alt.Tracking.State trackingState;

            var altTrackingActive = GetRawTrackingState(out trackingState);
            if (!altTrackingActive) {
                return;
            }

            if (PvrSdkManager.PVRNeck) {
                PvrSdkManager.PVRNeck = false;
            }

            if (altTrackingActive && trackingState.stability.stage == Antilatency.Alt.Tracking.Stage.Tracking6Dof && (_alignment != null)) {
                var result = _alignment.update(
                    Antilatency.Math.doubleQ.FromQuaternion(trackingState.pose.rotation),
                    Antilatency.Math.doubleQ.FromQuaternion(_b.localRotation),
                    Time.realtimeSinceStartup);

                ExtrapolationTime = (float)result.timeBAheadOfA;
                _placement.rotation = result.rotationARelativeToB.ToQuaternion();

                _bSpace.localRotation = result.rotationBSpace.ToQuaternion();
            }

            if (GetTrackingState(out trackingState)) {
                if (_hmd6Dof) {
                    if (trackingState.stability.stage == Antilatency.Alt.Tracking.Stage.Tracking6Dof) {
                        var aWorldSpace = _aSpace.TransformPoint(trackingState.pose.position);
                        var a = _aSpace.InverseTransformPoint(aWorldSpace);
                        var bSpace =_bSpace.localPosition;
                        var b = _aSpace.InverseTransformPoint(_b.position);

                        Vector3 averagePositionInASpace;

                        if (!_altInitialPositionApplied) {
                            averagePositionInASpace = (b * 0.0f + a * 100.0f) / (100.0f + 0.0f);
                            _altInitialPositionApplied = true;
                        } else {
                            averagePositionInASpace = (b * _bQuality + a * trackingState.stability.value) / (trackingState.stability.value + _bQuality);
                        }

                        _bSpace.localPosition += averagePositionInASpace - b;
                    }
                } else {
                    transform.localPosition = trackingState.pose.position;
                }
            }
        }

        protected override Pose GetPlacement() {
            var result = Pose.identity;

            using (var localStorage = Integration.StorageClient.GetLocalStorage()) {

                if (localStorage == null) {
                    return result;
                }

                var placementCode = localStorage.read("placement", "default");

                if (string.IsNullOrEmpty(placementCode)) {
                    Debug.LogError("Failed to get placement code");
                } else {
                    result = _trackingLibrary.createPlacement(placementCode);
                }

                return result;
            }
        }
    }
}