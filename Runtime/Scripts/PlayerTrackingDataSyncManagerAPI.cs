using UdonSharp;
using VRC.SDKBase;

namespace JanSharp
{
    [SingletonScript("e1c4a0c5bf308221f8d6afe54d207905")] // Runtime/Prefabs/PlayerTrackingDataSyncManager.prefab
    public abstract class PlayerTrackingDataSyncManagerAPI : UdonSharpBehaviour
    {
        /// <summary>
        /// <para>Local only, or in other words, the sending side.</para>
        /// </summary>
        /// <param name="trackingType"></param>
        /// <returns><see langword="true"/> when it just started tracking.</returns>
        public abstract bool BeginTracking(VRCPlayerApi.TrackingDataType trackingType);
        /// <summary>
        /// <para>Local only, or in other words, the sending side.</para>
        /// </summary>
        /// <param name="trackingType"></param>
        /// <returns><see langword="true"/> when it just truly stopped tracking, there is nothing tracking
        /// this <paramref name="trackingType"/> anymore.</returns>
        public abstract bool StopTracking(VRCPlayerApi.TrackingDataType trackingType);

        public abstract void SendBeginTrackingIA(VRCPlayerApi.TrackingDataType trackingType);
        public abstract void WriteBeginTrackingData(VRCPlayerApi.TrackingDataType trackingType);
        public abstract void ReadBeginTrackingData();

        public abstract void SendStopTrackingIA(VRCPlayerApi.TrackingDataType trackingType);
        public abstract void WriteStopTrackingData(VRCPlayerApi.TrackingDataType trackingType);
        public abstract void ReadStopTrackingData();
    }
}
