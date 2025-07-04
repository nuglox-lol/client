using Mirror;
using UnityEngine;

public class NetworkChat : NetworkBehaviour
{
    public static NetworkChat LocalInstance;

    private Player localPlayer;

    public override void OnStartLocalPlayer()
    {
        LocalInstance = this;
        localPlayer = GetComponent<Player>();
    }

    [Command]
    public void CmdSendMessage(string message)
    {
        Player sender = GetComponent<Player>();
        string username = (sender != null && !string.IsNullOrEmpty(sender.username)) ? sender.username : "Unknown";

        string formatted = $"{username}: {message}";
        RpcReceiveMessage(formatted);
    }

    [ClientRpc]
    void RpcReceiveMessage(string message)
    {
        ChatService.ReceiveMessage(message);
    }
}
