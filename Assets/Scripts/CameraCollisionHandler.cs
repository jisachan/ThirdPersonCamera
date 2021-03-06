﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraCollisionHandler
{
	Camera camera;

	[SerializeField]
	LayerMask collisionLayer;
	//value used to increase or decrease the size of your collision space
	[SerializeField]
	float collisionSpaceSize = 3.41f;

	[System.NonSerialized]
	public bool colliding = false;
	[System.NonSerialized]
	public Vector3[] adjustedCameraClipPoints;
	[System.NonSerialized]
	public Vector3[] desiredCameraClipPoints;

	public void Initialize(Camera cam)
	{
		camera = cam;

		//4 clip points + cam position = [5] vectors.
		adjustedCameraClipPoints = new Vector3[5]; 
		desiredCameraClipPoints = new Vector3[5];
	}
	public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion atRotation, ref Vector3[] intoArray)
	{
		if (!camera)
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
	}
	bool CollisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 fromPosition)
	{
		for (int i = 0; i < clipPoints.Length; i++)
		{
			// cast ray at fromPosition in point position direction for distance's distance <- best comment sentence ever written. keeping this. YOLO ^^/
			// if it runs into collision layer, return true. otherwise return false
			Ray ray = new Ray(fromPosition, clipPoints[i] - fromPosition);
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
		for (int i = 0; i < desiredCameraClipPoints.Length; i++)
		{
			Ray ray = new Ray(from, desiredCameraClipPoints[i] - from);
			RaycastHit hit;

			// Note to self: this piece of code is so sexy, yum *w* 
			// need to have Key do a voice over for this code >ㅂ<
			if (Physics.Raycast(ray, out hit))
			{
				if (distance == -1)
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
		if (CollisionDetectedAtClipPoints(desiredCameraClipPoints, targetPosition))
		{
			colliding = true;
		}
		else
		{
			colliding = false;
		}
	}
}
