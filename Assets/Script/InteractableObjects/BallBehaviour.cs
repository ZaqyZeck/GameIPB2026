using System.Collections.Generic;
using UnityEngine;

public class BallBehaviour : ObjectBehaviour
{
    [SerializeField] float callArea;
    List<Pet> petsCalled;
    public override void OnDropBehaviour()
    {

    }

    public override void OnFloorBehaviour()
    {
        //petsCalled = PetManager.Instance.GetPetsWithAction(trait);
        //foreach (Pet pet in petsCalled)
        //{
        //    if (pet == null) continue;
        //    if (pet.Movement.IsNear(transform.position, callArea))
        //        pet.ChangeTextAction("goto ball");
        //        pet.Movement.MoveTo(MapManager.Instance.GetPositionNear(pet.transform.position, transform.position), () =>
        //        {

        //            pet.BehaviorController.TryExecuteAction(trait);
        //        });
        //}
    }

    public override void OnPickupBehaviour()
    {
        //if (petsCalled == null) return;
        //foreach (Pet pet in petsCalled)
        //{
        //    if (pet.Movement.IsNear(transform.position, callArea))
        //        pet.BehaviorController.TryStopAction(trait);
        //}
    }
}