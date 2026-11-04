using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.RemoteConfig;

// สลับหน้าตาปุ่มตามค่า isEvent ใน Remote Config
// isEvent = false -> ปุ่มปกติ (สีขาว, Left / Right)
// isEvent = true  -> ปุ่มสีส้ม, ข้อความเป็น L / R
public class RemoteConfigButtonSkin : MonoBehaviour
{
    public struct userAttributes { }
    public struct appAttributes { }

    [System.Serializable]
    public class ButtonSkin
    {
        public Button button;
        [Tooltip("ข้อความที่จะเปลี่ยนเป็นตอนอีเวนต์เปิด เช่น L หรือ R")]
        public string eventText;
    }

    [Header("Remote Config")]
    public string environmentId;
    [Tooltip("ดึงค่าใหม่ทุกกี่วินาที (0 = ดึงครั้งเดียวตอนเริ่ม)")]
    public float refreshInterval = 3f;
    public string keyIsEvent = "isEvent";

    [Header("ปุ่ม")]
    public ButtonSkin[] buttons;
    public Color eventColor = new Color(1f, 0.55f, 0f); // สีส้ม

    private Color[] normalColors;
    private string[] normalTexts;
    private bool subscribed = false;

    private async void Start()
    {
        CaptureNormalLook();

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("ไม่มีอินเทอร์เน็ต ใช้หน้าตาปุ่มปกติ");
            return;
        }

        try
        {
            await InitializeRemoteConfigAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Remote Config เริ่มต้นไม่สำเร็จ: " + e);
            return;
        }

        // ถ้าเปลี่ยน Scene ระหว่างรอ สคริปต์นี้จะถูกทำลายไปแล้ว
        if (this == null) return;

        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteConfig;
        subscribed = true;

        FetchConfig();

        if (refreshInterval > 0f)
        {
            StartCoroutine(RefreshLoop());
        }
    }

    private async Task InitializeRemoteConfigAsync()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        RemoteConfigService.Instance.SetEnvironmentID(environmentId);
    }

    private IEnumerator RefreshLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);
            FetchConfig();
        }
    }

    private void FetchConfig()
    {
        RemoteConfigService.Instance.FetchConfigs<userAttributes, appAttributes>(
            new userAttributes(), new appAttributes());
    }

    private void ApplyRemoteConfig(ConfigResponse response)
    {
        bool eventOn = RemoteConfigService.Instance.appConfig.GetBool(keyIsEvent);
        ApplySkin(eventOn);
    }

    private void ApplySkin(bool eventOn)
    {
        if (buttons == null || normalColors == null) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            Button b = buttons[i].button;
            if (b == null) continue;

            Image img = GetButtonImage(b);
            if (img != null)
            {
                img.color = eventOn ? eventColor : normalColors[i];
            }

            bool useEventText = eventOn && !string.IsNullOrEmpty(buttons[i].eventText);
            SetLabel(b, useEventText ? buttons[i].eventText : normalTexts[i]);
        }
    }

    // เก็บสีและข้อความเดิมไว้ เพื่อคืนค่าตอนปิดอีเวนต์
    private void CaptureNormalLook()
    {
        if (buttons == null) return;

        normalColors = new Color[buttons.Length];
        normalTexts = new string[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            Button b = buttons[i].button;
            if (b == null) continue;

            Image img = GetButtonImage(b);
            if (img != null) normalColors[i] = img.color;

            normalTexts[i] = GetLabel(b);
        }
    }

    private Image GetButtonImage(Button b)
    {
        Image img = b.targetGraphic as Image;
        if (img == null) img = b.GetComponent<Image>();
        return img;
    }

    // รองรับทั้ง TextMeshPro และ Text แบบเก่า
    private string GetLabel(Button b)
    {
        TMP_Text tmp = b.GetComponentInChildren<TMP_Text>();
        if (tmp != null) return tmp.text;

        Text legacy = b.GetComponentInChildren<Text>();
        if (legacy != null) return legacy.text;

        return "";
    }

    private void SetLabel(Button b, string text)
    {
        TMP_Text tmp = b.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = text;
            return;
        }

        Text legacy = b.GetComponentInChildren<Text>();
        if (legacy != null)
        {
            legacy.text = text;
        }
    }

    private void OnDestroy()
    {
        if (subscribed)
        {
            RemoteConfigService.Instance.FetchCompleted -= ApplyRemoteConfig;
        }
    }
}