using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class GO_HoverManager : MonoBehaviour
{
    public Image background; // Referencia al fondo que cambiar� (Frame7 o Rectangulo47)
    public Sprite frame7Sprite; // Sprite para Frame7
    public Sprite defaultSprite; // Sprite original (Rectangulo47)
    public TMP_Text descriptionText; // Referencia al texto de descripci�n

    // Texto explicativo para cada bot�n
    [TextArea]
    public string continueDescription = "Contin�a el juego desde el �ltimo punto de guardado.";
    public string quitDescription = "Cierra el juego.";
    public string newTextDescription = "Explicaci�n adicional del nuevo texto.";

    // M�todo llamado cuando el puntero entra en el bot�n
    public void OnHoverEnter(string buttonType)
    {
        if (background != null)
        {
            background.sprite = frame7Sprite; // Cambia el fondo a Frame7
        }

        // Cambia la descripci�n seg�n el bot�n
        if (descriptionText != null)
        {
            switch (buttonType)
            {
                case "Continue":
                    descriptionText.text = continueDescription;
                    break;
                case "Quit":
                    descriptionText.text = quitDescription;
                    break;
                case "NewText":
                    descriptionText.text = newTextDescription;
                    break;
            }
        }
    }

    // M�todo llamado cuando el puntero sale del bot�n
    public void OnHoverExit()
    {
        if (background != null)
        {
            background.sprite = defaultSprite; // Restaura el fondo original
        }

        if (descriptionText != null)
        {
            descriptionText.text = ""; // Limpia la descripci�n
        }
    }
}
