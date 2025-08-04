using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GptTurboScript : MonoBehaviour
{
    /// <summary>
    /// api地址
    /// </summary>
    //public string m_ApiUrl = "https://api.openai.com/v1/chat/completions";
    string m_ApiUrl = "https://open.bigmodel.cn/api/paas/v4/chat/completions";
    /// <summary>
    /// gpt-3.5-turbo
    /// </summary>
    //public string m_gptModel = "gpt-3.5-turbo";
    string m_gptModel = "glm-4-flash";
    /// <summary>
    /// 缓存对话
    /// </summary>
    [SerializeField] public List<SendData> m_DataList = new List<SendData>();
    /// <summary>
    /// AI人设
    /// </summary>
    public string Prompt;

    private void Start()
    {
        //运行时，添加人设
        m_DataList.Add(new SendData("system", Prompt));
    }

    /// <summary>
    /// 调用接口
    /// </summary>
    /// <param name="_postWord">发送的消息</param>
    /// <param name="_openAI_Key">密钥</param>
    /// <param name="_callback">GPT的回调</param>
    public void GetPostData(string _postWord, string _openAI_Key, System.Action<string> _callback)
    {
        StartCoroutine(IEGetPostData(_postWord, _openAI_Key, _callback));
    }

    /// <summary>
    /// 调用接口
    /// </summary>
    /// <param name="_postWord">发送的消息</param>
    /// <param name="_openAI_Key">密钥</param>
    /// <param name="_callback">GPT的回调</param>
    /// <returns></returns>
    IEnumerator IEGetPostData(string _postWord, string _openAI_Key, System.Action<string> _callback)
    {
        //缓存发送的信息列表
        m_DataList.Add(new SendData("user", _postWord));

        using (UnityWebRequest request = new UnityWebRequest(m_ApiUrl, "POST"))
        {
            PostData _postData = new PostData
            {
                model = m_gptModel,
                messages = m_DataList
            };

            string _jsonText = JsonUtility.ToJson(_postData);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", _openAI_Key));


            yield return request.SendWebRequest();

            if (request.responseCode == 200)
            {
                Debug.Log(request.downloadHandler.text);
                string _msg = request.downloadHandler.text;
                MessageBack _textback = JsonUtility.FromJson<MessageBack>(_msg);
                if (_textback != null && _textback.choices.Count > 0)
                {
                    string _backMsg = _textback.choices[0].message.content;
                    //添加记录
                    Debug.Log(_backMsg);
                    m_DataList.Add(new SendData("assistant", _backMsg));
                    _callback(_backMsg);
                }
            }
            else
            {
                Debug.Log(request.responseCode);
                string _backMsg = "连接断开！";
                m_DataList.Add(new SendData("assistant", _backMsg));
                _callback(_backMsg);
            }
        }
    }
    #region 数据包

    [Serializable]
    public class PostData
    {
        public string model;
        public List<SendData> messages;
    }

    [Serializable]
    public class SendData
    {
        public string role;
        public string content;
        public SendData() { }
        public SendData(string _role, string _content)
        {
            role = _role;
            content = _content;
        }

    }
    [Serializable]
    public class MessageBack
    {
        public string id;
        public string created;
        public string model;
        public List<MessageBody> choices;
    }
    [Serializable]
    public class MessageBody
    {
        public Message message;
        public string finish_reason;
        public string index;
    }
    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    #endregion
}

