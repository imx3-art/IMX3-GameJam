using StarterAssets;
using System;
using System.Collections;
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

    [SerializeField] private TMP_Text LoreText; // Texto que se mostrará en el libro.

    [Header("UI Colors")]
    public Color defaultColor = Color.gray; // Color para los números.
    public Color deleteColor = Color.red;  // Color del botón de borrar.
    public Color submitColor = Color.green; // Color del botón de enviar.

    [Header("Door Animation")]
    [SerializeField] private GameObject doorLeft;
    [SerializeField] private GameObject doorRight;

    public float doorMoveDistance = 2f; // Distancia que las puertas deben moverse.
    public float doorMoveSpeed = 2f; // Velocidad de apertura.

    private char[] userInput; // Array para manejar el input del usuario.
    private float delayBeforeClosing = 1f;

    private float _fov;
    private bool isShaking;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        _fov = GO_MainCamera.MainCamera.fieldOfView;
    }

    private void Update()
    {
        if (isShaking)
        {
            ShakeCamera(false);
        }
    }

    // Muestra el panel para ingresar el código de la puerta.
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

    // Oculta el panel de información del librito.
    public void HideBookPanel()
    {
        bookPanel.SetActive(false);
    }

    // Añade un número al campo de texto desde la UI.
    public void AddNumberToInput(string number)
    {
        for (int i = 0; i < userInput.Length; i++)
        {
            // Encuentra la primera posición vacía ('_') y reemplázala con el número.
            if (userInput[i] == '_')
            {
                if (GO_AudioManager.Instance != null)
                {
                    GO_AudioManager.Instance.PlayUISound("GO_Numbers_Panel");
                }
                userInput[i] = number[0];
                UpdateInputField();
                return;
            }
        }
    }

    // Limpia el campo de texto.
    public void ClearInput()
    {
        if (userInput != null)
        {
            // Resetea las posiciones que no tienen número predefinido a '_'.
            for (int i = 0; i < userInput.Length; i++)
            {
                if (GO_CodeManager.displayedCode[i] == '_')
                {
                    userInput[i] = '_';
                }
            }
            if (GO_AudioManager.Instance != null)
            {
                GO_AudioManager.Instance.PlayUISound("GO_Clean_Password");
            }
            UpdateInputField();
        }
    }

    // Valida el código ingresado.
    public void SubmitCode()
    {
        string finalInput = new string(userInput);

        if (finalInput.Contains("_"))
        {
            Debug.Log("Debe completar el código antes de enviarlo.");
            return;
        }

        // Encuentra el `GO_CodeManager` correspondiente.
        GO_CodeManager codeManager = FindObjectOfType<GO_CodeManager>(); // Encuentra el manager en la escena actual.

        if (codeManager == null)
        {
            Debug.LogError("No se encontró un GO_CodeManager en esta escena.");
            return;
        }

        if (codeManager.ValidateCode(finalInput))
        {
            if (GO_AudioManager.Instance != null)
            {
                GO_AudioManager.Instance.PlayUISound("GO_Correct_Password");
            }
            Debug.Log("¡Código correcto! Abriendo la puerta...");
            GO_InputsPlayer.IsPause = false;
            HideCodePanel();
            //StartCoroutine(OpenDoor());
            GO_LevelManager.instance.RPC_OpenFinalDoor();
        }
        else
        {
            if (GO_AudioManager.Instance != null)
            {
                GO_AudioManager.Instance.PlayUISound("GO_Wrong_Password");
            }
            Debug.Log("Código incorrecto.");
        }
    }


    public IEnumerator OpenDoor()
    {
        Debug.Log("Abriendo puerta");
        if (GO_AudioManager.Instance != null)
        {
            GO_AudioManager.Instance.PlayUISound("GO_Open_Door_Lab");
        }

        Vector3 leftStartPosition = doorLeft.transform.position;
        Vector3 rightStartPosition = doorRight.transform.position;

        Vector3 leftEndPosition = leftStartPosition + Vector3.left * doorMoveDistance;
        Vector3 rightEndPosition = rightStartPosition + Vector3.right * doorMoveDistance;

        float elapsedTime = 0f;

        isShaking = true;

        while (elapsedTime < doorMoveDistance / doorMoveSpeed)
        {
            doorLeft.transform.position = Vector3.Lerp(leftStartPosition, leftEndPosition, elapsedTime * doorMoveSpeed / doorMoveDistance);
            doorRight.transform.position = Vector3.Lerp(rightStartPosition, rightEndPosition, elapsedTime * doorMoveSpeed / doorMoveDistance);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        isShaking = false;
        ShakeCamera();
        doorLeft.transform.position = leftEndPosition;
        doorRight.transform.position = rightEndPosition;

        Debug.Log("¡Puerta abierta!");
        // Espera antes de cerrar la puerta
        yield return new WaitForSeconds(delayBeforeClosing);
        // Inicia la corrutina para cerrar la puerta
        StartCoroutine(CloseDoor(leftStartPosition, rightStartPosition));
    }

    public void ShakeCamera(bool _value = true)
    {
        GO_MainCamera.cinemachineBrain.enabled = _value;
        GO_MainCamera.MainCamera.fieldOfView = Mathf.Lerp(GO_MainCamera.MainCamera.fieldOfView, UnityEngine.Random.Range(_fov * .95f, _fov * 1.35f), Time.deltaTime * 1);
    }

    public IEnumerator CloseDoor(Vector3 leftStartPosition, Vector3 rightStartPosition)
    {
        Debug.Log("Cerrando puerta");

        if (GO_AudioManager.Instance != null)
        {
            GO_AudioManager.Instance.PlayUISound("GO_Close_Door_Lab");
        }

        Vector3 leftEndPosition = doorLeft.transform.position;
        Vector3 rightEndPosition = doorRight.transform.position;

        float elapsedTime = 0f;

        while (elapsedTime < doorMoveDistance / doorMoveSpeed)
        {
            doorLeft.transform.position = Vector3.Lerp(leftEndPosition, leftStartPosition, elapsedTime * doorMoveSpeed / doorMoveDistance);
            doorRight.transform.position = Vector3.Lerp(rightEndPosition, rightStartPosition, elapsedTime * doorMoveSpeed / doorMoveDistance);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        doorLeft.transform.position = leftStartPosition;
        doorRight.transform.position = rightStartPosition;

        Debug.Log("¡Puerta cerrada!");
    }

    // Actualiza el campo de entrada visualmente.
    private void UpdateInputField()
    {
        inputField.text = new string(userInput);

        // Cambia el color del texto dependiendo de la longitud ingresada.
        if (!new string(userInput).Contains("_"))
        {
            inputField.color = submitColor; // Verde cuando está listo para enviar.
        }
        else
        {
            inputField.color = defaultColor; // Gris mientras se escribe.
        }
    }

}
