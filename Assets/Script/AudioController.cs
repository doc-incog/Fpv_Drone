using UnityEngine;

public class AudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource propellerSource;
    public AudioSource impactSource;
    public AudioSource uiSource;

    [Header("Audio Clips")]
    public AudioClip propellerClip;
    public AudioClip impactClip;
    public AudioClip gateChimeClip;
    public AudioClip batteryLowClip;

    [Header("Settings")]
    public float minPropPitch = 0.3f;
    public float maxPropPitch = 2.0f;
    public float impactVolume = 0.8f;
    public float gateChimeVolume = 0.6f;

    private DroneController drone;
    private float lastBeepTime;
    private float beepInterval = 1.5f;

    void Start()
    {
        drone = GetComponent<DroneController>();
        SetupAudioSources();
        LoadVolumeSettings();
    }

    void Update()
    {
        if (propellerSource != null && drone != null)
        {
            float stick = (drone.throttleInput + 1f) * 0.5f;
            propellerSource.pitch = Mathf.Lerp(minPropPitch, maxPropPitch, stick);
            propellerSource.volume = Mathf.Lerp(0.2f, 0.6f, stick);
            if (!propellerSource.isPlaying)
                propellerSource.Play();
        }
    }

    public void PlayImpact(Vector3 position, float magnitude)
    {
        if (impactSource != null && impactClip != null)
        {
            impactSource.transform.position = position;
            impactSource.volume = Mathf.Clamp01(magnitude) * impactVolume;
            impactSource.PlayOneShot(impactClip);
        }
    }

    public void PlayGateChime()
    {
        if (uiSource != null && gateChimeClip != null)
        {
            uiSource.PlayOneShot(gateChimeClip, gateChimeVolume);
        }
    }

    public void PlayLowBatteryBeep()
    {
        if (uiSource != null && batteryLowClip != null && Time.time - lastBeepTime > beepInterval)
        {
            uiSource.PlayOneShot(batteryLowClip, 0.5f);
            lastBeepTime = Time.time;
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (propellerSource != null) propellerSource.volume = Mathf.Clamp01(volume) * 0.6f;
        if (impactSource != null) impactSource.volume = Mathf.Clamp01(volume) * impactVolume;
        if (uiSource != null) uiSource.volume = Mathf.Clamp01(volume) * 0.8f;
        PlayerPrefs.SetFloat("sfx_volume", volume);
    }

    public void LoadVolumeSettings()
    {
        float sfx = PlayerPrefs.GetFloat("sfx_volume", 1f);
        SetSFXVolume(sfx);
    }

    private AudioClip CreateBeepClip()
    {
        int sampleRate = 44100;
        int duration = 2;
        int samples = sampleRate * duration;
        AudioClip clip = AudioClip.Create("BatteryBeep", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float tone = Mathf.Sin(t * 2200f * Mathf.PI * 2f) * 0.5f;
            float env = t < 0.1f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.1f) / 0.5f);
            data[i] = tone * env;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreatePropellerClip()
    {
        int sampleRate = 44100;
        int duration = 2;
        int samples = sampleRate * duration;
        AudioClip clip = AudioClip.Create("PropellerHum", samples, 1, sampleRate, true);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float hum = Mathf.Sin(t * 120f * Mathf.PI * 2f) * 0.3f;
            float buzz = Mathf.Sin(t * 240f * Mathf.PI * 2f) * 0.15f;
            float noise = (Random.value - 0.5f) * 0.05f;
            data[i] = hum + buzz + noise;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateImpactClip()
    {
        int sampleRate = 44100;
        int duration = 1;
        int samples = sampleRate * duration;
        AudioClip clip = AudioClip.Create("Impact", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float noise = (Random.value - 0.5f) * 2f;
            float envelope = Mathf.Exp(-t * 10f);
            data[i] = noise * envelope * 0.3f;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateGateChimeClip()
    {
        int sampleRate = 44100;
        int duration = 1;
        int samples = sampleRate * duration;
        AudioClip clip = AudioClip.Create("GateChime", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float tone1 = Mathf.Sin(t * 880f * Mathf.PI * 2f) * 0.4f;
            float tone2 = Mathf.Sin(t * 1320f * Mathf.PI * 2f) * 0.2f;
            float envelope = t < 0.05f ? t / 0.05f : Mathf.Exp(-(t - 0.05f) * 3f);
            data[i] = (tone1 + tone2) * envelope;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private void SetupAudioSources()
    {
        if (propellerSource == null)
        {
            GameObject go = new GameObject("PropellerAudio");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            propellerSource = go.AddComponent<AudioSource>();
            propellerSource.loop = true;
            propellerSource.playOnAwake = false;
            propellerSource.spatialBlend = 1f;
            propellerSource.minDistance = 1f;
            propellerSource.maxDistance = 30f;
        }

        if (impactSource == null)
        {
            GameObject go = new GameObject("ImpactAudio");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            impactSource = go.AddComponent<AudioSource>();
            impactSource.spatialBlend = 1f;
            impactSource.minDistance = 1f;
            impactSource.maxDistance = 20f;
        }

        if (uiSource == null)
        {
            GameObject go = new GameObject("UIAudio");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            uiSource = go.AddComponent<AudioSource>();
            uiSource.spatialBlend = 0f;
        }

        if (propellerClip == null)
            propellerClip = Resources.Load<AudioClip>("Audio/quad_flying");
        if (propellerClip == null)
            propellerClip = CreatePropellerClip();

        if (impactClip == null)
            impactClip = Resources.Load<AudioClip>("Audio/drone_impact");
        if (impactClip == null)
            impactClip = CreateImpactClip();

        if (gateChimeClip == null)
            gateChimeClip = Resources.Load<AudioClip>("Audio/gate_chime");
        if (gateChimeClip == null)
            gateChimeClip = CreateGateChimeClip();

        if (batteryLowClip == null)
            batteryLowClip = CreateBeepClip();

        if (propellerClip != null) propellerSource.clip = propellerClip;
    }
}
