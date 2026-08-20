using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PetManager : MonoBehaviour
{
    public static PetManager Instance;

    [SerializeField] private List<GhostPet> ghostPets = new();
    public bool isPetAvailable;
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
    public void AddPet(GhostPet ghostPet)
    {
        ghostPets.Add(ghostPet);
        //isPetAvailable = true;
    }
    public void RemovePet(GhostPet ghostPet)
    {
        ghostPets.Remove(ghostPet);
        //CheckPetAvailability();
    }

    public bool CheckPetAvailability()
    {
        foreach (GhostPet pet in ghostPets)
        {
            if (!pet.isOwnerArrived) return true;
        }
        return false;
    }
}
