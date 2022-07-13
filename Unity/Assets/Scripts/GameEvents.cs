using System;

public static class GameEvents
{
    public static event Action<RopeRoot.RopeDirection> RopeAttachedEvent;
    public static void RopeAttachedInvoke(RopeRoot.RopeDirection direction)
    {
        RopeAttachedEvent?.Invoke(direction);
    }

    public static event Action<int> JointDetachEvent;
    public static void JointDetachInvoke(int hashCode)
    {
        JointDetachEvent?.Invoke(hashCode);
    }

    public static event Action<int> DoorTriggerOpenEvent;
    public static void DoorTriggerOpenInvoke(int id)
    {
        DoorTriggerOpenEvent?.Invoke(id);
    }

    public static event Action<int> DoorTriggerCloseEvent;
    public static void DoorTriggerCloseInvoke(int id)
    {
        DoorTriggerCloseEvent?.Invoke(id);
    }

    public static event Action<GameLevel> LevelChangeEvent;
    public static void LevelChangeInvoke(GameLevel level)
    {
        LevelChangeEvent?.Invoke(level);
    }

    public static event Action<bool> GravitySwitchEvent;
    public static void GravitySwitchInvoke(bool changeGravity=true)
    {
        GravitySwitchEvent?.Invoke(changeGravity);
    }

    public static event Action<int> PortalTriggerEnterEvent;
    public static void PortalTriggerEnterInvoke(int doorId)
    {
        PortalTriggerEnterEvent?.Invoke(doorId);
    }

    public static event Action<int> PortalTriggerExitEvent;
    public static void PortalTriggerExitInvoke(int doorId)
    {
        PortalTriggerExitEvent?.Invoke(doorId);
    }

    public static event Action<int> CinematicAIDialogEvent;
    public static void CinematicAIDialogInvoke(int dialogId)
    {
        CinematicAIDialogEvent?.Invoke(dialogId);
    }

    public static event Action<int, bool> FloatingObjectMovedEvent;
    public static void FloatingObjectMovedInvoke(int hashCode, bool isOutside)
    {
        FloatingObjectMovedEvent?.Invoke(hashCode, isOutside);
    }

    public static event Action<bool> PauseMenuToggleEvent;
    public static void PauseMenuToggleInvoke(bool value)
    {
        PauseMenuToggleEvent?.Invoke(value);
    }

    public static event Action<bool> DebugConsoleToggleEvent;
    public static void DebugConsoleToggleInvoke(bool value)
    {
        DebugConsoleToggleEvent?.Invoke(value);
    }
}
