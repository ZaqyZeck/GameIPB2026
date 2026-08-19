using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PetManager : MonoBehaviour
{
    public static PetManager Instance;

    private List<GhostPet> ghostPets = new();

    private void Awake()
    {
        Instance = this;
    }

    public GhostPet GetAvailableGhostPet()
    {
        List<GhostPet> availablePets = new();

        foreach (GhostPet ghostPet in ghostPets)
        {
            if (!ghostPet.isOwnerArrived)
            {
                availablePets.Add(ghostPet);
            }
        }

        if (availablePets.Count == 0)
        {
            return null;
        }

        return availablePets[Random.Range(0, availablePets.Count)];
    }
}
