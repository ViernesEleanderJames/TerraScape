using UnityEngine;
using UnityEngine.UI;
using TMPro;       //Import TMP namespace
using System;

public class AlbumCard : MonoBehaviour
{
    [Header("UI References")]
    public Image Thumbnail;          // Thumbnail
    public Button ViewButton;        // ViewButton
    public Button DeleteButton;      // DeleteButton 
    public Button RenameButton;      // (Optional) RenameButton 
    public TMP_Text fileNameText;    // TMP Text

    [HideInInspector] public string id;  // filename 
    private Sprite sprite;

    // Events AlbumManager
    public Action<Sprite, string> onViewClicked;
    public Action<AlbumCard, string> onDeleteClicked;
    public Action<AlbumCard, string> onRenameClicked;


    /// AlbumCard
    public void Setup(Sprite s, string fileId)
    {
        sprite = s;
        id = fileId;

        // Thumbnail
        if (Thumbnail != null)
        {
            Thumbnail.sprite = s;
            Thumbnail.preserveAspect = true;

            Debug.Log($"[AlbumCard] Thumbnail set for {id} (size: {s.texture.width}x{s.texture.height})");

            AspectRatioFitter arf = Thumbnail.GetComponent<AspectRatioFitter>();
            if (arf != null && s != null)
            {
                float aspect = (float)s.texture.width / s.texture.height;
                arf.aspectRatio = aspect;
                arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

                Debug.Log($"[AlbumCard] AspectRatioFitter updated for {id} (aspect={aspect})");
            }
        }
        else
        {
            Debug.LogWarning($"[AlbumCard] Thumbnail Image is NULL for {id}");
        }

        // Filename text
        if (fileNameText != null)
        {
            fileNameText.text = fileId;
            Debug.Log($"[AlbumCard] Filename text set: {fileId}");
        }
        else
        {
            Debug.LogWarning($"[AlbumCard] fileNameText is NULL for {id}");
        }

        //Buttons
        if (ViewButton != null)
        {
            ViewButton.onClick.RemoveAllListeners();
            ViewButton.onClick.AddListener(() => onViewClicked?.Invoke(sprite, id));
        }

        if (DeleteButton != null)
        {
            DeleteButton.onClick.RemoveAllListeners();
            DeleteButton.onClick.AddListener(() => onDeleteClicked?.Invoke(this, id));
        }

        if (RenameButton != null)
        {
            RenameButton.onClick.RemoveAllListeners();
            RenameButton.onClick.AddListener(() => onRenameClicked?.Invoke(this, id));
        }
    }
}
