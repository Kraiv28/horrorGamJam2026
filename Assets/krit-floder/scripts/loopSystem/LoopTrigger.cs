using UnityEngine;

public class LoopTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบว่า GameObject ที่ชนต้องมี Tag เป็น "Player"
        if (other.CompareTag("Player"))
        {
            PTLoopManager.Instance.OnPlayerReachEnd();
        }
    }
}