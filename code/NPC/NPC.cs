﻿using Saandy;
using Sandbox;
using Sandbox.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

public class NPC : Component, IInteractable
{
	public const string FILE_PATH = "Data/";
	const string FILE_NAME = "Me.color";

	public enum AnimationBehaviour
	{
		Default,
		Wave,
		Cheer,
		Dance
	}

	const float MIN_WALKSPEED = 40f;
	const float MAX_WALKSPEED = 70f;
	[Property] float WalkSpeed { get; set; } = 50f;
	[Sync][Property] bool UseGravity { get; set; } = true;
	[Sync][Property] public Guid ConnectionId { get; set; } = default;

	[Rpc.Broadcast]
	public void SetOwner( Guid connectionId )
	{
		if(IsProxy) { return; }

		this.ConnectionId = connectionId;
	}

	[Rpc.Broadcast]
	public void ClearOwner()
	{
		if ( IsProxy ) { return; }

		this.ConnectionId = default;
	}

	[Rpc.Owner]
	public void CopyFrom(Guid other)
	{
		NPC npc = Scene.Directory.FindByGuid( other ).Components.Get<NPC>(true);

		SetColor( npc.Color.RgbaInt );
		Face.Eyebrows.SetTextureID( npc.Face.Eyebrows.TextureId );
		Face.Eyes.SetTextureID( npc.Face.Eyes.TextureId );
		Face.Nose.SetTextureID( npc.Face.Nose.TextureId );
		Face.Mouth.SetTextureID( npc.Face.Mouth.TextureId );
	}

	public void ClientCopyFrom( Guid other )
	{
		NPC npc = Scene.Directory.FindByGuid( other ).Components.Get<NPC>( true );

		ClientSetColor( npc.Color.RgbaInt );
		Face.Eyebrows.Renderer.Texture = npc.Face.Eyebrows.Texture;
		Face.Eyes.Renderer.Texture = npc.Face.Eyes.Texture;
		Face.Nose.Renderer.Texture = npc.Face.Nose.Texture;
		Face.Mouth.Renderer.Texture = npc.Face.Mouth.Texture;

	}

	public bool IsAlike( Guid other )
	{
		NPC npc = Scene.Directory.FindByGuid( other ).Components.Get<NPC>( true );
		return IsAlike( npc );
	}
	public bool IsAlike(NPC other)
	{

		if ( other.Color == Color
			&& other.Face.Eyebrows.Texture == Face.Eyebrows.Texture
			&& other.Face.Eyes.Texture == Face.Eyes.Texture
			&& other.Face.Nose.Texture == Face.Nose.Texture
			&& other.Face.Mouth.Texture == Face.Mouth.Texture
			)
		{
			return true;
		}

		return false;
	}

	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public ModelPhysics Physics { get; set; }

	[Property] public CapsuleCollider Collider { get; set; }

	[Property] public CharacterController Controller { get; set; }
	[Property] public Vector3? WantedPosition { get; private set; }
	public float DistanceToWantedPosition => (WantedPosition.Value -WorldPosition).Length;

	[Property] public Face Face { get; set; }

	public Color Color => !Renderer.IsValid() ? Color.White : Renderer.Tint;

	public GameObject LookAtObject => Scene.Directory.FindByGuid( LookAtObjectId );
	[Property][Sync] public Guid LookAtObjectId { get; private set; }

	[Rpc.Broadcast]
	public void LookAt(Guid obj)
	{
		LookAtObjectId = obj;
	}

	public Transform? ForwardReference => Renderer.GetAttachment( "forward_reference" );

	public bool IsInteractableBy( Player player ) => true;

	public void OnMouseEnter( Guid playerId )
	{
		Renderer.Set( "b_big_head", true );
		Sound.Play( "sounds/npc_hover.sound" );
	}

	public void OnMouseExit( Guid playerId )
	{
		Renderer.Set( "b_big_head", false );
		List<int> i = new();
	}

	public void OnInteract( Guid playerId , SceneTraceResult traceResult )
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

		WalkSpeed = Game.Random.Float( MIN_WALKSPEED, MAX_WALKSPEED );
		float sp = Math2d.InverseLerp( 0.75f, 1.4f, WalkSpeed );

		// Math2d.Map(WalkSpeed, MIN_WALKSPEED, MAX_WALKSPEED, 0.75f, 1.4f )
		//Renderer.Set( "f_walk_speed", 0.9f );

	}

	[Rpc.Broadcast]
	public void LoadFromConnection(Guid connectionId)
	{
		if ( Connection.Local.Id != connectionId ) { return; }

		if (!Face.IsValid()) { return; }

		if(!Load())
		{
			Save();
		}
	}

	public void Save()
	{
		string fullPath = Path.Combine( FILE_PATH, FILE_NAME );

		Sandbox.FileSystem.Data.CreateDirectory( FILE_PATH );
		Sandbox.FileSystem.Data.WriteJson( fullPath, Color.RgbaInt );

		Face.Save();

		Log.Info( "Saved goober." );

	}

	public bool Load()
	{
		string fullPath = Path.Combine( FILE_PATH, FILE_NAME );

		if ( !Sandbox.FileSystem.Data.FileExists( fullPath ) ) { return false; }

		uint data = Sandbox.FileSystem.Data.ReadJson<uint>( fullPath );

		SetColor( data );

		if(!Face.Load())
		{
			Face.Save();
		}

		Log.Info( "Loaded goober." );

		return true;
	}

	/// <summary>
	/// Make sure that client has NPC data.
	/// </summary>
	public static void AssureClientData()
	{
		IEnumerable<NPC> npcs = PartyFacesManager.Instance.Scene.Components.GetAll<NPC>( FindMode.EverythingInSelfAndChildren );
		if(npcs.Count() == 0) { Log.Error( "Can't assure client data because there's no dummy NPC in scene." ); return; }

		NPC npc = npcs.GetRandom();

		if (!npc.Load())
		{
			npc.Save();
		}
	}

	[Rpc.Broadcast]
	public void SetClientAnimationBehaviour( Guid player, AnimationBehaviour behaviour )
	{
		if ( Scene.Directory.FindByGuid( player ).IsProxy ) { return; }

		Renderer.Set( "b_crouching", false );
		Renderer.Set( "e_behaviour", (int)behaviour );


		if (behaviour == AnimationBehaviour.Cheer)
		{
			Sound.Play( "sounds/npc_cheer.sound" );
		}

	}

	[Rpc.Broadcast]
	public void SetAnimationBehaviour( AnimationBehaviour behaviour )
	{
		Renderer.Set( "b_crouching", false );
		Renderer.Set( "e_behaviour", (int)behaviour );

		if ( behaviour == AnimationBehaviour.Cheer )
		{
			Sound.Play( "sounds/npc_cheer.sound" );
		}
	}

	[Rpc.Broadcast]
	public void Crouch(bool crouch = true)
	{
		// Don't crouch if waving or cheering
		if(Renderer.GetInt("e_behaviour") != 0 ) { return; }

		Renderer.Set( "b_crouching", crouch );
	}

	[Rpc.Broadcast]
	public void Float( bool enable = true )
	{
		Renderer.Set( "b_falling", enable );
		UseGravity = !enable;	

		if (enable)
		{
			Renderer.Set( "b_crouching", false );
		}
		
	}

	[Rpc.Broadcast]
	public void Speak(string sound)
	{
		SoundHandle handle = Sound.Play( sound );
		handle.Position = WorldPosition + ( Vector3.Up * 50 );
	}

	[Rpc.Broadcast]
	public void Wave(float seconds = 2)
	{
		WaveAsync();

		async void WaveAsync()
		{

			Renderer.Set( "e_behaviour", (int)AnimationBehaviour.Wave );
			await Task.Delay( (int)(seconds * 1000) );

			if(Renderer.GetInt("e_behaviour") == (int)AnimationBehaviour.Wave )
			{
				Renderer.Set( "e_behaviour", (int)AnimationBehaviour.Default );
			}

		}
	}


	[Rpc.Broadcast]
	public void Jog( bool jog = true)
	{
		Renderer.Set( "f_walk_speed", jog ? 1.1f : 0.9f );
	}

	[Rpc.Owner]
	public void ToggleCrouch()
	{
		if(IsProxy) { return; }

		Crouch( !Renderer.GetBool( "b_crouching" ) );
	}

	public void OnRoundEnter()
	{
		if ( IsProxy ) { return; }

	}

	public void OnRoundExit()
	{
		if(IsProxy) { return; }
	}

	[Rpc.Broadcast]
	public void Randomize()
	{
		if(IsProxy) { return; }

		Face.Randomize();
		SetRandomColor();
	}

	[Rpc.Broadcast]
	public void Spawn( Transform spawnTransform )
	{
		Renderer.Set( "e_behaviour", (int)NPC.AnimationBehaviour.Default );
		Crouch( false );

		if ( IsProxy ) { return; }

		Teleport( spawnTransform.Position );
		WorldRotation = spawnTransform.Rotation;

		Float( false );
		Controller.Velocity = 0;
		UseGravity = true;

	}

	[Rpc.Owner]
	public void Teleport(Vector3 to)
	{
		WorldPosition = to;
		Transform.ClearInterpolation();
	}

	[Rpc.Owner]
	public void SetRandomColor()
	{
		SetColor( ColorX.MiiColors.GetRandom().RgbaInt );
	}

	[Rpc.Broadcast]
	public void SetColor( uint rgba )
	{
		Color col = Color.FromRgba( rgba );
		Face.SetColor( rgba );
		Renderer.Tint = col;
	}

	public void ClientSetColor( uint rgba )
	{
		Color col = Color.FromRgba( rgba );
		Face.ClientSetColor( rgba );
		Renderer.Tint = col;
	}

	protected override void OnUpdate()
	{

		if (LookAtObject != null) {

			Vector3 dir = 0;

			// If look object is orthographic camera, look parallel to camera
			if( LookAtObject == Scene?.Camera?.GameObject && Scene.Camera.Orthographic )
			{
				dir = Scene.Camera.WorldRotation.Backward;
			}
			else
			{
				Vector3 from = ForwardReference?.Position ?? 0;
				dir = (LookAtObject.WorldPosition - from);
			}

			Renderer.SetLookDirection( "aim_head", dir );

		}

		if(IsProxy) { return; }

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

		if( !Controller.IsValid() ) { return; }

		if (Controller.IsOnGround )
		{
			Controller.ApplyFriction( 3f );
		}

		if ( WantedPosition != null )
		{

			Vector3 wantedDir = (WantedPosition.Value - WorldPosition).Normal;
			//Gizmo.Draw.Color = Color.Red;
			//Gizmo.Draw.Line( Transform.Position, Transform.Position + wantedDir * WalkSpeed );
			float angle = WorldRotation.Yaw();
			WorldRotation = Rotation.Lerp( Rotation.FromYaw( angle ), Vector3.VectorAngle( wantedDir ).WithPitch( 0 ).WithRoll( 0 ), Time.Delta * 2 );
			Controller.Accelerate( WorldRotation.Forward * 60 );

			if ( !Controller.IsOnGround )
			{
				Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
			}

		}
		
		if ( !Controller.IsOnGround && UseGravity )
		{
			Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
		}


		Controller?.Move();
	}

	[Rpc.Broadcast]
	public void MoveTowards(Vector3 wantedPosition)
	{

		Renderer.Set( "b_walking", true );
		Renderer.Set( "b_crouching", false );

		if (IsProxy) { return; }

		this.WantedPosition = wantedPosition;

	}

	[Rpc.Broadcast]
	public void StopMoving()
	{

		Renderer.Set( "b_walking", false );

		if ( IsProxy ) { return; }

		this.WantedPosition = null;

	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		// Make sure faces textures are updated to match IDs.
		Face.UpdateRenderer();
		//Describe();

	}

	[Rpc.Broadcast]
	public void Describe()
	{
		Log.Warning( "- - - - - - - - - - -" );
		Log.Info( "I'm " + GameObject.Name );
		Log.Info( "and I'm " + Renderer.Tint.ToString() );
		Log.Info( "eyebrow id is " + Face.Eyebrows.TextureId );
		Log.Info( "eye id is " + Face.Eyes.TextureId );
		Log.Info( "nose id is " + Face.Nose.TextureId );
		Log.Info( "mouth id is " + Face.Mouth.TextureId );
		Log.Warning( "- - - - - - - - - - -" );
	}


	//[Authority]
	//public void AddCrown()
	//{
	//	// NPC already has crown.
	//	if( GameObject.Children.Find(x => x.Name == "Crown") != null ) { return; }

	//	SkinnedModelRenderer npc = Renderer;

	//	Model model = Sandbox.Model.Load( PartyFacesManager.Instance.CrownModel );

	//	if ( model is null || model.IsError ) { return; }

	//	var go = new GameObject( false, "Crown" );
	//	go.Parent = npc.GameObject;
	//	go.Tags.Add( "clothing" );

	//	var r = go.Components.Create<SkinnedModelRenderer>();
	//	r.Model = model;
	//	r.BoneMergeTarget = npc;

	//	go.Enabled = true;
	//	go.NetworkSpawn();

	//}

}
