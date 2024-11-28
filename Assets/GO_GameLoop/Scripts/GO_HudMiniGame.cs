using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GO_HudMiniGame : MonoBehaviour
{
    [SerializeField] private Image leftPlayer;
    [SerializeField] private Image rightPlayer;
    [SerializeField] private Image remainTimeIMG;
    [SerializeField] private GameObject[] spaceKey;
    private float dificult = .55f;
    int lastLeft;
    int lastRight;
    float inc = .1f;
    float maxTime = .05f;
    float timeKeySpaceRestore = .05f;
    public void SetRemainTime(float _value)
    {
        remainTimeIMG.fillAmount = _value;
    }
    public void SetPullCount(int _value, bool _left = true)
    {
        if (_left)
        {
            if (lastLeft < _value)
            {
                leftPlayer.fillAmount += inc;
            }
            lastLeft = _value;
        }
        else
        {
            if(lastRight < _value)
            {
                rightPlayer.fillAmount += inc;
            }
            lastRight = _value;
        }
    }

    public void ResetPullCount()
    {
        leftPlayer.fillAmount = rightPlayer.fillAmount = lastRight = lastLeft = 0;
    }

    public void PressSpace()
    {
        spaceKey[0].SetActive(false);
        spaceKey[1].SetActive(true);
        timeKeySpaceRestore = maxTime;
    }
    private void LateUpdate()
    {
        if (leftPlayer.fillAmount > 0)
        {
            leftPlayer.fillAmount -= Time.deltaTime * dificult;
        }
        if (rightPlayer.fillAmount > 0)
        {
            rightPlayer.fillAmount -= Time.deltaTime * dificult;
        }
        if(timeKeySpaceRestore > 0)
        {
            if((timeKeySpaceRestore -= Time.deltaTime) < 0)
            {
                spaceKey[0].SetActive(true);
                spaceKey[1].SetActive(false);
            }

        }
    }
}
