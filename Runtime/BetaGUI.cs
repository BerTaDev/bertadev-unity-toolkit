using UnityEngine;

namespace BerTaDEV.Toolkit
{
    /// <summary>
    /// Build info overlay for test builds: product and company name, version, FPS and your
    /// own build number, drawn in the top-left corner.
    /// </summary>
    [AddComponentMenu("BerTaDEV/Beta GUI")]
    [DisallowMultipleComponent]
    public class BetaGUI : MonoBehaviour
    {
        public static BetaGUI instance;

        [Tooltip("Your own build number, shown as \"build : N\".")]
        public int build_index;

        [Tooltip("Frames per second, averaged over the last refresh interval.")]
        public float FrameCount;

        [Tooltip("How often the FPS value refreshes, in seconds.")]
        [Min(0.05f)] public float fpsRefreshInterval = 0.1f;

        [Tooltip("Only draw the overlay in the Editor and in Development Builds.")]
        public bool developmentBuildsOnly;

        public Color infoColor = Color.black;
        public Color statsColor = Color.red;

        int frames;
        float elapsed;

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        void Update()
        {
            // Counting frames over an interval gives a steadier number than 1 / deltaTime of a single frame.
            frames++;
            elapsed += Time.unscaledDeltaTime;
            if (elapsed < fpsRefreshInterval) return;

            FrameCount = frames / elapsed;
            frames = 0;
            elapsed = 0f;
        }

        void OnGUI()
        {
            if (developmentBuildsOnly && !Debug.isDebugBuild) return;

            GUI.depth = 2;
            GUI.color = infoColor;
            GUI.Label(new Rect(10, 10, 500, 20), Application.productName + " By:" + Application.companyName);
            GUI.Label(new Rect(10, 30, 500, 20), "Version: " + Application.version);
            GUI.color = statsColor;
            GUI.Label(new Rect(10, 50, 500, 20), "FPS : " + Mathf.Round(FrameCount));
            GUI.Label(new Rect(10, 70, 500, 20), "build : " + build_index);
            GUI.color = Color.white;
        }
    }
}
