using UnityEngine;
using UnityEngine.EventSystems;

public class GO_ButtonHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GO_AudioManager.Instance != null)
        {
            GO_AudioManager.Instance.PlayUISound("GO_Button_Hover");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}