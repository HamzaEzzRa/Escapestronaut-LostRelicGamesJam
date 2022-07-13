using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AnimationManager))]
public class GameDoor : MonoBehaviour
{
    public static Dictionary<int, GameDoor> gameDoors = new Dictionary<int, GameDoor>();

    public bool IsPowered
    {
        get => isPowered;
        set
        {
            isPowered = value;

            if (value)
            {
                foreach (Light light in lights)
                {
                    light.color = onColor;
                }
            }
            else
            {
                foreach (Light light in lights)
                {
                    light.color = offColor;
                }
            }
        }
    }

    public int CurrentPower
    {
        get => currentPower;
        set
        {
            currentPower = value;
            IsPowered = powerNeeded <= currentPower;
        }
    }

    public int PowerNeeded => powerNeeded;

    public int Id => id;

    [SerializeField] private int powerNeeded = 1;

    [SerializeField] private int id;

    [SerializeField, Range(0f, 2f)] private float animationSpeed = 1f;

    [SerializeField] private Light[] lights;
    [SerializeField, ColorUsage(true, true)] private Color offColor, onColor;

    [SerializeField] private bool isPowered;

    private int currentPower = 0;

    private readonly int
        DOOR_ACTION = Animator.StringToHash("Door_Action");

    private AnimationManager animationManager;

    private float normalizedTime = 0f;

    private Coroutine currentCoroutine = null;

    private void Awake()
    {
        animationManager = GetComponent<AnimationManager>();
    }

    private void OnPortalTriggerEnter(int doorId)
    {
        if (id != doorId)
        {
            return;
        }

        CurrentPower = Mathf.Min(CurrentPower + 1, PowerNeeded);
    }

    private void OnPortalTriggerExit(int doorId)
    {
        if (id != doorId)
        {
            return;
        }

        CurrentPower = Mathf.Max(CurrentPower - 1, 0);
    }

    private void OnDoorTriggerOpen(int doorId)
    {
        if (id != doorId || !IsPowered)
        {
            return;
        }

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(DoorOpenCoroutine(animationSpeed));
    }

    private IEnumerator DoorOpenCoroutine(float speed)
    {
        while (normalizedTime < 1f)
        {
            normalizedTime = Mathf.Min(1f, normalizedTime + speed * Time.deltaTime);
            animationManager.ControlAnimation(DOOR_ACTION, normalizedTime);
            yield return null;
        }

        animationManager.ControlAnimation(DOOR_ACTION, 1f);
        normalizedTime = 1f;
    }

    private void OnDoorTriggerClose(int doorId)
    {
        if (id != doorId)
        {
            return;
        }

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(DoorCloseCoroutine(1.1f * animationSpeed));
    }

    private IEnumerator DoorCloseCoroutine(float speed)
    {
        while (normalizedTime > 0f)
        {
            normalizedTime = Mathf.Max(0f, normalizedTime - speed * Time.deltaTime);
            animationManager.ControlAnimation(DOOR_ACTION, normalizedTime);
            yield return null;
        }

        animationManager.ControlAnimation(DOOR_ACTION, 0f);
        normalizedTime = 0f;
    }

    private void OnEnable()
    {
        gameDoors.TryAdd(id, this);

        GameEvents.PortalTriggerEnterEvent += OnPortalTriggerEnter;
        GameEvents.PortalTriggerExitEvent += OnPortalTriggerExit;

        GameEvents.DoorTriggerOpenEvent += OnDoorTriggerOpen;
        GameEvents.DoorTriggerCloseEvent += OnDoorTriggerClose;
    }

    private void OnDisable()
    {
        gameDoors.Remove(id);

        GameEvents.PortalTriggerEnterEvent -= OnPortalTriggerEnter;
        GameEvents.PortalTriggerExitEvent -= OnPortalTriggerExit;

        GameEvents.DoorTriggerOpenEvent -= OnDoorTriggerOpen;
        GameEvents.DoorTriggerOpenEvent -= OnDoorTriggerClose;
    }

    private void OnValidate()
    {
        if (isPowered)
        {
            foreach (Light light in lights)
            {
                light.color = onColor;
            }
        }
        else
        {
            foreach (Light light in lights)
            {
                light.color = offColor;
            }
        }
    }
}
