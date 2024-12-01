using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    public class GO_TutorialTrigger : MonoBehaviour
    {
        [Header("Tutorial Settings")] [Tooltip("Referencia al Canvas del tutorial")]
        public GameObject tutorialCanvas;

        [Tooltip("Tecla para cerrar el tutorial")]
        public KeyCode closeKey = KeyCode.F;

        private bool isPlayerInside = false;

        void Start()
        {
            if (tutorialCanvas == null)
            {
                Debug.LogError("Tutorial Canvas no está asignado en el Inspector.");
            }
            else
            {
                tutorialCanvas.SetActive(false);
            }
        }

        void Update()
        {
            if (isPlayerInside && tutorialCanvas.activeSelf && Input.GetKeyDown(closeKey))
            {
                CloseTutorial();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (other.GetComponentInParent<GO_PlayerNetworkManager>().isLocalPlayer)
                {
                    GO_InputsPlayer.IsPause = true;
                    isPlayerInside = true;
                    OpenTutorial();
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (other.GetComponentInParent<GO_PlayerNetworkManager>().isLocalPlayer)
                {
                    isPlayerInside = false;
                }
            }
        }

        void OpenTutorial()
        {
            if (tutorialCanvas != null)
            {
                tutorialCanvas.SetActive(true);
            }
        }

        void CloseTutorial()
        {
            if (tutorialCanvas != null)
            {
                GO_InputsPlayer.IsPause = false;
                tutorialCanvas.SetActive(false);
            }

            Destroy(gameObject);
        }
    }
}