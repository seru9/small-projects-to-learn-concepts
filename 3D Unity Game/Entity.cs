using System.Collections;
using TMPro.EditorUtilities;
using UnityEngine;

public class Entity : MonoBehaviour
{
	protected Rigidbody2D rb;
	protected Collider2D col;
	protected SpriteRenderer sr;
	[SerializeField] private Material damageMaterial;
	[SerializeField] private Coroutine damageFeedbackCoroutine;

	[Header("Attack details")]
	[SerializeField] protected float attackRadius;
	[SerializeField] protected Transform attackPoint;
	[SerializeField] protected LayerMask whatIsTarget;
	
	[Header("Health details")]
	[SerializeField] protected int maxHealth = 100;
	[SerializeField] protected int currentHealth;

	[SerializeField] protected float jumpForce = 8;
	private bool isMoving;
	protected Animator anim;
	protected bool isFacingRight = true;
	protected int facingDir = 1;
	
	[Header("Collision details")]
	[SerializeField] private float groundCheckDistance;
	protected bool isGrounded;
	protected bool canMove = true;
	[SerializeField] private LayerMask whatIsGround;
	protected virtual void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponentInChildren<Animator>();
		col = GetComponent<Collider2D>();
		sr = GetComponentInChildren<SpriteRenderer>();
		currentHealth = maxHealth;
	}
	protected virtual void Update()
	{
		HandleCollision();
		HandleMovement();
		HandleAnimations();
		HandleFlip();
	}
	public void DamageTargets()
	{
		Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsTarget); // takes all colliders in  radius and make tha array out of this
		foreach(var enemyCollider in enemyColliders)
		{
			Entity entityTarget = enemyCollider.GetComponent<Entity>();
			entityTarget.TakeDamage();
		}
	} 
	private IEnumerator DamageFeedbackCo()
	{
		Material originalMaterial = sr.material;

		sr.material = damageMaterial;

		yield return new WaitForSeconds(0.2f); // DURATION OF THE DAMAGE FEEDBACK
		sr.material = originalMaterial;
	}
	public virtual void EnableJumpAndMovement(bool enable)
	{
		canMove = enable;
	}

	[ContextMenu("Flip")]
	public void Flip()
	{
		transform.Rotate(0, 180, 0);
		isFacingRight = !isFacingRight;
		facingDir *= -1;
	}
	protected void HandleAnimations()
	{
		anim.SetBool("isGrounded", isGrounded);
		anim.SetFloat("xVelocity", rb.linearVelocity.x);
		anim.SetFloat("yVelocity", rb.linearVelocity.y);
	}
	protected virtual void HandleFlip()
	{
		if(rb.linearVelocity.x > 0 && !isFacingRight)
		{
			Flip();
		}
		else if(rb.linearVelocity.x < 0 && isFacingRight)
		{
			Flip();
		}
	}
	protected virtual void HandleAttack()
	{

		if (isGrounded)
		{
			anim.SetTrigger("attack");
		}
	}
	
	protected virtual void HandleMovement()
	{
		// IMPLEMENT IN CHILD CLASSES	
	}
	protected virtual void HandleCollision()
	{
		isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
	}
	protected virtual void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
		if(attackPoint != null)
		{
			Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
		}
	}

	private void TakeDamage()
	{
		currentHealth -= 10;
		PlayDamageFeedback();
		if (currentHealth <= 0)
		{
			Die();
		}
	}

	private void PlayDamageFeedback()
	{
		if (damageFeedbackCoroutine != null)
		{
			StopCoroutine(damageFeedbackCoroutine);
		}
		damageFeedbackCoroutine = StartCoroutine(DamageFeedbackCo());
	}

	protected virtual void Die()
	{
		anim.enabled = false; // DISABLES THE ANIMATOR TO PREVENT ANY FURTHER ANIMATION CHANGES
		col.enabled = false; // DISABLES THE COLLIDER TO PREVENT ANY FURTHER COLLISIONS
		rb.gravityScale = 12; // INCREASES THE GRAVITY SCALE TO MAKE THE ENTITY FALL FASTER
		rb.linearVelocity = new Vector2(rb.linearVelocity.x, 15); // BOUNCE BACK UP WHEN DIE
		Destroy(gameObject, 3); 
	}
}

/*
 * When adjusting a player's Animator make sure it's Position is set to 0.5 0 0 
 */