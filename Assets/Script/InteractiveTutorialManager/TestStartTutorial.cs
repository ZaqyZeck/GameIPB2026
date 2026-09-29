using TutorialSystem;
using UnityEngine;

public class TestStartTutorial : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject tutorialGameObject;

    [ContextMenu("Trigger Start Tutorial")]
    public void StartTutorialManually()
    {
        if (tutorialGameObject != null)
        {
            tutorialGameObject.SetActive(true);
        }

        if (InteractiveTutorialManager.Instance != null)
        {
            InteractiveTutorialManager.Instance.StartTutorial();
        }
    }
}