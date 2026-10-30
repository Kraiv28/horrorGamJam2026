using UnityEngine;

public class ClickableTeleport : MonoBehaviour
{
    public TeleportController teleportController;

    private void OnMouseDown()
    {
        if (teleportController != null)
        {
            teleportController.TriggerTeleport();
        }
    }
}