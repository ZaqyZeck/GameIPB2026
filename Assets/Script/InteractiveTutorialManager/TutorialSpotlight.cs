using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TutorialSystem
{
    [RequireComponent(typeof(Image))]
    public class TutorialSpotlight : MonoBehaviour
    {
        private const int MaxHighlights = 4;

        [SerializeField] private Canvas canvas;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private Camera worldCamera;

        [Header("Sizing")]
        [SerializeField] private float paddingPixels = 20f;
        [SerializeField] private float softnessPixels = 15f;
        [SerializeField] private float defaultWorldTargetRadiusPixels = 80f;

        [Header("Visuals")]
        [Tooltip("Turn the glowing ring around the spotlight on or off.")]
        [SerializeField] private bool showRing = true;

        [Header("Animation")]
        [SerializeField] private float transitionDuration = 0.45f;
        [SerializeField] private AnimationCurve transitionEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private static readonly int CentersProp = Shader.PropertyToID("_Centers");
        private static readonly int RadiiProp = Shader.PropertyToID("_Radii");
        private static readonly int CountProp = Shader.PropertyToID("_HighlightCount");
        private static readonly int SoftnessProp = Shader.PropertyToID("_Softness");
        private static readonly int AspectProp = Shader.PropertyToID("_AspectRatio");
        private static readonly int EnableRingProp = Shader.PropertyToID("_EnableRing");

        private Image overlayImage;
        private Material overlayMaterialInstance;

        private readonly Vector2[] currentCenters = new Vector2[MaxHighlights];
        private readonly float[] currentRadii = new float[MaxHighlights];
        private readonly bool[] currentActive = new bool[MaxHighlights];

        private Coroutine transitionRoutine;

        private void Awake()
        {
            overlayImage = GetComponent<Image>();
            overlayMaterialInstance = new Material(overlayImage.material);
            overlayImage.material = overlayMaterialInstance;

            if (worldCamera == null) worldCamera = Camera.main;
        }
        public void Update()
        {
            if (gameObject.activeInHierarchy && transitionRoutine == null)
    {
        float screenW = Screen.width;
        float screenH = Screen.height;
        
        overlayMaterialInstance.SetFloat(AspectProp, screenW / screenH);
        overlayMaterialInstance.SetFloat(SoftnessProp, softnessPixels / screenH);
    }
        }
        public void Highlight(List<Transform> targets)
        {
            gameObject.SetActive(true);

            int requestedCount = targets?.Count ?? 0;
            int count = Mathf.Min(requestedCount, MaxHighlights);
            if (requestedCount > MaxHighlights)
            {
                Debug.LogWarning($"TutorialSpotlight: {requestedCount} targets requested but only " +
                                  $"{MaxHighlights} are supported - extras will be ignored.");
            }

            var toCenters = new Vector2[MaxHighlights];
            var toRadii = new float[MaxHighlights];

            float screenW = Screen.width;
            float screenH = Screen.height;

            for (int i = 0; i < count; i++)
            {
                if (targets[i] == null) continue;

                GetScreenCircle(targets[i], out Vector2 centerScreen, out float radiusScreen);

                toCenters[i] = new Vector2(centerScreen.x / screenW, centerScreen.y / screenH);
                toRadii[i] = radiusScreen / screenH;
            }

            BeginTransition(toCenters, toRadii, count);
        }

        public void Highlight(Transform target) => Highlight(new List<Transform> { target });

        /// <summary>
        /// Like Highlight, but instead of drawing one ring per target, fits a
        /// single ring around the combined bounds of all of them. Use this for
        /// a highlighted path/region (e.g. a drag path across several tiles)
        /// where one ring enclosing the whole thing reads better than several
        /// separate, overlapping rings - and it isn't capped at MaxHighlights
        /// since it only ever produces one circle regardless of target count.
        /// </summary>
        public void HighlightBounds(List<Transform> targets)
        {
            gameObject.SetActive(true);

            var toCenters = new Vector2[MaxHighlights];
            var toRadii = new float[MaxHighlights];

            List<Vector2> screenPoints = new List<Vector2>();
            if (targets != null)
            {
                foreach (var t in targets)
                {
                    if (t != null) CollectScreenPoints(t, screenPoints);
                }
            }

            if (screenPoints.Count == 0)
            {
                BeginTransition(toCenters, toRadii, 0);
                return;
            }

            Vector2 min = screenPoints[0];
            Vector2 max = screenPoints[0];
            for (int i = 1; i < screenPoints.Count; i++)
            {
                min = Vector2.Min(min, screenPoints[i]);
                max = Vector2.Max(max, screenPoints[i]);
            }

            Vector2 centerScreen = (min + max) * 0.5f;

            float maxDist = 0f;
            foreach (var p in screenPoints)
            {
                float d = Vector2.Distance(centerScreen, p);
                if (d > maxDist) maxDist = d;
            }

            float radiusScreen = maxDist + paddingPixels;

            float screenW = Screen.width;
            float screenH = Screen.height;

            toCenters[0] = new Vector2(centerScreen.x / screenW, centerScreen.y / screenH);
            toRadii[0] = radiusScreen / screenH;

            BeginTransition(toCenters, toRadii, 1);
        }

        /// <summary>
        /// Draws TWO independent highlight groups at once, sharing the same MaxHighlights slot
        /// budget and the same single animated transition (so neither group's appearance
        /// overwrites or fights the other's):
        ///   - keyTargets: each gets its own individual circle, same sizing/positioning as
        ///     Highlight(List&lt;Transform&gt;) - one slot per target.
        ///   - pathTargets: all combined into ONE bounds circle, same sizing/positioning as
        ///     HighlightBounds(List&lt;Transform&gt;) - always exactly one slot if any are present.
        /// A path bounds circle (if requested) always reserves its own slot first, since it's
        /// usually the more important "what to actually do next" highlight for DragToTile/
        /// KnockToTile steps; key targets fill whatever slots remain and are trimmed (with a
        /// warning) if there isn't room for all of them. Either list can be null/empty - passing
        /// both null/empty clears the spotlight, same as Highlight(null) would.
        /// </summary>
        public void HighlightGroups(List<Transform> keyTargets, List<Transform> pathTargets)
        {
            gameObject.SetActive(true);

            var toCenters = new Vector2[MaxHighlights];
            var toRadii = new float[MaxHighlights];
            int count = 0;

            float screenW = Screen.width;
            float screenH = Screen.height;

            bool hasPathTargets = pathTargets != null && pathTargets.Count > 0;
            int keyRequested = keyTargets?.Count ?? 0;

            // Path bounds circle reserves one slot up front (if it has anything to draw) -
            // key targets get whatever's left.
            int maxKeySlots = hasPathTargets ? MaxHighlights - 1 : MaxHighlights;
            int keyCount = Mathf.Min(keyRequested, Mathf.Max(maxKeySlots, 0));

            if (keyRequested > maxKeySlots)
            {
                Debug.LogWarning($"TutorialSpotlight: {keyRequested} key targets requested but only " +
                                  $"{maxKeySlots} slot(s) available ({(hasPathTargets ? "1 reserved for the path highlight" : "no path highlight active")}) " +
                                  "- extras will be ignored.");
            }

            for (int i = 0; i < keyCount; i++)
            {
                if (keyTargets[i] == null) continue;

                GetScreenCircle(keyTargets[i], out Vector2 centerScreen, out float radiusScreen);

                toCenters[count] = new Vector2(centerScreen.x / screenW, centerScreen.y / screenH);
                toRadii[count] = radiusScreen / screenH;
                count++;
            }

            if (hasPathTargets)
            {
                List<Vector2> screenPoints = new List<Vector2>();
                foreach (var t in pathTargets)
                {
                    if (t != null) CollectScreenPoints(t, screenPoints);
                }

                if (screenPoints.Count > 0)
                {
                    Vector2 min = screenPoints[0];
                    Vector2 max = screenPoints[0];
                    for (int i = 1; i < screenPoints.Count; i++)
                    {
                        min = Vector2.Min(min, screenPoints[i]);
                        max = Vector2.Max(max, screenPoints[i]);
                    }

                    Vector2 centerScreen = (min + max) * 0.5f;

                    float maxDist = 0f;
                    foreach (var p in screenPoints)
                    {
                        float d = Vector2.Distance(centerScreen, p);
                        if (d > maxDist) maxDist = d;
                    }

                    float radiusScreen = maxDist + paddingPixels;

                    toCenters[count] = new Vector2(centerScreen.x / screenW, centerScreen.y / screenH);
                    toRadii[count] = radiusScreen / screenH;
                    count++;
                }
            }

            BeginTransition(toCenters, toRadii, count);
        }

        public void Hide()
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
            }

            for (int i = 0; i < MaxHighlights; i++)
            {
                currentCenters[i] = Vector2.zero;
                currentRadii[i] = 0f;
                currentActive[i] = false;
            }

            gameObject.SetActive(false);
        }

        private void BeginTransition(Vector2[] toCenters, float[] toRadii, int toCount)
        {
            if (transitionRoutine != null) StopCoroutine(transitionRoutine);
            transitionRoutine = StartCoroutine(AnimateTransition(toCenters, toRadii, toCount));
        }

        private IEnumerator AnimateTransition(Vector2[] toCenters, float[] toRadii, int toCount)
        {
            var fromCenters = new Vector2[MaxHighlights];
            var fromRadii = new float[MaxHighlights];
            var toActive = new bool[MaxHighlights];
            int renderCount = 0;

            bool hasExistingHighlight = false;
            Vector2 firstExistingCenter = Vector2.zero;
            for (int j = 0; j < MaxHighlights; j++)
            {
                if (currentActive[j])
                {
                    hasExistingHighlight = true;
                    firstExistingCenter = currentCenters[j];
                    break;
                }
            }

            for (int i = 0; i < MaxHighlights; i++)
            {
                toActive[i] = i < toCount;
                bool wasActive = currentActive[i];

                if (toActive[i] && !wasActive)
                {
                    if (hasExistingHighlight)
                    {
                        fromCenters[i] = firstExistingCenter;
                        fromRadii[i] = 0f;
                    }
                    else
                    {
                        Vector2 dirFromCenter = (toCenters[i] - new Vector2(0.5f, 0.5f)).normalized;
                        if (dirFromCenter == Vector2.zero) dirFromCenter = Vector2.up; 
                        
                        fromCenters[i] = toCenters[i] + (dirFromCenter * 2f); 
                        fromRadii[i] = toRadii[i]; 
                    }
                }
                else if (toActive[i] && wasActive)
                {
                    fromCenters[i] = currentCenters[i];
                    fromRadii[i] = currentRadii[i];
                }
                else if (!toActive[i] && wasActive)
                {
                    fromCenters[i] = currentCenters[i];
                    fromRadii[i] = currentRadii[i];
                    toCenters[i] = currentCenters[i];
                    toRadii[i] = 0f; 
                }
                else
                {
                    fromCenters[i] = currentCenters[i];
                    fromRadii[i] = 0f;
                    toCenters[i] = currentCenters[i];
                    toRadii[i] = 0f;
                }

                if (toActive[i] || wasActive) renderCount = i + 1;

                currentActive[i] = toActive[i] || currentActive[i];
            }

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = transitionEase.Evaluate(Mathf.Clamp01(elapsed / transitionDuration));

                ApplyFrame(fromCenters, fromRadii, toCenters, toRadii, renderCount, t);
                yield return null;
            }

            ApplyFrame(fromCenters, fromRadii, toCenters, toRadii, renderCount, 1f);

            for (int i = 0; i < MaxHighlights; i++)
            {
                currentActive[i] = toActive[i];
            }

            SendToShader(currentCenters, currentRadii, toCount);
            transitionRoutine = null;
        }

        private void ApplyFrame(Vector2[] fromCenters, float[] fromRadii, Vector2[] toCenters, float[] toRadii, int renderCount, float t)
        {
            for (int i = 0; i < MaxHighlights; i++)
            {
                currentCenters[i] = Vector2.LerpUnclamped(fromCenters[i], toCenters[i], t);
                currentRadii[i] = Mathf.LerpUnclamped(fromRadii[i], toRadii[i], t);
            }

            SendToShader(currentCenters, currentRadii, renderCount);
        }

        private void SendToShader(Vector2[] centers, float[] radii, int count)
        {
            var centerVectors = new Vector4[MaxHighlights];
            for (int i = 0; i < MaxHighlights; i++)
            {
                centerVectors[i] = new Vector4(centers[i].x, centers[i].y, 0f, 0f);
            }

            float screenW = Screen.width;
            float screenH = Screen.height;

            overlayMaterialInstance.SetVectorArray(CentersProp, centerVectors);
            overlayMaterialInstance.SetFloatArray(RadiiProp, radii);
            overlayMaterialInstance.SetInt(CountProp, count);
            overlayMaterialInstance.SetFloat(SoftnessProp, softnessPixels / screenH);
            overlayMaterialInstance.SetFloat(AspectProp, screenW / screenH);
            overlayMaterialInstance.SetFloat(EnableRingProp, showRing ? 1f : 0f); 
        }

        private void GetScreenCircle(Transform target, out Vector2 centerScreen, out float radiusScreen)
        {
            if (target is RectTransform rect)
            {
                Vector3[] worldCorners = new Vector3[4];
                rect.GetWorldCorners(worldCorners);

                Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCamera;

                Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, worldCorners[0]);
                Vector2 max = min;
                for (int i = 1; i < 4; i++)
                {
                    Vector2 p = RectTransformUtility.WorldToScreenPoint(cam, worldCorners[i]);
                    min = Vector2.Min(min, p);
                    max = Vector2.Max(max, p);
                }

                centerScreen = (min + max) * 0.5f;
                
                // Updated Logic: Uses the larger of the width or height to create a tighter circle
                Vector2 size = max - min;
                radiusScreen = Mathf.Max(size.x, size.y) * 0.5f + paddingPixels;
            }
            else
            {
                Camera cam = worldCamera != null ? worldCamera : Camera.main;
                centerScreen = cam != null ? (Vector2)cam.WorldToScreenPoint(target.position) : (Vector2)target.position;

                float overrideRadius = -1f;
                var tutorialTarget = target.GetComponent<TutorialTarget>();
                if (tutorialTarget != null) overrideRadius = tutorialTarget.highlightRadiusOverride;

                radiusScreen = (overrideRadius >= 0f ? overrideRadius : defaultWorldTargetRadiusPixels) + paddingPixels;
            }
        }

        /// <summary>
        /// Like GetScreenCircle, but instead of returning a single center+radius
        /// for one target, appends the raw screen-space points that describe
        /// this target's footprint (its four corners for UI, or four cardinal
        /// points around its own radius for world targets) so multiple targets'
        /// points can be combined into one bounding circle by the caller.
        /// </summary>
        private void CollectScreenPoints(Transform target, List<Vector2> screenPoints)
        {
            if (target is RectTransform rect)
            {
                Vector3[] worldCorners = new Vector3[4];
                rect.GetWorldCorners(worldCorners);

                Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCamera;

                for (int i = 0; i < 4; i++)
                {
                    screenPoints.Add(RectTransformUtility.WorldToScreenPoint(cam, worldCorners[i]));
                }
            }
            else
            {
                Camera cam = worldCamera != null ? worldCamera : Camera.main;
                Vector2 centerScreen = cam != null ? (Vector2)cam.WorldToScreenPoint(target.position) : (Vector2)target.position;

                float overrideRadius = -1f;
                var tutorialTarget = target.GetComponent<TutorialTarget>();
                if (tutorialTarget != null) overrideRadius = tutorialTarget.highlightRadiusOverride;

                float radius = overrideRadius >= 0f ? overrideRadius : defaultWorldTargetRadiusPixels;

                screenPoints.Add(centerScreen + new Vector2(radius, 0f));
                screenPoints.Add(centerScreen + new Vector2(-radius, 0f));
                screenPoints.Add(centerScreen + new Vector2(0f, radius));
                screenPoints.Add(centerScreen + new Vector2(0f, -radius));
            }
        }
    }
}