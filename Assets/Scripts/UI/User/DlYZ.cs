using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;

public class DlYZ : MonoBehaviour
{
    public TMP_Text register_loginText;
    public TMP_InputField usernameInput; // 用户名输入框（邮箱）
    public TMP_InputField passwordInput; // 密码输入框（六位数）
    public Button registerButton;
    public Button loginButton;
    public string nextSceneName;
    public TMP_Text infoText;

    public Button passwordToggleButton;
    public Image toggleIcon; // 可选，用于切换图标
    public Sprite eyeOpen;
    public Sprite eyeClosed;

    public Toggle rememberMeToggle;

    public TMP_Text welcomeText;

    public Button logoutButton;


    private bool isPasswordVisible = false;


    void Start()
    {
        /*// ⚠️ 临时清空所有保存的数据（用于测试）
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();*/

        // 默认密码为隐藏
        passwordInput.contentType = TMP_InputField.ContentType.Password;
        passwordInput.ForceLabelUpdate(); // 更新显示

        isPasswordVisible = false; // 默认是不可见
        toggleIcon.sprite = eyeClosed; // 默认图标为闭眼

        // 加载是否记住信息
        if (PlayerPrefs.GetInt("rememberMe", 0) == 1)
        {
            usernameInput.text = PlayerPrefs.GetString("savedUsername", "");
            passwordInput.text = PlayerPrefs.GetString("savedPassword", "");
            rememberMeToggle.isOn = true;
        }

        registerButton.onClick.AddListener(Register);
        loginButton.onClick.AddListener(Login);
        passwordToggleButton.onClick.AddListener(TogglePasswordVisibility);

        infoText.text = ""; // 清空提示
        welcomeText.text = "";
        welcomeText.gameObject.SetActive(false);

    }

    // ✅ 正确邮箱判断
    bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    bool ContainsLetter(string input)
    {
        return Regex.IsMatch(input, "[A-Za-z]");
    }

    bool ContainsDigit(string input)
    {
        return Regex.IsMatch(input, "[0-9]");
    }

    void ClearInfoText()
    {
        infoText.text = "";
    }


    void Register()
    {
        string email = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        infoText.text = ""; // 清空旧提示

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            infoText.text = "Please enter your email address and password";
            Invoke("ClearInfoText", 2);
            return;
        }

        if (!IsValidEmail(email))
        {
            infoText.text = "Incorrect email format";
            Invoke("ClearInfoText", 2);
            return;
        }

        // ✅ 密码长度不够
        if (password.Length < 8)
        {
            infoText.text = "Password must be at least 8 characters long";
            Invoke("ClearInfoText", 2);
            return;
        }

        // ✅ 检查是否包含字母和数字
        if (!ContainsLetter(password) || !ContainsDigit(password))
        {
            infoText.text = "Passwords must contain letters and numbers";
            Invoke("ClearInfoText", 2);
            return;
        }

        PlayerPrefs.SetString("username", email);
        PlayerPrefs.SetString("password", password);
        PlayerPrefs.Save();

        infoText.text = "Registration successful";
        Invoke("ClearInfoText", 3);

        // ✅ 清空输入框
        usernameInput.text = "";
        passwordInput.text = "";
    }

    void Login()
    {
        string savedUsername = PlayerPrefs.GetString("username");
        string savedPassword = PlayerPrefs.GetString("password");

        string inputUsername = usernameInput.text.Trim();
        string inputPassword = passwordInput.text.Trim();

        infoText.text = ""; // 清空旧提示

        if (string.IsNullOrEmpty(inputUsername) || string.IsNullOrEmpty(inputPassword))
        {
            infoText.text = "Please enter your email address and password";
            Invoke("ClearInfoText", 2);
            return;
        }

        if (!IsValidEmail(inputUsername))
        {
            infoText.text = "Incorrect email format";
            Invoke("ClearInfoText", 2);
            return;
        }

        if (inputPassword.Length < 8)
        {
            infoText.text = "Password must be at least 8 characters long";
            Invoke("ClearInfoText", 2);
            return;
        }

        // ✅ 如果从未注册过账户（即没有任何记录）
        if (string.IsNullOrEmpty(savedUsername))
        {
            infoText.text = "No account found. Please register first.";
            Invoke("ClearInfoText", 2);
            return;
        }

        // ✅ 有账户记录时，无论用户名或密码错误都提示统一信息
        if (inputUsername != savedUsername || inputPassword != savedPassword)
        {
            infoText.text = "Incorrect username or password";
            Invoke("ClearInfoText", 2);
            return;
        }

        if (inputUsername == savedUsername && inputPassword == savedPassword)
        {
            // ✅ 登录成功时保存“记住我”的信息
            if (rememberMeToggle.isOn)
            {
                PlayerPrefs.SetInt("rememberMe", 1);
                PlayerPrefs.SetString("savedUsername", inputUsername);
                PlayerPrefs.SetString("savedPassword", inputPassword);
            }
            else
            {
                PlayerPrefs.SetInt("rememberMe", 0);
                PlayerPrefs.DeleteKey("savedUsername");
                PlayerPrefs.DeleteKey("savedPassword");
            }

            PlayerPrefs.Save();

            // 可选：隐藏登录界面按钮等
            loginButton.gameObject.SetActive(false);
            registerButton.gameObject.SetActive(false);
            usernameInput.gameObject.SetActive(false);
            passwordInput.gameObject.SetActive(false);
            passwordToggleButton.gameObject.SetActive(false);
            rememberMeToggle.gameObject.SetActive(false);
            register_loginText.gameObject.SetActive(false);
            logoutButton.gameObject.SetActive(true);



            welcomeText.text = $"Welcome, {inputUsername}!";
            welcomeText.gameObject.SetActive(true); // ✅ 确保显示出来


        }

    }

    public void Logout()
    {
        // 显示回所有登录控件
        loginButton.gameObject.SetActive(true);
        registerButton.gameObject.SetActive(true);
        usernameInput.gameObject.SetActive(true);
        passwordInput.gameObject.SetActive(true);
        passwordToggleButton.gameObject.SetActive(true);
        rememberMeToggle.gameObject.SetActive(true);
        register_loginText.gameObject.SetActive(true);

        // ✅ 判断“记住我”的勾选状态，决定是否保留账号密码
        if (PlayerPrefs.GetInt("rememberMe", 0) == 1)
        {
            usernameInput.text = PlayerPrefs.GetString("savedUsername", "");
            passwordInput.text = PlayerPrefs.GetString("savedPassword", "");
            rememberMeToggle.isOn = true;
        }
        else
        {
            usernameInput.text = "";
            passwordInput.text = "";
            rememberMeToggle.isOn = false;
        }

        infoText.text = "";

        // 隐藏登出按钮
        logoutButton.gameObject.SetActive(false);
        welcomeText.gameObject.SetActive(false);
    }

    void TogglePasswordVisibility()
    {
        isPasswordVisible = !isPasswordVisible;

        if (isPasswordVisible)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
            toggleIcon.sprite = eyeOpen;
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            toggleIcon.sprite = eyeClosed;
        }

        passwordInput.ForceLabelUpdate(); // 强制更新输入框显示
    }

}
