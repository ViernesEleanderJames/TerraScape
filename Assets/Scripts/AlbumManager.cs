using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

public class AlbumManager : MonoBehaviour
{
    public static AlbumManager Instance;

    [Header("UI References (auto-assign on AlbumScene load)")]
    public RectTransform contentParent;      // ScrollView Content
    public GameObject AlbumCardPrefab;       // AlbumCard prefab 
    public AlbumViewer viewer;               // AlbumViewer panel

    [Header("Settings")]
    public string albumSceneName = "AlbumScene"; // Name of the Scene

    private string albumFolderPath;
    private readonly List<Sprite> images = new List<Sprite>();
    private readonly List<AlbumCard> cards = new List<AlbumCard>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        albumFolderPath = Path.Combine(Application.persistentDataPath, "Albums");
        if (!Directory.Exists(albumFolderPath))
            Directory.CreateDirectory(albumFolderPath);

        Debug.Log($"[AlbumManager] Album folder: {albumFolderPath}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == albumSceneName)
        {
            StartCoroutine(RebindUIAndRefresh());
        }
    }

    private IEnumerator RebindUIAndRefresh()
    {
        yield return null;

        if (contentParent == null)
        {
            var go = GameObject.Find("Content");
            if (go != null) contentParent = go.GetComponent<RectTransform>();
        }

        if (AlbumCardPrefab == null)
        {
            var prefab = Resources.Load<GameObject>("AlbumCardPrefab");
            if (prefab != null) AlbumCardPrefab = prefab;
        }

        if (viewer == null)
        {
            viewer = FindObjectOfType<AlbumViewer>(true);
        }

        Debug.Log($"[AlbumManager] Re-bound UI for {albumSceneName}. contentParent={(contentParent!=null)}, prefab={(AlbumCardPrefab!=null)}, viewer={(viewer!=null)}");
        RefreshAlbum();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == albumSceneName)
            RefreshAlbum();
    }

    void OnEnable()
    {
        if (SceneManager.GetActiveScene().name == albumSceneName)
            RefreshAlbum();
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //Refresh the Album
    public void RefreshAlbum()
    {
        if (contentParent == null)
        {
            Debug.LogWarning("[AlbumManager] contentParent is null; skipping refresh.");
            return;
        }

        foreach (var c in cards)
            if (c != null) Destroy(c.gameObject);

        cards.Clear();
        images.Clear();

        LoadSavedImages();
    }

    private void LoadSavedImages()
    {
        if (!Directory.Exists(albumFolderPath))
        {
            Debug.LogWarning("[AlbumManager] Album folder missing.");
            return;
        }

        string[] files = Directory.GetFiles(albumFolderPath, "*.*")
            .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                        f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Debug.Log($"[AlbumManager] Found {files.Length} image files.");

        foreach (string f in files)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(f);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(bytes))
                {
                    Sprite s = Sprite.Create(tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f));

                    images.Add(s);
                    CreateCardForSprite(s, Path.GetFileName(f));
                }
                else
                {
                    Debug.LogWarning($"[AlbumManager] Failed to LoadImage for: {f}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AlbumManager] Error loading {f}: {e.Message}");
            }
        }

        Debug.Log($"[AlbumManager] Finished loading {images.Count} images.");
    }

    // Add new texture to album
    public void AddTextureAndSave(Texture2D tex, string filename = null)
    {
        if (tex == null)
        {
            Debug.LogWarning("[AlbumManager] Null texture!");
            return;
        }

        string fn = filename ?? $"img_{DateTime.Now:yyyyMMddHHmmss}.png";
        string fullPath = Path.Combine(albumFolderPath, fn);

        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Debug.Log($"[AlbumManager] Saved file at {fullPath}");

        Sprite s = Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f));

        images.Add(s);

        if (contentParent != null)
            CreateCardForSprite(s, fn);
    }

    // Create album card UI
    private void CreateCardForSprite(Sprite s, string id)
    {
        if (AlbumCardPrefab == null || contentParent == null)
        {
            Debug.LogWarning("[AlbumManager] Missing prefab or content parent.");
            return;
        }

        GameObject go = Instantiate(AlbumCardPrefab, contentParent);
        AlbumCard card = go.GetComponent<AlbumCard>();

        if (card != null)
        {
            card.Setup(s, id);
            card.onViewClicked += OnCardView; 
            cards.Add(card);
        }
        else
        {
            Debug.LogError("[AlbumManager] AlbumCard prefab has no AlbumCard component.");
        }
    }

    //Image View
    private void OnCardView(Sprite s, string id)
    {
        if (viewer == null)
        {
            Debug.LogWarning("[AlbumManager] Viewer not assigned!");
            return;
        }

        viewer.Show(s, id);
    }

    // Delete image 
    public void DeleteImage(string fileName)
    {
        string path = Path.Combine(albumFolderPath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[AlbumManager] Deleted file: {fileName}");
        }

        var card = cards.FirstOrDefault(c => c.id == fileName);
        if (card != null)
        {
            cards.Remove(card);
            Destroy(card.gameObject);
        }
    }

    //Rename image and update AlbumCard
    public bool RenameImage(string oldFileName, string newFileName)
    {
        string oldPath = Path.Combine(albumFolderPath, oldFileName);
        string newPath = Path.Combine(albumFolderPath, newFileName);

        if (File.Exists(oldPath) && !File.Exists(newPath))
        {
            File.Move(oldPath, newPath);

            var card = cards.FirstOrDefault(c => c.id == oldFileName);
            if (card != null)
            {
                card.id = newFileName;
                if (card.fileNameText != null)
                    card.fileNameText.text = Path.GetFileNameWithoutExtension(newFileName);
            }

            Debug.Log($"[AlbumManager] Renamed {oldFileName} → {newFileName}");
            return true;
        }

        Debug.LogWarning($"[AlbumManager] Rename failed: {oldFileName} → {newFileName}");
        return false;
    }

    // Called from AlbumViewer to sync after rename (optional)
    public void UpdateCardName(string oldFileName, string newFileName)
    {
        var card = cards.FirstOrDefault(c => c.id == oldFileName);
        if (card != null)
        {
            card.id = newFileName;
            if (card.fileNameText != null)
                card.fileNameText.text = Path.GetFileNameWithoutExtension(newFileName);
            Debug.Log($"[AlbumManager] Updated UI name for {newFileName}");
        }
    }
}
