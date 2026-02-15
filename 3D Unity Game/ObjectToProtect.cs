using UnityEngine;

public class ObjectToProtect : Entity
{
	private Transform player;

	protected override void Awake()
	{
		base.Awake();
		Player playerScript = FindFirstObjectByType<Player>();

		if (playerScript != null)
		{
			player = playerScript.transform;
		}
	}
	protected override void Update()
	{
		HandleFlip();
	}
	protected override void HandleFlip()
	{
		if (player == null) return; 
		if (player.position.x > transform.position.x && !isFacingRight)
		{
			Flip();
		}
		else if (player.position.x < transform.position.x && isFacingRight)
		{
			Flip();
		}
	}
	protected override void Die()
	{
		base.Die();
		UI.instance.EnableGameOverUI();
	}
}
