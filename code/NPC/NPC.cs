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

	public enum AnimationBehaviour
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

	[Broadcast]
	public void CopyFrom(Guid other)
	{
		NPC npc = Scene.Directory.FindByGuid( other ).Components.Get < NPC >();

		SetColor( npc.Color );
		Face.Eyebrows.SetTextureID( npc.Face.Eyebrows.Data.ID );
		Face.Eyes.SetTextureID( npc.Face.Eyes.Data.ID );
		Face.Nose.SetTextureID( npc.Face.Nose.Data.ID );
		Face.Mouth.SetTextureID( npc.Face.Mouth.Data.ID );

	}

	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public ModelPhysics Physics { get; set; }

	[Property] public CapsuleCollider Collider { get; set; }

	[Property] public CharacterController Controller { get; set; }
	[Property] public Vector3? WantedPosition { get; private set; }
	public float DistanceToWantedPosition => (WantedPosition.Value -Transform.Position).Length;

	[Property] public Face Face { get; set; }

	public Color Color { get; private set; }

	public GameObject LookAtObject => Scene.Directory.FindByGuid( LookAtObjectId );
	[Property][Sync] public Guid LookAtObjectId { get; private set; }

	[Broadcast]
	public void LookAt(Guid obj)
	{
		LookAtObjectId = obj;
	}

	public Transform? ForwardReference => Renderer.GetAttachment( "forward_reference" );

	public bool IsInteractableBy( Player player ) => true;

	public void OnMouseEnter( Guid playerId )
	{
		Renderer.Set( "b_big_head", true );
	}

	public void OnMouseExit( Guid playerId )
	{
		Renderer.Set( "b_big_head", false );
		List<int> i = new();
	}

	public void OnInteract( Guid playerId )
	{
		LevelHandler.Instance.CurrentLevelData.ClientClickedOnNPC( playerId, this );

		Log.Info( "Interacted with " + GameObject.Name );

	}

	public NPC() { }

	protected override void OnStart()
	{
		base.OnStart();	

		PartyFacesManager.Instance.OnRoundEnter += OnRoundEnter;
		PartyFacesManager.Instance.OnRoundExit += OnRoundExit;

		if (IsProxy) { return; }

		SetRandomColor();

		WalkSpeed = Game.Random.Float( MIN_WALKSPEED, MAX_WALKSPEED );
		float sp = Math2d.InverseLerp( 0.75f, 1.4f, WalkSpeed );

		Renderer.Set( "f_walk_speed", Math2d.Map(WalkSpeed, MIN_WALKSPEED, MAX_WALKSPEED, 0.75f, 1.4f ) );

	}


	public void SetClientAnimationBehaviour( Guid player, AnimationBehaviour behaviour )
	{
		if ( Scene.Directory.FindByGuid( player ).IsProxy ) { return; }

		Renderer.Set( "e_behaviour", (int)behaviour );

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

	public void Spawn( Transform spawnTransform )
	{
		Renderer.Set( "e_behaviour", (int)NPC.AnimationBehaviour.Default );

		if ( IsProxy ) { return; }

		Transform.Position = spawnTransform.Position;
		Transform.Rotation = spawnTransform.Rotation;

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

		if (LookAtObject != null) {

			Vector3 from = ForwardReference?.Position ?? 0;
			Vector3 dir = ( LookAtObject.Transform.Position - from);
			Renderer.SetLookDirection( "aim_head", dir );

		}

		//if(WantedPosition.HasValue)
		//{
		//	Gizmo.Draw.Color = Color.White;
		//	Gizmo.Draw.Line( Transform.Position, WantedPosition.Value );
		//}

	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if(IsProxy) { return; }

		if(Controller == null ) { return; }

		if (Controller.IsOnGround )
		{
			Controller.ApplyFriction( 3f );
		}

		if ( WantedPosition != null )
		{

			Vector3 wantedDir = (WantedPosition.Value - Transform.Position).Normal;
			//Gizmo.Draw.Color = Color.Red;
			//Gizmo.Draw.Line( Transform.Position, Transform.Position + wantedDir * WalkSpeed );
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

}
