using Sandbox;
using System;

public sealed class TargetComponent : Component, IInteractable
{
	const int HIT_SCORE = 3000;
	const int GOLD_HIT_SCORE = 6000;
	const float GOLD_CHANCE = 0.2f;

	[Property] private Prop Prop { get; set; }

	private bool IsHit { get; set; } = false;

	[Sync] private bool IsGold { get; set; } = false;

	private Color Red { get; set; } = new Color( 0.93f, 0.07f, 0.07f );
	private Color Gold { get; set; } = new Color( 0.88f, 0.37f, 0.00f );

	public bool IsInteractableBy( Player player ) => !IsHit;

	public TimeSince TimeSinceSpawn { get; private set; } = 0;

	protected override void OnAwake()
	{
		base.OnAwake();

		PartyFacesManager.Instance.OnRoundExit += Despawn;

		Prop.Tint = Red;

		if ( IsProxy ) { return; }

		if ( Game.Random.Float( 0f, 1f ) < GOLD_CHANCE ) { MakeGold(); }

	}

	[Broadcast]
	private void MakeGold()
	{
		IsGold = true;
		Prop.Tint = Gold;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) { return; }

		if ( TimeSinceSpawn > 8f )
		{
			TimeSinceSpawn = 0;
			Despawn();
		}
	}

	[Broadcast]
	private void Despawn() { DespawnAsync(); }

	private async void DespawnAsync()
	{
		Components.Get<SkinnedModelRenderer>().Set( "b_despawn", true );

		if ( IsProxy ) { return; }

		await Task.Delay( 2500 );
		GameObject.Destroy();
	}

	public void OnInteract( Guid playerId, SceneTraceResult traceResult )
	{
		Hit( playerId );
	}

	[Authority]
	private void Hit( Guid playerId )
	{
		if ( IsHit ) { return; }

		Sound.Play( "sounds/gun_shot.sound" );
		Sound.Play( "sounds/target_shatter.sound" );
		IsHit = true;

		if ( IsProxy ) { return; }

		int score = IsGold ? GOLD_HIT_SCORE : HIT_SCORE;
		PartyFacesManager.Instance.LabelHandler.SpawnLabel( $"+{score}", MiniGame.Camera.PointToScreenNormal( Transform.Position ), Vector2.Up * 50, false, isPositive: true );
		Scene.Directory.FindByGuid( playerId ).Components.Get<Player>().AddScore( score );

		SpawnGibs();

		GameObject.Destroy();

	}


	[Broadcast]
	private void SpawnGibs()
	{
		List<Gib> gibs = Prop.CreateGibs();

		foreach ( Gib gib in gibs )
		{
			gib.Tint = Prop.Tint;

			Rigidbody rb = gib.Components.Get<Rigidbody>();
			Vector3 dir = (rb.PhysicsBody.MassCenterPoint().Transform.Position - Transform.Position).Normal;
			rb.Velocity = (dir * 50) + (Vector3.Up * 150);
			rb.AngularVelocity = Vector3.One * Game.Random.Float( -4, 4 );
		}
	}


	protected override void OnDestroy()
	{
		base.OnDestroy();

		PartyFacesManager.Instance.OnRoundExit -= Despawn;

	}

}
