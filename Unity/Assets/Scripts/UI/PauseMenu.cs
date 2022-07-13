using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance => instance;

    public bool IsPaused
    {
        get => isPaused;
        set
        {
            GameEvents.PauseMenuToggleInvoke(value);
            isPaused = value;
        }
    }

    private PlayerInputActions playerInputActions;
    private InputAction pause, select, confirm;

    private bool isPaused;

    private Transform[] childrenTransforms;
    
    private List<TextButton> activeMenuButtons = new List<TextButton>();
    private int currentButtonIndex = -1;

    private static PauseMenu instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        playerInputActions = new PlayerInputActions();
        childrenTransforms = GetComponentsInChildren<Transform>(true);

        TextButton[] allTextButtons = GetComponentsInChildren<TextButton>(true);
        foreach (TextButton button in allTextButtons)
        {
            if (button.IsEnabled)
            {
                activeMenuButtons.Add(button);
            }
        }
    }

    public void ToMainMenu()
    {
        TogglePauseMenu();

        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.EndGame();
    }

    public void TogglePauseMenu()
    {
        TogglePauseMenu(new InputAction.CallbackContext());
    }

    private void TogglePauseMenu(InputAction.CallbackContext context)
    {
        if (DebugConsole.Instance != null && DebugConsole.Instance.IsActive)
        {
            return;
        }

        IsPaused = !IsPaused;

        if (childrenTransforms != null)
        {
            for (int i = 1; i < childrenTransforms.Length; i++)
            {
                childrenTransforms[i].gameObject.SetActive(IsPaused);
            }
        }
    }

    private void OnSelect(InputAction.CallbackContext context)
    {
        if (DebugConsole.Instance != null && DebugConsole.Instance.IsActive)
        {
            return;
        }

        if (!IsPaused)
        {
            return;
        }

        Vector2 input = select.ReadValue<Vector2>();
        int xChange = 0;
        if (input.x > 0.3f)
        {
            xChange = 1;
        }
        else if (input.x < -0.3f)
        {
            xChange = -1;
        }

        if (currentButtonIndex >= 0)
        {
            activeMenuButtons[currentButtonIndex].OnPointerExit(null);
        }
        currentButtonIndex = ((currentButtonIndex + xChange) + activeMenuButtons.Count) % activeMenuButtons.Count;
        activeMenuButtons[currentButtonIndex].OnPointerEnter(null);
    }

    private void OnConfirm(InputAction.CallbackContext context)
    {
        if (DebugConsole.Instance != null && DebugConsole.Instance.IsActive)
        {
            return;
        }

        if (!IsPaused)
        {
            return;
        }

        if (currentButtonIndex >= 0)
        {
            activeMenuButtons[currentButtonIndex].OnPointerDown(null);
            if (activeMenuButtons[currentButtonIndex].ResetOnClick)
            {
                currentButtonIndex = -1;
            }
        }
    }

    private void OnDebugConsoleToggle(bool value)
    {
        if (value)
        {
            playerInputActions.UI.Disable();
        }
        else
        {
            playerInputActions.UI.Enable();
        }
    }

    private void OnEnable()
    {
        pause = playerInputActions.UI.Pause;
        pause.performed += TogglePauseMenu; 
        pause.Enable();

        select = playerInputActions.UI.Select;
        select.performed += OnSelect;
        select.Enable();

        confirm = playerInputActions.UI.Confirm;
        confirm.performed += OnConfirm;
        confirm.Enable();

        GameEvents.DebugConsoleToggleEvent += OnDebugConsoleToggle;
    }

    private void OnDisable()
    {
        pause.performed -= TogglePauseMenu;
        pause.Disable();

        select.performed -= OnSelect;
        select.Disable();

        confirm.performed -= OnConfirm;
        confirm.Disable();

        GameEvents.DebugConsoleToggleEvent -= OnDebugConsoleToggle;
    }
}
