using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCameraController : MonoBehaviour
{
	#region Input IDs
	const string mouseXId = "Mouse X";
	const string mouseYId = "Mouse Y";
	const string mouseScrollId = "Mouse ScrollWheel";
	#endregion

	public Transform target, player;

	public bool smoothFollow = true;
	public float smooth = 0.05f;

	Vector3 camVel = Vector3.zero;
	Vector3 destination = Vector3.zero;
	Vector3 adjustedDestination = Vector3.zero;

	[Header("Collision Handler")]
	[SerializeField]
	CameraCollisionHandler collision = new CameraCollisionHandler();

	[Header("Rotation")]
	[SerializeField]
	float rotationSpeed = 5.0f;

	[Header("Control Keys")]
	[SerializeField]
	KeyCode cameraFreeMovementKey = KeyCode.LeftShift;

	[Header("Cursor Settings")]
	[SerializeField]
	bool cursorvisibility = false;
	[SerializeField]
	CursorLockMode cursorlockstate;

	[Header("Zoom Settings")]
	[SerializeField]
	float zoomSpeed = 25f;
	[SerializeField]
	float maxZoomDistance = 151.51f;
	[SerializeField]
	float minZoomDistance = 0f;
	[SerializeField]
	public float distanceFromTarget = 8f;
	[SerializeField]
	public float adjustmentDistance = -8f;
	float mouseXValue;
	float mouseYValue;

	[Header("Collision Debug")]
	[SerializeField]
	public bool drawDesiredCollisionLines = true;
	[SerializeField]
	public bool drawAdjustedCollisionLines = true;

	// Start is called before the first frame update
	void Start()
	{
		Cursor.visible = cursorvisibility;
		Cursor.lockState = cursorlockstate;

		collision.Initialize(GetComponent<Camera>());
	}

	// Update is called once per frame
	void Update()
	{
		destination = target.position - transform.forward * distanceFromTarget;

		// used for shooting raycasts to check for collision
		collision.UpdateCameraClipPoints(destination, transform.rotation, ref collision.desiredCameraClipPoints);
		// currently used for debugging adjusted camera position after collision
		collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);

		//draw debug lines
		for (int i = 0; i < 5; i++)
		{
			if (drawDesiredCollisionLines)
			{
				Debug.DrawLine(target.position, collision.desiredCameraClipPoints[i], Color.magenta);
			}
			if (drawAdjustedCollisionLines)
			{
				Debug.DrawLine(target.position, collision.adjustedCameraClipPoints[i], Color.cyan);
			}
		}

		CamControl();
	}

	void CamControl()
	{
		mouseXValue += Input.GetAxis(mouseXId) * rotationSpeed;
		mouseYValue -= Input.GetAxis(mouseYId) * rotationSpeed;

		mouseYValue = Mathf.Clamp(mouseYValue, -35, 60);
		
		//allow player to control camera seperate from player movement
		if (Input.GetKey(cameraFreeMovementKey))
		{
			target.rotation = Quaternion.Euler(mouseYValue, mouseXValue, 0);
		}
		//turn player along with camera's rotation
		else
		{
			target.rotation = Quaternion.Euler(mouseYValue, mouseXValue, 0);
			player.rotation = Quaternion.Euler(0, mouseXValue, 0);
		}

		//zoom controls and functionality
		if (Input.GetAxis(mouseScrollId) != 0)
		{
			//prevent getting stuck in a loop when zooming back out from 0 distance;
			if (distanceFromTarget == 0)
			{
				distanceFromTarget++;
			}
			distanceFromTarget += -Input.GetAxis(mouseScrollId) * zoomSpeed;
			distanceFromTarget = Mathf.Clamp(distanceFromTarget, minZoomDistance, maxZoomDistance);
		}
		//enter first person view when zooming in at a close distance
		else if (distanceFromTarget < 2)
		{
			distanceFromTarget = 0;
		}

		//using raycasts here
		collision.CheckColliding(target.position);
		adjustmentDistance = collision.GetAdjustedDistanceWithRayFrom(target.position);

		if (collision.colliding)
		{
			//enter first person view when colliding at a close distance behind player
			if (adjustmentDistance < 1)
			{
				adjustmentDistance = 0;
			}
			adjustedDestination = target.position - transform.forward * adjustmentDistance;

			if (smoothFollow)
			{
				//smoothening camera's position change transition
				transform.position = Vector3.SmoothDamp(transform.position, adjustedDestination, ref camVel, smooth);
			}
			else
			{
				transform.position = adjustedDestination;
			}
		}
		else
		{
			destination = target.position - transform.forward * distanceFromTarget;
			if (smoothFollow)
			{
				transform.position = Vector3.SmoothDamp(transform.position, destination, ref camVel, smooth);
			}
			else
			{
				transform.position = destination;
			}			
		}
	}
}
