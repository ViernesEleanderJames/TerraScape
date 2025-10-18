using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // ✅ Added for reloading scene
using System.IO;

public class AlbumViewer : MonoBehaviour
{
    [Header("UI References")]
    public Image displayImage; // Image 
    public Button closeButton; // Close Button
    public Button deleteButton; // Delete Button
    public Button renameButton; // Rename Button

    [Header("Rename UI")]
    public GameObject renamePanel;       // Panel for rename input
    public TMP_InputField renameInput;   // Input field for new name
    public Button confirmRenameButton;   // Confirm button
    public Button cancelRenameButton;    // Cancel button

    private string currentId;
    private Sprite currentSprite;

    void Awake()
    {
        gameObject.SetActive(false); // hide viewer

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(DeleteCurrent);

        if (renameButton != null)
            renameButton.onClick.AddListener(OpenRenamePanel);

        if (confirmRenameButton != null)
            confirmRenameButton.onClick.AddListener(ConfirmRename);

        if (cancelRenameButton != null)
            cancelRenameButton.onClick.AddListener(() => renamePanel.SetActive(false));
    }

    public void Show(Sprite sprite, string id)
    {
        if (sprite == null) return;

        currentSprite = sprite;
        currentId = id;

        if (displayImage != null)
        {
            displayImage.sprite = sprite;
            displayImage.preserveAspect = true;
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        renamePanel?.SetActive(false);
        currentId = null;
        currentSprite = null;
    }

    private void OpenRenamePanel()
    {
        if (renamePanel != null && renameInput != null)
        {
            renamePanel.SetActive(true);
            renameInput.text = Path.GetFileNameWithoutExtension(currentId);
        }
    }

    private void ConfirmRename()
    {
        if (string.IsNullOrEmpty(renameInput.text) || string.IsNullOrEmpty(currentId)) return;

        string newName = renameInput.text.Trim() + ".png";
        string oldPath = Path.Combine(Application.persistentDataPath, "Albums", currentId);
        string newPath = Path.Combine(Application.persistentDataPath, "Albums", newName);

        if (File.Exists(newPath))
        {
            Debug.LogWarning("[AlbumViewer] File with this name already exists.");
            return;
        }

        try
        {
            File.Move(oldPath, newPath);
            Debug.Log($"[AlbumViewer] Renamed {currentId} → {newName}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[AlbumViewer] Rename failed: " + e.Message);
            return;
        }

        // Find the AlbumCard
        AlbumCard targetCard = null;
        AlbumCard[] cards = GameObject.FindObjectsOfType<AlbumCard>();
        foreach (var c in cards)
        {
            if (c.id == currentId)
            {
                targetCard = c;
                break;
            }
        }

        // Update the Name
        if (targetCard != null && AlbumManager.Instance != null)
        {
            bool renamed = AlbumManager.Instance.RenameImage(currentId, newName);
            if (renamed)
            {
                targetCard.id = newName;
                if (targetCard.fileNameText != null)
                    targetCard.fileNameText.text = newName;
                currentId = newName;
            }
        }

        renamePanel.SetActive(false);

        // AlbumReload
        SceneManager.LoadScene("AlbumScene"); //Name of AlbumScene
    }

    private void DeleteCurrent()
    {
        if (string.IsNullOrEmpty(currentId)) return;

        string path = Path.Combine(Application.persistentDataPath, "Albums", currentId);
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
                Debug.Log("[AlbumViewer] Deleted file: " + path);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[AlbumViewer] Delete failed: " + e.Message);
            }
        }

        if (AlbumManager.Instance != null)
        {
            AlbumCard[] cards = GameObject.FindObjectsOfType<AlbumCard>();
            foreach (var c in cards)
            {
                if (c.id == currentId)
                {
                    Destroy(c.gameObject);
                    break;
                }
            }
        }

        Hide();
    }
}
