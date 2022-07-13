using UnityEngine;
using UnityEngine.InputSystem;

public class DebugConsole : MonoBehaviour
{
    [SerializeField] private Color textColor = Color.white;

    public static DebugConsole Instance => instance;

    private static DebugConsole instance;

    private static readonly DebugCommand DEBUG_HELP = new DebugCommand(
        "help",
        "List the available debugging commands and their effects.",
        "help",
        () => {
            foreach (DebugCommandBase commandBase in commandList)
            {
                outputHistory.Enqueue(commandBase.CommandFormat + " : " + commandBase.CommandDesc);
            }
        }
    );
    private static readonly DebugCommand DEBUG_CLEAR = new DebugCommand(
        "clear",
        "Clear the command console history.",
        "clear",
        () => {
            outputHistory.Clear();
        }
    );
    private static readonly DebugCommand<int> POWER_ON_DOOR = new DebugCommand<int>(
        "door.powerOn",
        "Power on the door with the matching id.",
        "door.powerOn <id>",
        (int id) =>
        {
            if (GameDoor.gameDoors.TryGetValue(id, out GameDoor door))
            {
                door.CurrentPower = door.PowerNeeded;
            }
        }
    );
    private static readonly DebugCommand<int> POWER_OFF_DOOR = new DebugCommand<int>(
        "door.powerOff",
        "Power off the door with the matching id.",
        "door.powerOff <id>",
        (int id) =>
        {
            if (GameDoor.gameDoors.TryGetValue(id, out GameDoor door))
            {
                door.CurrentPower = 0;
            }
        }
    );
    private static readonly DebugCommand POWER_ON_ALL_DOORS = new DebugCommand(
        "door.powerOnAll",
        "Power on all doors.",
        "door.powerOnAll",
        () =>
        {
            foreach (var pair in GameDoor.gameDoors)
            {
                GameDoor door = pair.Value;
                door.CurrentPower = door.PowerNeeded;
            }
        }
    );
    private static readonly DebugCommand POWER_OFF_ALL_DOORS = new DebugCommand(
        "door.powerOffAll",
        "Power off all doors.",
        "door.powerOffAll",
        () =>
        {
            foreach (var pair in GameDoor.gameDoors)
            {
                GameDoor door = pair.Value;
                door.CurrentPower = 0;
            }
        }
    );

    private static DebugCommandBase[] commandList = {
        DEBUG_CLEAR,
        DEBUG_HELP,
        POWER_ON_DOOR,
        POWER_OFF_DOOR,
        POWER_ON_ALL_DOORS,
        POWER_OFF_ALL_DOORS,
    };

    private static int historySize = 10;
    private static HistoryQueue<string> outputHistory = new HistoryQueue<string>(historySize);
    private static HistoryQueue<string> commandHistory = new HistoryQueue<string>(historySize);

    private PlayerInputActions playerInputActions;
    private InputAction debug;

    private int historyIndex = -1;

    public bool IsActive { get; set; }

    private string input;
    private Vector2 scrollView;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        playerInputActions = new PlayerInputActions();
    }

    private void OnGUI()
    {
        if (!IsActive)
        {
            ResetInput();
            GUI.FocusControl(null);

            return;
        }

        float y = 0f;
        float visibleViewportHeight = Screen.height * 0.3f;
        GUI.Box(new Rect(0f, y, Screen.width, visibleViewportHeight), "");

        GUI.contentColor = textColor;
        float labelHeight = Screen.height * 0.03f;
        int fontSize = (int)(labelHeight * 0.75f);
        GUIStyle guiStyle = new GUIStyle()
        {
            fontSize = fontSize,
        };
        guiStyle.normal.textColor = textColor;
        GUI.skin.textField.fontSize = fontSize;

        float totalViewportHeight = labelHeight * (historySize + 1);
        Rect viewport = new Rect(0f, y, Screen.width, totalViewportHeight);
        scrollView = GUI.BeginScrollView(new Rect(0f, y + 5f, viewport.width, visibleViewportHeight * 0.95f), scrollView, viewport);
        for (int i = 0; i < outputHistory.Count; i++)
        {
            Rect rect = new Rect(5f, labelHeight * i, viewport.width, labelHeight);
            GUI.Label(rect, outputHistory.At(i), guiStyle);
        }
        GUI.EndScrollView();

        y += visibleViewportHeight;
        GUI.backgroundColor = new Color(0f, 0f, 0f);
        GUI.SetNextControlName("CommandInput");
        input = GUI.TextField(new Rect(0f, y + 1f, Screen.width, Screen.height * 0.035f), input);

        if (GUI.GetNameOfFocusedControl() == "CommandInput" && Event.current.type == EventType.KeyUp)
        {
            if (Event.current.keyCode == KeyCode.Return)
            {
                if (input == "")
                {
                    return;
                }

                outputHistory.Enqueue(input);
                commandHistory.Enqueue(input);

                ParseInput();
                ResetInput();
            }

            if (Event.current.keyCode == KeyCode.F1)
            {
                IsActive = !IsActive;
            }
            else if (Event.current.keyCode == KeyCode.UpArrow)
            {
                Event.current.Use();

                if (historyIndex < 0)
                {
                    historyIndex = commandHistory.Count;
                }

                if (historyIndex <= 0)
                {
                    return;
                }

                historyIndex -= 1;
                input = commandHistory.At(historyIndex);
            }
            else if (Event.current.keyCode == KeyCode.DownArrow)
            {
                Event.current.Use();

                if (historyIndex < 0)
                {
                    return;
                }

                if (historyIndex >= commandHistory.Count - 1)
                {
                    ResetInput();
                    return;
                }

                historyIndex += 1;
                input = commandHistory.At(historyIndex);
            }
        }
    }

    private void ParseInput()
    {
        string[] args = input.Split(' ');

        for (int i = 0; i < commandList.Length; i++)
        {
            DebugCommandBase commandBase = commandList[i];
            if (args[0].Equals(commandBase.CommandId))
            {
                if (commandBase as DebugCommand != null)
                {
                    (commandBase as DebugCommand).Invoke();
                }
                else if (args.Length == 2 && commandBase as DebugCommand<int> != null)
                {
                    (commandBase as DebugCommand<int>).Invoke(int.Parse(args[1]));
                }

                return;
            }
        }

        outputHistory.Enqueue("Invalid command \"" + outputHistory.At(outputHistory.Count - 1) + "\" - Type \"help\" to list available commands.");
    }

    private void ResetInput()
    {
        input = "";
        historyIndex = -1;
    }

    private void ToggleDebugConsole(InputAction.CallbackContext context)
    {
        IsActive = !IsActive;
        GameEvents.DebugConsoleToggleInvoke(IsActive);
    }

    private void OnEnable()
    {
        debug = playerInputActions.Debug.Console;
        debug.performed += ToggleDebugConsole;
        debug.Enable();
    }

    private void OnDisable()
    {
        debug.performed -= ToggleDebugConsole;
        debug.Disable();
    }
}
