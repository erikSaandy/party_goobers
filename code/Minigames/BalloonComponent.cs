using Sandbox;
using System;

public sealed class BalloonComponent : Component, IInteractable
{
	const int HIT_SCORE = 2500;

	public static Color[] Colors { get; set; } = new Color[6]
	{
		ColorX.FromHex("F60909"),ColorX.FromHex("096CF6"),ColorX.FromHex("FF00E6"),
		ColorX.FromHex("0CA523"),ColorX.FromHex("DDBB10"),ColorX.FromHex("DEECFD")
	};


	[RequireComponent][Property] private ModelRenderer Renderer { get; set; }
	[RequireComponent][Property] private Rigidbody Body { get; set; }

	[Property] private float Speed { get; set; } = 350;

	private bool IsHit { get; set; } = false;

	public bool IsInteractableBy( Player player ) => !IsHit;

	public TimeSince TimeSinceSpawn { get; private set; } = 0;

	protected override void OnAwake()
	{
		base.OnAwake();

		PartyFacesManager.Instance.OnRoundExit += Despawn;

		if ( IsProxy ) { return; }

		SetColor( Colors.GetRandom() );

		Speed = Game.Random.Int( 350, 650 );

	}

	protected override void OnStart()
	{
		base.OnStart();

	}

	[Broadcast]
	private void SetColor( Color color )
	{
		Renderer.Tint = color;
	}

	protected override void OnUpdate()
	{
		if(IsProxy) { return; }

		//if ( TimeSinceSpawn > 8f )
		//{
		//	TimeSinceSpawn = 0;
		//	Despawn();
		//}
			
		Body.Velocity = Vector3.Up * Speed;

		float angular = MathF.Sin( Time.Now );
		Body.AngularVelocity = new Vector3(0, angular, 0 );

	}

	[Authority]
	private async void Despawn()
	{
		await Task.Delay( 1500 );

		GameObject.Destroy();
	}

	public void OnInteract( Guid playerId, SceneTraceResult traceResult )
	{
		Hit( playerId );
	}

	[Broadcast]
	private void Hit( Guid playerId )
	{
		if ( IsHit ) { return; }
		
		Sound.Play( "sounds/gun_shot.sound" );
		
		if ( IsProxy ) { return; }


		IsHit = true;

		PartyFacesManager.Instance.LabelHandler.SpawnLabel( $"+{HIT_SCORE}", MiniGame.Camera.PointToScreenNormal( Transform.Position ), Vector2.Up * 50, false, isPositive: true );
		Scene.Directory.FindByGuid( playerId ).Components.Get<Player>().AddScore( HIT_SCORE );

		GameObject.Destroy();

	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		PartyFacesManager.Instance.OnRoundExit -= Despawn;

	}

}
