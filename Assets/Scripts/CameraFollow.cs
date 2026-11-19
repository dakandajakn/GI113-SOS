using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[Header("Target")]
	[SerializeField] Transform target; 
	[Header("Mario Camera Settings")]
	[SerializeField] Vector3 offset = new Vector3(2f, 1f, -10f); 
	[SerializeField] float smoothSpeed = 5f; 
	[SerializeField] bool followY = false; 
	[SerializeField] bool lockLeftScroll = false; 
	[Header("Level Boundaries (ขอบเขตฉาก)")]
	[SerializeField] float minX = 0f;  
	[SerializeField] float maxX = 50f;  

	void LateUpdate()
	{
		if (target == null) return;
		
		float desiredX = target.position.x + offset.x;

		if (lockLeftScroll && transform.position.x > minX)
		{
			minX = transform.position.x;
		}

		float clampedX = Mathf.Clamp(desiredX, minX, maxX);

		float desiredY = followY ? (target.position.y + offset.y) : offset.y;

		Vector3 targetPosition = new Vector3(clampedX, desiredY, offset.z);

		transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
	}
}
