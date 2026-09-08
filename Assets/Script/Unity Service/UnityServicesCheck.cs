using Unity.Services.Core;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnityServicesCheck : MonoBehaviour
{
    private void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CheckUnityServices();
        }
    }

    private void CheckUnityServices()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            Debug.Log("Unity Services sudah diinisialisasi.");
        }
        else
        {
            Debug.LogWarning($"Unity Services belum diinisialisasi. Current State: {UnityServices.State}");
        }
    }
}