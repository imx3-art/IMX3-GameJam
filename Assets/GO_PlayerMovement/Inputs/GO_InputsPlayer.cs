using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class GO_InputsPlayer : StarterAssetsInputs
    {
        [Header("Custom Input Values")]
        public bool stealth;
        public bool Grab;
        public bool drag;
        public static bool IsPause = false;

#if ENABLE_INPUT_SYSTEM
        public new void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }
        public void OnStealth(InputValue value)
        {
            StealthInput(value.isPressed);
        }
        public void OnDrag(InputValue value)
        {
            DragInput(value.isPressed);
        }

        public void OnGrab(InputValue value)
        {
            GrabInput(value.isPressed);
        }
#endif
        public void StealthInput(bool newStealthState)
        {
            stealth = newStealthState;
        }

        public void DragInput(bool newDragthState)
        {
            Debug.Log("***EL Drag esta: " + drag);
            if(drag) 
            {
                GO_PlayerNetworkManager.localPlayer.EnddragMode(true);
                move = Vector2.zero;
            }
            drag = newDragthState;
            //GO_PlayerNetworkManager.localPlayer.isDrag = (short)(drag ? 1 : 0);
        }

        public void GrabInput(bool newGrabState)
        {
            Grab = newGrabState;
        }

        public new void MoveInput(Vector2 newMoveDirection)
        {
            if (GO_PlayerNetworkManager.localPlayer.isDrag == 1)
            {
                GO_PlayerNetworkManager.localPlayer.movePlayerNetwork = newMoveDirection;
            }
            else
            {
                GO_PlayerNetworkManager.localPlayer.movePlayerNetwork = Vector2.zero;
                move.x = IsPause ? Vector2.zero.x : newMoveDirection.x;
                move.y = IsPause ? Vector2.zero.y : newMoveDirection.y;
            }
        }
        Vector2 moveTMP;
        private void Update()
        {
            if (GO_PlayerNetworkManager.localPlayer.isDrag == 1)
            {
                Debug.Log("****Revisando vector: " + GO_PlayerNetworkManager.localPlayer.movePlayerNetwork);
                if (GO_PlayerNetworkManager.localPlayer.otherPlayerTarget != null)
                {
                    Vector2 newMoveDirection = GO_PlayerNetworkManager.localPlayer.movePlayerNetwork + GO_PlayerNetworkManager.localPlayer.otherPlayerTarget.movePlayerNetwork;
                    moveTMP = Vector2.Lerp(moveTMP, newMoveDirection, Time.deltaTime );
                    move.x = IsPause ? Vector2.zero.x : moveTMP.x / 2;
                    move.y = IsPause ? Vector2.zero.y : moveTMP.y / 2;
                }                
            }
            else if(moveTMP != Vector2.zero)
            {
                move = moveTMP = Vector2.zero;
            }
        }
    }
}