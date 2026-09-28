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
    }

    public override void OnPickupBehaviour()
    {
    }
}