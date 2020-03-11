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

	[Header("Rotation")]
	[SerializeField]
	float rotationSpeed = 5.0f;

	[Header("Cursor Settings")]
	[SerializeField]
	bool cursorvisibility;
	[SerializeField]
	CursorLockMode cursorlockstate;

	[Header("Zoom Settings")]
	[SerializeField]
	float zoomSpeed = 25f;
	[SerializeField]
	float maxZoomDistance = 151.51f;
	[SerializeField]
	float minZoomDistance = 0f;

	float mouseXValue;
	float mouseYValue;

	public Transform target, player;

	// you can remove this camera :)
	Camera camera;

	//new vars idk if i will use all
	[SerializeField]
	public float distanceFromTarget = 8f;
	[SerializeField]
	public float adjustmentDistance = -8f;

	[SerializeField]
	public bool drawDesiredCollisionLines = true;
	[SerializeField]
	public bool drawAdjustedCollisionLines = true;

	public CollisionHandler collision = new CollisionHandler();

	public Vector3 destination = Vector3.zero;
	Vector3 adjustedDestination = Vector3.zero;
	Vector3 camVel = Vector3.zero;

	// Start is called before the first frame update
	void Start()
	{
		camera = GetComponent<Camera>();
		Cursor.visible = cursorvisibility;
		Cursor.lockState = cursorlockstate;

		//need MoveToTarget()??? - we don't need to make the camera move to target because it's a child of the target 

		collision.Initialize(camera);
		// if you remove camera variable in this class, you can change the line this below.
		//collision.Initialize(GetComponent<Camera>());
	}

	// Update is called once per frame
	void Update()
	{
		destination = target.position - transform.forward * distanceFromTarget;

		collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);
		collision.UpdateCameraClipPoints(destination, transform.rotation, ref collision.desiredCameraClipPoints);

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


		if (Input.GetKey(KeyCode.LeftShift))
		{
			target.rotation = Quaternion.Euler(mouseYValue, mouseXValue, 0);
		}
		else
		{
			target.rotation = Quaternion.Euler(mouseYValue, mouseXValue, 0);
			player.rotation = Quaternion.Euler(0, mouseXValue, 0);
		}

		if (Input.GetAxis(mouseScrollId) != 0)
		{
			if (distanceFromTarget == 0)
			{
				distanceFromTarget++;
			}
			distanceFromTarget += -Input.GetAxis(mouseScrollId) * zoomSpeed;
			distanceFromTarget = Mathf.Clamp(distanceFromTarget, minZoomDistance, maxZoomDistance);
		}
		else if (distanceFromTarget < 2)
		{
			distanceFromTarget = 0;
		}

		// I moved 2 lines below from FixedUpdate() to here for more clear readability.
		collision.CheckColliding(target.position); //using raycasts here
		adjustmentDistance = collision.GetAdjustedDistanceWithRayFrom(target.position);

		Debug.Log(collision.colliding);
		if(collision.colliding)
		{
			if (adjustmentDistance < 1)
			{
				adjustmentDistance = 0;
			}
			adjustedDestination = target.position - transform.forward * adjustmentDistance;
			transform.position = adjustedDestination;
		}
		else
		{ 
			destination = target.position - transform.forward * distanceFromTarget;
			transform.position = destination;
		}

		// I think the reason we don't need LookAt function anymore is becasue we are setting camera's position depends on if the camera's raycasts are colliding or not.
		//transform.LookAt(target);
	}

	//void MoveToTarget()
	//{
	//	targetPos = Target.position + Vector3.up*ImagePosition.targetPosOffset.y 
	//}

	[System.Serializable]
	public class CollisionHandler
	{
		public LayerMask collisionLayer;

		[System.NonSerialized]
		public bool colliding = false;
		[System.NonSerialized] 
		public Vector3[] adjustedCameraClipPoints;
		[System.NonSerialized] 
		public Vector3[] desiredCameraClipPoints;

		//value used to increase or decrease the size of your collision space
		public float collisionSpaceSize = 3.41f;


		Camera camera;

		public void Initialize(Camera cam)
		{
			camera = cam;
			adjustedCameraClipPoints = new Vector3[5]; //4 clip points + cam position
			desiredCameraClipPoints = new Vector3[5];
		}
		public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion atRotation, ref Vector3[] intoArray)
		{
			if(!camera)
			{
				return; 
			}

			//clear the contents of intoArray to make room for new content
			intoArray = new Vector3[5];

			// distance from camera position to near clip plane
			float z = camera.nearClipPlane; 
			float x = Mathf.Tan(camera.fieldOfView / collisionSpaceSize) * z;
			float y = x / camera.aspect;

			//top-left 
			intoArray[0] = (atRotation * new Vector3(-x, y, z)) + cameraPosition;
			//top-right
			intoArray[1] = (atRotation * new Vector3(x, y, z)) + cameraPosition;
			//bottom-left
			intoArray[2] = (atRotation * new Vector3(-x, -y, z)) + cameraPosition;
			//bottom-right
			intoArray[3] = (atRotation * new Vector3(x, -y, z)) + cameraPosition;
			//camera position
			intoArray[4] = cameraPosition;
			//intoArray[4] = cameraPosition; // remove subtracting forward. the reason is.. if you minus camera's forward which has length of 1, the actual position will be set futher back than the camera's position. So the problem was, when you locate the camera parellel with the ground, and move backwards toward to the wall, the camera snaps before the camera hits the wall. because camera's forward vecter is subtracted.
		}
		bool CollisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 fromPosition)
		{
			for(int i = 0; i < clipPoints.Length; i++)
			{
				// cast ray at fromPosition in point position direction for distance's distance
				// if it runs into collision layer, return true. otherwise return false
				Ray ray = new Ray(fromPosition, clipPoints[i]-fromPosition);
				float distance = Vector3.Distance(clipPoints[i], fromPosition);

				if (Physics.Raycast(ray, distance, collisionLayer))
				{
					return true;
				}
			}
			return false;
		}

		public float GetAdjustedDistanceWithRayFrom(Vector3 from)
		{
			float distance = -1;
			for (int i = 0; i<desiredCameraClipPoints.Length; i++)
			{
				Ray ray = new Ray(from, desiredCameraClipPoints[i]-from);
				RaycastHit hit;

				//side note: this piece of code is so sexy, yum *w* 
				//Need to have Key do a voice over for this code >ㅂ<
				if(Physics.Raycast(ray, out hit))
				{
					if(distance == -1)
					{ 
						distance = hit.distance; 
					}
					else
					{
						if (hit.distance < distance)
						{
							distance = hit.distance;
						}
					}
				}
			}
			if (distance == -1)
			{
				return 0;
			}
			else
			{
				return distance;
			}
		}

		public void CheckColliding(Vector3 targetPosition)
		{
			if(CollisionDetectedAtClipPoints(desiredCameraClipPoints, targetPosition /*fromposition*/))
			{
				//collision happened with at least one clip point
				colliding = true;
			}
			else 
			{
				//collision didn't happen
				colliding = false;
			}
		}
	}
}
