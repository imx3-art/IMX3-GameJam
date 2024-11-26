using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
#endif

namespace StarterAssets
{
    public class GO_InputsPlayer : StarterAssetsInputs
    {
        [Header("Custom Input Values")]
        public bool stealth;
        public bool Grab;
        public bool drag;
        public bool pull;
        public bool interact;
        public bool grabDropItem;
        public static bool IsPause = false;
        public bool nextLevel;

        public event System.Action onInteract;

#if ENABLE_INPUT_SYSTEM
        public new void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }
        public new void OnLook(InputValue value)
        {
            LookInput(value.Get<Vector2>());            
        }
        public void OnStealth(InputValue value)
        {
            StealthInput(value.isPressed);
        }
        public void OnDrag(InputValue value)
        {
            DragInput(value.isPressed);
        }
        public void OnPull(InputValue value)
        {
            PullInput(value.isPressed);
        }
        public void OnGrabDropItem(InputValue value)
        {
            GrabDropItemInput(value.isPressed);
        }
        public void OnGrab(InputValue value)
        {
            GrabInput(value.isPressed);
        }
        public void OnInteract(InputValue value)
        {
            ToggleInteract();
        }

        public void OnNextLevel(InputValue value)
        {
            NextLevelInput(value.isPressed);
        }
#endif
        private void ToggleInteract()
        {
            interact = !interact; // Invierte el estado actual.
            Debug.Log($"Interactuar est� ahora: {interact}");
            onInteract?.Invoke(); // Dispara el evento de interacci�n.
        }
        public void StealthInput(bool newStealthState)
        {
            stealth = newStealthState;
        }
        public void PullInput(bool newStealthState)
        {
            pull = newStealthState;
            if (GO_PlayerNetworkManager.localPlayer.isDrag > 0 && newStealthState)
            {
                GO_PlayerNetworkManager.localPlayer.pullMiniGame++;
            }

        }
         public void GrabDropItemInput(bool newStealthState)
        {
            
            grabDropItem = newStealthState;
            
        }

        public void DragInput(bool newDragthState)
        {
            if (!newDragthState)
            {
                return;
            }

            if (drag) 
            {
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
            if (GO_PlayerNetworkManager.localPlayer.isDrag > 0)
            {
                //GO_PlayerNetworkManager.localPlayer.movePlayerNetwork = newMoveDirection;
                move = Vector2.zero;
            }
            else
            {
                //GO_PlayerNetworkManager.localPlayer.movePlayerNetwork = Vector2.zero;
                move.x = IsPause ? Vector2.zero.x : newMoveDirection.x;
                move.y = IsPause ? Vector2.zero.y : newMoveDirection.y;
            }
        }

        public new void LookInput(Vector2 newLookDirection)
        {
            look.x = IsPause ? Vector2.zero.x : newLookDirection.x;
            look.y = IsPause ? Vector2.zero.y : newLookDirection.y;
        }
        public void SetCursorState(bool newState)
        {
            Cursor.lockState = (cursorLocked = newState) ? CursorLockMode.Locked : CursorLockMode.None;
        }
        
        public void NextLevelInput(bool newState)
        {
            GO_LevelManager.instance.HandlePlayerEnterArea();
        }
    }
}