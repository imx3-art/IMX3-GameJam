using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using StarterAssets;
using TMPro;

public class GO_PlayerUIManager : MonoBehaviour
{
    public static GO_PlayerUIManager instance;

    [Header("Vidas UI")]
    public Transform livesPanel; // Panel donde se agregarán las imágenes de las vidas
    public GameObject lifePrefab; // Prefab de la vida (debe ser un GameObject con un componente Image)
    public Sprite fullLifeSprite; // Sprite para una vida completa
    public Sprite emptyLifeSprite; // Sprite para una vida vacía

    [Header("Stamina UI")]
    public GameObject staminaPanel;
    public Image staminaBar; // La imagen de la barra de stamina

    [Header("Secret Code")]
    [SerializeField] private Transform bookNumbersContainer; // Contenedor en el HUD
    [SerializeField] private GameObject bookNumberPrefab; 
    
    [Header("Books UI")]
    public TextMeshProUGUI booksCounterText; // Texto UI para mostrar el contador
    private int totalBooksInMap = 0;
    private int collectedBooks = 0;// Prefab del número del libro

    [Header("Session Info")]
    [SerializeField] private TextMeshProUGUI sessionPlayersCount;
    [SerializeField] private TextMeshProUGUI popUpSharedCodeSessionName;
    [SerializeField] private TextMeshProUGUI armsCount;
    [SerializeField] private GameObject[] armsState;
    [SerializeField] GameObject popUpSharedCode;
    [SerializeField] CanvasGroup popUpShowMsj;


    private GO_PlayerNetworkManager playerNetworkManager;
    public GO_ThirdPersonController controller;

    private int totalLives = 0;
    private List<Image> heartImages = new List<Image>();

    private float previousStamina;
    private Coroutine hideStaminaPanelCoroutine;

    private PlayerState currentPlayerState;
    private Coroutine _coroutineShowMsj;
    private Coroutine _coroutineHideMsj;
    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (GO_LevelManager.instance != null &&
               GO_LevelManager.instance.isReady)
            {

                if (GO_PlayerNetworkManager.localPlayer != null)
                {

                    playerNetworkManager = GO_PlayerNetworkManager.localPlayer;
                    controller = GO_PlayerNetworkManager.localPlayer.playerTransform.GetComponent<GO_ThirdPersonController>();

                    GO_LevelManager.instance.OnLivesChanged += UpdateLivesUI;
                    controller.OnStaminaChanged += UpdateStaminaUI;
                    GO_PlayerNetworkManager.localPlayer.inputPlayer.onShowShared += ShowCodeSession;
                    GO_PlayerNetworkManager.localPlayer.actionPlayer.onChangeArms += ShowArms;
                    GO_PlayerNetworkManager.localPlayer.inputPlayer.onShowMsj += ShowMsj;

                    previousStamina = controller.Stamina;


                    playerNetworkManager.OnPlayerStateChanged += OnPlayerStateChanged;
                    GO_RunnerManager.Instance.OnEventTriggeredPlayerChange += ChangePlayerNumber;

                    totalLives = GO_LevelManager.instance.totalLives;

                    for (int i = 0; i < totalLives; i++)
                    {
                        GameObject lifeImage = Instantiate(lifePrefab, livesPanel);
                        Image heartImage = lifeImage.GetComponent<Image>();
                        heartImage.sprite = fullLifeSprite;
                        heartImages.Add(heartImage);
                    }

                    // Inicializar la UI
                    UpdateLivesUI(playerNetworkManager.playerLives);
                    UpdateStaminaUI(controller.Stamina);
                    break;
                }
                else
                {
                    Debug.LogError("Local player no está asignado en GO_PlayerNetworkManager.");
                }

            }
        }
        ChangePlayerNumber();
    }

    private void OnDestroy()
    {
        // Desuscribirse de eventos
        if (GO_LevelManager.instance != null)
        {
            GO_LevelManager.instance.OnLivesChanged -= UpdateLivesUI;
        }

        if (controller != null)
        {
            controller.OnStaminaChanged -= UpdateStaminaUI;
        }

        if (playerNetworkManager != null)
        {
            playerNetworkManager.OnPlayerStateChanged -= OnPlayerStateChanged;
        }
        if (GO_RunnerManager.Instance != null)
        {
            GO_RunnerManager.Instance.OnEventTriggeredPlayerChange -= ChangePlayerNumber;
        }

        GO_PlayerNetworkManager.localPlayer.inputPlayer.onShowShared -= ShowCodeSession;
        GO_PlayerNetworkManager.localPlayer.actionPlayer.onChangeArms -= ShowArms;
        GO_PlayerNetworkManager.localPlayer.inputPlayer.onShowMsj -= ShowMsj;
    }

    private void UpdateLivesUI(float currentLives)
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentLives)
            {
                heartImages[i].sprite = fullLifeSprite; // Vida llena
            }
            else
            {
                heartImages[i].sprite = emptyLifeSprite; // Vida vacía
            }
        }
    }

    public void ShowCodeSession()
    {
        popUpSharedCodeSessionName.text = GO_RunnerManager.Instance._runner.SessionInfo.Name;
        popUpSharedCode.SetActive(!popUpSharedCode.activeSelf);
        GO_PlayerNetworkManager.localPlayer.inputPlayer.SetCursorState(!popUpSharedCode.activeSelf);
    }
    private void ShowArms()
    {
        StartCoroutine(ShowArmsThred());
    }
    private IEnumerator ShowArmsThred()
    {
        yield return new WaitForSeconds(.5f);
        armsCount.text = GO_PlayerNetworkManager.localPlayer.actionPlayer.CountArms() + "/3";
        foreach (GameObject img in armsState)
        {
            img.SetActive(false);
        }
        armsState[GO_PlayerNetworkManager.localPlayer.actionPlayer.CountArms()].SetActive(true);
    }

    private void ShowMsj(int _value)
    {

        switch (_value)
        {
            case -1:
            case 0:
                if (popUpShowMsj.gameObject.activeSelf || _value == -1)
                {
                    if (_coroutineShowMsj != null)
                    {
                        StopCoroutine(_coroutineShowMsj);
                        _coroutineShowMsj = null;
                    }
                    _coroutineHideMsj = StartCoroutine(HideMsjCoroutine());
                }
                else
                {
                    if (_coroutineHideMsj != null)
                    {
                        StopCoroutine(_coroutineHideMsj);
                        _coroutineHideMsj = null;
                    }
                    _coroutineShowMsj = StartCoroutine(ShowMsjCoroutine());
                }
                return;
            case 1:
                GO_PlayerNetworkManager.localPlayer.RPC_Gesture("GO_BeCareful");                                    
                break;
            case 2:
                GO_PlayerNetworkManager.localPlayer.RPC_Gesture("GO_Close");
                break; 
            case 3:
                GO_PlayerNetworkManager.localPlayer.RPC_Gesture("GO_Help");
                break; 
            case 4:
                GO_PlayerNetworkManager.localPlayer.RPC_Gesture("GO_GiveMe");
                break;
            }
        ShowMsj(-1);
    }

    IEnumerator ShowMsjCoroutine()
    {
        popUpShowMsj.alpha = 0;
        GO_PlayerNetworkManager.localPlayer.msjGesture.HideGesture();

        popUpShowMsj.gameObject.SetActive(true);
        do
        {
            popUpShowMsj.alpha = Mathf.Lerp(popUpShowMsj.alpha, 1, Time.deltaTime * 4);
            yield return null;
        } while (popUpShowMsj.alpha < .95f);

        yield return new WaitForSeconds(2);
        _coroutineShowMsj = null;
        StartCoroutine(HideMsjCoroutine());

    }
    IEnumerator HideMsjCoroutine()
    {
        //popUpShowMsj.alpha = 1;
        if (popUpShowMsj.gameObject.activeSelf)
        {
            do
            {
                popUpShowMsj.alpha = Mathf.Lerp(popUpShowMsj.alpha, 0, Time.deltaTime * 4);
                yield return null;
            } while (popUpShowMsj.alpha > .05f);
            popUpShowMsj.gameObject.SetActive(false);
        }
        _coroutineHideMsj = null;
    }

    private void UpdateStaminaUI(float currentStamina)
    {
        float normalizedStamina = Mathf.InverseLerp(controller.MinStamina, controller.MaxStamina, currentStamina);
        staminaBar.fillAmount = normalizedStamina;

        if (currentPlayerState == PlayerState.Persecution)
        {
            if (!staminaPanel.activeSelf)
            {
                staminaPanel.SetActive(true);
            }
        }
        else
        {
            if (currentStamina >= controller.MaxStamina)
            {
                if (staminaPanel.activeSelf)
                {
                    staminaPanel.SetActive(false);
                }
            }
            else
            {
                if (!staminaPanel.activeSelf)
                {
                    staminaPanel.SetActive(true);
                }
            }
        }
    }

    private IEnumerator HideStaminaPanelAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        staminaPanel.SetActive(false);
        hideStaminaPanelCoroutine = null;
    }

    private void OnPlayerStateChanged(PlayerState newState)
    {
        currentPlayerState = newState;

        if (currentPlayerState == PlayerState.Persecution)
        {
            if (!staminaPanel.activeSelf)
            {
                staminaPanel.SetActive(true);
            }
        }
        else
        {
            if (controller.Stamina >= controller.MaxStamina)
            {
                if (staminaPanel.activeSelf)
                {
                    staminaPanel.SetActive(false);
                }
            }
            else
            {
                if (!staminaPanel.activeSelf)
                {
                    staminaPanel.SetActive(true);
                }
            }
        }
    }
    private void ChangePlayerNumber()
    {
        sessionPlayersCount.text = GO_RunnerManager.Instance._runner.SessionInfo.PlayerCount + "/6";
    }

    public void AddBookNumber(string number, Color color)
    {
        // Verificar si el número ya existe en el HUD
        foreach (Transform child in bookNumbersContainer)
        {
            // Acceder al texto directamente usando GetChild
            TextMeshProUGUI existingNumberText = child.GetChild(1).GetComponent<TextMeshProUGUI>(); // Asumiendo que el texto es el segundo hijo
            Color ColorFromExistinNumber = child.GetChild(0).GetComponent<Image>().color;
            if (existingNumberText != null && existingNumberText.text == number && color == ColorFromExistinNumber)
            {
                return; // Salir del método si el número ya existe
            }
        }

        // Instanciar el prefab
        GameObject newBookNumber = Instantiate(bookNumberPrefab, bookNumbersContainer);

        // Configurar el fondo y el texto
        Transform backgroundTransform = newBookNumber.transform.GetChild(0); // Primer hijo: fondo
        Transform textTransform = newBookNumber.transform.GetChild(1);      // Segundo hijo: texto

        Image background = backgroundTransform.GetComponent<Image>();
        TextMeshProUGUI numberText = textTransform.GetComponent<TextMeshProUGUI>();

        background.color = color;
        numberText.text = number;

        // Organizar manualmente hacia la derecha
        int childCount = bookNumbersContainer.childCount;
        float spacing = 120f; // Espaciado entre elementos
        newBookNumber.transform.localPosition = new Vector3(spacing * (childCount - 2), 0, 0);
    }

    public void RemoveBooksNumber()
    {
        foreach (Transform child in bookNumbersContainer)
        {
            // Destruye el objeto hijo
            Destroy(child.gameObject);
        }
    }

    public void UpdateBooks()
    {
        totalBooksInMap = FindObjectsOfType<GO_InteractableBook>().Length;
        UpdateBooksCounterUI();
    }
    public void BookCollected()
    {
        collectedBooks++;
        UpdateBooksCounterUI();
    }
    private void UpdateBooksCounterUI()
    {
        int booksRemaining = totalBooksInMap - collectedBooks;
        booksCounterText.text = $"{booksRemaining}";
    }
    private void OnEnable()
    {
        SpawnCodeManager.OnCodeManagerReady += OnPlayerChangeScene;
    }

    private void OnDisable()
    {
        SpawnCodeManager.OnCodeManagerReady -= OnPlayerChangeScene;
    }
    private void OnPlayerChangeScene()
    {
        StartCoroutine(UpdateBooksAfterDelay());
    }
    
    private IEnumerator UpdateBooksAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        GO_InteractableBook[] allBooksInScene = FindObjectsOfType<GO_InteractableBook>(true);

        List<GO_InteractableBook> activeBooks = new List<GO_InteractableBook>();

        foreach (GO_InteractableBook book in allBooksInScene)
        {
            if (book.gameObject.activeInHierarchy)
            {
                activeBooks.Add(book);
            }
        }

        totalBooksInMap = activeBooks.Count;
    
        collectedBooks = 0;

        UpdateBooksCounterUI();
    }
}
