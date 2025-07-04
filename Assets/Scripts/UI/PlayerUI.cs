using UnityEngine;
using ImGuiNET;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
{
    private List<string> playerNames = new List<string>();

    private bool showChat = true;
    private bool showSettings = false;

    private float volume = 0.5f;
    private bool enableShadows = false;
    private bool enableMusic = false;

    private string chatInput = "";
    private List<string> chatMessages = new List<string>();

    private int unreadMessages = 0;
    private float refreshTimer = 0f;

    void OnEnable()
    {
        ImGuiUn.Layout += OnLayout;
        ChatService.OnMessageReceived += OnChatMessageReceived;
    }

    void OnDisable()
    {
        ImGuiUn.Layout -= OnLayout;
        ChatService.OnMessageReceived -= OnChatMessageReceived;
    }

    void Update()
    {
        refreshTimer += Time.deltaTime;
        if (refreshTimer >= 1f)
        {
            RefreshPlayerList();
            refreshTimer = 0f;
        }
    }

    void OnChatMessageReceived(string message)
    {
        chatMessages.Add(message);
        if (!showChat) unreadMessages++;
    }

    void RefreshPlayerList()
    {
        playerNames.Clear();

        Player[] players = GameObject.FindObjectsOfType<Player>();
        foreach (var player in players)
            if (player.GetComponent<ObjectClass>().className == "Player")
                playerNames.Add(player.username);
    }

    void OnLayout()
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.zero);

        Vector2 playerListPos = new Vector2(screenSize.x - 180 - 10, 10);
        Vector2 playerListSize = new Vector2(180, 200);

        ImGui.SetNextWindowPos(playerListPos, ImGuiCond.Always);
        ImGui.SetNextWindowSize(playerListSize, ImGuiCond.Always);

        ImGui.Begin("##playerList", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar);

        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.13f, 0.13f, 0.13f, 0.9f));
        ImGui.BeginChild("playerListChild", new Vector2(0, playerListSize.y - 28), false);

        foreach (var name in playerNames)
            ImGui.TextUnformatted(name);

        ImGui.EndChild();
        ImGui.PopStyleColor();

        ImGui.End();

        Vector2 chatTogglePos = new Vector2(10, screenSize.y - 40);
        Vector2 chatToggleSize = new Vector2(100, 30);

        ImGui.SetNextWindowPos(chatTogglePos, ImGuiCond.Always);
        ImGui.SetNextWindowSize(chatToggleSize, ImGuiCond.Always);

        ImGui.Begin("##chatToggle", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar);

        if (ImGui.Button("Chat", chatToggleSize))
        {
            showChat = !showChat;
            if (showChat) unreadMessages = 0;
        }

        if (unreadMessages > 0)
        {
            Vector2 badgePos = ImGui.GetItemRectMin() + new Vector2(chatToggleSize.x - 18, 6);
            ImGui.GetWindowDrawList().AddCircleFilled(badgePos + new Vector2(8, 8), 8, ImGui.GetColorU32(new Vector4(1, 0, 0, 1)));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));
            ImGui.GetWindowDrawList().AddText(badgePos + new Vector2(4, 1), ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), unreadMessages.ToString());
            ImGui.PopStyleColor();
        }

        ImGui.End();

        if (showChat)
        {
            Vector2 chatPos = new Vector2(10, screenSize.y - 300 - 50);
            Vector2 chatSize = new Vector2(320, 300);

            ImGui.SetNextWindowPos(chatPos, ImGuiCond.Always);
            ImGui.SetNextWindowSize(chatSize, ImGuiCond.Always);

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.13f, 0.13f, 0.13f, 0.9f));
            ImGui.Begin("##chatWindow", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);

            ImGui.BeginChild("chatMessages", new Vector2(0, chatSize.y - 60), false);

            foreach (var msg in chatMessages)
                ImGui.TextWrapped(msg);

            if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
                ImGui.SetScrollHereY(1.0f);

            ImGui.EndChild();

            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputText("##chatInput", ref chatInput, 256, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                if (!string.IsNullOrWhiteSpace(chatInput))
                {
                    if (NetworkChat.LocalInstance != null)
                        NetworkChat.LocalInstance.CmdSendMessage(chatInput);

                    chatInput = "";
                    ImGui.SetKeyboardFocusHere(-1);
                }
            }

            ImGui.End();
            ImGui.PopStyleColor();
        }

        if (showSettings)
        {
            Vector2 settingsPos = new Vector2(10, 10);
            Vector2 settingsSize = new Vector2(200, 140);

            ImGui.SetNextWindowPos(settingsPos, ImGuiCond.Always);
            ImGui.SetNextWindowSize(settingsSize, ImGuiCond.Always);

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.13f, 0.13f, 0.13f, 0.95f));
            ImGui.Begin("##settings", ref showSettings, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);

            ImGui.Checkbox("Shadows", ref enableShadows);
            ImGui.Checkbox("Music", ref enableMusic);
            ImGui.SliderFloat("Volume", ref volume, 0f, 1f);

            if (ImGui.Button("Close", new Vector2(-1, 0)))
                showSettings = false;

            ImGui.End();
            ImGui.PopStyleColor();
        }

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor();
    }
}
