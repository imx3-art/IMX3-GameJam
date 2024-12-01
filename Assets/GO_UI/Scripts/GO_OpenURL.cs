using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_OpenURL : MonoBehaviour
{
    public string url = "https://discord.gg/vFbxH8Sj";

    public void OpenLink()
    {
        Application.OpenURL(url);
    }
}
