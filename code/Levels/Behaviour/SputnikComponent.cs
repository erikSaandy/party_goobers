using System;

namespace Sandbox;

public sealed class SputnikComponent : Component, IInteractable
{
	const int HIT_SCORE = 500;

	[RequireComponent][Property] private ModelRenderer Renderer { get; set; }
	[RequireComponent][Property] private Rigidbody Body { get; set; }

	public static bool IsHit { get; private set; }

	[Property] private GameObject ExplosionParticle { get; set; }

	public bool IsInteractableBy( Player player ) => !IsHit;

	public void OnInteract( Guid playerId, SceneTraceResult traceResult )
	{
		if(IsHit) { return; }

		PartyFacesManager.Instance.LabelHandler.SpawnLabel( $"+{HIT_SCORE}", MiniGame.Camera.PointToScreenNormal( WorldPosition ), Vector2.Up * 50, false, isPositive: true );
		Scene.Directory.FindByGuid( playerId ).Components.Get<Player>().AddScore( HIT_SCORE );

		OnHit();

	}

	[Rpc.Broadcast]
	private void OnHit()
	{
		if ( IsHit ) { return; }

		IsHit = true;

		Sound.Play( "sounds/gun_shot.sound" );
		Sound.Play( "sounds/explosion01.sound" );

		GameObject expl = ExplosionParticle.Clone();
		expl.Enabled = true;
		expl.WorldPosition = WorldPosition + Scene.Camera.WorldRotation.Backward * 12;

		if (IsProxy) { return; }

		Body.Velocity += Vector3.Down * 150;
		Body.AngularVelocity = new Vector3( -5, Game.Random.Float( -5f, 5f ), Game.Random.Float( -5f, 5f ) );

	}


	protected override void OnStart()
	{
		base.OnStart();

		if(IsProxy) { return; }

		if(IsHit) { GameObject.Destroy(); }

		PartyFacesManager.Instance.OnGameEnd -= OnGameEnd;
		PartyFacesManager.Instance.OnGameEnd += OnGameEnd;

		Body.Velocity = Vector3.Right * 150 + Vector3.Up * 50;
		Body.AngularVelocity = new Vector3( .1f, .1f, -.1f );

	}

	[Rpc.Broadcast]
	static void OnGameEnd()
	{
		// Rest IsHit when game ends.
		IsHit = false;
	}

}
