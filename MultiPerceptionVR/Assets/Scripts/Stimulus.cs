using UnityEngine;

[System.Serializable]
public class BRStimulus
{
    [Header("Left eye")]
    [Tooltip("Left-eye image. Leave empty for black.")]
    public string left;
    [Tooltip("If true, 'left' is an absolute file path. If false, it's a Resources name (no extension).")]
    public bool leftIsFilePath = false;

    [Header("Right eye")]
    [Tooltip("Right-eye image. Leave empty for black.")]
    public string right;
    [Tooltip("If true, 'right' is an absolute file path. If false, it's a Resources name (no extension).")]
    public bool rightIsFilePath = false;

    [Header("Timing (per-item overrides, in milliseconds)")]
    [Tooltip("If <= 0, uses BRSet.defaultDurationMs.")]
    public int durationMs = 0;

    [Tooltip("If <= 0, uses BRSet.defaultIsiMs.")]
    public int isiMs = 0;
}