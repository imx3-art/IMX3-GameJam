using StarterAssets;
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
    public TMP_Text bookText; // Texto para mostrar el n�mero del librito.
    public TMP_Text inputField; // Campo de texto para ingresar el c�digo.

    [SerializeField] private TMP_Text LoreText; // Texto que se mostrar� en el libro.

    [Header("UI Colors")]
    public Color defaultColor = Color.gray; // Color para los n�meros.
    public Color deleteColor = Color.red;  // Color del bot�n de borrar.
    public Color submitColor = Color.green; // Color del bot�n de enviar.

    [Header("Door Animation")]
    [SerializeField] private GameObject doorLeft;
    [SerializeField] private GameObject doorRight;

    public float doorMoveDistance = 2f; // Distancia que las puertas deben moverse.
    public float doorMoveSpeed = 2f; // Velocidad de apertura.

    private string userInput = "";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Muestra el panel para ingresar el c�digo de la puerta.
    public void ShowCodePanel()
    {
        inputField.text = GO_CodeManager.displayedCode;
        codePanel.SetActive(true);
    }

    // Oculta el panel de la puerta y limpia la entrada.
    public void HideCodePanel()
    {
        codePanel.SetActive(false);
        ClearInput();
    }

    // Muestra el n�mero del librito en el panel.
    public void ShowBookNumber(string number, Color positionColor, string textLore)
    {
        LoreText.text = textLore;
        bookText.text = number.ToString();
        bookText.color = positionColor;
        bookPanel.SetActive(true);
    }

    // Oculta el panel de informaci�n del librito.
    public void HideBookPanel()
    {
        bookPanel.SetActive(false);
    }

    public void SetCodeField(string codePart)
    {
        inputField.text = codePart;
    }

    // A�ade un n�mero al campo de texto desde la UI.
    public void AddNumberToInput(string number)
    {
        if (userInput.Length < 4) // L�mite de 4 d�gitos.
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

    // Valida el c�digo ingresado.
    public void SubmitCode()
    {
        if (userInput.Length != 4)
        {
            Debug.Log("Debe ingresar un c�digo de 4 d�gitos.");
            return;
        }

        if (GO_CodeManager.Instance.ValidateCode(userInput))
        {
            Debug.Log("�C�digo correcto! Abriendo la puerta...");
            GO_InputsPlayer.IsPause = false;
            HideCodePanel();

            StartCoroutine(OpenDoor());
        }
        else
        {
            Debug.Log("C�digo incorrecto.");
        }
    }

    private IEnumerator OpenDoor()
    {
        Vector3 leftStartPosition = doorLeft.transform.position;
        Vector3 rightStartPosition = doorRight.transform.position;

        // Calcula las posiciones finales.
        Vector3 leftEndPosition = leftStartPosition + Vector3.left * doorMoveDistance;
        Vector3 rightEndPosition = rightStartPosition + Vector3.right * doorMoveDistance;

        float elapsedTime = 0f;

        while (elapsedTime < doorMoveDistance / doorMoveSpeed)
        {
            // Mueve las puertas hacia sus posiciones finales.
            doorLeft.transform.position = Vector3.Lerp(leftStartPosition, leftEndPosition, elapsedTime * doorMoveSpeed / doorMoveDistance);
            doorRight.transform.position = Vector3.Lerp(rightStartPosition, rightEndPosition, elapsedTime * doorMoveSpeed / doorMoveDistance);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Aseg�rate de que las puertas terminen exactamente en sus posiciones finales.
        doorLeft.transform.position = leftEndPosition;
        doorRight.transform.position = rightEndPosition;

        Debug.Log("�Puerta abierta!");
    }

    // Actualiza el campo de entrada visualmente.
    private void UpdateInputField()
    {
        inputField.text = userInput;

        // Cambia el color del texto dependiendo de la longitud ingresada.
        if (userInput.Length == 4)
        {
            inputField.color = submitColor; // Verde cuando est� listo para enviar.
        }
        else
        {
            inputField.color = defaultColor; // Gris mientras se escribe.
        }
    }
}
