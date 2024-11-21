using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GO_UIManager : MonoBehaviour
{
    public static GO_UIManager Instance;

    [Header("Panels")]
    public GameObject codePanel; // Panel de la puerta.
    public GameObject bookPanel; // Panel de los libritos.

    [Header("Text Elements")]
    public TMP_Text bookText; // Texto para mostrar el número del librito.
    public TMP_Text inputField; // Campo de texto para ingresar el código.

    [Header("UI Colors")]
    public Color defaultColor = Color.gray; // Color para los números.
    public Color deleteColor = Color.red;  // Color del botón de borrar.
    public Color submitColor = Color.green; // Color del botón de enviar.

    private string userInput = "";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Muestra el panel para ingresar el código de la puerta.
    public void ShowCodePanel()
    {
        codePanel.SetActive(true);
    }

    // Oculta el panel de la puerta y limpia la entrada.
    public void HideCodePanel()
    {
        codePanel.SetActive(false);
        ClearInput();
    }

    // Muestra el número del librito en el panel.
    public void ShowBookNumber(string number)
    {
        bookText.text = $"Número: {number}";
        bookPanel.SetActive(true);
    }

    // Oculta el panel de información del librito.
    public void HideBookPanel()
    {
        bookPanel.SetActive(false);
    }

    public void SetCodeField(string codePart)
    {
        inputField.text = codePart;
        Debug.Log($"Código restante en el panel: {codePart}");
    }

    // Añade un número al campo de texto desde la UI.
    public void AddNumberToInput(string number)
    {
        if (userInput.Length < 4) // Límite de 4 dígitos.
        {
            userInput += number;
            UpdateInputField();
        }
    }

    // Limpia el campo de texto.
    public void ClearInput()
    {
        userInput = "";
        UpdateInputField();
    }

    // Valida el código ingresado.
    public void SubmitCode()
    {
        if (userInput.Length != 4)
        {
            Debug.Log("Debe ingresar un código de 4 dígitos.");
            return;
        }

        if (GO_CodeManager.Instance.ValidateCode(userInput))
        {
            Debug.Log("¡Código correcto! Abriendo la puerta...");
            
            HideCodePanel();
            // Implementa la lógica para abrir la puerta.
        }
        else
        {
            Debug.Log("Código incorrecto.");
        }
    }

    // Actualiza el campo de entrada visualmente.
    private void UpdateInputField()
    {
        inputField.text = userInput;

        // Cambia el color del texto dependiendo de la longitud ingresada.
        if (userInput.Length == 4)
        {
            inputField.color = submitColor; // Verde cuando está listo para enviar.
        }
        else
        {
            inputField.color = defaultColor; // Gris mientras se escribe.
        }
    }
}
