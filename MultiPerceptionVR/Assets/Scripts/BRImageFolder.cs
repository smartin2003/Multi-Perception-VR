using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BRImageFolder : MonoBehaviour
{
    [Header("Assign the two per-eye RawImages")]
    public RawImage leftImage;    // Canvas_Left/LeftImage
    public RawImage rightImage;   // Canvas_Right/RightImage

    [Header("Folder of images (absolute path)")]
    [Tooltip("Example: C:\\Users\\Mark\\Documents\\Stimuli")]
    public string rootFolder = "";

    [Tooltip("Include subfolders when scanning")]
    public bool includeSubfolders = false;

    [Header("Playback timing (milliseconds)")]
    public int defaultDurationMs = 2000;
    public int defaultIsiMs = 1000;

    [Header("File types to include")]
    public string[] extensions = { ".png", ".jpg", ".jpeg" };

    // Runtime state
    private readonly List<string> _files = new List<string>();
    private int _index = 0;
    private Texture2D _black;
    private readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

    // Saved sequence (pairs of absolute paths or null for black)
    private readonly List<(string leftPath, string rightPath)> _savedPairs = new List<(string, string)>();
    private string _currentLeftPath = null;
    private string _currentRightPath = null;

    void Awake()
    {
        _black = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _black.SetPixel(0, 0, Color.black);
        _black.Apply();

        if (leftImage) leftImage.texture = _black;
        if (rightImage) rightImage.texture = _black;
    }

    void Start()
    {
        RefreshFileList();
        PrintHelp();
        ShowIndexHint();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) Move(-1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Move(+1);
        if (Input.GetKeyDown(KeyCode.PageUp)) Move(-10);
        if (Input.GetKeyDown(KeyCode.PageDown)) Move(+10);

        if (Input.GetKeyDown(KeyCode.L)) AssignToLeft(CurrentPath());
        if (Input.GetKeyDown(KeyCode.R)) AssignToRight(CurrentPath());

        if (Input.GetKeyDown(KeyCode.X)) AssignToLeft(null);
        if (Input.GetKeyDown(KeyCode.Y)) AssignToRight(null);

        if (Input.GetKeyDown(KeyCode.A)) AddCurrentPair();

        if (Input.GetKeyDown(KeyCode.S)) StartCoroutine(PlaySavedSequence());

        if (Input.GetKeyDown(KeyCode.G)) RefreshFileList();
    }

    // ---------- Core ----------
    void RefreshFileList()
    {
        _files.Clear();

        if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder))
        {
            Debug.LogWarning($"BRImageFolder: Root folder missing/invalid: {rootFolder}");
            return;
        }

        var opt = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        try
        {
            foreach (var path in Directory.EnumerateFiles(rootFolder, "*.*", opt))
            {
                string ext = Path.GetExtension(path).ToLowerInvariant();
                foreach (var e in extensions)
                {
                    if (ext == e)
                    {
                        _files.Add(path);
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"BRImageFolder: Scan error: {e.Message}");
        }

        _files.Sort(StringComparer.OrdinalIgnoreCase);
        _index = Mathf.Clamp(_index, 0, Mathf.Max(0, _files.Count - 1));
        Debug.Log($"BRImageFolder: Found {_files.Count} files in {rootFolder}");
    }

    void Move(int delta)
    {
        if (_files.Count == 0) { Debug.Log("BRImageFolder: (no files)"); return; }
        _index = Mathf.Clamp(_index + delta, 0, _files.Count - 1);
        ShowIndexHint();
    }

    string CurrentPath()
    {
        if (_files.Count == 0) return null;
        return _files[_index];
    }

    void AssignToLeft(string path)
    {
        _currentLeftPath = path;
        if (leftImage) leftImage.texture = LoadOrBlack(path);
        Debug.Log($"Left <= {(path ?? "[BLACK]")}");
    }

    void AssignToRight(string path)
    {
        _currentRightPath = path;
        if (rightImage) rightImage.texture = LoadOrBlack(path);
        Debug.Log($"Right <= {(path ?? "[BLACK]")}");
    }

    Texture2D LoadOrBlack(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return _black;

        if (_cache.TryGetValue(path, out var tex) && tex)
            return tex;

        try
        {
            if (!File.Exists(path)) { Debug.LogWarning($"Missing file: {path}"); return _black; }
            byte[] data = File.ReadAllBytes(path);
            var t = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            if (!t.LoadImage(data, markNonReadable: false))
            {
                Debug.LogWarning($"Decode failed: {path}");
                Destroy(t);
                return _black;
            }
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Bilinear;
            _cache[path] = t;
            return t;
        }
        catch (Exception e)
        {
            Debug.LogError($"Load error: {e.Message}");
            return _black;
        }
    }

    void AddCurrentPair()
    {
        _savedPairs.Add((_currentLeftPath, _currentRightPath));
        Debug.Log($"Saved pair #{_savedPairs.Count}: L={(_currentLeftPath ?? "[BLACK]")} | R={(_currentRightPath ?? "[BLACK]")}");
    }

    System.Collections.IEnumerator PlaySavedSequence()
    {
        if (_savedPairs.Count == 0)
        {
            Debug.LogWarning("BRImageFolder: No saved pairs. Press 'A' to add the current L/R selection.");
            yield break;
        }

        float dur = Mathf.Max(0, defaultDurationMs) / 1000f;
        float isi = Mathf.Max(0, defaultIsiMs) / 1000f;

        Debug.Log($"BRImageFolder: Playing sequence ({_savedPairs.Count} pairs)  dur={defaultDurationMs}ms  isi={defaultIsiMs}ms");

        for (int i = 0; i < _savedPairs.Count; i++)
        {
            var (lp, rp) = _savedPairs[i];

            if (leftImage) leftImage.texture = LoadOrBlack(lp);
            if (rightImage) rightImage.texture = LoadOrBlack(rp);

            yield return new WaitForSecondsRealtime(dur);

            if (isi > 0f)
            {
                if (leftImage) leftImage.texture = _black;
                if (rightImage) rightImage.texture = _black;
                yield return new WaitForSecondsRealtime(isi);
            }
        }

        Debug.Log("BRImageFolder: Sequence finished.");
    }

    // ---------- Utility ----------
    void ShowIndexHint()
    {
        if (_files.Count == 0) { Debug.Log("BRImageFolder: (empty folder)"); return; }
        Debug.Log($"[{_index + 1}/{_files.Count}]  {_files[_index]}  |  Up/Down navigate, L->Left, R->Right, X/Y clear, A add, S play, G rescan");
    }

    void PrintHelp()
    {
        Debug.Log(
            "BRImageFolder Controls:\n" +
            "  Up/Down (PageUp/PageDown): browse files\n" +
            "  L: assign current file to LEFT eye\n" +
            "  R: assign current file to RIGHT eye\n" +
            "  X: set LEFT eye to BLACK\n" +
            "  Y: set RIGHT eye to BLACK\n" +
            "  A: add current L/R pair to saved sequence\n" +
            "  S: play saved sequence (uses defaultDurationMs/defaultIsiMs)\n" +
            "  G: rescan folder"
        );
    }
}