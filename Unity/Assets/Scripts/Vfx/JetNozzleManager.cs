using UnityEngine;
using System.Collections.Generic;

public class JetNozzleManager : MonoBehaviour
{
    public static List<JetNozzle> backpackNozzles = new List<JetNozzle>();
    public static List<JetNozzle> ropeNozzles = new List<JetNozzle>();

    public static void BoostEffect(Vector2 input)
    {
        List<JetNozzle.NozzleDirection> activeDirections = GetNozzleDirections(input);
        if (activeDirections.Count == 0)
        {
            foreach (JetNozzle nozzle in backpackNozzles)
            {
                nozzle.StopEffect();
            }
        }
        else
        {
            foreach (JetNozzle nozzle in backpackNozzles)
            {
                if (activeDirections.Exists(x => x == nozzle.Direction))
                {
                    nozzle.PlayEffect();
                }
                else
                {
                    nozzle.StopEffect();
                }
            }
        }
    }

    public static void HookEffect(JetNozzle.NozzleDirection direction)
    {
        foreach (JetNozzle nozzle in ropeNozzles)
        {
            if (nozzle.Direction == direction)
            {
                nozzle.PlayEffect();
            }
        }
    }

    public static void ReleaseHookEffect(JetNozzle.NozzleDirection direction)
    {
        foreach (JetNozzle nozzle in ropeNozzles)
        {
            if (nozzle.Direction == direction)
            {
                nozzle.StopEffect();
            }
        }
    }

    private static List<JetNozzle.NozzleDirection> GetNozzleDirections(Vector2 vec)
    {
        List<JetNozzle.NozzleDirection> list = new List<JetNozzle.NozzleDirection>();
        
        if (vec.x > 0f)
        {
            list.Add(JetNozzle.NozzleDirection.WEST);
        }
        else if (vec.x < 0f)
        {
            list.Add(JetNozzle.NozzleDirection.EAST);
        }

        if (vec.y > 0f)
        {
            list.Add(JetNozzle.NozzleDirection.SOUTH);
        }
        else if (vec.y < 0f)
        {
            list.Add(JetNozzle.NozzleDirection.NORTH);
        }

        return list;
    }

    private static List<JetNozzle.NozzleDirection> GetNozzleDirections(float x)
    {
        List<JetNozzle.NozzleDirection> list = new List<JetNozzle.NozzleDirection>();

        if (x > 0f)
        {
            list.Add(JetNozzle.NozzleDirection.WEST);
        }
        else if (x < 0f)
        {
            list.Add(JetNozzle.NozzleDirection.EAST);
        }

        return list;
    }
}
