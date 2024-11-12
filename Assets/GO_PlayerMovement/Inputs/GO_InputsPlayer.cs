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
#endif
        public void StealthInput(bool newStealthState)
        {
            stealth = newStealthState;
        }

        public void DragInput(bool newDragthState)
        {
            if(drag) 
            {
                GO_PlayerNetworkManager.localPlayer.EnddragMode(true);
            }
            drag = newDragthState;
            //GO_PlayerNetworkManager.localPlayer.isDrag = (short)(drag ? 1 : 0);
        }

        public new void MoveInput(Vector2 newMoveDirection)
        {
            if (GO_PlayerNetworkManager.localPlayer.isDrag == 1)
            {
                GO_PlayerNetworkManager.localPlayer.movePlayerNetwork = newMoveDirection;
            }
            else
            {
                move.x = IsPause ? Vector2.zero.x : newMoveDirection.x;
                move.y = IsPause ? Vector2.zero.y : newMoveDirection.y;
            }
        }

        private void Update()
        {
            if (GO_PlayerNetworkManager.localPlayer.isDrag == 1)
            {
                Debug.Log("Revisando vector: " + GO_PlayerNetworkManager.localPlayer.movePlayerNetwork);
                if (GO_PlayerNetworkManager.localPlayer.otherPlayerTarget != null)
                {
                    Vector2 newMoveDirection = GO_PlayerNetworkManager.localPlayer.movePlayerNetwork + GO_PlayerNetworkManager.localPlayer.otherPlayerTarget.movePlayerNetwork;
                    move.x = IsPause ? Vector2.zero.x : newMoveDirection.x;
                    move.y = IsPause ? Vector2.zero.y : newMoveDirection.y;
                }
                
            }
        }
    }
}