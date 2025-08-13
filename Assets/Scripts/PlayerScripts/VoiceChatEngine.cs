using UnityEngine;
using UnityEngine.Networking;
using Mirror;
using System.Collections;

public class VoiceChatEngine : NetworkBehaviour
{
    public AudioSource audioSource;
    public GameObject speakingIcon;
    public float sendInterval = 0.05f;

    private AudioClip micClip;
    private string micDevice;
    private float timer;
    private int sampleSize = 2048;
    private float[] micBuffer;

    private float[] audioStreamBuffer = new float[44100 * 5];
    private int writePos;
    private int readPos;
    private int sampleRate = 44100;
    private int channels = 1;
    private bool streamStarted;

    IEnumerator Start()
    {
        yield return StartCoroutine(VoiceChatServer.Initialize());

        if (!VoiceChatServer.Enabled) yield break;

        if (isLocalPlayer)
        {
            micDevice = Microphone.devices[0];
            micClip = Microphone.Start(micDevice, true, 1, sampleRate);
            micBuffer = new float[sampleSize];
        }

        if (!audioSource)
            audioSource = GetComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;

        if (!isLocalPlayer)
        {
            AudioClip streamClip = AudioClip.Create("stream", audioStreamBuffer.Length, channels, sampleRate, true, OnAudioRead, OnAudioSetPosition);
            audioSource.clip = streamClip;
            audioSource.loop = true;
            audioSource.Play();
            streamStarted = true;
        }

        if (speakingIcon != null)
            speakingIcon.SetActive(false);
    }

    void Update()
    {
        if (!VoiceChatServer.Enabled) return;
        if (!isLocalPlayer) return;

        timer += Time.deltaTime;
        if (timer < sendInterval) return;

        int micPos = Microphone.GetPosition(micDevice);
        if (micPos < sampleSize) return;

        micClip.GetData(micBuffer, micPos - sampleSize);

        float volume = 0f;
        for (int i = 0; i < micBuffer.Length; i++)
            volume += Mathf.Abs(micBuffer[i]);
        volume /= micBuffer.Length;

        bool shouldSpeak = volume > 0.02f;
        if (shouldSpeak)
        {
            byte[] data = new byte[micBuffer.Length * 4];
            System.Buffer.BlockCopy(micBuffer, 0, data, 0, data.Length);
            CmdSendVoice(data);
        }
        SetSpeakingIcon(shouldSpeak);
        timer = 0f;
    }

    [Command]
    void CmdSendVoice(byte[] data)
    {
        RpcReceiveVoice(data);
    }

    [ClientRpc]
    void RpcReceiveVoice(byte[] data)
    {
        if (isLocalPlayer) return;

        float[] samples = new float[data.Length / 4];
        System.Buffer.BlockCopy(data, 0, samples, 0, data.Length);

        lock (audioStreamBuffer)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                audioStreamBuffer[writePos] = samples[i];
                writePos = (writePos + 1) % audioStreamBuffer.Length;
            }
        }
        SetSpeakingIcon(true);
        CancelInvoke(nameof(ResetSpeakingIcon));
        Invoke(nameof(ResetSpeakingIcon), 0.3f);
    }

    void OnAudioRead(float[] data)
    {
        lock (audioStreamBuffer)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (readPos != writePos)
                {
                    data[i] = audioStreamBuffer[readPos];
                    readPos = (readPos + 1) % audioStreamBuffer.Length;
                }
                else
                {
                    data[i] = 0f;
                }
            }
        }
    }

    void OnAudioSetPosition(int newPosition) { }

    void SetSpeakingIcon(bool active)
    {
        if (speakingIcon != null)
            speakingIcon.SetActive(active);
    }

    void ResetSpeakingIcon()
    {
        if (!isLocalPlayer)
            SetSpeakingIcon(false);
    }
}

public static class VoiceChatServer
{
    public static bool Enabled = false;

    public static IEnumerator Initialize()
    {
        string url = GetArgs.Get("baseUrl") + "v1/voicechat/isenabled?gameid=" + GetArgs.Get("gameid");
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("VoiceChatServer check failed: " + request.error);
                Enabled = false;
            }
            else
            {
                string response = request.downloadHandler.text.Trim().ToLower();
                Enabled = response == "true";
                Debug.Log("VoiceChatServer.Enabled = " + Enabled);
            }
        }
    }
}