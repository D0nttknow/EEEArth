using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;

public class ProfileImageManager : MonoBehaviour
{
    public const string AvatarKey = "avatar_base64";

    [SerializeField] private int maxSize = 128; // ย่อรูปให้ไม่เกิน 128x128

    // ---------- เลือกรูป + อัปโหลด ----------

    [ContextMenu("Pick And Upload Avatar")]
    public void PickAndUpload()
    {
#if UNITY_ANDROID || UNITY_IOS
        NativeGallery.GetImageFromGallery(async path =>
        {
            if (string.IsNullOrEmpty(path)) return;
            await UploadFromPath(path);
        }, "เลือกรูปโปรไฟล์");
#else
        Debug.LogWarning("NativeGallery ใช้ได้บนมือถือเท่านั้น ใน Editor ให้ใช้ UploadFromPath(path) ทดสอบ");
#endif
    }

    public async Task UploadFromPath(string path)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("ต้อง Sign in ก่อน");
            return;
        }

        try
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);

            Texture2D source = new Texture2D(2, 2);
            source.LoadImage(fileBytes);

            Texture2D small = Resize(source, maxSize);
            byte[] jpg = small.EncodeToJPG(75);
            string base64 = Convert.ToBase64String(jpg);

            Destroy(source);
            Destroy(small);

            var data = new Dictionary<string, object> { { AvatarKey, base64 } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(
                data,
                new Unity.Services.CloudSave.Models.Data.Player.SaveOptions(
                    new Unity.Services.CloudSave.Models.Data.Player.PublicWriteAccessClassOptions())
            );

            Debug.Log($"อัปโหลดรูปสำเร็จ ({base64.Length} ตัวอักษร)");
        }
        catch (Exception e)
        {
            Debug.LogError("อัปโหลดรูปล้มเหลว: " + e);
        }
    }

    // ---------- ดึงรูปของผู้เล่นคนอื่น (ใช้ใน Leaderboard) ----------
    public ProfileImageManager profileImages;
    public async Task<Texture2D> LoadAvatar(string playerId)
    {
        try
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { AvatarKey },
                new Unity.Services.CloudSave.Models.Data.Player.LoadOptions(
                    new Unity.Services.CloudSave.Models.Data.Player.PublicReadAccessClassOptions(playerId))
            );

            if (!result.TryGetValue(AvatarKey, out Item item)) return null;

            byte[] bytes = Convert.FromBase64String(item.Value.GetAs<string>());
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            return tex;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"โหลดรูปของ {playerId} ไม่สำเร็จ: {e.Message}");
            return null;
        }
    }

    private Texture2D Resize(Texture2D src, int max)
    {
        float scale = Mathf.Min(1f, (float)max / Mathf.Max(src.width, src.height));
        int w = Mathf.Max(1, Mathf.RoundToInt(src.width * scale));
        int h = Mathf.Max(1, Mathf.RoundToInt(src.height * scale));

        RenderTexture rt = RenderTexture.GetTemporary(w, h);
        Graphics.Blit(src, rt);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D result = new Texture2D(w, h, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        result.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }
    [SerializeField] private string ImagePath = "D:/Pic/Screenshot_20261001_014055.jpg";

    [ContextMenu("Upload Image")]
    private void TestUploadFromPath()
    {
        _ = UploadFromPath(ImagePath);
    }
}