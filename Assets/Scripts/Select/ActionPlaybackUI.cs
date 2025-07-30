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
        clearButton.onClick.AddListener(ClearAllActionsAndResetSliders); 

        UpdateButtonStates(); // 初始化按钮状态
    }

    void Update()
    {
        UpdateButtonStates(); // 每帧检查一次选中角色状态（也可以优化成事件驱动）
    }

    void UpdateButtonStates()
    {
        var target = Controller;
        bool valid = target != null && target.isPlacedCharacter;

        playPauseResumeButton.interactable = valid;
        clearButton.interactable = valid;
    }

    void ClearAllActionsAndResetSliders()
    {
        Controller?.ClearActions();

        // 找到所有界面上的 ActionSelectUI 实例，把滑动条清零
        var allActionUIs = FindObjectsOfType<ActionSelectUI>();
        foreach (var ui in allActionUIs)
        {
            ui.ResetSlider();
        }

        Debug.Log("动作已清空，所有滑动条已重置为0");
    }


    void OnPlayPauseResumeClicked()
    {
        if (Controller == null) return;

        if (!Controller.IsPlaying)
        {
            Controller.PlaySequence();
            currentState = PlaybackState.Playing;
            Debug.Log("开始播放");
        }
        else
        {
            if (currentState == PlaybackState.Playing)
            {
                Controller.PauseSequence();
                currentState = PlaybackState.Paused;
                Debug.Log("已暂停");
            }
            else if (currentState == PlaybackState.Paused)
            {
                Controller.ResumeSequence();
                currentState = PlaybackState.Playing;
                Debug.Log("继续播放");
            }
        }
    }

    /*// 关闭按钮事件
    [ContextMenu("测试关闭Canvas")]
    void OnCloseButtonClicked()
    {
        Destroy(gameObject);
    }*/



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
