using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class GO_ReloadPage : MonoBehaviour
{ 
    [DllImport("__Internal")]
    private static extern void reloadGameLanding();

    public void ReloadPage()
    {
        Debug.Log("ReloadGameLanding");
            // Llamamos a la función reloadGameLanding en JS
            reloadGameLanding();
            Debug.Log("ReloadGameLanding");
    }
}
