using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(Renderer))]
public class AudioColorSync : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup targetGroup = default;
    [SerializeField, Range(0.001f, 0.5f)] private float updateStep = 0.05f;
    [SerializeField] private int sampleDataLength = 256;

    [SerializeField] private int materialIndex = 0;
    [SerializeField, ColorUsage(true, true)] private Color materialColor = Color.black;
    [SerializeField] private float intensityFactor = 3f;
    [SerializeField] private float minIntensity = 0f, maxIntensity = 1f;
    [SerializeField, Range(0.1f, 10f)] private float smoothFactor = 2f;

    private float currentUpdateTime, currentLoudness;
    private float[] sampleData;

    private Renderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        sampleData = new float[sampleDataLength];
    }

    private void Update()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }
        currentUpdateTime += Time.deltaTime;

        Sound currentVoice = AudioManager.Instance.CurrentVoice;
        if (currentUpdateTime >= updateStep && currentVoice != null && currentVoice.clip != null && currentVoice.mixerGroup.name == targetGroup.name)
        {
            currentUpdateTime = 0f;
            if (currentVoice.clip.GetData(sampleData, currentVoice.source.timeSamples))
            {
                float targetLoudness = 0f;
                foreach (float sample in sampleData)
                {
                    targetLoudness += Mathf.Abs(sample);
                }
                targetLoudness = (targetLoudness * intensityFactor) / sampleDataLength;
                targetLoudness = Mathf.Min(targetLoudness, maxIntensity);

                currentLoudness = Mathf.Lerp(minIntensity, targetLoudness, smoothFactor * Time.deltaTime);
                meshRenderer.materials[materialIndex].SetColor("_EmissionColor", materialColor * currentLoudness);
            }
        }
    }
}
