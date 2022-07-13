using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public abstract class CollisionJoint<T> : MonoBehaviour where T : Joint
{
    public T Joint => joint;
    public bool HasJoint => GetComponent<T>() != null;

    [SerializeField] private LayerMask collisionLayer = default;

    [SerializeField] private float breakForce = float.PositiveInfinity;
    [SerializeField] private float breakTorque = float.PositiveInfinity;
    
    [SerializeField] private bool enableCollision;
    [SerializeField] private bool enablePreprocessing;

    protected T joint;

    protected RopeRoot ropeRoot;

    protected bool continueCollision;

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (HasJoint || ((1 << collision.gameObject.layer) & collisionLayer) == 0 || !collision.gameObject.TryGetComponent(out Rigidbody _))
        {
            continueCollision = false;
            return;
        }

        ropeRoot = collision.gameObject.GetComponentInParent<RopeRoot>();
        if (ropeRoot == null || !ropeRoot.IsDeployed || ropeRoot.IsAttached)
        {
            return;
        }

        // Joint Setup
        joint = gameObject.AddComponent<T>();

        ContactPoint contact = collision.contacts[0];
        joint.anchor = transform.InverseTransformPoint(contact.point);
        joint.connectedBody = collision.contacts[0].otherCollider.transform.GetComponent<Rigidbody>();

        joint.breakForce = breakForce;
        joint.breakTorque = breakTorque;

        joint.enableCollision = enableCollision;
        joint.enablePreprocessing = enablePreprocessing;

        // Fire rope attached event in subclassed CollisionJoint
        GameEvents.RopeAttachedInvoke(ropeRoot.Direction);

        continueCollision = true;
    }

    private void OnJointDetach(int hashCode)
    {
        if (ropeRoot == null || ropeRoot.GetHashCode() != hashCode)
        {
            return;
        }

        //ropeRoot.IsAttached = false;
        ropeRoot = null;

        if (HasJoint)
        {
            Destroy(joint);
            joint = null;
        }
    }

    protected virtual void OnEnable()
    {
        GameEvents.JointDetachEvent += OnJointDetach;
    }

    protected virtual void OnDisable()
    {
        GameEvents.JointDetachEvent -= OnJointDetach;
    }
}
