using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GO_HudMiniGame : MonoBehaviour
{
    [SerializeField] private Image leftPlayer;
    [SerializeField] private Image rightPlayer;
    [SerializeField] private Image remainTimeIMG;
    private float dificult = .55f;
    int lastLeft;
    int lastRight;
    float inc = .1f;
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

    }
}
