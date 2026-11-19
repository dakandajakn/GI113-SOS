using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
	[SerializeField] string playerName = "Alien";
	[Range(1, 100)]
	[SerializeField] float moveSpeed = 5.5f;
	[Range(1, 100)]
	[SerializeField] float jumpForce = 5.5f;
	[Header("Rope Climbing")]
	[SerializeField] float climbSpeed = 4f;
	bool isNearRope = false;
	bool isClimbing = false;
	float originalGravity;
	[Header("Coin System")]
	[SerializeField] int totalCoin = 10;
	int coin = 0;
	[SerializeField] TMP_Text coinText;
	bool isGrounded = true;
	Rigidbody2D rb;
	SpriteRenderer sprites;
	Vector2 move;
	void Start()
	{
		InputSystem.actions.Enable();
		rb = GetComponent<Rigidbody2D>();
		sprites = GetComponent<SpriteRenderer>();
		originalGravity = rb.gravityScale;
		UpdateCoinUI();
	}

	void Update()
	{
		move = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();

		if (isNearRope && Mathf.Abs(move.y) > 0.1f)
		{
			isClimbing = true;
		}
		if (isClimbing)
		{
			rb.gravityScale = 0f;

			if (InputSystem.actions.FindAction("Jump").WasPressedThisFrame())
			{
				isClimbing = false;
				rb.gravityScale = originalGravity;
				rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
			}
		}
		else
		{
			rb.gravityScale = originalGravity;

			if (InputSystem.actions.FindAction("Jump").WasPressedThisFrame() && isGrounded)
			{
				rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
			}
		}
		if (move.x > 0.1f)
		{
			sprites.flipX = false;
		}
		else if (move.x < -0.01f)
		{
			sprites.flipX = true;
		}
	}
	private void FixedUpdate()
	{
		if (isClimbing)
		{
			rb.linearVelocity = new Vector2(move.x * moveSpeed, move.y * climbSpeed);
		}
		else
		{
			rb.linearVelocity = new Vector2(move.x * moveSpeed, rb.linearVelocity.y);
		}
	}
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Coin"))
		{
			coin++;
			Destroy(collision.gameObject);
			UpdateCoinUI();

			Debug.Log($"เก็บเหรียญได้: {coin} / {totalCoin}");

			if (coin >= totalCoin)
			{
				Debug.Log("เก็บเหรียญครบ 10 เหรียญแล้ว!");
			}
		}
		if (collision.CompareTag("Rope"))
		{
			isNearRope = true;
		}
	}
	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Rope"))
		{
			isNearRope = false;
			isClimbing = false;
			rb.gravityScale = originalGravity;
		}
	}
	void UpdateCoinUI()
	{
		if (coinText != null)
		{
			coinText.text = $"Coin: {coin} / {totalCoin}";
		}
	}
	void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.GetContact(0).normal.y > 0.5f)
		{
			isGrounded = true;
		}
	}
	void OnCollisionExit2D(Collision2D collision)
	{
		isGrounded = false;
	}
}
