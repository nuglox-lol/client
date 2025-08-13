using System.Collections;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Sound : NetworkBehaviour
{
    private AudioSource audioSource;
    private AudioClip clip;

    [SyncVar(hook = nameof(OnSoundIDChanged))] private int soundID;
    [SyncVar(hook = nameof(OnPlayingChanged))] private bool _isPlaying;
    [SyncVar(hook = nameof(OnAutoplayChanged))] private bool autoplay;
    [SyncVar(hook = nameof(OnVolumeChanged))] private float volume;
    [SyncVar(hook = nameof(OnTimeChanged))] private float time;
    [SyncVar(hook = nameof(OnLoopChanged))] private bool loop;
    [SyncVar(hook = nameof(OnPlayInWorldChanged))] private bool playInWorld;
    [SyncVar(hook = nameof(OnPitchChanged))] private float pitch = 1f;

    public bool Playing { get => _isPlaying; }

    public int SoundID
    {
        get => soundID;
        set
        {
            if (!isServer && SceneHelper.GetCurrentSceneName() != "Studio") return;
            soundID = value;
            StartCoroutine(GetAudioClip(soundID));
        }
    }

    public float Pitch
    {
        get => pitch;
        set
        {
            if (!isServer && SceneHelper.GetCurrentSceneName() != "Studio") return;
            pitch = value;
            audioSource.pitch = pitch;
        }
    }

    public float Length => audioSource.clip?.length ?? 0f;

    new public Vector3 Size
    {
        get => Vector3.one;
        set => transform.localScale = Vector3.one;
    }

    new public Vector3 Rotation
    {
        get => Vector3.one;
        set => transform.rotation = Quaternion.identity;
    }

    public float Time
    {
        get => audioSource.time;
        set
        {
            if (!isServer && SceneHelper.GetCurrentSceneName() != "Studio") return;
            time = value;
            audioSource.time = value;
        }
    }

    IEnumerator GetAudioClip(int id)
    {
        string baseUrl = GetArgs.Get("baseUrl");
        if (string.IsNullOrEmpty(baseUrl))
            baseUrl = "https://nuglox.com/";

        string url = $"{baseUrl}catalog_storage/audios/{id}.mp3";

        using (var www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                audioSource.Stop();
                clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;

                if ((autoplay || _isPlaying) && SceneHelper.GetCurrentSceneName() != "Studio")
                {
                    audioSource.Play();
                }
            }
        }
    }

    public bool Autoplay
    {
        get => autoplay;
        set
        {
            if (!isServer && SceneHelper.GetCurrentSceneName() != "Studio") return;
            autoplay = value;
        }
    }

    public bool Loop
    {
        get => loop;
        set
        {
            if (!isServer && SceneHelper.GetCurrentSceneName() != "Studio") return;
            loop = value;
            audioSource.loop = value;
        }
    }

    public bool PlayInWorld
    {
        get => playInWorld;
        set
        {
            if (!isServer && SceneHelper.GetCurrentSceneName() != "Studio") return;
            playInWorld = value;
            audioSource.spatialBlend = value ? 1 : 0;
        }
    }

    public float Volume
    {
        get => audioSource.volume;
        set
        {
            if (!isServer && SceneHelper.GetCurrentSceneName() != "Studio") return;
            volume = value;
            audioSource.volume = value;
        }
    }

    public void Play()
    {
        if (!isServer && SceneHelper.GetCurrentSceneName() != "Studio") return;
        _isPlaying = true;
        RpcPlay();
    }

    public void Stop()
    {
        if (!isServer && SceneHelper.GetCurrentSceneName() != "Studio") return;
        _isPlaying = false;
        RpcStop();
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 1f;
        audioSource.pitch = 1f;
    }

    void Start()
    {
        if (!isServer)
        {
            if (soundID > 0)
                StartCoroutine(GetAudioClip(soundID));

            if (_isPlaying)
                audioSource.Play();

            audioSource.loop = loop;
            audioSource.spatialBlend = playInWorld ? 1 : 0;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
        }
        else if (autoplay)
        {
            Play();
        }
    }

    [ClientRpc]
    private void RpcPlay()
    {
        if (isServer) return;
        audioSource.Play();
    }

    [ClientRpc]
    private void RpcStop()
    {
        if (isServer) return;
        audioSource.Stop();
    }

    private void OnSoundIDChanged(int oldValue, int newValue)
    {
        if (!isServer)
            StartCoroutine(GetAudioClip(newValue));
    }

    private void OnPlayingChanged(bool oldValue, bool newValue)
    {
        if (!isServer)
        {
            if (newValue)
                audioSource.Play();
            else
                audioSource.Stop();
        }
    }

    private void OnAutoplayChanged(bool oldValue, bool newValue)
    {
        autoplay = newValue;
    }

    private void OnVolumeChanged(float oldValue, float newValue)
    {
        if (!isServer)
            audioSource.volume = newValue;
    }

    private void OnTimeChanged(float oldValue, float newValue)
    {
        if (!isServer)
            audioSource.time = newValue;
    }

    private void OnLoopChanged(bool oldValue, bool newValue)
    {
        loop = newValue;
        if (!isServer)
            audioSource.loop = newValue;
    }

    private void OnPlayInWorldChanged(bool oldValue, bool newValue)
    {
        playInWorld = newValue;
        if (!isServer)
            audioSource.spatialBlend = newValue ? 1 : 0;
    }

    private void OnPitchChanged(float oldValue, float newValue)
    {
        pitch = newValue;
        if (!isServer)
            audioSource.pitch = newValue;
    }
}