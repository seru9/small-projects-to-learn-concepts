using UnityEngine;

public class Player : Entity
{
	private float xInput;
	protected bool canJump = true;
	[Header("Movement details")]
	[SerializeField] protected float moveSpeed = 3.5f; // ALOWS US TO SET THE SPEED IN THE INSPECTOR EVEN YHOU CAN'T ACCESS IT DIRECTLY
	protected override void Update()
	{
		base.Update();
		HandleInput();
	}
	protected override void HandleMovement()
	{
		if (canMove)
			rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
		else
			rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
	}
	private void TryToJump()
	{
		if (isGrounded && canJump)
			rb.linearVelocity = new Vector2(xInput, jumpForce);

	}
	private void HandleInput()
	{
		xInput = Input.GetAxisRaw("Horizontal"); // GETS THE HORIZONTAL INPUT FROM THE PLAYER (-1 TO 1)
		if (Input.GetButtonDown("Jump"))
		{
			TryToJump();
		}
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			HandleAttack();
		}

	}
	public virtual void EnableJumpAndMovement(bool enable)
	{
		canMove = enable;
		canJump = enable;
	}
	protected override void Die()
	{
		base.Die();
		if (UI.instance != null)
		{
			UI.instance.EnableGameOverUI();
		}
	}
}
