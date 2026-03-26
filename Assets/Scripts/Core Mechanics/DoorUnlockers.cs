using UnityEngine;

public class DoorUnlockers : MonoBehaviour
{
    public LockKeySystem lockKey;
    public Animator doorAnim;

    private void OnTriggerEnter(Collider other)
    {
        if (lockKey.hasKey == true)
        {
            if (other.CompareTag("Player"))
            {
                doorAnim.SetTrigger("doorOpen");
            }
        }
    }
}
