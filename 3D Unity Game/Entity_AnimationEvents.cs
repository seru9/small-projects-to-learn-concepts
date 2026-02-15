using System;
using UnityEngine;

public class Entity_AnimationEvents : MonoBehaviour
{
	private Entity player;
	private void Awake()
	{
		player = GetComponentInParent<Entity>(); // BECAUSE PARENT IS A PARENT OF ANIMATOR, NOT THE SAME GAMEOBJECT
	}
	private void EnableJumpAndMovement()
	{
		player.EnableJumpAndMovement(true);
	}
	private void DisableJumpAndMovement()
	{
		player.EnableJumpAndMovement(false);
	}
	public void DamageTargets()
	{
		player.DamageTargets();
	}
}
