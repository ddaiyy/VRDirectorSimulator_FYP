using UnityEngine;

public class FootstepController : MonoBehaviour
{
    public CharacterController characterController;
    public AudioSource footstepAudioSource;
    public AudioClip[] footstepClips;  // 多个脚步声可选
    public float stepInterval = 0.5f;  // 脚步间隔时间

    private float stepTimer = 0f;

    void Update()
    {
        // 检测是否在移动
        if (characterController.isGrounded && characterController.velocity.magnitude > 0.1f)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer > stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f; // 停止时重置
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length > 0)
        {
            int index = Random.Range(0, footstepClips.Length);
            footstepAudioSource.clip = footstepClips[index];
            footstepAudioSource.Play();
        }
    }
}
