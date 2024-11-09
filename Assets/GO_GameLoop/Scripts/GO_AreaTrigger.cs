using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_AreaTrigger : MonoBehaviour
{
    public static event Action OnPlayerEnterArea;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entro el player");
        if (other.CompareTag("Player"))
        {
            OnPlayerEnterArea?.Invoke();
            gameObject.SetActive(false);
        }
    }
}