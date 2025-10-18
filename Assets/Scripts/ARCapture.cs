using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ARCapture : MonoBehaviour
{
    public Button captureButton;         // Assign in Inspector
    public Canvas uiCanvas;              // Canvas to hide while capturing
    public bool saveToPhoneGallery = true;
    public int downscale = 2;            // 1 = full res, 2 = half, etc.

    void Start()
    {
        if (captureButton != null)
        {
            captureButton.onClick.AddListener(StartCapture);
        }
    }

    public void StartCapture()
    {
        StartCoroutine(CaptureFromScreen());
    }

    private IEnumerator CaptureFromScreen()
    {
        // Hide UI (para di makita ang buttons sa screenshot)
        if (uiCanvas != null) uiCanvas.gameObject.SetActive(false);

        // wait for frame to finish without UI
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Capture screen (AR camera feed + objects)
        Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();

        // Downscale
        int targetW = Mathf.Max(1, tex.width / Mathf.Max(1, downscale));
        int targetH = Mathf.Max(1, tex.height / Mathf.Max(1, downscale));
        Texture2D scaled = ScaleTexture(tex, targetW, targetH);
        Destroy(tex);

        // Generate filename
        string fileName = "img_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";

        // Ensure Albums folder exists
        string folderPath = Path.Combine(Application.persistentDataPath, "Albums");
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        // Full path
        string fullPath = Path.Combine(folderPath, fileName);

        // Save PNG file
        File.WriteAllBytes(fullPath, scaled.EncodeToPNG());
        Debug.Log($"[ARCapture] Saved screenshot to: {fullPath}");

        // Save to AlbumManager (para may card sa AlbumScene)
        if (AlbumManager.Instance != null)
        {
            // ⚡ Directly add the saved image
            AlbumManager.Instance.AddTextureAndSave(scaled, fileName);
            Debug.Log($"[ARCapture] Added screenshot to AlbumManager: {fileName}");
        }
        else
        {
            Debug.LogWarning("[ARCapture] AlbumManager.Instance not found. Make sure AlbumManager exists in AlbumScene.");
            Destroy(scaled);
        }

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if (saveToPhoneGallery)
        {
            NativeGallery.SaveImageToGallery(fullPath, "Terrascape AR", fileName);
            Debug.Log($"[ARCapture] Saved to phone gallery: {fullPath}");
        }
#endif

        // Show UI again
        if (uiCanvas != null) uiCanvas.gameObject.SetActive(true);
    }

    // Helper: scale texture using RenderTexture
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        Graphics.Blit(source, rt);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D newTex = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
        newTex.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        newTex.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        return newTex;
    }
}
