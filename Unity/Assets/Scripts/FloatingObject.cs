using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FloatingObject : MonoBehaviour, IPersistentObject
{
    [System.Serializable] public struct ObjectData
    {
        public Vector3 position;
        public bool floatState;
    };

    [SerializeField] private int id;

    [SerializeField] private Vector3 floatingAcceleration = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] private Vector3 maxVelocity = new Vector3(2f, 2f, 2f);

    private Rigidbody rb;

    private bool shouldFloat, isFirstFloatStateLoaded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        shouldFloat = !GameLevel.IsGravityOn;
    }

    private void Update()
    {
        rb.velocity = new Vector3(
            Mathf.Min(rb.velocity.x, maxVelocity.x),
            Mathf.Min(rb.velocity.y, maxVelocity.y),
            Mathf.Min(rb.velocity.z, maxVelocity.z)
        );
    }

    private void FixedUpdate()
    {
        if (shouldFloat)
        {
            rb.useGravity = false;

            if (floatingAcceleration.sqrMagnitude != 0)
            {
                rb.AddForce(rb.mass * floatingAcceleration, ForceMode.Force);
            }
        }
        else
        {
            rb.useGravity = true;
        }
    }

    private void OnFloatingObjectMoved(int hashCode, bool isOutside)
    {
        if (isFirstFloatStateLoaded && hashCode == GetHashCode())
        {
            //Debug.Log("Object Moved Event " + id);
            shouldFloat = !GameLevel.IsGravityOn || isOutside;
        }
    }

    private void OnGravitySwitch(bool changeGravity)
    {
        //Debug.Log("Gravity Switch Event " + id + " " + changeGravity);
        if (changeGravity)
        {
            shouldFloat = !shouldFloat || GameLevel.IsOutside;
        }
        //else
        //{
        //    shouldFloat = !GameLevel.IsGravityOn || GameLevel.IsOutside;
        //}
    }

    private void OnEnable()
    {
        GameEvents.GravitySwitchEvent += OnGravitySwitch;
        GameEvents.FloatingObjectMovedEvent += OnFloatingObjectMoved;

        SerializationManager.AddPersistentObject(this);
    }

    private void OnDisable()
    {
        GameEvents.GravitySwitchEvent -= OnGravitySwitch;
        GameEvents.FloatingObjectMovedEvent -= OnFloatingObjectMoved;

        SerializationManager.RemovePersistentObject(this);
    }

    public void SaveData(GameData data)
    {
        //Debug.Log("========== Save " + id + " ==========");
        if (data.movableObjectsData.ContainsKey(id))
        {
            data.movableObjectsData[id] = new ObjectData
            {
                position = transform.position,
                floatState = shouldFloat
            };
        }
        else
        {
            data.movableObjectsData.Add(id,
                new ObjectData
                {
                    position = transform.position,
                    floatState = shouldFloat
                }
            );
        }

        isFirstFloatStateLoaded = true;
    }

    public void LoadData(GameData data)
    {
        //Debug.Log("========== Load " + id + " ==========");
        if (data.movableObjectsData.TryGetValue(id, out ObjectData savedData))
        {
            transform.position = savedData.position;
            shouldFloat = savedData.floatState;
        }

        isFirstFloatStateLoaded = true;
    }
}
