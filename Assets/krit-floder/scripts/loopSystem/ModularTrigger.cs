using UnityEngine;

public class ModularTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true; // ป้องกันการชนซ้ำซ้อนในชิ้นเดียวกัน
            PTModularLoopManager.Instance.OnPlayerReachTrigger();
        }
    }
}