using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, InputSystem_Actions.IUIActions
{
    [SerializeField] private InputSystem_Actions inputActions;
    private Vector2 mousePosition;
    public void OnEnable()
    {

        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
        }
        inputActions.UI.SetCallbacks(this);
        inputActions.UI.Enable();
    }
    public void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.UI.Disable();
        }
    }
    public void OnCancel(InputAction.CallbackContext context)
    {

    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            GameManager.instance.DropIngredient();
        }

    }

    public void OnMiddleClick(InputAction.CallbackContext context)
    {

    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
    }

    public void OnPoint(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            CursorManager.instance.cursorPosition = context.ReadValue<Vector2>();


        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {

    }

    public void OnScrollWheel(InputAction.CallbackContext context)
    {

    }

    public void OnSubmit(InputAction.CallbackContext context)
    {

    }

    public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
    {

    }

    public void OnTrackedDevicePosition(InputAction.CallbackContext context)
    {

    }


}
