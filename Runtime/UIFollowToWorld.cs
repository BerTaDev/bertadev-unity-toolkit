using UnityEngine;

namespace BerTaDEV.Toolkit
{
    /// <summary>
    /// Keeps a UI element over a world-space target: health bars, name tags, quest markers.
    /// It moves the RectTransform's anchors to the target's viewport position, so the
    /// element's parent should cover the whole screen.
    /// </summary>
    [AddComponentMenu("BerTaDEV/UI Follow To World")]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class UIFollowToWorld : MonoBehaviour
    {
        [Tooltip("World object to follow.")]
        public Transform Target;

        [Tooltip("World-space offset from the target, e.g. (0, 2, 0) to sit above a character's head.")]
        public Vector3 offset;

        [Tooltip("Camera that renders the target. Leave empty to use Camera.main.")]
        public Camera targetCamera;

        [Tooltip("Hide the element while the target is behind the camera (otherwise it shows up mirrored).")]
        public bool hideWhenBehindCamera = true;

        RectTransform rectTransform;
        CanvasGroup canvasGroup;
        bool hidden;
        float alphaBeforeHide;
        bool raycastsBeforeHide;

        void Awake()
        {
            rectTransform = (RectTransform)transform;
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // LateUpdate runs after the camera and the target have moved, so the element doesn't lag or jitter.
        void LateUpdate()
        {
            if (Target == null) return;
            var cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam == null) return;

            Vector3 viewportPoint = cam.WorldToViewportPoint(Target.position + offset);
            bool behind = viewportPoint.z < 0f;
            SetHidden(behind && hideWhenBehindCamera);
            if (hidden) return;

            rectTransform.anchorMin = viewportPoint;
            rectTransform.anchorMax = viewportPoint;
        }

        void SetHidden(bool hide)
        {
            if (hide == hidden) return;
            hidden = hide;

            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            if (hide)
            {
                // Remember the group's own values so fades and raycast settings survive hiding.
                alphaBeforeHide = canvasGroup.alpha;
                raycastsBeforeHide = canvasGroup.blocksRaycasts;
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                canvasGroup.alpha = alphaBeforeHide;
                canvasGroup.blocksRaycasts = raycastsBeforeHide;
            }
        }
    }
}
