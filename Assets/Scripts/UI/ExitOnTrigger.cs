using UnityEngine;

public class ExitOnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered the exit trigger: " + other.name);

        // 检查玩家是否进入触发区域
        if (other.CompareTag("Player")) // 你的摄像机/角色需要设置为"Player"标签
        {
            // 退出程序
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // 如果在编辑器中，停止播放
#else
            Application.Quit(); // 如果是打包后运行，退出程序
#endif
        }
    }
}
