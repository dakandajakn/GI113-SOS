using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
	[SerializeField] string playerName = "";
	[Range(1, 100)]
	[SerializeField] float moveSpeed = 5.5f;
	[Range(1, 100)]
	[SerializeField] float jumpForce = 5.5f;
	[Header("Health System")]
	[SerializeField] int maxHealth = 3;
	int currentHealth;
	[SerializeField] GameObject[] heartIcons;
	[SerializeField] TMP_Text healthText;
	[SerializeField] float invincibleDuration = 1.2f;
	[SerializeField] float knockbackForce = 6f;
	bool isInvincible = false;
	[Header("Rope Climbing")]
	[SerializeField] float climbSpeed = 4f;
	bool isNearRope = false;
	bool isClimbing = false;
	float originalGravity;
	[Header("Coin System")]
	[SerializeField] int totalCoin = 10;
	int coin = 0;
	[SerializeField] TMP_Text coinText;
	[Header("Goal Chest")]
	[SerializeField] GameObject treasureChest;

	[Header("Fall / Game Over")]
	[SerializeField] GameObject restartUI;
	bool isGrounded = true;
	bool isPaused = false;
	Rigidbody2D rb;
	SpriteRenderer sprites;
	Vector2 move;
	void Start()
	{
		Time.timeScale = 1f;
		InputSystem.actions.Enable();
		rb = GetComponent<Rigidbody2D>();
		sprites = GetComponent<SpriteRenderer>();
		originalGravity = rb.gravityScale;

		currentHealth = maxHealth;
		UpdateHealthUI();
		UpdateCoinUI();

		if (treasureChest != null)
		{
			treasureChest.SetActive(false);
		}

		if (restartUI != null)
		{
			restartUI.SetActive(false);
		}
	}

	void Update()
	{
		if (isPaused) return;

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
		if (isPaused) return;

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

			if (coin >= totalCoin && treasureChest != null)
			{
				treasureChest.SetActive(true);
			}
		}

		if (collision.CompareTag("Rope"))
		{
			isNearRope = true;
		}

		if (collision.gameObject.name == "FallZone")
		{
			FallGameOver();
		}

		CheckEnemyHit(collision.gameObject);
		CheckWinAndPause(collision.gameObject);
	}
	private void OnTriggerStay2D(Collider2D collision)
	{
		CheckEnemyHit(collision.gameObject);
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
	private void OnCollisionEnter2D(Collision2D collision)
	{
		CheckEnemyHit(collision.gameObject);
		CheckWinAndPause(collision.gameObject);
	}
	void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.GetContact(0).normal.y > 0.5f)
		{
			isGrounded = true;
		}

		CheckEnemyHit(collision.gameObject);
	}
	void OnCollisionExit2D(Collision2D collision)
	{
		isGrounded = false;
	}
	void CheckEnemyHit(GameObject hitObject)
	{
		if (isPaused || isInvincible) return;

		if (hitObject.name.Contains("Enemy"))
		{
			currentHealth--;
			UpdateHealthUI();

			if (currentHealth <= 0)
			{
				FallGameOver();
			}
			else
			{
				isClimbing = false;
				rb.gravityScale = originalGravity;
				float knockDir = (transform.position.x < hitObject.transform.position.x) ? -1f : 1f;
				rb.linearVelocity = new Vector2(knockDir * knockbackForce, jumpForce * 0.7f);

				StartCoroutine(InvincibleRoutine());
			}
		}
	}
	IEnumerator InvincibleRoutine()
	{
		isInvincible = true;
		float elapsed = 0f;

		while (elapsed < invincibleDuration)
		{
			sprites.enabled = !sprites.enabled;
			yield return new WaitForSeconds(0.12f);
			elapsed += 0.12f;
		}

		sprites.enabled = true;
		isInvincible = false;
	}
	void UpdateHealthUI()
	{
		if (heartIcons != null)
		{
			for (int i = 0; i < heartIcons.Length; i++)
			{
				if (heartIcons[i] != null)
				{
					heartIcons[i].SetActive(i < currentHealth);
				}
			}
		}

		if (healthText != null)
		{
			healthText.text = $"HP: {currentHealth} / {maxHealth}";
		}
	}
	void FallGameOver()
	{
		if (isPaused) return;

		isPaused = true;
		sprites.enabled = true;
		rb.linearVelocity = Vector2.zero;
		Time.timeScale = 0f;

		if (restartUI != null)
		{
			restartUI.SetActive(true);
		}
	}
	void CheckWinAndPause(GameObject hitObject)
	{
		if (isPaused) return;

		bool isChest = (treasureChest != null && hitObject == treasureChest) || hitObject.name == "Collectibles_77";

		if (isChest && coin >= totalCoin)
		{
			isPaused = true;
			rb.linearVelocity = Vector2.zero;
			Time.timeScale = 0f;
			Debug.Log("เก็บเหรียญครบและชนกล่องแล้ว - เกม Pause!");
		}
	}
	void UpdateCoinUI()
	{
		if (coinText != null)
		{
			coinText.text = $"Coin: {coin} / {totalCoin}";
		}
	}
	public void RestartGame()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}