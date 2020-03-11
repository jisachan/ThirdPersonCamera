using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCharacterController : MonoBehaviour
{
	Vector3 direction;

	[Header("Movement")]
	[SerializeField]
	float speed = 5.0f;

	// Update is called once per frame
	void Update()
	{
		GetInput();
	}

	private void GetInput()
	{
		direction = Vector3.zero;

		if (Input.GetKey(KeyCode.W))
		{
			direction += transform.forward;
		}
		if (Input.GetKey(KeyCode.S))
		{
			direction += -transform.forward;
		}
		CharacterMovement();
	}

	private void CharacterMovement()
	{
		transform.position = transform.position + (direction * speed * Time.deltaTime);
	}
}
