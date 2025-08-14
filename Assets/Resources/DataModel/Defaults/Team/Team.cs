using UnityEngine;
using Mirror;
using System;

public class Team : NetworkBehaviour
{
    [SyncVar] public string TeamName;
    [SyncVar] public Color TeamColor;
    [SyncVar] public int Score;
    [SyncVar] public int MaxPlayers = 10;

    public static event Action<Team, Player> OnPlayerAdded;
    public static event Action<Team, Player> OnPlayerRemoved;

    public int CurrentPlayers
    {
        get
        {
            int count = 0;
            foreach (var player in FindObjectsOfType<Player>())
            {
                if (player.TeamName == TeamName)
                    count++;
            }
            return count;
        }
    }

    public bool IsNeutral => TeamName == "Neutral";
    public bool IsFull => CurrentPlayers >= MaxPlayers;
    public bool IsEmpty => CurrentPlayers == 0;

    public void AddPlayer(Player player)
    {
        if (!IsFull && player.TeamName != TeamName)
        {
            player.TeamName = TeamName;
            OnPlayerAdded?.Invoke(this, player);
        }
    }

    public void RemovePlayer(Player player)
    {
        if (player.TeamName == TeamName)
        {
            player.TeamName = "";
            OnPlayerRemoved?.Invoke(this, player);
        }
    }
}