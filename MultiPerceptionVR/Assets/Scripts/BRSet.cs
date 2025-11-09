using UnityEngine;

[CreateAssetMenu(fileName = "BRSet", menuName = "BR/Stimulus Set")]
public class BRSet : ScriptableObject
{
    [Tooltip("Ordered list of binocular stimuli to present.")]
    public BRStimulus[] stimuli;

    [Header("Default timing (milliseconds)")]
    [Tooltip("Used when a stimulus.durationMs <= 0.")]
    public int defaultDurationMs = 2000;

    [Tooltip("Used when a stimulus.isiMs <= 0.")]
    public int defaultIsiMs = 1000;

    [Header("Run options")]
    [Tooltip("Randomize presentation order at runtime.")]
    public bool randomize = false;
}