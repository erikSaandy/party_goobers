using Sandbox;
using System;

public sealed class DistractionComponent : Component, IInteractable
{
	[RequireComponent][Property] private SkinnedModelRenderer Renderer { get; set; }

	public bool IsInteractableBy( Player player ) => true;

	public void OnInteract( Guid playerId, SceneTraceResult traceResult ) { Log.Info( "player clicked on distraction." ); }

	[Broadcast][Button("Activate")]
	public void Distract()
	{
		Renderer.Set( "b_distract", true );
	}

	protected override  void OnStart()
	{
		base.OnStart();

		Renderer.Tint = Color.White;

	}

}
