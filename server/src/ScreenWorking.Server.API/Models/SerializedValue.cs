using System;
using System.Collections.Generic;

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

        public static SerializedValue FromVector3(float x, float y, float z) => new SerializedValue
        {
            ValueType = SerializedValueType.Vector3,
            X = x,
            Y = y,
            Z = z
        };
    }
}
