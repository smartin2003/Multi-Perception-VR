using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BRRunner : MonoBehaviour
{
    [Header("Assign the two per-eye RawImages (world-space UI)")]
    public RawImage leftImage;   // Canvas_Left/LeftImage
    public RawImage rightImage;  // Canvas_Right/RightImage

    [Header("Stimulus Set to run")]
    public BRSet config;

    [Header("Options")]
    [Tooltip("Start automatically on play.")]
    public bool autoStart = true;

    private Texture2D _black;

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
        if (autoStart) StartRun();
    }

    public void StartRun()
    {
        if (config == null || config.stimuli == null || config.stimuli.Length == 0)
        {
            Debug.LogError("BRRunner: No BRSet assigned or set is empty.");
            return;
        }
        StopAllCoroutines();
        StartCoroutine(RunSequence());
    }

    System.Collections.IEnumerator RunSequence()
    {
        // Build order
        int n = config.stimuli.Length;
        int[] order = new int[n];
        for (int i = 0; i < n; i++) order[i] = i;
        if (config.randomize) Shuffle(order);

        for (int k = 0; k < n; k++)
        {
            var s = config.stimuli[order[k]];

            // Resolve timings (ms -> sec)
            float durSec = ((s.durationMs > 0) ? s.durationMs : config.defaultDurationMs) / 1000f;
            float isiSec = ((s.isiMs > 0) ? s.isiMs : config.defaultIsiMs) / 1000f;

            // Load textures per eye; empty string => black
            Texture2D lt = string.IsNullOrWhiteSpace(s.left) ? null : LoadTexture(s.left, s.leftIsFilePath);
            Texture2D rt = string.IsNullOrWhiteSpace(s.right) ? null : LoadTexture(s.right, s.rightIsFilePath);

            if (leftImage) leftImage.texture = lt ?? _black;
            if (rightImage) rightImage.texture = rt ?? _black;

            yield return new WaitForSecondsRealtime(Mathf.Max(0f, durSec));

            if (isiSec > 0f)
            {
                if (leftImage) leftImage.texture = _black;
                if (rightImage) rightImage.texture = _black;
                yield return new WaitForSecondsRealtime(Mathf.Max(0f, isiSec));
            }
        }

        Debug.Log("BRRunner: sequence complete.");
    }

    static void Shuffle(int[] a)
    {
        for (int i = a.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (a[i], a[j]) = (a[j], a[i]);
        }
    }

    static Texture2D LoadTexture(string key, bool isFilePath)
    {
        try
        {
            if (!isFilePath)
            {
                var tex = Resources.Load<Texture2D>(key);
                if (!tex) Debug.LogWarning($"BRRunner: Resources texture not found: {key}");
                return tex;
            }

            if (!File.Exists(key))
            {
                Debug.LogWarning($"BRRunner: File not found: {key}");
                return null;
            }

            byte[] data = File.ReadAllBytes(key);
            var t = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            if (!t.LoadImage(data, markNonReadable: false))
            {
                Debug.LogWarning($"BRRunner: Decode failed: {key}");
                UnityEngine.Object.Destroy(t);
                return null;
            }
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Bilinear;
            return t;
        }
        catch (Exception e)
        {
            Debug.LogError($"BRRunner: LoadTexture error ({key}): {e.Message}");
            return null;
        }
    }
}