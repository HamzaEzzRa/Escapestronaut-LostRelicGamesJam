using UnityEngine;

public class HingeCollisionJoint : CollisionJoint<HingeJoint>
{
    [SerializeField] private Vector3 axis = Vector3.right;
    [SerializeField] private bool autoConfigureConnectedAnchor = true;
    //[SerializeField] private Vector3 connectedAnchor;

    [SerializeField] private bool useSpring;
    [SerializeField] private float spring, damper, targetPosition;

    [SerializeField] private bool useMotor;
    [SerializeField] private float targetVelocity, force;
    [SerializeField] private bool freeSpin;

    [SerializeField] private bool useLimits;
    [SerializeField] private float min, max, bounciness, bounceMinVelocity, contactDistance;

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if (!HasJoint)
        {
            return;
        }

        joint.axis = axis;
        joint.autoConfigureConnectedAnchor = autoConfigureConnectedAnchor;
        //joint.connectedAnchor = connectedAnchor;

        joint.useSpring = useSpring;
        joint.spring = new JointSpring
        {
            spring = spring,
            damper = damper,
            targetPosition = targetPosition,
        };

        joint.useMotor = useMotor;
        joint.motor = new JointMotor
        {
            targetVelocity = targetVelocity,
            force = force,
            freeSpin = freeSpin,
        };

        joint.useLimits = useLimits;
        joint.limits = new JointLimits
        {
            min = min,
            max = max,
            bounciness = bounciness,
            bounceMinVelocity = bounceMinVelocity,
            contactDistance = contactDistance,
        };
    }
}
