using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [SerializeField] private float maxSpeed;
    [SerializeField] private float rotationSmoothSpeed;
    private Vector3 touchStartPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
		{
            transform.position += CalculateMoveVector() * Time.deltaTime;
            LookAtMouse();
        }

        
        //transform.LookAt(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane)), transform.forward*-1);
    }

    private Vector3 CalculateMoveVector()
	{
        float normalisedMagnitude = 0;
        Vector3 difVec = Input.mousePosition - touchStartPos;
        Vector3 dirVec = difVec.normalized;

        if(Screen.height > Screen.width)
		{
            normalisedMagnitude = difVec.magnitude / (Screen.width / 2);
		}
		else
		{
            normalisedMagnitude = difVec.magnitude / (Screen.height / 2);
        }

        return Vector3.Lerp(dirVec * 0, dirVec * maxSpeed, normalisedMagnitude);
	}

    private void LookAtMouse()
	{
        Vector3 target_pos = Input.mousePosition;
        target_pos.z = 10; //The distance between the camera and object
        Vector3 start_pos = touchStartPos;
        target_pos.x = target_pos.x - start_pos.x;
        target_pos.y = target_pos.y - start_pos.y;
        float angle = Mathf.Atan2(target_pos.y, target_pos.x) * Mathf.Rad2Deg;

        Quaternion smoothedRot = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, 0, angle)), rotationSmoothSpeed * Time.deltaTime);

        transform.rotation = smoothedRot;
    }
}
