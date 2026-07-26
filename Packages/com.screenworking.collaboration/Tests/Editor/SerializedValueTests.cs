using NUnit.Framework;
using ScreenWorking.Collaboration.Editor.Models;
using UnityEngine;

namespace ScreenWorking.Collaboration.Tests.Editor
{
    [TestFixture]
    public class SerializedValueTests
    {
        [Test]
        public void PrimitiveConversion_BehavesAsExpected()
        {
            var intVal = SerializedValue.FromInt(42);
            Assert.AreEqual(SerializedValueType.Integer, intVal.ValueType);
            Assert.AreEqual(42, intVal.IntValue);

            var floatVal = SerializedValue.FromFloat(3.14159f);
            Assert.AreEqual(SerializedValueType.Float, floatVal.ValueType);
            Assert.AreEqual(3.14159f, (float)floatVal.FloatValue, 1e-5);

            var strVal = SerializedValue.FromString("Hello ScreenWorking");
            Assert.AreEqual(SerializedValueType.String, strVal.ValueType);
            Assert.AreEqual("Hello ScreenWorking", strVal.StringValue);
        }

        [Test]
        public void VectorAndQuaternionConversion_PreservesPrecision()
        {
            var v3 = new Vector3(1.23f, 4.56f, 7.89f);
            var sv3 = SerializedValue.FromVector3(v3);
            Assert.AreEqual(SerializedValueType.Vector3, sv3.ValueType);
            Assert.AreEqual(v3, sv3.ToVector3());

            var q = Quaternion.Euler(45f, 90f, 180f);
            var sq = SerializedValue.FromQuaternion(q);
            Assert.AreEqual(SerializedValueType.Quaternion, sq.ValueType);
            Assert.AreEqual(q.x, sq.ToQuaternion().x, 1e-4f);
            Assert.AreEqual(q.y, sq.ToQuaternion().y, 1e-4f);
        }

        [Test]
        public void ObjectReferenceConversion_PreservesGuids()
        {
            var sceneRef = SerializedValue.FromSceneObjectRef("scene-guid-12345");
            Assert.AreEqual(SerializedValueType.SceneObjectRef, sceneRef.ValueType);
            Assert.AreEqual("scene-guid-12345", sceneRef.TargetGuid);

            var assetRef = SerializedValue.FromAssetRef("asset-guid-67890", 1001);
            Assert.AreEqual(SerializedValueType.AssetRef, assetRef.ValueType);
            Assert.AreEqual("asset-guid-67890", assetRef.TargetGuid);
            Assert.AreEqual(1001, assetRef.LocalFileId);
        }
    }
}
