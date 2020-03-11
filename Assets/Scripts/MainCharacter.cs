using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacter : MonoBehaviour
{
	//   #region Input IDs
	//   const string mouseXId = "Mouse X";
	//   const string mouseYId = "Mouse Y";
	//#endregion

	[Header("Movement")]
	[SerializeField]
	float speed = 5.0f;

	//[Header("Rotation")]
	//[SerializeField]
	//float rotationSpeed = 5.0f;
	//float mouseXValue;
	//float mouseYValue;

	Vector3 position;
	Vector3 direction;

	// Start is called before the first frame update
	void Start()
	{
	}

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
			CharacterMovement();
		}
		if (Input.GetKey(KeyCode.S))
		{
			direction += -transform.forward;
			CharacterMovement();
		}
		//if(Input.GetMouseButton(1))
		//{
		//    mouseXValue = Input.GetAxis(mouseXId);
		//    transform.Rotate(new Vector3(0, mouseXValue * rotationSpeed, 0));
		//}
	}

	private void CharacterMovement()
	{
		transform.position = transform.position + (direction * speed * Time.deltaTime);
	}
}
