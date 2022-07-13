using UnityEngine;

public class FixedCollisionJoint : CollisionJoint<FixedJoint>
{
    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if (!HasJoint)
        {
            return;
        }
    }
}
