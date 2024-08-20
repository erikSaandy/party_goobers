using Sandbox;
using System;
using System.Reflection.Metadata;

public class FlipperComponent : Component, IInteractable
{
	[Property] public SkinnedModelRenderer Renderer { get; set; }

	private SoundHandle sound { get; set; }

	public bool IsInteractableBy( Player player ) => true;

	[Sync] public TimeSince TimeSinceHit { get; set; } = 0;

	protected override void OnAwake()
	{
		base.OnAwake();

	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

	}

	public void OnInteract( Guid playerId, SceneTraceResult traceResult )
	{
		if( traceResult.Hitbox == null) { return; }

		if(TimeSinceHit < 1.5f) { return; }

		if( traceResult.Hitbox.Tags.Contains( "target" ) )
		{
			HitTarget();
		}

	}

	[Broadcast]
	void HitTarget()
	{
		TimeSinceHit = 0;
		sound?.Stop();
		sound = Sound.Play( "sounds/flipper_hit.sound" );
		Sound.Play( "sounds/gun_shot.sound" );
		Renderer.Set( "b_hit", true );
	}

}
