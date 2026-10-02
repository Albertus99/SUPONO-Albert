using UnityEngine;

namespace Supono.Cameras
{
    /// <summary>
    /// Keeps a camera's framing on narrow (portrait) screens. Unity's field of view is vertical, so a tall
    /// screen sees much less sideways; this widens the vertical FOV to preserve the horizontal view the
    /// shot was composed for, capped so it never turns into a fisheye.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class AspectFieldOfView : MonoBehaviour
    {
        [SerializeField, Tooltip("Aspect (width / height) the shot was composed for.")]
        float designAspect = 16f / 9f;
        [SerializeField, Range(20f, 110f)] float maxVerticalFov = 78f;

        Camera view;
        float designFov;

        void Awake()
        {
            view = GetComponent<Camera>();
            designFov = view.fieldOfView;
        }

        void LateUpdate()
        {
            float aspect = view.aspect;
            if (aspect >= designAspect)
            {
                view.fieldOfView = designFov;
                return;
            }
            float halfHorizontal = Mathf.Atan(Mathf.Tan(designFov * 0.5f * Mathf.Deg2Rad) * designAspect);
            float vertical = 2f * Mathf.Atan(Mathf.Tan(halfHorizontal) / aspect) * Mathf.Rad2Deg;
            view.fieldOfView = Mathf.Min(maxVerticalFov, vertical);
        }
    }
}
