using UnityEngine;
using UnityEngine.UI;

public class ActionPlaybackUI : MonoBehaviour
{
    public Button playPauseResumeButton;
    public Button clearButton;

    private enum PlaybackState { Idle, Playing, Paused }
    private PlaybackState currentState = PlaybackState.Idle;

    private CharacterActionController Controller => SelectedCharacterManager.CurrentSelectedCharacter;

    void Start()
    {
        playPauseResumeButton.onClick.AddListener(OnPlayPauseResumeClicked);
        clearButton.onClick.AddListener(() => Controller?.ClearActions());
    }

    void OnPlayPauseResumeClicked()
    {
        if (Controller == null) return;

        switch (currentState)
        {
            case PlaybackState.Idle:
                Controller.PlaySequence();
                currentState = PlaybackState.Playing;
                Debug.Log("开始播放");
                break;

            case PlaybackState.Playing:
                Controller.PauseSequence();
                currentState = PlaybackState.Paused;
                Debug.Log("已暂停");
                break;

            case PlaybackState.Paused:
                Controller.ResumeSequence();
                currentState = PlaybackState.Playing;
                Debug.Log("继续播放");
                break;
        }
    }

    // 👇 添加右键测试：播放/暂停/恢复
    [ContextMenu("测试播放")]
    void TestPlayPauseResume()
    {
        OnPlayPauseResumeClicked();
    }

    // 👇 添加右键测试：清空动作序列
    [ContextMenu("测试清空动作序列")]
    void TestClearActions()
    {
        if (Controller != null)
        {
            Controller.ClearActions();
            Debug.Log("动作序列已清空");
        }
        else
        {
            Debug.LogWarning("未选择角色！");
        }
    }
}
