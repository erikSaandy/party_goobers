using Sandbox;
using System;

public sealed class FlipperComponent : Component, IInteractable
{
	[Property] public SkinnedModelRenderer Renderer { get; set; }

	public bool IsInteractableBy( Player player ) => true;

	[Sync] public TimeSince TimeSinceHit { get; set; } = 0;

	protected override void OnUpdate()
	{
		base.OnUpdate();

	}

	public void OnInteract( Guid playerId, SceneTraceResult traceResult )
	{
		if( traceResult.Hitbox == null) { return; }

		if(TimeSinceHit < 1) { return; }

		if( traceResult.Hitbox.Tags.Contains( "target" ) )
		{
			HitTarget();
		}

	}

	[Broadcast]
	void HitTarget()
	{
		TimeSinceHit = 0;
		Renderer.Set( "b_hit", true );
	}

}
