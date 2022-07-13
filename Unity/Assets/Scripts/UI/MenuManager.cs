using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Transform astronaut;
    [SerializeField] private SkinnedMeshRenderer astronautMeshRenderer;
    [SerializeField, ColorUsage(true, true)] private Color powerOnColor;
    [SerializeField] private GameObject mainMenu;

    [SerializeField] private Transform background;
    [SerializeField, Range(1f, 10f)] private float astronautRotationSmoothFactor = 2f;
    [SerializeField, Range(1f, 10f)] private float backgroundRotationSmoothFactor = 2f;

    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private GameObject settingsMenu;

    [SerializeField] private Slider musicSlider, sfxSlider, voiceSlider;
    [SerializeField] private Toggle subtitlesToggle;
    [SerializeField] private TextButton loadButton;

    private Vector3 astronautTargetRotation;
    private Vector3 backgroundTargetRotation;

    private PlayerInputActions playerInputActions;
    private InputAction select, confirm;

    private bool areSettingsActive;
    private List<TextButton> menuTextButtons = new List<TextButton>();
    private int currentMenuButton = -1;

    private Slider[] settingsSliders;
    private Toggle[] settingsToggles;
    private List<TextButton> settingsTextButtons = new List<TextButton>();
    private int currentSettingsButton = -1;

    private void Awake()
    {
        if (astronaut != null)
        {
            astronautTargetRotation = astronaut.rotation.eulerAngles;
        }
        if (astronautMeshRenderer != null)
        {
            astronautMeshRenderer.materials[1].SetColor("_EmissionColor", Color.black);
        }

        if (background != null)
        {
            backgroundTargetRotation = background.rotation.eulerAngles;
        }

        if (loadButton != null && SerializationManager.HasSaves())
        {
            loadButton.Enable();
        }

        if (mainMenu != null)
        {
            TextButton[] allTextButtons = mainMenu.GetComponentsInChildren<TextButton>();
            foreach (TextButton button in allTextButtons)
            {
                if (button.IsEnabled)
                {
                    menuTextButtons.Add(button);
                }
            }
        }

        if (settingsMenu != null)
        {
            settingsSliders = settingsMenu.GetComponentsInChildren<Slider>();
            settingsToggles = settingsMenu.GetComponentsInChildren<Toggle>();

            TextButton[] allTextButtons = settingsMenu.GetComponentsInChildren<TextButton>();
            foreach (TextButton button in allTextButtons)
            {
                if (button.IsEnabled)
                {
                    settingsTextButtons.Add(button);
                }
            }
        }

        playerInputActions = new PlayerInputActions();
    }

    private void Start()
    {
        if (musicSlider != null)
        {
            SetMusicVolume(musicSlider.value);
        }
        if (sfxSlider != null)
        {
            SetSFXVolume(sfxSlider.value);
        }
        if (voiceSlider != null)
        {
            SetVoiceVolume(voiceSlider.value);
        }
        SetSubtitles(true);
    }

    private void Update()
    {
        astronautTargetRotation += Vector3.forward * Random.Range(5f, 10f) * Time.deltaTime;
        backgroundTargetRotation += new Vector3(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 0f) * Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (astronaut != null)
        {
            astronaut.rotation = Quaternion.Slerp(
                astronaut.rotation,
                Quaternion.Euler(astronautTargetRotation),
                astronautRotationSmoothFactor * Time.deltaTime
            );
        }

        if (background != null)
        {
            background.rotation = Quaternion.Slerp(
                background.rotation,
                Quaternion.Euler(backgroundTargetRotation),
                backgroundRotationSmoothFactor * Time.deltaTime
            );
        }
    }

    private void StartGameHelper(System.Action callback)
    {
        if (!mainMenu.activeInHierarchy)
        {
            return;
        }

        mainMenu.SetActive(false);
        AudioManager.Instance.Stop("Main Menu");

        LeanTween.value(0f, 2.4f, 2.5f)
            .setOnStart(() =>
            {
                AudioManager.Instance.Play("Helmet Start");
            })
            .setOnUpdate((float value) =>
            {
                astronautMeshRenderer.materials[1].SetColor("_EmissionColor", new Color(value * powerOnColor.r, value * powerOnColor.g, value * powerOnColor.b));
            })
            .setOnComplete(() =>
            {
                float value = 2.4f;
                astronautMeshRenderer.materials[1].SetColor("_EmissionColor", new Color(value * powerOnColor.r, value * powerOnColor.g, value * powerOnColor.b));

                GameManager.Instance.LoadScene(SceneMap.GAME_SCENE, callback: callback);
            })
            .setEaseOutBounce();
    }

    #region Text Buttons Functions
    public void NewGame()
    {
        StartGameHelper(null);
    }

    public void LoadGame()
    {
        StartGameHelper(() =>
        {
            SerializationManager.LoadGame();
        });
    }

    public void ShowSettings()
    {
        if (!mainMenu.activeInHierarchy)
        {
            return;
        }
        
        settingsMenu.SetActive(true);
        areSettingsActive = true;
    }

    public void HideSettings()
    {
        if (!mainMenu.activeInHierarchy)
        {
            return;
        }
        
        settingsMenu.SetActive(false);
        areSettingsActive = false;
    }

    public void Exit()
    {
        Application.Quit();
    }
    #endregion

    #region Settings Functions
    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20f);
    }

    public void SetSFXVolume(float volume)
    {
        mainMixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20f);
    }

    public void SetVoiceVolume(float volume)
    {
        mainMixer.SetFloat("voiceVolume", Mathf.Log10(volume) * 20f);
    }

    public void SubtitlesToggle()
    {
        subtitlesToggle.isOn = !subtitlesToggle.isOn;
    }

    public void SetSubtitles(bool value)
    {
        GameManager.Instance.SubtitlesOn = value;
    }
    #endregion

    private void OnSelect(InputAction.CallbackContext context)
    {
        Vector2 input = select.ReadValue<Vector2>();
        int xChange = 0, yChange = 0;
        if (input.y > 0.3f)
        {
            yChange = -1;
        }
        else if (input.y < -0.3f)
        {
            yChange = 1;
        }

        if (input.x > 0.3f)
        {
            xChange = 1;
        }
        else if (input.x < -0.3f)
        {
            xChange = -1;
        }

        if (yChange != 0 && xChange == 0)
        {
            if (!areSettingsActive)
            {
                if (currentMenuButton >= 0)
                {
                    menuTextButtons[currentMenuButton].OnPointerExit(null);
                }
                currentMenuButton = ((currentMenuButton + yChange) + menuTextButtons.Count) % menuTextButtons.Count;
                menuTextButtons[currentMenuButton].OnPointerEnter(null);
            }
            else
            {
                if (currentSettingsButton >= 0)
                {
                    settingsTextButtons[currentSettingsButton].OnPointerExit(null);
                }
                currentSettingsButton = ((currentSettingsButton + yChange) + settingsTextButtons.Count) % settingsTextButtons.Count;
                settingsTextButtons[currentSettingsButton].OnPointerEnter(null);
            }
        }

        if (xChange != 0 && yChange == 0)
        {
            if (areSettingsActive)
            {
                if (currentSettingsButton < settingsSliders.Length)
                {
                    settingsSliders[currentSettingsButton].value += xChange * 0.1f;
                }
            }
        }
    }

    private void OnConfirm(InputAction.CallbackContext context)
    {
        if (!areSettingsActive)
        {
            if (currentMenuButton >= 0)
            {
                menuTextButtons[currentMenuButton].OnPointerDown(null);
                if (menuTextButtons[currentMenuButton].ResetOnClick)
                {
                    currentMenuButton = -1;
                }
            }
        }
        else
        {
            if (currentSettingsButton >= 0)
            {
                settingsTextButtons[currentSettingsButton].OnPointerDown(null);
                if (settingsTextButtons[currentSettingsButton].ResetOnClick)
                {
                    currentSettingsButton = -1;
                }
            }
        }
    }

    private void OnEnable()
    {
        select = playerInputActions.UI.Select;
        select.performed += OnSelect;
        select.Enable();

        confirm = playerInputActions.UI.Confirm;
        confirm.performed += OnConfirm;
        confirm.Enable();
    }

    private void OnDisable()
    {
        if (select != null)
        {
            select.performed -= OnSelect;
            select.Disable();
        }

        if (confirm != null)
        {
            confirm.performed -= OnConfirm;
            confirm.Disable();
        }
    }
}
