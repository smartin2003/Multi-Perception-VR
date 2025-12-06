using UnityEngine;

// Attach this to the OVROverlay object
public class DualEyeOverlayController : MonoBehaviour
{
    [Header("Core Overlay")]
    public OVROverlay overlay;   // The overlay with Stereo texture layout

    [Header("Image Pairs (Left & Right Eye)")]
    public Texture[] leftImages;
    public Texture[] rightImages;

    [Header("Timing Controls")]
    [Tooltip("How long each image pair should remain before switching to the next pair.")]
    public float imageDisplayDuration = 2f;

    [Tooltip("Flash frequency (times per second). Set to 0 to disable flashing.")]
    public float flashFrequency = 0f;

    [Header("Auto Start")]
    public bool startOnAwake = false;

    // --- Runtime fields ---
    [HideInInspector] public bool experimentRunning = false;

    private int index = 0;
    private float imageTimer = 0f;
    private float flashTimer = 0f;
    private bool flashVisible = true;

    void Start()
    {
        if (startOnAwake)
        {
            BeginExperiment();
        }
        else
        {
            // Make sure overlay is off until we start
            if (overlay != null)
                overlay.enabled = false;
        }
    }

    void Update()
    {
        if (!experimentRunning) return;
        if (leftImages.Length == 0 || rightImages.Length == 0) return;
        if (overlay == null) return;

        // --- Handle image switching duration ---
        imageTimer += Time.deltaTime;
        if (imageTimer >= imageDisplayDuration)
        {
            imageTimer = 0f;
            NextPair();
        }

        // --- Handle flashing ---
        if (flashFrequency > 0f)
        {
            flashTimer += Time.deltaTime;
            float flashInterval = 1f / flashFrequency;

            if (flashTimer >= flashInterval)
            {
                flashTimer = 0f;
                flashVisible = !flashVisible;
                overlay.enabled = flashVisible;
            }
        }
    }

    // Called by menu when the experiment begins
    public void BeginExperiment()
    {
        experimentRunning = true;

        index = 0;
        imageTimer = 0f;
        flashTimer = 0f;
        flashVisible = true;

        if (overlay != null)
            overlay.enabled = true;

        ApplyPair(index);
    }

    // Optional: if you ever want to stop from somewhere else
    public void StopExperiment()
    {
        experimentRunning = false;

        if (overlay != null)
            overlay.enabled = false;

        imageTimer = 0f;
        flashTimer = 0f;
        flashVisible = true;
    }

    // Apply current left/right pair
    private void ApplyPair(int i)
    {
        if (overlay == null || leftImages.Length == 0 || rightImages.Length == 0)
            return;

        overlay.textures[0] = leftImages[i];
        overlay.textures[1] = rightImages[i];

        // Force refresh overlay
        overlay.enabled = false;
        overlay.enabled = true;
    }

    // Go to next image pair
    public void NextPair()
    {
        index = (index + 1) % leftImages.Length;
        ApplyPair(index);
    }

    // Jump to specific pair
    public void SetPair(int i)
    {
        index = Mathf.Clamp(i, 0, leftImages.Length - 1);
        imageTimer = 0f;
        flashTimer = 0f;
        flashVisible = true;
        ApplyPair(index);
    }
}
