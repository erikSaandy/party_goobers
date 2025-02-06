using System;
using System.Security.Claims;

namespace Sandbox;

public sealed class TelevisionComponent : Component, IInteractable
{
	const int HIT_SCORE = 50;
	const int AD_COUNT = 4;

	enum Scroll
	{
		Off,
		Sputnik
	}

	enum Screen
	{
		Off,
		Broken
	}
	
	[Property] private SkinnedModelRenderer Renderer { get; set; }

	TimeSince TimeSinceScreenUpdate { get; set; } = 0;
	float displayTime = 2f;
	[Sync][Property] int ScreenDisplay { get; set; }

	[Property] bool IsBroken { get; set; } = false;

	[Property] private GameObject ExplosionParticle { get; set; }

	public bool IsInteractableBy( Player player ) => !IsBroken;

	[Rpc.Broadcast]
	public void OnInteract( Guid playerId, SceneTraceResult traceResult )
	{
		if(IsProxy) { return; }
		if(IsBroken) { return; }

		IsBroken = true;

		OnHit(playerId);
	}

	protected override void OnStart()
	{

		if ( SputnikComponent.IsHit )
		{
			Renderer.SetBodyGroup( "scroll", (int)Scroll.Sputnik );
		}

		if ( IsProxy ) { return; }

		ScreenDisplay = Game.Random.Int( 0, AD_COUNT );
		displayTime = Game.Random.Float( displayTime * 0.5f, displayTime * 1.5f );
		TimeSinceScreenUpdate = Game.Random.Float( 0f, displayTime );
	}

	[Rpc.Broadcast]
	private void OnHit( Guid playerId )
	{

		if (!IsProxy)
		{
			PartyFacesManager.Instance.LabelHandler.SpawnLabel( $"+{HIT_SCORE}", MiniGame.Camera.PointToScreenNormal( Transform.Position ), Vector2.Up * 50, false, isPositive: true );
			Scene.Directory.FindByGuid( playerId ).Components.Get<Player>().AddScore( HIT_SCORE );
		}

		UpdateScreenDisplay( (int)Screen.Broken );

		Renderer.SetBodyGroup( "scroll", (int)Scroll.Off );

		Sound.Play( "sounds/gun_shot.sound" );

		ExplosionParticle.Enabled = true;

	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( IsBroken ) { return; }

		if (!IsProxy) 
		{

			if ( TimeSinceScreenUpdate > displayTime ) {
				TimeSinceScreenUpdate = 0;
				ScreenDisplay = (ScreenDisplay % AD_COUNT) + 1;
				UpdateScreenDisplay( ScreenDisplay + 1 );
			}

		}

	}

	[Rpc.Broadcast]
	void UpdateScreenDisplay(int id)
	{

		Renderer.SetBodyGroup( "screen", id );
	}

}


