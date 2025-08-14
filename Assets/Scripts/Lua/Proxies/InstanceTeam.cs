using MoonSharp.Interpreter;
using UnityEngine;
using System.Collections.Generic;

[MoonSharpUserData]
public class InstanceTeam : InstanceDatamodel
{
    private Team team;
    public LuaEvent OnPlayerAdded;
    public LuaEvent OnPlayerRemoved;

    public InstanceTeam(GameObject go, Script luascript) : base(go, luascript)
    {
        this.team = go.GetComponent<Team>();
        OnPlayerAdded = new LuaEvent(go, luascript);
        OnPlayerRemoved = new LuaEvent(go, luascript);
        Team.OnPlayerAdded += HandlePlayerAdded;
        Team.OnPlayerRemoved += HandlePlayerRemoved;
    }

    void HandlePlayerAdded(Team t, Player p)
    {
        if (t == team)
            OnPlayerAdded.Fire(UserData.Create(new InstancePlayer(p.gameObject)));
    }

    void HandlePlayerRemoved(Team t, Player p)
    {
        if (t == team)
            OnPlayerRemoved.Fire(UserData.Create(new InstancePlayer(p.gameObject)));
    }

    public string TeamName
    {
        get => team.TeamName;
        set => team.TeamName = value;
    }

    public Color TeamColor
    {
        get => team.TeamColor;
        set => team.TeamColor = value;
    }

    public int Score
    {
        get => team.Score;
        set => team.Score = value;
    }

    public int MaxPlayers
    {
        get => team.MaxPlayers;
        set => team.MaxPlayers = value;
    }

    public int CurrentPlayers => team.CurrentPlayers;
    public bool IsNeutral => team.IsNeutral;
    public bool IsFull => team.IsFull;
    public bool IsEmpty => team.IsEmpty;

    public void AddPlayer(Player player) => team.AddPlayer(player);
    public void RemovePlayer(Player player) => team.RemovePlayer(player);

    public List<InstancePlayer> GetPlayers()
    {
        var list = new List<InstancePlayer>();
        foreach (var player in Object.FindObjectsOfType<Player>())
        {
            if (player.TeamName == team.TeamName)
                list.Add(new InstancePlayer(player.gameObject));
        }
        return list;
    }

    public InstancePlayer OnTeam(string playerName)
    {
        foreach (var player in Object.FindObjectsOfType<Player>())
        {
            if (player.TeamName == team.TeamName && player.username == playerName)
                return new InstancePlayer(player.gameObject);
        }
        return null;
    }
}
