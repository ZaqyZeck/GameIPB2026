using UnityEngine;
using TutorialSystem;

public class AutoStartTutorial : MonoBehaviour
{
    [SerializeField] private float startDelay = 0.5f;

    private void Start()
    {
        Invoke(nameof(StartTheTutorial), startDelay);
    }

    private void StartTheTutorial()
    {
        if (InteractiveTutorialManager.Instance != null)
        {
            InteractiveTutorialManager.Instance.StartTutorial();
        }
    }
}