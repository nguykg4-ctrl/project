using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScreenWorking.Collaboration.Editor.Identity
{
    /// <summary>
    /// Registry and manager for mapping ScreenWorking unique object GUIDs to active GameObjects in loaded scenes.
    /// </summary>
    public class ScreenWorkingIdentityManager
    {
        private static readonly Dictionary<string, ScreenWorkingIdentity> IdentityMap = new Dictionary<string, ScreenWorkingIdentity>();

        /// <summary>
        /// Registers a GameObject and ensures it has a valid <see cref="ScreenWorkingIdentity"/>.
        /// </summary>
        /// <param name="go">Target GameObject to register.</param>
        /// <param name="explicitId">Optional explicit ID to assign.</param>
        /// <returns>The associated <see cref="ScreenWorkingIdentity"/>.</returns>
        public static ScreenWorkingIdentity Register(GameObject go, string explicitId = null)
        {
            if (go == null) return null;

            var identity = go.GetComponent<ScreenWorkingIdentity>();
            if (identity == null)
            {
                identity = go.AddComponent<ScreenWorkingIdentity>();
                identity.hideFlags = HideFlags.DontSaveInBuild;
            }

            if (!string.IsNullOrEmpty(explicitId))
            {
                identity.AssignId(explicitId);
            }
            else
            {
                identity.EnsureValidId();
            }

            // Check for duplicate GUID in current map
            if (IdentityMap.TryGetValue(identity.ObjectId, out var existing) && existing != null && existing != identity)
            {
                // Duplicate detected (e.g. via Unity Duplicate command), regenerate new ID
                identity.RegenerateId();
            }

            IdentityMap[identity.ObjectId] = identity;
            return identity;
        }

        /// <summary>
        /// Unregisters an object identity from the global map.
        /// </summary>
        /// <param name="objectId">Target object ID.</param>
        public static void Unregister(string objectId)
        {
            if (!string.IsNullOrEmpty(objectId))
            {
                IdentityMap.Remove(objectId);
            }
        }

        /// <summary>
        /// Finds a active <see cref="ScreenWorkingIdentity"/> by its unique GUID.
        /// </summary>
        /// <param name="objectId">Target object GUID.</param>
        /// <returns>Matching identity component, or null if not found.</returns>
        public static ScreenWorkingIdentity FindById(string objectId)
        {
            if (string.IsNullOrEmpty(objectId)) return null;

            if (IdentityMap.TryGetValue(objectId, out var identity) && identity != null)
            {
                return identity;
            }

            // Fallback scan across active scenes
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                var roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    var identities = root.GetComponentsInChildren<ScreenWorkingIdentity>(true);
                    foreach (var idComp in identities)
                    {
                        if (idComp.ObjectId == objectId)
                        {
                            IdentityMap[objectId] = idComp;
                            return idComp;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets or creates an object ID for a given GameObject.
        /// </summary>
        /// <param name="go">Target GameObject.</param>
        /// <returns>Unique object GUID string.</returns>
        public static string GetOrCreateId(GameObject go)
        {
            if (go == null) return null;
            var identity = Register(go);
            return identity.ObjectId;
        }

        /// <summary>
        /// Clears all cached registrations.
        /// </summary>
        public static void ClearRegistry()
        {
            IdentityMap.Clear();
        }
    }
}
