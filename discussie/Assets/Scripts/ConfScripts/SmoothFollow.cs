using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    [SerializeField] public Transform targetTransform;
    [SerializeField] private bool followPosition;
    [SerializeField] private float positionSmoothSpeed;
    [SerializeField] private bool followRotation;

	private void Start()
	{
	}

	// Update is called once per frame
	void FixedUpdate()
    {
        if(targetTransform == null) { return; }
		if (followPosition)
		{
            Vector3 curPos = transform.position;
            Vector3 targetPos = new Vector3(targetTransform.position.x, targetTransform.position.y, curPos.z);
            Vector3 smoothedPosition = Vector3.Lerp(curPos, targetPos, positionSmoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }

		if (followRotation)
		{
            transform.rotation = targetTransform.rotation;
		}
    }
}
