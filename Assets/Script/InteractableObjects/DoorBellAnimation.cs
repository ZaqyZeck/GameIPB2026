using DG.Tweening;
using UnityEngine;

public class DoorBellAnimation : MonoBehaviour
{
    [SerializeField] private float scaleDuration = 0.25f;
    [SerializeField] private float rotateDuration = 0.15f;

    private Sequence rotateSequence;

    private void OnEnable()
    {
        transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        transform.localRotation = Quaternion.identity;

        rotateSequence?.Kill();

        transform.DOScale(Vector3.one, scaleDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                rotateSequence = DOTween.Sequence();

                rotateSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, 15f), rotateDuration));
                rotateSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, -15f), rotateDuration));

                rotateSequence.SetLoops(-1, LoopType.Yoyo);
            });
    }

    private void OnDisable()
    {
        transform.DOKill();

        rotateSequence?.Kill();
        rotateSequence = null;

        transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        transform.localRotation = Quaternion.identity;
    }
}
