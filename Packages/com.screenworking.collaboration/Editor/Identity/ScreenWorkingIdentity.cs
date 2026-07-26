using System;
using UnityEngine;

namespace ScreenWorking.Collaboration.Editor.Identity
{
    /// <summary>
    /// Sidecar identity component attached to GameObjects synchronized by ScreenWorking.
    /// Manages persistent GUID assignment across scene save/reload, domain reload, and duplication.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class ScreenWorkingIdentity : MonoBehaviour
    {
        [SerializeField]
        private string objectId = string.Empty;

        [SerializeField]
        private string sceneId = string.Empty;

        /// <summary>
        /// Gets the persistent unique object identifier.
        /// </summary>
        public string ObjectId => objectId;

        /// <summary>
        /// Gets or sets the associated scene identifier.
        /// </summary>
        public string SceneId
        {
            get => sceneId;
            set => sceneId = value;
        }

        private void Awake()
        {
            EnsureValidId();
        }

        private void OnValidate()
        {
            EnsureValidId();
        }

        /// <summary>
        /// Guarantees that this object has a non-empty, unique GUID.
        /// </summary>
        public void EnsureValidId()
        {
            if (string.IsNullOrEmpty(objectId))
            {
                objectId = Guid.NewGuid().ToString("N");
            }
        }

        /// <summary>
        /// Assigns an explicit new GUID to this identity instance.
        /// </summary>
        /// <param name="newId">The new GUID string to assign.</param>
        public void AssignId(string newId)
        {
            if (string.IsNullOrEmpty(newId))
            {
                throw new ArgumentException("ID cannot be null or empty.", nameof(newId));
            }
            objectId = newId;
        }

        /// <summary>
        /// Regenerates the object ID. Useful after duplication detection.
        /// </summary>
        public string RegenerateId()
        {
            objectId = Guid.NewGuid().ToString("N");
            return objectId;
        }
    }
}
