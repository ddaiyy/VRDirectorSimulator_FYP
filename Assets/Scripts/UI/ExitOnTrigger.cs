using UnityEngine;

public class ExitOnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered the exit trigger: " + other.name);

        // 检查是否为玩家（Tag = Player）
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered the exit zone. Quitting application...");

            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            Debug.Log("Entered object is not tagged as Player.");
        }
    }
}
