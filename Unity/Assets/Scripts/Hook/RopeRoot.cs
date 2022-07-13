using UnityEngine;
using Unity.Mathematics;

public class RopeRoot : MonoBehaviour
{
    public enum RopeDirection
    {
        LEFT,
        RIGHT,
    }

    public RopeDirection Direction => direction;

    public bool IsDeployed
    {
        get => isDeployed;
        set
        {
            isDeployed = value;
        }
    }

    public bool IsAttached
    {
        get => isAttached;
        set
        {
            isAttached = value;
        }
    }

    [SerializeField] private Rigidbody rootConnection = default;

    [SerializeField] private float mass = 0.1f;
    [SerializeField] private float drag = 0f;
    [SerializeField] private float angularDrag = 0.05f;
    [SerializeField] private bool useGravity;
    [SerializeField] private bool isKinematic;
    [SerializeField] private CollisionDetectionMode collisionDetectionMode =
        CollisionDetectionMode.Discrete;
    [SerializeField] private bool3 freezePosition;
    [SerializeField] private bool3 freezeRotation;

    [SerializeField] private bool hasCollider = true;
    [SerializeField] private Vector3 colliderCenter;
    [SerializeField] private float colliderRadius = 0.1f;
    [SerializeField] private float colliderHeight = 0.1f;

    [SerializeField] ConfigurableJointMotion xMotion = ConfigurableJointMotion.Free,
        yMotion = ConfigurableJointMotion.Free,
        zMotion = ConfigurableJointMotion.Free;
    [SerializeField] ConfigurableJointMotion angularXMotion = ConfigurableJointMotion.Free,
        angularYMotion = ConfigurableJointMotion.Free,
        angularZMotion = ConfigurableJointMotion.Free;
    [SerializeField] private bool enablePreprocessing;

    [SerializeField] private RopeDirection direction;

    [SerializeField] private CapsuleCollider[] referenceColliders;
    [SerializeField] private float updateColliderSize = 0.05f;
    //[SerializeField] private int tolerance = 0;

    private bool isDeployed, isAttached;

    private int[] updateColliderCounts;
    private CapsuleCollider[][] updateColliders;

    private void Start()
    {
        updateColliderCounts = new int[referenceColliders.Length - 1];
        updateColliders = new CapsuleCollider[referenceColliders.Length - 1][];
    }

    private void Update()
    {
        for (int i = 0; i < referenceColliders.Length - 1; i++)
        {
            //Debug.DrawLine(Camera.main.transform.position, referenceColliders[i].bounds.center, Color.red);
            
            float distance = Vector3.Distance(referenceColliders[i].bounds.center, referenceColliders[i + 1].bounds.center);
            int colliderCount = (int)(distance / (updateColliderSize * 2)) - 2;

            //if (Mathf.Abs(updateColliderCounts[i] - colliderCount) > tolerance)
            //{
                ClearUpdateColliders(i);

                if (colliderCount > 0)
                {
                    updateColliders[i] = new CapsuleCollider[colliderCount];

                    updateColliderCounts[i] = colliderCount;
                    Vector3 directionVector = (referenceColliders[i + 1].bounds.center - referenceColliders[i].bounds.center).normalized;
                    //Debug.Log(direction.ToString() + " Segment " + (i + 1) + ": " + colliderCount);
                    //Debug.Log("Start Position : " + startPosition);

                    for (int j = 0; j < colliderCount; j++)
                    {
                        CapsuleCollider updateCollider = referenceColliders[i].gameObject.AddComponent<CapsuleCollider>();
                        Vector2 center = directionVector * (j + 1) * updateColliderSize;
                        center.y = -updateColliderSize * 2f * (j + 1);
                        updateCollider.center = center;
                        updateCollider.radius = updateColliderSize;
                        updateCollider.height = updateColliderSize;

                        updateColliders[i][j] = updateCollider;
                    }
                }
            //}
        }
    }

    private void ClearUpdateColliders(int index)
    {
        if (updateColliders == null || updateColliders[index] == null)
        {
            return;
        }

        int length = updateColliders[index].Length;
        for (int i = length - 1; i >= 0; i--)
        {
            Destroy(updateColliders[index][i]);
        }
        updateColliders[index] = null;
    }

    public void Generate()
    {
        Clear();

        Transform[] children = transform.GetComponentsInChildren<Transform>();
        SetupRigidbody(children[0].gameObject.AddComponent<Rigidbody>());

        FixedJoint joint = children[0].gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = rootConnection;
        joint.enablePreprocessing = enablePreprocessing;

        //if (hasCollider)
        //{
        //    SetupCollider(children[0].gameObject.AddComponent<CapsuleCollider>());
        //}

        for (int i = 1; i < children.Length; i++)
        {
            SetupRigidbody(children[i].gameObject.AddComponent<Rigidbody>());
            SetupJoint(children[i].gameObject.AddComponent<ConfigurableJoint>());

            if (hasCollider)
            {
                SetupCollider(children[i].gameObject.AddComponent<CapsuleCollider>());
            }
        }
    }

    public void Clear()
    {
        Transform[] children = transform.GetComponentsInChildren<Transform>();

        for (int i = children.Length - 1; i > 0; i--)
        {
            if (children[i].TryGetComponent(out ConfigurableJoint childJoint))
            {
                SafeDestroy(childJoint);
            }
            if (children[i].TryGetComponent(out Rigidbody childRb))
            {
                SafeDestroy(childRb);
            }
            if (children[i].TryGetComponent(out CapsuleCollider childCollider))
            {
                SafeDestroy(childCollider);
            }
        }

        if (children[0].TryGetComponent(out FixedJoint joint))
        {
            SafeDestroy(joint);
        }
        if (children[0].TryGetComponent(out Rigidbody rb))
        {
            SafeDestroy(rb);
        }
        if (children[0].TryGetComponent(out CapsuleCollider collider))
        {
            SafeDestroy(collider);
        }
    }

    private void SetupRigidbody(Rigidbody rb)
    {
        rb.mass = mass;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        rb.useGravity = useGravity;
        rb.isKinematic = isKinematic;
        rb.collisionDetectionMode = collisionDetectionMode;

        if (freezePosition.x)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionX;
        }
        if (freezePosition.y)
        {
            rb.constraints |= RigidbodyConstraints.FreezePositionY;
        }
        if (freezePosition.z)
        {
            rb.constraints |= RigidbodyConstraints.FreezePositionZ;
        }
        if (freezeRotation.x)
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotationX;
        }
        if (freezeRotation.y)
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotationY;
        }
        if (freezeRotation.z)
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void SetupCollider(CapsuleCollider collider)
    {
        collider.height = colliderHeight;
        collider.radius = colliderRadius;
        collider.center = colliderCenter;
    }

    private void SetupJoint(ConfigurableJoint joint)
    {
        if (joint.transform.parent.TryGetComponent(out Rigidbody parentRb))
        {
            joint.connectedBody = parentRb;
        }

        joint.xMotion = xMotion;
        joint.yMotion = yMotion;
        joint.zMotion = zMotion;

        joint.angularXMotion = angularXMotion;
        joint.angularYMotion = angularYMotion;
        joint.angularZMotion = angularZMotion;

        joint.enablePreprocessing = enablePreprocessing;
    }

    private void SafeDestroy(Object obj)
    {
        if (Application.isEditor)
        {
            DestroyImmediate(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
}
