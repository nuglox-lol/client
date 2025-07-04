using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnCharacterAppearanceChanged))]
    public int characterAppearanceId;

    [SyncVar]
    public int userID;

    [SyncVar]
    public string username;

    [SyncVar]
    public bool isAdmin;

    private InstancePlayer instancePlayer;

    public override void OnStartClient()
    {
        base.OnStartClient();

        instancePlayer = new InstancePlayer(gameObject);
        if (characterAppearanceId > 0)
        {
            instancePlayer.CharacterAppearance = characterAppearanceId;
        }
    }

    void OnCharacterAppearanceChanged(int oldValue, int newValue)
    {
        if (instancePlayer == null)
        {
            instancePlayer = new InstancePlayer(gameObject);
        }

        instancePlayer.CharacterAppearance = newValue;
    }

    [Command]
    public void CmdSetCharacterAppearance(int newId)
    {
        characterAppearanceId = newId;
    }
}
