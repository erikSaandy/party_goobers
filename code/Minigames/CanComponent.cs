using Sandbox;
using System;
using System.Numerics;
using static Sandbox.Gizmo;

public sealed class CanComponent : Component, IInteractable
{
	const int HIT_SCORE = 500;

	[RequireComponent][Property] private ModelRenderer Renderer { get; set; }
	[RequireComponent][Property] private Rigidbody Body { get; set; }

	[Property] private Model CanCrumpled { get; set; }

	TimeSince TimeSinceHit = 0;
	int hitCount = 0;

	public bool IsInteractableBy( Player player ) => true;

	protected override void OnAwake()
	{
		base.OnAwake();

		PartyFacesManager.Instance.OnRoundExit += Despawn;

	}

	protected override void OnStart()
	{
		base.OnStart();

		int dirX = MiniGame.Camera.Transform.World.PointToLocal( Transform.Position ).y < 0 ? -1 : 1;

		Body.Velocity = new Vector3( dirX * 150, 0, 450 );
		Sound.Play( "sounds/can_throw.sound" );
	}


	float mass = 300;
	protected override void OnFixedUpdate()
	{

		if(mass < 700)
		{
			mass += Time.Delta * 10;
		}

		Body.Velocity -= Vector3.Up * mass * Time.Delta;

		if(Transform.Position.y < -1000)
		{
			Despawn();
		}

	}

	public void OnInteract( Guid playerId, SceneTraceResult traceResult )
	{

		if ( TimeSinceHit < 0.3f ) { return; }
		TimeSinceHit = 0;
		hitCount++;

		Sound.Play( "sounds/gun_shot.sound" );

		int score = HIT_SCORE * hitCount;
		PartyFacesManager.Instance.LabelHandler.SpawnLabel( $"+{score}", MiniGame.Camera.PointToScreenNormal( Transform.Position ), Vector2.Up * 50, false, isPositive: true );
		Scene.Directory.FindByGuid( playerId ).Components.Get<Player>().AddScore( score );

		//Vector3 off = MiniGame.Camera.Transform.World.PointToLocal(traceResult.HitPosition) - MiniGame.Camera.Transform.World.PointToLocal(Body.PhysicsBody.MassCenter);

		Vector3 off = MiniGame.Camera.PointToScreenNormal( traceResult.HitPosition ) - MiniGame.Camera.PointToScreenNormal( Body.PhysicsBody.MassCenter );

		off = (MiniGame.Camera.Transform.Rotation.Up * off.y) + (MiniGame.Camera.Transform.Rotation.Left * off.x) * 200;

		//Log.Info( off * 20 );
		Body.Velocity = (Vector3.Up * 400) + (off * 20);
		Body.AngularVelocity = new Vector3( off.x * 2, off.y, Game.Random.Float(-5f, 5f) );

		Renderer.Model = CanCrumpled;

	}


	private async void Despawn()
	{
		await Task.Delay( 1500 );

		GameObject.Destroy();
		PartyFacesManager.Instance.OnRoundExit -= Despawn;

	}


}
