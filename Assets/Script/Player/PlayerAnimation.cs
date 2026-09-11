using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] Animator animator;

    public void SetXMovement(float x)
    {
        animator.SetFloat("xMovement", x);
    }

    public void SetIsIdle(bool isIdle)
    {
        animator.SetBool("isIdle", isIdle);
    }
}
