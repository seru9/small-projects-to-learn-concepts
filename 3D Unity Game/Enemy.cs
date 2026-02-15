using UnityEngine;

public class Enemy : Entity
{
	private bool playerDetected;
	[Header("Movement details")]
	[SerializeField] protected float moveSpeed = 3.5f; // ALOWS US TO SET THE SPEED IN THE INSPECTOR EVEN YHOU CAN'T ACCESS IT DIRECTLY
	protected override void Update()
	{
		base.Update();
		HandleAttack();
	}
	protected override void HandleAttack()
	{
		if (playerDetected)
		{
			Debug.Log("Enemy attack!"); // FOR TESTING PURPOSES
			anim.SetTrigger("attack"); 
		}
	}
    protected override void HandleMovement()
    {
		if (canMove)
			rb.linearVelocity = new Vector2(facingDir * moveSpeed, rb.linearVelocity.y);
		else
			rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
	}
	protected override void HandleCollision()
	{
		base.HandleCollision();
		playerDetected = Physics2D.OverlapCircle(attackPoint.position, attackRadius, whatIsTarget);
	}
}