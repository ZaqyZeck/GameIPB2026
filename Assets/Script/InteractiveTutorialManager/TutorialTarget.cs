using UnityEngine;

namespace TutorialSystem
{
    // Tempel di objek UI/dunia yang mau di-highlight atau ditunjuk sebuah step.
    public class TutorialTarget : MonoBehaviour
    {
        [Tooltip("Harus sama persis dengan targetKey pada TutorialStep.")]
        public string key;

        [Tooltip("Only used when this target is NOT a RectTransform (world-space object) - " +
                 "a plain Transform has no inherent size, so the spotlight can't derive a " +
                 "radius from it automatically. -1 = use TutorialSpotlight's default radius.")]
        public float highlightRadiusOverride = -1f;
    }
}