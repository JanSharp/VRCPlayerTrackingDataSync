using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerTrackingDataSync : CustomPlayerData
    {
        public override string PlayerDataInternalName => "jansharp.tracking-data-sync";
        public override string PlayerDataDisplayName => "Tracking Data Sync";
        public override bool SupportsImportExport => false;
        public override uint DataVersion => 0u;
        public override uint LowestSupportedDataVersion => 0u;

        [HideInInspector][SerializeField][SingletonReference] private PlayerTrackingDataSyncManagerAPI manager;

        /// <summary><para>Game state safe.</para></summary>
        [System.NonSerialized] public bool isTrackingHead;
        /// <summary><para>Not game state safe.</para></summary>
        [System.NonSerialized] public float prevHeadTime;
        /// <summary><para>Not game state safe.</para></summary>
        [System.NonSerialized] public Vector3 prevHeadPosition;
        /// <summary><para>Not game state safe.</para></summary>
        [System.NonSerialized] public Quaternion prevHeadRotation;
        /// <summary>
        /// <para>Must not be equal to <see cref="prevHeadTime"/> while <see cref="isTrackingHead"/> is
        /// <see langword="true"/>.</para>
        /// <para>Not game state safe.</para>
        /// </summary>
        [System.NonSerialized] public float headTime;
        /// <summary><para>Game state safe.</para></summary>
        [System.NonSerialized] public Vector3 headPosition;
        /// <summary><para>Game state safe.</para></summary>
        [System.NonSerialized] public Quaternion headRotation;

        /// <summary><para>Game state safe.</para></summary>
        [System.NonSerialized] public bool isTrackingLeftHand;
        /// <summary><para>Not game state safe.</para></summary>
        [System.NonSerialized] public float prevLeftHandTime;
        /// <summary><para>Not game state safe.</para></summary>
        [System.NonSerialized] public Vector3 prevLeftHandPosition;
        /// <summary><para>Not game state safe.</para></summary>
        [System.NonSerialized] public Quaternion prevLeftHandRotation;
        /// <summary>
        /// <para>Must not be equal to <see cref="prevLeftHandTime"/> while <see cref="isTrackingLeftHand"/>
        /// is <see langword="true"/>.</para>
        /// <para>Not game state safe.</para>
        /// </summary>
        [System.NonSerialized] public float leftHandTime;
        /// <summary><para>Game state safe.</para></summary>
        [System.NonSerialized] public Vector3 leftHandPosition;
        /// <summary><para>Game state safe.</para></summary>
        [System.NonSerialized] public Quaternion leftHandRotation;

        /// <summary><para>Game state safe.</para></summary>
        [System.NonSerialized] public bool isTrackingRightHand;
        /// <summary><para>Not game state safe.</para></summary>
        [System.NonSerialized] public float prevRightHandTime;
        /// <summary><para>Not game state safe.</para></summary>
        [System.NonSerialized] public Vector3 prevRightHandPosition;
        /// <summary><para>Not game state safe.</para></summary>
        [System.NonSerialized] public Quaternion prevRightHandRotation;
        /// <summary>
        /// <para>Must not be equal to <see cref="prevRightHandTime"/> while <see cref="isTrackingRightHand"/>
        /// is <see langword="true"/>.</para>
        /// <para>Not game state safe.</para>
        /// </summary>
        [System.NonSerialized] public float rightHandTime;
        /// <summary><para>Game state safe.</para></summary>
        [System.NonSerialized] public Vector3 rightHandPosition;
        /// <summary><para>Game state safe.</para></summary>
        [System.NonSerialized] public Quaternion rightHandRotation;

        [System.NonSerialized] public Vector3 resultPosition;
        [System.NonSerialized] public Quaternion resultRotation;

        public void GetCurrentPosition(VRCPlayerApi.TrackingDataType trackingType)
        {
            if (trackingType == VRCPlayerApi.TrackingDataType.RightHand)
                GetCurrentRightHandPosition();
            else if (trackingType == VRCPlayerApi.TrackingDataType.LeftHand)
                GetCurrentLeftHandPosition();
            else if (trackingType == VRCPlayerApi.TrackingDataType.Head)
                GetCurrentHeadPosition();
        }

        public void GetCurrentPosition(HumanBodyBones bone)
        {
            if (bone == HumanBodyBones.RightHand)
                GetCurrentRightHandPosition();
            else if (bone == HumanBodyBones.LeftHand)
                GetCurrentLeftHandPosition();
            else if (bone == HumanBodyBones.Head)
                GetCurrentHeadPosition();
        }

        public void GetCurrentHeadPosition()
        {
            float percent = (Time.time - prevHeadTime) / (headTime - prevHeadTime);
            resultPosition = Vector3.Lerp(prevHeadPosition, headPosition, percent);
            resultRotation = Quaternion.Lerp(prevHeadRotation, headRotation, percent);
        }

        public void GetCurrentLeftHandPosition()
        {
            float percent = (Time.time - prevLeftHandTime) / (leftHandTime - prevLeftHandTime);
            resultPosition = Vector3.Lerp(prevLeftHandPosition, leftHandPosition, percent);
            resultRotation = Quaternion.Lerp(prevLeftHandRotation, leftHandRotation, percent);
        }

        public void GetCurrentRightHandPosition()
        {
            float percent = (Time.time - prevRightHandTime) / (rightHandTime - prevRightHandTime);
            resultPosition = Vector3.Lerp(prevRightHandPosition, rightHandPosition, percent);
            resultRotation = Quaternion.Lerp(prevRightHandRotation, rightHandRotation, percent);
        }

        public override bool PersistPlayerDataWhileOffline()
        {
            return false;
        }

        public override void Serialize(bool isExport)
        {
            lockstep.WriteFlags(isTrackingHead, isTrackingLeftHand, isTrackingRightHand);
            if (isTrackingHead)
            {
                lockstep.WriteVector3(headPosition);
                lockstep.WriteQuaternion(headRotation);
            }
            if (isTrackingLeftHand)
            {
                lockstep.WriteVector3(leftHandPosition);
                lockstep.WriteQuaternion(leftHandRotation);
            }
            if (isTrackingRightHand)
            {
                lockstep.WriteVector3(rightHandPosition);
                lockstep.WriteQuaternion(rightHandRotation);
            }
        }

        public override void Deserialize(bool isImport, uint importedDataVersion)
        {
            lockstep.ReadFlags(out isTrackingHead, out isTrackingLeftHand, out isTrackingRightHand);
            float time = Time.time;
            if (isTrackingHead)
            {
                headTime = time;
                headPosition = lockstep.ReadVector3();
                headRotation = lockstep.ReadQuaternion();
                prevHeadTime = time - 1f; // Prevent division by 0, which could lead to potential NaN.
                prevHeadPosition = headPosition;
                prevHeadRotation = headRotation;
            }
            if (isTrackingLeftHand)
            {
                leftHandTime = time;
                leftHandPosition = lockstep.ReadVector3();
                leftHandRotation = lockstep.ReadQuaternion();
                prevLeftHandTime = time - 1f;
                prevLeftHandPosition = leftHandPosition;
                prevLeftHandRotation = leftHandRotation;
            }
            if (isTrackingRightHand)
            {
                rightHandTime = time;
                rightHandPosition = lockstep.ReadVector3();
                rightHandRotation = lockstep.ReadQuaternion();
                prevRightHandTime = time - 1f;
                prevRightHandPosition = rightHandPosition;
                prevRightHandRotation = rightHandRotation;
            }
        }

        public void ReadHeadIncrement()
        {
            prevHeadTime = headTime;
            prevHeadPosition = headPosition;
            prevHeadRotation = headRotation;
            headTime = Time.time;
            headPosition = lockstep.ReadVector3();
            headRotation = lockstep.ReadQuaternion();
        }

        public void ReadLeftHandIncrement()
        {
            prevLeftHandTime = leftHandTime;
            prevLeftHandPosition = leftHandPosition;
            prevLeftHandRotation = leftHandRotation;
            leftHandTime = Time.time;
            leftHandPosition = lockstep.ReadVector3();
            leftHandRotation = lockstep.ReadQuaternion();
        }

        public void ReadRightHandIncrement()
        {
            prevRightHandTime = rightHandTime;
            prevRightHandPosition = rightHandPosition;
            prevRightHandRotation = rightHandRotation;
            rightHandTime = Time.time;
            rightHandPosition = lockstep.ReadVector3();
            rightHandRotation = lockstep.ReadQuaternion();
        }
    }
}
