using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScreenWorking.Collaboration.Editor.Models
{
    [Serializable]
    public class SerializedValue
    {
        public SerializedValueType ValueType { get; set; } = SerializedValueType.Null;
        public long IntValue { get; set; }
        public double FloatValue { get; set; }
        public bool BoolValue { get; set; }
        public string StringValue { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public string TargetGuid { get; set; }
        public long LocalFileId { get; set; }

        public List<SerializedValue> ArrayValues { get; set; } = new List<SerializedValue>();

        public static SerializedValue FromInt(int val) => new SerializedValue { ValueType = SerializedValueType.Integer, IntValue = val };
        public static SerializedValue FromLong(long val) => new SerializedValue { ValueType = SerializedValueType.Long, IntValue = val };
        public static SerializedValue FromFloat(float val) => new SerializedValue { ValueType = SerializedValueType.Float, FloatValue = val };
        public static SerializedValue FromDouble(double val) => new SerializedValue { ValueType = SerializedValueType.Double, FloatValue = val };
        public static SerializedValue FromBool(bool val) => new SerializedValue { ValueType = SerializedValueType.Boolean, BoolValue = val };
        public static SerializedValue FromString(string val) => new SerializedValue { ValueType = SerializedValueType.String, StringValue = val ?? string.Empty };

        public static SerializedValue FromVector3(Vector3 v) => new SerializedValue
        {
            ValueType = SerializedValueType.Vector3,
            X = v.x,
            Y = v.y,
            Z = v.z
        };

        public static SerializedValue FromQuaternion(Quaternion q) => new SerializedValue
        {
            ValueType = SerializedValueType.Quaternion,
            X = q.x,
            Y = q.y,
            Z = q.z,
            W = q.w
        };

        public static SerializedValue FromColor(Color c) => new SerializedValue
        {
            ValueType = SerializedValueType.Color,
            X = c.r,
            Y = c.g,
            Z = c.b,
            W = c.a
        };

        public static SerializedValue FromSceneObjectRef(string sceneObjectId) => new SerializedValue
        {
            ValueType = SerializedValueType.SceneObjectRef,
            TargetGuid = sceneObjectId
        };

        public static SerializedValue FromAssetRef(string assetGuid, long localFileId) => new SerializedValue
        {
            ValueType = SerializedValueType.AssetRef,
            TargetGuid = assetGuid,
            LocalFileId = localFileId
        };

        public Vector3 ToVector3() => new Vector3(X, Y, Z);
        public Quaternion ToQuaternion() => new Quaternion(X, Y, Z, W);
        public Color ToColor() => new Color(X, Y, Z, W);
    }
}
