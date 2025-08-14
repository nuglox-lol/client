using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public enum CameraMode { Mode3D, Mode2D }

    public float rotationSpeed = 5f;
    public float zoomSpeed = 2f;
    public float minZoom = 0.1f;
    public float maxZoom = 6.6f;
    public float yOffset = 1.7f;
    public CameraMode cameraMode = CameraMode.Mode3D;
    private bool isMobile;

    private GameObject shiftLockIcon;
    private Transform target;
    private float currentZoom;
    private float rotationX;
    private float rotationY;
    private bool isFirstPerson;
    private bool isShiftLock;
    private bool shiftKeyDownLastFrame;
    private Coroutine interpolateRoutine;

    private Vector2 lastTouchPos;
    private bool isTouching;

    private int joystickLayer;

    private void Awake()
    {
        isMobile = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
        joystickLayer = LayerMask.NameToLayer("Joystick");
    }

    private void Start()
    {
        shiftLockIcon = GameObject.Find("CoreGui/ShiftLock");
        Camera cam = GetComponent<Camera>();
        if (cam != null) cam.enabled = true;
        AudioListener listener = GetComponent<AudioListener>();
        if (listener != null) listener.enabled = true;
        currentZoom = maxZoom;
        gameObject.tag = "MainCamera";
        if (shiftLockIcon) shiftLockIcon.SetActive(false);
    }

    public void SetTarget(Transform player)
    {
        target = player;
        Vector3 euler = target.rotation.eulerAngles;
        rotationX = euler.y;
        rotationY = 0f;
    }

    public void Interpolate(Vector3 pos, Quaternion rot, float time)
    {
        if (interpolateRoutine != null) StopCoroutine(interpolateRoutine);
        interpolateRoutine = StartCoroutine(SmoothMove(pos, rot, time));
    }

    private IEnumerator SmoothMove(Vector3 endPos, Quaternion endRot, float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
        transform.rotation = endRot;
    }

    public Ray ScreenPointToRay(Vector3 screenPos)
    {
        return GetComponent<Camera>().ScreenPointToRay(screenPos);
    }

    private void Update()
    {
        if (target == null) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        if (!isMobile)
        {
            bool shiftPressed = Input.GetKey(KeyCode.LeftShift);
            if (shiftPressed && !shiftKeyDownLastFrame)
            {
                isShiftLock = !isShiftLock;
                if (shiftLockIcon) shiftLockIcon.SetActive(isShiftLock);
            }
            shiftKeyDownLastFrame = shiftPressed;
        }
        else
        {
            isShiftLock = false;
            if (shiftLockIcon) shiftLockIcon.SetActive(false);
        }

        isFirstPerson = currentZoom <= minZoom + 0.01f;

        bool rotateCamera = false;
        if (isMobile)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    lastTouchPos = touch.position;
                    isTouching = true;
                }
                else if (touch.phase == TouchPhase.Moved && isTouching)
                {
                    Vector2 delta = touch.position - lastTouchPos;
                    lastTouchPos = touch.position;
                    rotationX += delta.x * rotationSpeed * 0.1f;
                    rotationY -= delta.y * rotationSpeed * 0.1f;
                    rotationY = Mathf.Clamp(rotationY, -45f, 45f);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    isTouching = false;
                }
                rotateCamera = isTouching;
            }
        }
        else
        {
            rotateCamera = isFirstPerson || isShiftLock || Input.GetMouseButton(1);
            if (rotateCamera)
            {
                rotationX += Input.GetAxis("Mouse X") * rotationSpeed;
                rotationY -= Input.GetAxis("Mouse Y") * rotationSpeed;
                rotationY = Mathf.Clamp(rotationY, -45f, 45f);
            }
        }

        Cursor.lockState = (isFirstPerson || isShiftLock) ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !(isFirstPerson || isShiftLock);

        Quaternion rot = Quaternion.Euler(rotationY, rotationX, 0f);
        Vector3 focusPoint = target.position + Vector3.up * yOffset;

        Vector3 camOffset = Vector3.zero;
        if (cameraMode == CameraMode.Mode3D)
        {
            camOffset = isFirstPerson ? Vector3.zero : rot * Vector3.back * currentZoom;
            transform.position = focusPoint + camOffset;
            transform.rotation = rot;
        }
        else if (cameraMode == CameraMode.Mode2D)
        {
            GetComponent<Camera>().orthographic = true;
            transform.position = focusPoint + new Vector3(0, 0, -10);
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        if (isFirstPerson || isShiftLock)
        {
            Vector3 lookDir = new Vector3(transform.forward.x, 0f, transform.forward.z);
            if (lookDir != Vector3.zero)
            {
                target.rotation = Quaternion.Slerp(target.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 20f);
            }
        }
    }

    public bool RaycastIgnoreJoystick(Ray ray, out RaycastHit hit, float maxDistance)
    {
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            if (hit.collider.gameObject.layer == joystickLayer)
                return false;
            return true;
        }
        return false;
    }
}
