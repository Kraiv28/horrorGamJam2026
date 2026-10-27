using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoorScript
{
    [RequireComponent(typeof(AudioSource))]
    public class Door : MonoBehaviour
    {
        public bool open;
        public float smooth = 1.0f;
        float DoorOpenAngle = -90.0f;
        float DoorCloseAngle = 0.0f;
        public AudioSource asource;
        public AudioClip openDoor, closeDoor;

        private Collider doorCollider;

        void Start()
        {
            asource = GetComponent<AudioSource>();
            doorCollider = GetComponent<Collider>();
        }

        void Update()
        {
            var targetOpen = Quaternion.Euler(0, DoorOpenAngle, 0);
            var targetClose = Quaternion.Euler(0, DoorCloseAngle, 0);

            if (open)
            {
                // กำลังเปิดออก: ปิด Collider ทันทีเพื่อให้เดินผ่านได้
                if (doorCollider != null)
                {
                    doorCollider.enabled = false;
                }

                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetOpen, Time.deltaTime * 5 * smooth);
            }
            else
            {
                // กำลังปิด: ปิด Collider ไว้ก่อน (เดินทะลุได้ตอนกำลังขยับ)
                if (doorCollider != null)
                {
                    doorCollider.enabled = false;
                }

                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetClose, Time.deltaTime * 5 * smooth);

                // ตรวจสอบว่าประตูหมุนมาถึงตำแหน่งปิดสนิท (CloseAngle) แล้วหรือยัง (เช็กความต่างน้อยกว่า 0.5 องศา)
                if (Quaternion.Angle(transform.localRotation, targetClose) < 0.5f)
                {
                    // ปิดสนิทแล้ว! เปิด Collider ให้กลับมาชนได้เหมือนเดิม
                    if (doorCollider != null)
                    {
                        doorCollider.enabled = true;
                    }
                }
            }
        }

        public void OpenDoor()
        {
            open = !open;
            asource.clip = open ? openDoor : closeDoor;
            asource.Play();
        }
    }
}