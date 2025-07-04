using UnityEngine;
using ImGuiNET;

public class MainEngine : MonoBehaviour
{
    public Camera buildCamera;
    public float flySpeed = 10f;
    public float mouseSensitivity = 3f;

    private bool isBuildMode = true;
    private bool isMouseLocked;
    private bool mouseFree = false;

    private void Start()
    {
        buildCamera.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnEnable() => ImGuiUn.Layout += OnLayout;
    private void OnDisable() => ImGuiUn.Layout -= OnLayout;

    private void Update()
    {
        if (isBuildMode)
        {
            HandleBuildModeControls();
        }

        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            mouseFree = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (mouseFree && Input.GetKeyDown(KeyCode.LeftControl))
        {
            mouseFree = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnLayout()
    {
        if (mouseFree)
        {
            var io = ImGui.GetIO();
            var center = new Vector2(io.DisplaySize.x / 2f, io.DisplaySize.y / 2f);
            var text = "Press Left Control to return to editing";
            var size = ImGui.CalcTextSize(text);

            ImGui.SetNextWindowPos(new Vector2(center.x - size.x / 2f, center.y - size.y / 2f));
            ImGui.Begin("MouseFreeInfo", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBackground);
            ImGui.Text(text);
            ImGui.End();
        }
    }

    private void ToggleMode()
    {
        isBuildMode = !isBuildMode;
        buildCamera.gameObject.SetActive(isBuildMode);
    }

    private void HandleBuildModeControls()
    {
        if (mouseFree) return;

        float axisH = Input.GetAxis("Horizontal");
        float axisV = Input.GetAxis("Vertical");

        Vector3 forward = buildCamera.transform.forward.normalized;
        Vector3 right = buildCamera.transform.right.normalized;

        Vector3 move = forward * axisV + right * axisH;

        buildCamera.transform.position += move * flySpeed * Time.deltaTime;

        if (Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isMouseLocked = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isMouseLocked = false;
        }

        if (isMouseLocked)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity;

            buildCamera.transform.Rotate(Vector3.up, mouseX, Space.World);
            buildCamera.transform.Rotate(Vector3.right, mouseY, Space.Self);

            Vector3 euler = buildCamera.transform.localEulerAngles;
            euler.x = ClampAngle(euler.x, -90f, 90f);
            buildCamera.transform.localEulerAngles = new Vector3(euler.x, euler.y, 0f);
        }
    }

    private float ClampAngle(float angle, float min, float max)
    {
        angle = (angle > 180) ? angle - 360 : angle;
        return Mathf.Clamp(angle, min, max);
    }
}
