using UnityEngine;

public class Enemy : MonoBehaviour
{
	[SerializeField] float speed = 2f;
	[SerializeField] float moveDistance = 3f; 
	[SerializeField] bool spriteFacesRight = false; 

	Vector3 startPos;
	int direction = 1; 
	SpriteRenderer sprite;
	float turnCooldown = 0f;

	void Start()
	{
		startPos = transform.position;
		sprite = GetComponent<SpriteRenderer>();
		UpdateFacing();
	}

	void Update()
	{
		if (turnCooldown > 0f)
		{
			turnCooldown -= Time.deltaTime;
		}

		transform.Translate(Vector2.right * direction * speed * Time.deltaTime);

		if (transform.position.x >= startPos.x + moveDistance)
		{
			transform.position = new Vector3(startPos.x + moveDistance, transform.position.y, transform.position.z);
			direction = -1;
			UpdateFacing();
		}
		else if (transform.position.x <= startPos.x - moveDistance)
		{
			transform.position = new Vector3(startPos.x - moveDistance, transform.position.y, transform.position.z);
			direction = 1;
			UpdateFacing();
		}
	}
	private void OnCollisionEnter2D(Collision2D collision)
	{
		CheckWallTurn(collision);
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		CheckWallTurn(collision);
	}
	void CheckWallTurn(Collision2D collision)
	{
		if (turnCooldown > 0f || collision.gameObject.CompareTag("Player")) return;

		foreach (ContactPoint2D contact in collision.contacts)
		{
			if (Mathf.Abs(contact.normal.x) > 0.5f)
			{
				direction *= -1;
				UpdateFacing();
				turnCooldown = 0.25f;
				break;
			}
		}
	}
	void UpdateFacing()
	{
		if (sprite == null) return;

		if (direction == 1) 
		{
			sprite.flipX = !spriteFacesRight;
		}
		else 
		{
			sprite.flipX = spriteFacesRight;
		}
	}
}
