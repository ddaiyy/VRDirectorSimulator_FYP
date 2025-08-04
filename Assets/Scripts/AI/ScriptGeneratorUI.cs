using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScriptGeneratorUI : MonoBehaviour
{
    public TMP_InputField keywordInput;      // 用户输入关键字
    public Button generateButton;        // 点击按钮触发生成
    public TMP_Text outputText;              // 显示AI生成的剧本
    public GptTurboScript gptScript;     // 引用你的GPT脚本组件
    public string apiKey = "你的API密钥";

    public GameObject scrollViewPanel;
    public Button closeButton;
    public GameObject keywordLabel;               // 提示文字（如“请输入关键词”）
    public GameObject generateButtonObj;          // 按钮的 GameObject
    public GameObject generatingHintText;         // “生成需要等待10秒”的提示文本

    private void Start()
    {
        generateButton.onClick.AddListener(OnGenerateClicked);
        // 初始隐藏 ScrollView 和提示
        scrollViewPanel.SetActive(false);
        generatingHintText.SetActive(false);
    }

    /*void OnGenerateClicked()
    {
        string keyword = keywordInput.text;
        if (!string.IsNullOrEmpty(keyword))
        {
            gptScript.GetPostData("请根据关键词 '" + keyword + "' 生成一个电影剧本概要。", apiKey, OnScriptReceived);
        }
    }*/
    void OnGenerateClicked()
    {
        string keyword = keywordInput.text;
        if (!string.IsNullOrEmpty(keyword))
        {
            // 显示“等待提示”
            generatingHintText.SetActive(true);

            // 用英文提示词替代中文提示
            string prompt = $"Please generate a movie script outline based on the following keyword: '{keyword}', within 100 words.";
            gptScript.GetPostData(prompt, apiKey, OnScriptReceived);
        }
    }


    void OnScriptReceived(string result)
    {
        outputText.text = result;
        // 显示 Scroll View（如果之前是隐藏的）
        scrollViewPanel.SetActive(true);

        keywordLabel.SetActive(false);
        keywordInput.gameObject.SetActive(false);
        generateButtonObj.SetActive(false);

        // 隐藏“等待提示”
        generatingHintText.SetActive(false);
    }

    public void OnCloseClicked()
    {
        // 隐藏 ScrollView 面板
        scrollViewPanel.SetActive(false);

        // 恢复原始 UI 元素
        keywordLabel.SetActive(true);
        keywordInput.gameObject.SetActive(true);
        generateButtonObj.SetActive(true);

        // 清空输出（可选）
        outputText.text = "";
    }
}
