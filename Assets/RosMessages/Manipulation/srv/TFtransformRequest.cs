//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Manipulation
{
    [Serializable]
    public class TFtransformRequest : Message
    {
        public const string k_RosMessageName = "manipulation/TFtransform";
        public override string RosMessageName => k_RosMessageName;

        public string source_frame;
        public string target_frame;

        public TFtransformRequest()
        {
            this.source_frame = "";
            this.target_frame = "";
        }

        public TFtransformRequest(string source_frame, string target_frame)
        {
            this.source_frame = source_frame;
            this.target_frame = target_frame;
        }

        public static TFtransformRequest Deserialize(MessageDeserializer deserializer) => new TFtransformRequest(deserializer);

        private TFtransformRequest(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.source_frame);
            deserializer.Read(out this.target_frame);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.source_frame);
            serializer.Write(this.target_frame);
        }

        public override string ToString()
        {
            return "TFtransformRequest: " +
            "\nsource_frame: " + source_frame.ToString() +
            "\ntarget_frame: " + target_frame.ToString();
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