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

    private void Start()
    {
        generateButton.onClick.AddListener(OnGenerateClicked);
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
            // 用英文提示词替代中文提示
            string prompt = $"Please generate a movie script outline based on the keyword: '{keyword}', within 100 words.";
            gptScript.GetPostData(prompt, apiKey, OnScriptReceived);
        }
    }


    void OnScriptReceived(string result)
    {
        outputText.text = result;
    }
}
