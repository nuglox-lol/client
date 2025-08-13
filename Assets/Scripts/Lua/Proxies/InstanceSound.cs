using MoonSharp.Interpreter;
using UnityEngine;
using Mirror;

[MoonSharpUserData]
public class InstanceSound : InstanceDatamodel
{
    private NetworkIdentity networkIdentity;
    private Sound soundComponent;

    public InstanceSound(GameObject gameObject, Script lua = null) : base(gameObject, lua)
    {
        networkIdentity = gameObject.GetComponent<NetworkIdentity>();
        soundComponent = gameObject.GetComponent<Sound>();
    }

    public int SoundID
    {
        get => soundComponent.SoundID;
        set
        {
            soundComponent.SoundID = value;
            if (NetworkServer.active && networkIdentity != null)
                TargetSetSoundID(networkIdentity.connectionToClient, value);
        }
    }

    [TargetRpc]
    private void TargetSetSoundID(NetworkConnection target, int id)
    {
        soundComponent.SoundID = id;
    }

    public bool Playing => soundComponent.Playing;

    public bool Autoplay
    {
        get => soundComponent.Autoplay;
        set
        {
            soundComponent.Autoplay = value;
            if (NetworkServer.active && networkIdentity != null)
                TargetSetAutoplay(networkIdentity.connectionToClient, value);
        }
    }

    [TargetRpc]
    private void TargetSetAutoplay(NetworkConnection target, bool value)
    {
        soundComponent.Autoplay = value;
    }

    public bool Loop
    {
        get => soundComponent.Loop;
        set
        {
            soundComponent.Loop = value;
            if (NetworkServer.active && networkIdentity != null)
                TargetSetLoop(networkIdentity.connectionToClient, value);
        }
    }

    [TargetRpc]
    private void TargetSetLoop(NetworkConnection target, bool value)
    {
        soundComponent.Loop = value;
    }

    public bool PlayInWorld
    {
        get => soundComponent.PlayInWorld;
        set
        {
            soundComponent.PlayInWorld = value;
            if (NetworkServer.active && networkIdentity != null)
                TargetSetPlayInWorld(networkIdentity.connectionToClient, value);
        }
    }

    [TargetRpc]
    private void TargetSetPlayInWorld(NetworkConnection target, bool value)
    {
        soundComponent.PlayInWorld = value;
    }

    public float Volume
    {
        get => soundComponent.Volume;
        set
        {
            soundComponent.Volume = value;
            if (NetworkServer.active && networkIdentity != null)
                TargetSetVolume(networkIdentity.connectionToClient, value);
        }
    }

    [TargetRpc]
    private void TargetSetVolume(NetworkConnection target, float value)
    {
        soundComponent.Volume = value;
    }

    public float Pitch
    {
        get => soundComponent.Pitch;
        set
        {
            soundComponent.Pitch = value;
            if (NetworkServer.active && networkIdentity != null)
                TargetSetPitch(networkIdentity.connectionToClient, value);
        }
    }

    [TargetRpc]
    private void TargetSetPitch(NetworkConnection target, float value)
    {
        soundComponent.Pitch = value;
    }

    public float Length => soundComponent.Length;

    public float Time
    {
        get => soundComponent.Time;
        set
        {
            soundComponent.Time = value;
            if (NetworkServer.active && networkIdentity != null)
                TargetSetTime(networkIdentity.connectionToClient, value);
        }
    }

    [TargetRpc]
    private void TargetSetTime(NetworkConnection target, float value)
    {
        soundComponent.Time = value;
    }

    public void Play()
    {
        soundComponent.Play();
    }

    public void Stop()
    {
        soundComponent.Stop();
    }
}