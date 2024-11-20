using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class GO_HoverManager : MonoBehaviour
{
    public Image background; // Referencia al fondo que cambiará (Frame7 o Rectangulo47)
    public Sprite frame7Sprite; // Sprite para Frame7
    public Sprite defaultSprite; // Sprite original (Rectangulo47)
    public TMP_Text descriptionText; // Referencia al texto de descripción

    // Texto explicativo para cada botón
    [TextArea]
    public string continueDescription = "Continúa el juego desde el último punto de guardado.";
    public string quitDescription = "Cierra el juego.";
    public string newTextDescription = "Explicación adicional del nuevo texto.";

    // Método llamado cuando el puntero entra en el botón
    public void OnHoverEnter(string buttonType)
    {
        if (background != null)
        {
            background.sprite = frame7Sprite; // Cambia el fondo a Frame7
        }

        // Cambia la descripción según el botón
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

    // Método llamado cuando el puntero sale del botón
    public void OnHoverExit()
    {
        if (background != null)
        {
            background.sprite = defaultSprite; // Restaura el fondo original
        }

        if (descriptionText != null)
        {
            descriptionText.text = ""; // Limpia la descripción
        }
    }
}
