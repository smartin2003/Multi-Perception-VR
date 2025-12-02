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
    public bool startOnAwake = true;

    private int index = 0;
    private float imageTimer = 0f;
    private float flashTimer = 0f;
    private bool flashVisible = true;

    void Start()
    {
        if (startOnAwake)
            ApplyPair(index);
    }

    void Update()
    {
        if (leftImages.Length == 0 || rightImages.Length == 0) return;
        if (overlay == null) return;

        // --- Handle image duration switching ---
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

    // Apply the current index pair
    private void ApplyPair(int i)
    {
        overlay.textures[0] = leftImages[i];
        overlay.textures[1] = rightImages[i];

        overlay.enabled = false;
        overlay.enabled = true;
    }

    // Switch to next pair
    public void NextPair()
    {
        index = (index + 1) % leftImages.Length;
        ApplyPair(index);
    }

    // Manually jump to specific pair
    public void SetPair(int i)
    {
        index = Mathf.Clamp(i, 0, leftImages.Length - 1);
        imageTimer = 0;
        flashTimer = 0;
        flashVisible = true;
        ApplyPair(index);
    }
}
