using UnityEngine;
using TMPro;

public class VRMenuController : MonoBehaviour
{
    [Header("References")]
    public Canvas menuCanvas;
    public DualEyeOverlayController dualEye;

    [Header("Menu Text Items")]
    public TMP_Text optionStart;
    public TMP_Text optionImageDuration;
    public TMP_Text optionFlashFrequency;
    public TMP_Text valueDisplay;

    private int selectedIndex = 0;
    private float navCooldown = 0.25f;
    private float lastNavTime = 0f;

    void Start()
    {
        // Make sure experiment is not running when menu is up
        dualEye.experimentRunning = false;
        HighlightSelection();
        UpdateValueDisplay();
    }

    void Update()
    {
        HandleMenuNavigation();
        HandleAdjustment();
        HandleStartSelection();
    }

    // Navigate menu with joystick
    void HandleMenuNavigation()
    {
        float vertical = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;

        if (Time.time - lastNavTime > navCooldown)
        {
            if (vertical > 0.5f)
            {
                selectedIndex = Mathf.Max(0, selectedIndex - 1);
                lastNavTime = Time.time;
                HighlightSelection();
            }
            else if (vertical < -0.5f)
            {
                selectedIndex = Mathf.Min(2, selectedIndex + 1);
                lastNavTime = Time.time;
                HighlightSelection();
            }
        }
    }

    // Adjust image duration / flash frequency with A/B
    void HandleAdjustment()
    {
        if (selectedIndex == 0) return; // Start is not adjustable

        // A button = increment
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            if (selectedIndex == 1) dualEye.imageDisplayDuration += 0.1f;
            if (selectedIndex == 2) dualEye.flashFrequency += 1f;
            UpdateValueDisplay();
        }

        // B button = decrement
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            if (selectedIndex == 1) dualEye.imageDisplayDuration -= 0.1f;
            if (selectedIndex == 2) dualEye.flashFrequency -= 1f;
            UpdateValueDisplay();
        }

        dualEye.imageDisplayDuration = Mathf.Max(0.1f, dualEye.imageDisplayDuration);
        dualEye.flashFrequency = Mathf.Max(0f, dualEye.flashFrequency);
    }

    // Start experiment when Start is highlighted and A is pressed
    void HandleStartSelection()
    {
        if (selectedIndex == 0 && OVRInput.GetDown(OVRInput.Button.One))
        {
            menuCanvas.gameObject.SetActive(false);
            dualEye.BeginExperiment();
        }
    }

    // Highlight the current selection
    void HighlightSelection()
    {
        optionStart.color          = (selectedIndex == 0 ? Color.yellow : Color.white);
        optionImageDuration.color  = (selectedIndex == 1 ? Color.yellow : Color.white);
        optionFlashFrequency.color = (selectedIndex == 2 ? Color.yellow : Color.white);
    }

    // Update the numeric display text
    void UpdateValueDisplay()
    {
        valueDisplay.text =
            $"Image Duration: {dualEye.imageDisplayDuration:F2}s\n" +
            $"Flash Frequency: {dualEye.flashFrequency:F0} Hz";
    }
}
