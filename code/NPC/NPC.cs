using Saandy;
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


	const float MIN_WALKSPEED = 40f;
	const float MAX_WALKSPEED = 70f;
	[Property] float WalkSpeed { get; set; } = 50f;

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

	[Property] public CapsuleCollider Collider { get; set; }

	[Property] public CharacterController Controller { get; set; }
	[Property] public Vector3? WantedPosition { get; private set; }
	public float DistanceToWantedPosition => (WantedPosition.Value -Transform.Position).Length;

	[Property] public Face Face { get; set; }

	public Color Color { get; private set; }

	[Property] GameObject LookAt { get; set; }


	public Transform? ForwardReference => Renderer.GetAttachment( "forward_reference" );

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

		PartyFacesManager.Instance.OnRoundEnter += OnRoundEnter;
		PartyFacesManager.Instance.OnRoundExit += OnRoundExit;


		if (IsProxy) { return; }

		SetRandomColor();

		WalkSpeed = Game.Random.Float( MIN_WALKSPEED, MAX_WALKSPEED );
		float sp = Math2d.InverseLerp( 0.75f, 1.4f, WalkSpeed );

		Renderer.Set( "f_walk_speed", Math2d.Map(WalkSpeed, MIN_WALKSPEED, MAX_WALKSPEED, 0.75f, 1.4f ) );

	}

	public void OnRoundEnter()
	{
		if ( IsProxy ) { return; }

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

	[Button("ToggleVisible")]
	public void ToggleVisible()
	{
		if(GameObject.Enabled)
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
		GameObject.Enabled = false;
		//Renderer.Enabled = false;
		//Physics.Enabled = false;
		//Face.Hide();
	}

	[Broadcast]
	public void Show()
	{
		GameObject.Enabled = true;
		//Renderer.Enabled = true;
		//Physics.Enabled = true;
		//Face.Show();
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

		if(WantedPosition.HasValue)
		{
			Gizmo.Draw.Color = Color.White;
			Gizmo.Draw.Line( Transform.Position, WantedPosition.Value );
		}

	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if(IsProxy) { return; }

		if ( Controller.IsOnGround )
		{
			Controller.ApplyFriction( 3f );
		}

		if ( WantedPosition != null )
		{

			Vector3 wantedDir = (WantedPosition.Value - Transform.Position).Normal;
			Gizmo.Draw.Color = Color.Red;
			Gizmo.Draw.Line( Transform.Position, Transform.Position + wantedDir * WalkSpeed );
			float angle = Transform.Rotation.Yaw();
			Transform.Rotation = Rotation.Lerp( Rotation.FromYaw( angle ), Vector3.VectorAngle( wantedDir ).WithPitch( 0 ).WithRoll( 0 ), Time.Delta * 2 );
			Controller.Accelerate( Transform.Rotation.Forward * 60 );

			if ( !Controller.IsOnGround )
			{
				Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
			}

		}
		
		if ( !Controller.IsOnGround )
		{
			Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
		}


		Controller?.Move();
	}

	[Broadcast]
	public void MoveTowards(Vector3 wantedPosition)
	{

		Renderer.Set( "b_walking", true );

		if (IsProxy) { return; }

		this.WantedPosition = wantedPosition;

	}

	[Broadcast]
	public void StopMoving()
	{

		Renderer.Set( "b_walking", false );

		if ( IsProxy ) { return; }

		this.WantedPosition = null;
	}

	private void LookAtPosition( Vector3 pos )
	{
		Vector3 from = ForwardReference?.Position ?? 0;
		Vector3 dir = (pos - from);
		Renderer.SetLookDirection( "aim_head", dir );

	}

}
