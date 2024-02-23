//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Scripts
{
    [Serializable]
    public class TrackingStateRequest : Message
    {
        public const string k_RosMessageName = "Scripts/TrackingState";
        public override string RosMessageName => k_RosMessageName;

        public bool tracking;

        public TrackingStateRequest()
        {
            this.tracking = false;
        }

        public TrackingStateRequest(bool tracking)
        {
            this.tracking = tracking;
        }

        public static TrackingStateRequest Deserialize(MessageDeserializer deserializer) => new TrackingStateRequest(deserializer);

        private TrackingStateRequest(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.tracking);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.tracking);
        }

        public override string ToString()
        {
            return "TrackingStateRequest: " +
            "\ntracking: " + tracking.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}