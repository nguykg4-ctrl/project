using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScreenWorking.Collaboration.Editor.UI
{
    /// <summary>
    /// Scene View toolbar overlay for ScreenWorking providing room status, sync toggles, and user camera tracking.
    /// </summary>
    [Overlay(typeof(SceneView), "ScreenWorking Toolbar", true)]
    public class ScreenWorkingOverlay : Overlay
    {
        private bool isFollowing;

        public override VisualElement CreatePanelContent()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.paddingLeft = 4;
            container.style.paddingRight = 4;

            var statusBadge = new Label("[screen working] Online (Room Active)");
            statusBadge.style.fontSize = 11;
            statusBadge.style.marginRight = 6;
            statusBadge.style.unityFontStyleAndWeight = FontStyle.Bold;

            var syncToggle = new Toggle("Live Sync") { value = true };
            syncToggle.style.marginRight = 6;

            var followButton = new Button();
            followButton.text = "Follow User";
            followButton.clicked += () =>
            {
                isFollowing = !isFollowing;
                if (isFollowing)
                {
                    followButton.text = "Following (Active)";
                    followButton.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f);

                    if (SceneView.lastActiveSceneView != null)
                    {
                        if (Selection.activeGameObject != null)
                        {
                            SceneView.lastActiveSceneView.FrameSelected();
                        }
                        Debug.Log("[screen working] Camera tracking activated for active collaborator selection.");
                    }
                }
                else
                {
                    followButton.text = "Follow User";
                    followButton.style.backgroundColor = StyleKeyword.Null;
                    Debug.Log("[screen working] Camera tracking deactivated.");
                }
            };

            container.Add(statusBadge);
            container.Add(syncToggle);
            container.Add(followButton);

            return container;
        }
    }
}
