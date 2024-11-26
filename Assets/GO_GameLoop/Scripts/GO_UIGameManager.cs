using StarterAssets;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private char[] userInput; // Array para manejar el input del usuario.
    private GO_PlayerUIManager uiplayer;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        
    }

    // Muestra el panel para ingresar el c�digo de la puerta.
    public void ShowCodePanel()
    {
        // Inicializar el array del input con los valores mostrados en displayedCode.
        userInput = GO_CodeManager.displayedCode.ToCharArray();
        UpdateInputField();
        inputField.text = GO_CodeManager.displayedCode;
        codePanel.SetActive(true);
    }

    // Oculta el panel de la puerta y limpia la entrada.
    public void HideCodePanel()
    {
        codePanel.SetActive(false);
        ClearInput();
    }

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

    // A�ade un n�mero al campo de texto desde la UI.
    public void AddNumberToInput(string number)
    {
        for (int i = 0; i < userInput.Length; i++)
        {
            // Encuentra la primera posici�n vac�a ('_') y reempl�zala con el n�mero.
            if (userInput[i] == '_')
            {
                userInput[i] = number[0];
                UpdateInputField();
                return;
            }
        }
    }

    // Limpia el campo de texto.
    public void ClearInput()
    {
        // Resetea las posiciones que no tienen n�mero predefinido a '_'.
        for (int i = 0; i < userInput.Length; i++)
        {
            if (GO_CodeManager.displayedCode[i] == '_')
            {
                userInput[i] = '_';
            }
        }
        UpdateInputField();
    }

    // Valida el c�digo ingresado.
    public void SubmitCode()
    {
        string finalInput = new string(userInput);

        if (finalInput.Contains("_"))
        {
            Debug.Log("Debe completar el c�digo antes de enviarlo.");
            return;
        }

        // Encuentra el `GO_CodeManager` correspondiente.
        GO_CodeManager codeManager = FindObjectOfType<GO_CodeManager>(); // Encuentra el manager en la escena actual.

        if (codeManager == null)
        {
            Debug.LogError("No se encontr� un GO_CodeManager en esta escena.");
            return;
        }

        if (codeManager.ValidateCode(finalInput))
        {
            Debug.Log("�C�digo correcto! Abriendo la puerta...");
            GO_InputsPlayer.IsPause = false;
            uiplayer = FindObjectOfType<GO_PlayerUIManager>();
            HideCodePanel();
            uiplayer.RemoveBooksNumber();
            StartCoroutine(OpenDoor());
        }
        else
        {
            Debug.Log("C�digo incorrecto.");
        }
    }


    private IEnumerator OpenDoor()
    {
        Debug.Log("Abriendo puerta");
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
        inputField.text = new string(userInput);

        // Cambia el color del texto dependiendo de la longitud ingresada.
        if (!new string(userInput).Contains("_"))
        {
            inputField.color = submitColor; // Verde cuando est� listo para enviar.
        }
        else
        {
            inputField.color = defaultColor; // Gris mientras se escribe.
        }
    }

}
