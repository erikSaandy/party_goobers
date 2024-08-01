using Sandbox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NPC : Component, IInteractable
{

	private enum AnimationBehaviour
	{
		Default,
		Wave,
		Cheer
	}

	[Sync][Property] private Guid PlayerId { get; set; } = default;
	public Player Owner => PlayerId == default ? null : Scene.Directory.FindByGuid(PlayerId).Components.Get<Player>();

	[Broadcast]
	public void SetOwner(Guid playerId)
	{
		if(IsProxy) { return; }

		this.PlayerId = playerId;
	}

	[Broadcast]
	public void ClearOwner()
	{
		if ( IsProxy ) { return; }

		this.PlayerId = default;
	}

	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public ModelPhysics Physics { get; set; }

	[Property] public CharacterController Controller { get; set; }
	[Property] public Vector3? WantedPosition { get; set; }
	[Property] public float DistanceToWantedPosition => (WantedPosition.Value -Transform.Position).Length;

	[Property] public Face Face { get; set; }

	public Color Color { get; private set; }

	[Property] GameObject LookAt { get; set; }

	private Transform? ForwardReference => Renderer.GetAttachment( "forward_reference" );

	public bool IsInteractableBy( Player player ) => true;

	public void OnMouseEnter( Guid playerId )
	{
		Renderer.Set( "b_big_head", true );
	}

	public void OnMouseExit( Guid playerId )
	{
		Renderer.Set( "b_big_head", false );
	}

	public void OnInteract( Guid playerId )
	{
		Log.Info( "Interacted with " + GameObject.Name );
		Renderer.Set( "e_behaviour", (int)AnimationBehaviour.Wave );
	}

	public NPC() { }

	protected override void OnStart()
	{
		base.OnStart();	

		LookAt = Scene.Camera.GameObject;

		Renderer.Set( "b_walking", true );

		PartyFacesManager.Instance.OnRoundEnter += OnRoundEnter;
		PartyFacesManager.Instance.OnRoundExit += OnRoundExit;


		if (IsProxy) { return; }

		SetRandomColor();

	}

	public void OnRoundEnter()
	{
		if ( IsProxy ) { return; }

		if( FindSpawnLocation(out Transform t))
		{
			Spawn( t.Position );
		}

	}

	public void OnRoundExit()
	{
		if(IsProxy) { return; }



	}

	[Broadcast]
	public void Randomize()
	{
		if(IsProxy) { return; }
		SetRandomColor();
	}

	[Broadcast]
	private void Spawn( Vector3 position )
	{
		GameObject.Transform.Position = position;
		Renderer.Enabled = true;
	}

	private bool FindSpawnLocation(out Transform transform)
	{

		// If we have any SpawnPoint components in the scene, then use those
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		if ( spawnPoints.Length > 0 )
		{
			SpawnPoint sp = Random.Shared.FromArray( spawnPoints );
			transform = sp.Transform.World;
			sp.Destroy();
			return true;
		}

		// Failing that, spawn where we are
		transform = Transform.World;
		return false;
	}

	[Button("ToggleVisible")]
	public void ToggleVisible()
	{
		if(Renderer.Enabled)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	[Broadcast]
	public void Hide()
	{
		Renderer.Enabled = false;
		Physics.Enabled = false;
		Face.Hide();
	}

	[Broadcast]
	public void Show()
	{
		Renderer.Enabled = true;
		Physics.Enabled = true;
		Face.Show();
	}

	public void SetRandomColor()
	{
		SetColor( ColorX.MiiColors.GetRandom() );
	}

	[Broadcast]
	public void SetColor( Color color )
	{
		Face.SetColor( color );
		Renderer.Tint = color;
	}

	protected override void OnUpdate()
	{
		if(LookAt != null) {
			LookAtPosition( Scene.Camera.Transform.Position );	
		}

		Vector3 fwd = ForwardReference?.Forward ?? 0;
		Vector3 scale =  ( ForwardReference?.Scale ?? 1 );
		Face.Transform.Position = (ForwardReference?.Position ?? 0) + (fwd.Normal * 14f * scale);
		Face.Transform.Scale = 11 * scale;

		Controller?.Move();

	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if(IsProxy) { return; }

		if ( Controller.IsOnGround )
		{
			Controller.ApplyFriction( 2f );
		}

		if ( WantedPosition != null && DistanceToWantedPosition > 32f )
		{

			Vector3 wantedDir = (WantedPosition.Value - Transform.Position).Normal;
			Gizmo.Draw.Color = Color.Red;
			Gizmo.Draw.Line( Transform.Position, Transform.Position + wantedDir * 60 );
			float angle = Transform.Rotation.Yaw();
			Transform.Rotation = Rotation.Lerp( Rotation.FromYaw( angle ), Vector3.VectorAngle( wantedDir ).WithPitch( 0 ).WithRoll( 0 ), Time.Delta * 2 );
			Controller.Accelerate( Transform.Rotation.Forward * 70 );

			if ( !Controller.IsOnGround )
			{
				Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
			}

		}
		
		if ( !Controller.IsOnGround )
		{
			Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
		}


		Controller.Move();
	}

	[Broadcast]
	public void SetWantedPosition(Vector3 wantedPosition)
	{
		this.WantedPosition = wantedPosition;
	}

	private void LookAtPosition( Vector3 pos )
	{
		Vector3 from = ForwardReference?.Position ?? 0;
		Vector3 dir = (pos - from);
		Renderer.SetLookDirection( "aim_head", dir );

	}

}
