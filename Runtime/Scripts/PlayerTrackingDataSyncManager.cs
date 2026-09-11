using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace JanSharp.Internal
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerTrackingDataSyncManager : PlayerTrackingDataSyncManagerAPI
    {
        [HideInInspector][SerializeField][SingletonReference] private PlayerDataManagerAPI playerDataManager;
        [HideInInspector][SerializeField][SingletonReference] private LockstepAPI lockstep;

        private int playerDataIndex;

        private uint trackingHeadCount = 0u;
        private uint trackingLeftHandCount = 0u;
        private uint trackingRightHandCount = 0u;
        private uint totalTrackingCount = 0u;

        private VRCPlayerApi localPlayer;

        private bool updateLoopShouldBeRunning;
        private bool updateLoopShouldIsRunning;
        private const float UpdateLoopInterval = 0.25f;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
        }

        [PlayerDataEvent(PlayerDataEventType.OnRegisterCustomPlayerData)]
        public void OnRegisterCustomPlayerData()
        {
            playerDataManager.RegisterCustomPlayerData<PlayerTrackingDataSync>(nameof(PlayerTrackingDataSync));
        }

        [PlayerDataEvent(PlayerDataEventType.OnAllCustomPlayerDataRegistered)]
        public void OnAllCustomPlayerDataRegistered()
        {
            playerDataIndex = playerDataManager.GetPlayerDataClassNameIndex<PlayerTrackingDataSync>(nameof(PlayerTrackingDataSync));
        }

        public override bool BeginTracking(VRCPlayerApi.TrackingDataType trackingType)
        {
            if (trackingType == VRCPlayerApi.TrackingDataType.Head)
                trackingHeadCount++;
            else if (trackingType == VRCPlayerApi.TrackingDataType.LeftHand)
                trackingLeftHandCount++;
            else if (trackingType == VRCPlayerApi.TrackingDataType.RightHand)
                trackingRightHandCount++;
            else
            {
                Debug.LogError($"[PlayerTrackingDataSync] Attempt to begin tracking tracking data which is "
                    + $"not one of Head, LeftHand nor RightHand, which is not supported.");
                return false;
            }
            totalTrackingCount++;
            if (totalTrackingCount > 1u)
                return false;
            StartUpdateLoop();
            return true;
        }

        public override bool StopTracking(VRCPlayerApi.TrackingDataType trackingType)
        {
            if (trackingType == VRCPlayerApi.TrackingDataType.Head)
            {
                if (trackingHeadCount == 0u)
                {
                    Debug.LogError($"[PlayerTrackingDataSync] Attempt to stop tracking Head when it is not tracked.");
                    return false;
                }
                trackingHeadCount--;
            }
            else if (trackingType == VRCPlayerApi.TrackingDataType.LeftHand)
            {
                if (trackingLeftHandCount == 0u)
                {
                    Debug.LogError($"[PlayerTrackingDataSync] Attempt to stop tracking LeftHand when it is not tracked.");
                    return false;
                }
                trackingLeftHandCount--;
            }
            else if (trackingType == VRCPlayerApi.TrackingDataType.RightHand)
            {
                if (trackingRightHandCount <= 0u)
                {
                    Debug.LogError($"[PlayerTrackingDataSync] Attempt to stop tracking RightHand when it is not tracked.");
                    return false;
                }
                trackingRightHandCount--;
            }
            else
            {
                Debug.LogError($"[PlayerTrackingDataSync] Attempt to stop tracking tracking data which is "
                    + $"not one of Head, LeftHand nor RightHand, which is not supported.");
                return false;
            }

            totalTrackingCount--;
            if (totalTrackingCount > 0u)
                return false;
            StopUpdateLoop();
            return true;
        }

        public override void SendBeginTrackingIA(VRCPlayerApi.TrackingDataType trackingType)
        {
            if (!BeginTracking(trackingType))
                return;
            WriteBeginTrackingData(trackingType);
            lockstep.SendInputAction(beginTrackingIAId);
        }

        [HideInInspector][SerializeField] private uint beginTrackingIAId;
        [LockstepInputAction(nameof(beginTrackingIAId))]
        public void OnBeginTrackingIA()
        {
            ReadBeginTrackingData();
        }

        public override void WriteBeginTrackingData(VRCPlayerApi.TrackingDataType trackingType)
        {
            var data = localPlayer.GetTrackingData(trackingType);
            lockstep.WriteSmallInt((int)trackingType);
            lockstep.WriteVector3(data.position);
            lockstep.WriteQuaternion(data.rotation);
        }

        public override void ReadBeginTrackingData()
        {
            VRCPlayerApi.TrackingDataType trackingType = (VRCPlayerApi.TrackingDataType)lockstep.ReadSmallInt();
            Vector3 position = lockstep.ReadVector3();
            Quaternion rotation = lockstep.ReadQuaternion();
            PlayerTrackingDataSync playerData = (PlayerTrackingDataSync)playerDataManager.SendingPlayerData.customPlayerData[playerDataIndex];
            float time = Time.time;
            if (trackingType == VRCPlayerApi.TrackingDataType.Head)
            {
                playerData.isTrackingHead = true;
                playerData.headTime = time;
                playerData.headPosition = position;
                playerData.headRotation = rotation;
                playerData.prevHeadTime = time - 1f; // Prevent division by 0, which could lead to potential NaN.
                playerData.prevHeadPosition = position;
                playerData.prevHeadRotation = rotation;
            }
            else if (trackingType == VRCPlayerApi.TrackingDataType.LeftHand)
            {
                playerData.isTrackingLeftHand = true;
                playerData.leftHandTime = time;
                playerData.leftHandPosition = position;
                playerData.leftHandRotation = rotation;
                playerData.prevLeftHandTime = time - 1f;
                playerData.prevLeftHandPosition = position;
                playerData.prevLeftHandRotation = rotation;
            }
            else if (trackingType == VRCPlayerApi.TrackingDataType.RightHand)
            {
                playerData.isTrackingRightHand = true;
                playerData.rightHandTime = time;
                playerData.rightHandPosition = position;
                playerData.rightHandRotation = rotation;
                playerData.prevRightHandTime = time - 1f;
                playerData.prevRightHandPosition = position;
                playerData.prevRightHandRotation = rotation;
            }
        }

        public override void SendStopTrackingIA(VRCPlayerApi.TrackingDataType trackingType)
        {
            if (!StopTracking(trackingType))
                return;
            WriteStopTrackingData(trackingType);
            lockstep.SendInputAction(stopTrackingIAId);
        }

        [HideInInspector][SerializeField] private uint stopTrackingIAId;
        [LockstepInputAction(nameof(stopTrackingIAId))]
        public void OnStopTrackingIA()
        {
            ReadStopTrackingData();
        }

        public override void WriteStopTrackingData(VRCPlayerApi.TrackingDataType trackingType)
        {
            lockstep.WriteSmallInt((int)trackingType);
        }

        public override void ReadStopTrackingData()
        {
            VRCPlayerApi.TrackingDataType trackingType = (VRCPlayerApi.TrackingDataType)lockstep.ReadSmallInt();
            PlayerTrackingDataSync playerData = (PlayerTrackingDataSync)playerDataManager.SendingPlayerData.customPlayerData[playerDataIndex];
            if (trackingType == VRCPlayerApi.TrackingDataType.Head)
                playerData.isTrackingHead = false;
            else if (trackingType == VRCPlayerApi.TrackingDataType.LeftHand)
                playerData.isTrackingLeftHand = false;
            else if (trackingType == VRCPlayerApi.TrackingDataType.RightHand)
                playerData.isTrackingRightHand = false;
        }

        private void StartUpdateLoop()
        {
            updateLoopShouldBeRunning = true;
            if (updateLoopShouldIsRunning)
                return;
            updateLoopShouldIsRunning = true;
            SendCustomEventDelayedSeconds(nameof(UpdateLoop), UpdateLoopInterval);
        }

        private void StopUpdateLoop()
        {
            updateLoopShouldBeRunning = false;
        }

        public void UpdateLoop()
        {
            if (!updateLoopShouldBeRunning)
            {
                updateLoopShouldIsRunning = false;
                return;
            }

            SendIncrementIA();

            SendCustomEventDelayedSeconds(nameof(UpdateLoop), UpdateLoopInterval);
        }

        private void SendIncrementIA()
        {
            VRCPlayerApi.TrackingData data;
            if (trackingHeadCount != 0u)
            {
                data = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                lockstep.WriteVector3(data.position);
                lockstep.WriteQuaternion(data.rotation);
            }
            if (trackingLeftHandCount != 0u)
            {
                data = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                lockstep.WriteVector3(data.position);
                lockstep.WriteQuaternion(data.rotation);
            }
            if (trackingRightHandCount != 0u)
            {
                data = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
                lockstep.WriteVector3(data.position);
                lockstep.WriteQuaternion(data.rotation);
            }
            lockstep.SendInputAction(incrementIAId);
        }

        [HideInInspector][SerializeField] private uint incrementIAId;
        [LockstepInputAction(nameof(incrementIAId))]
        public void OnIncrementIA()
        {
            PlayerTrackingDataSync playerData = (PlayerTrackingDataSync)playerDataManager.SendingPlayerData.customPlayerData[playerDataIndex];
            if (playerData.isTrackingHead)
                playerData.ReadHeadIncrement();
            if (playerData.isTrackingLeftHand)
                playerData.ReadLeftHandIncrement();
            if (playerData.isTrackingRightHand)
                playerData.ReadRightHandIncrement();
        }
    }
}
