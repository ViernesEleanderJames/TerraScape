using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ARPlaneVisualizer : MonoBehaviour
{
    [Tooltip("Fade in/out speed multiplier applied during the alpha tweening. Lower value = slower.")]
    [Range(0.1f, 1.0f)]
    public float fadeSpeed = 1f;

    private Material planeMaterial;
    private int shaderAlphaPropertyID;
    private float targetAlpha;
    private float currentAlpha;

    void Awake()
    {
        shaderAlphaPropertyID = Shader.PropertyToID("_PlaneAlpha");
        planeMaterial = GetComponent<MeshRenderer>().material;
        currentAlpha = 1f;
        targetAlpha = 1f;
    }

    void Update()
    {
        // Smoothly fade toward the target alpha value
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
        planeMaterial.SetFloat(shaderAlphaPropertyID, currentAlpha);
    }

    /// <summary>
    /// Show or hide plane surfaces by changing alpha value
    /// </summary>
    public void SetVisible(bool visible)
    {
        targetAlpha = visible ? 1f : 0f;
    }
}
