using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GameLevel : MonoBehaviour, IPersistentObject
{
    public static bool IsGravityOn = false;
    public static bool IsOutside = false;
    public static bool CanChangeLevel = true;

    public static bool IsFirstLoadedLevel = true;
    public static int CurrentOrder;
    public static Vector3 PlayerOffset;

    public int Order => order;

    public Bounds Bounds => boxCollider.bounds;

    public bool IsActive => isActive;

    [SerializeField] private LayerMask triggerLayer = 1 << 8;
    [SerializeField] private int order;
    [SerializeField] private bool isOutside;

    private bool isActive;
    private BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    public static void Reset()
    {
        CanChangeLevel = true;
        IsFirstLoadedLevel = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out FloatingObject obj))
        {
            GameEvents.FloatingObjectMovedInvoke(obj.GetHashCode(), isOutside);
        }

        if (!CanChangeLevel)
        {
            return;
        }

        if (((1 << other.gameObject.layer) & triggerLayer) != 0)
        {
            CanChangeLevel = false;
            isActive = true;
            IsOutside = isOutside;

            if (IsActive)
            {
                PlayerOffset = 2f * Vector3.right * (CurrentOrder <= Order ? 1f : -1f);
                CurrentOrder = Order;

                if (!IsFirstLoadedLevel)
                {
                    SerializationManager.SaveGame();
                }
                else
                {
                    IsFirstLoadedLevel = false;
                }
            }

            GameEvents.LevelChangeInvoke(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & triggerLayer) == 0)
        {
            return;
        }

        isActive = false;
        CanChangeLevel = true;
    }

    private void OnGravitySwitch(bool changeGravity)
    {
        if (changeGravity && IsActive)
        {
            IsGravityOn = !IsGravityOn;
        }
    }

    private void OnEnable()
    {
        SerializationManager.AddPersistentObject(this);

        GameEvents.GravitySwitchEvent += OnGravitySwitch;
    }

    private void OnDisable()
    {
        SerializationManager.RemovePersistentObject(this);

        GameEvents.GravitySwitchEvent -= OnGravitySwitch;
    }

    public void SaveData(GameData data)
    {
        if (IsActive)
        {
            data.currentLevelOrder = Order;
            data.isGravityOn = IsGravityOn;
        }
    }

    public void LoadData(GameData data)
    {
        if (data.currentLevelOrder == Order)
        {
            CurrentOrder = data.currentLevelOrder;

            if (data.isGravityOn != IsGravityOn)
            {
                IsGravityOn = data.isGravityOn;
                GameEvents.GravitySwitchInvoke(false);
            }

            GameEvents.LevelChangeInvoke(this);
        }
    }
}
