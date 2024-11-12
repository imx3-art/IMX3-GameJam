using StarterAssets;
using UnityEngine;

public class GO_PlayerActions : MonoBehaviour
{
    public float maxDistance = 10f;
    public float maxUpDistance = 1.25f;
    public GO_PlayerNetworkManager otherPlayerNetworkManager;
    public LayerMask layerMask;
    GO_InputsPlayer inputPlayer;

    private void Start()
    {
        inputPlayer = GetComponent<GO_InputsPlayer>();
    }

    void Update()
    {
        if (inputPlayer.drag)
        {
            if (otherPlayerNetworkManager)
            {
                MiniGameDrag();                
            }

            Debug.DrawLine(transform.position + transform.up * maxUpDistance, transform.up * maxUpDistance + transform.position + transform.forward, Color.red);

            if (Physics.Raycast(transform.position + transform.up * maxUpDistance, transform.forward, out RaycastHit hitInfo, maxDistance, layerMask))
            {
                Debug.Log("Objeto detectado: " + hitInfo.collider.gameObject.name);
                Debug.Log("Distancia al objeto: " + hitInfo.distance);
                if (otherPlayerNetworkManager == null)
                {
                    otherPlayerNetworkManager = hitInfo.collider.gameObject.GetComponentInParent<GO_PlayerNetworkManager>();
                    GO_PlayerNetworkManager.localPlayer.StartdragMode(true);

                }
            }
            else
            {
                otherPlayerNetworkManager = null;
                GO_PlayerNetworkManager.localPlayer.EnddragMode(true);
            }
        }
    }

    private void MiniGameDrag()
    {

    }
}