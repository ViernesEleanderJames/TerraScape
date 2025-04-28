using UnityEngine;
using System.IO;
using System.Collections;



public class ScreenshotCapture : MonoBehaviour
{
    public Canvas uiCanvas; 
    public void CaptureScreenshot()
    {
        StartCoroutine(TakeScreenshot());
    }

    private IEnumerator TakeScreenshot()
    {
        // Hide UI before capturing
        if (uiCanvas != null)
            uiCanvas.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();

        // Create a texture the size of the screen
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        // Save to the gallery
        string fileName = $"Screenshot_{System.DateTime.Now.ToString("yyyyMMdd_HHmmss")}.png";
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(path, screenshot.EncodeToPNG());

        // Save to the gallery (Android & iOS)
        #if UNITY_ANDROID
            NativeGallery.SaveImageToGallery(path, "MyApp", fileName);
        #elif UNITY_IOS
            NativeGallery.SaveImageToGallery(path, "MyApp", fileName);
        #endif

        // Show UI after capturing
        if (uiCanvas != null)
            uiCanvas.gameObject.SetActive(true);

        Debug.Log($"Screenshot saved to: {path}");
        Destroy(screenshot);
    }
}
