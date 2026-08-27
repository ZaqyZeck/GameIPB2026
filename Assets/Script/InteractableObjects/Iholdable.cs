using UnityEngine;

public interface IHoldable
{
    Transform Transform { get; }
    void OnPickedUp(Transform holdPoint);
    void OnDropped(Transform dropParent);
    void SetFacing(bool isFacingPositiveX);
}