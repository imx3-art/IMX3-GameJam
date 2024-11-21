using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using StarterAssets;

public class GO_PlayerUIManager : MonoBehaviour
{
    [Header("Vidas UI")]
    public Transform livesPanel; // Panel donde se agregarán las imágenes de las vidas
    public GameObject lifePrefab; // Prefab de la vida (debe ser un GameObject con un componente Image)
    public Sprite fullLifeSprite; // Sprite para una vida completa
    public Sprite emptyLifeSprite; // Sprite para una vida vacía

    [Header("Stamina UI")]
    public Image staminaBar; // La imagen de la barra de stamina

    private GO_PlayerNetworkManager playerNetworkManager;
    public GO_ThirdPersonController controller;

    private int totalLives = 0;
    private List<Image> heartImages = new List<Image>();

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if(GO_LevelManager.instance != null &&
               GO_LevelManager.instance.isReady)
            {
                
                if (GO_PlayerNetworkManager.localPlayer != null)
                {
                    playerNetworkManager = GO_PlayerNetworkManager.localPlayer;
                    controller = GO_PlayerNetworkManager.localPlayer.playerTransform.GetComponent<GO_ThirdPersonController>();

                    GO_LevelManager.instance.OnLivesChanged += UpdateLivesUI;
                    controller.OnStaminaChanged += UpdateStaminaUI;
                    
                    
                    totalLives = GO_LevelManager.instance.totalLives; 
                    Debug.Log("vidas actuales"+totalLives);
                    
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


    private void UpdateStaminaUI(float currentStamina)
    {
        // Normalizar la stamina actual entre 0 y 1
        float normalizedStamina = Mathf.InverseLerp(controller.MinStamina, controller.MaxStamina, currentStamina);

        // Actualizar el valor de llenado de la imagen
        staminaBar.fillAmount = normalizedStamina;
    }
}
