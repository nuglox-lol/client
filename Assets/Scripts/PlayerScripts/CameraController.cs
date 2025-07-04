using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float rotationSpeed = 5f;

	public float zoomSpeed = 2f;

	public float minZoom;

	public float maxZoom = 6.6f;

	public float yOffset = 10f;

	private Transform target;

	private float currentZoom;

	private float rotationX;

	private float rotationY;

	private bool isFirstPerson;

	private void Start()
	{
		Camera component = GetComponent<Camera>();
		if (component != null)
		{
			component.enabled = true;
		}
		AudioListener component2 = GetComponent<AudioListener>();
		if (component2 != null)
		{
			component2.enabled = true;
		}
		currentZoom = maxZoom;
		base.gameObject.tag = "MainCamera";
	}

	public void SetTarget(Transform player)
	{
		target = player;
	}

	private void Update()
	{
		if (!(target == null))
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			currentZoom -= axis * zoomSpeed;
			currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
			if (currentZoom <= minZoom && !isFirstPerson)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				isFirstPerson = true;
			}
			else if (currentZoom > minZoom && isFirstPerson)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				isFirstPerson = false;
			}
			if (isFirstPerson || Input.GetMouseButton(1))
			{
				rotationX += Input.GetAxis("Mouse X") * rotationSpeed;
				rotationY -= Input.GetAxis("Mouse Y") * rotationSpeed;
				rotationY = Mathf.Clamp(rotationY, -45f, 45f);
			}
			Quaternion quaternion = Quaternion.Euler(rotationY, rotationX, 0f);
			Vector3 vector = target.position + Vector3.up * yOffset;
			Vector3 vector2 = quaternion * Vector3.back * currentZoom;
			base.transform.position = vector + vector2;
			base.transform.LookAt(vector);
		}
	}
}
