using Sandbox.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class Face : Component, Component.INetworkListener
{

	public const string FILE_NAME = "Me.face";

	[Property] public NPC Owner { get; set; } = null;

	[Property] public Eyebrows Eyebrows { get; set; }
	[Property] public Eyes Eyes { get; set; }
	[Property] public Nose Nose { get; set; }
	[Property] public Mouth Mouth { get; set; }

	private TimeSince TimeSinceBlink { get; set; } = 0;
	private float blinkTimer { get; set; } = 0;

	public void Randomize()
	{
		Eyebrows.Randomize();
		Eyes.Randomize();
		Nose.Randomize();
		Mouth.Randomize();

	}

	[Rpc.Broadcast]
	public void SetColor(uint rgba)
	{
		Color col = Color.FromRgba( rgba );
		col = col * 2;
		Eyebrows.SetColor( col );
		Eyes.SetColor( col );
		Nose.SetColor( col );
		Mouth.SetColor( col );
	}

	public void ClientSetColor( uint rgba )
	{
		Color col = Color.FromRgba( rgba );
		col = col * 2;
		Eyebrows.ClientSetColor( col );
		Eyes.ClientSetColor( col );
		Nose.ClientSetColor( col );
		Mouth.ClientSetColor( col );
	}

	[Rpc.Broadcast]
	public void Hide()
	{
		Eyebrows.Renderer.Enabled = false;
		Eyes.Renderer.Enabled = false;
		Nose.Renderer.Enabled = false;
		Mouth.Renderer.Enabled = false;
	}

	[Rpc.Broadcast]
	public void Show()
	{
		Eyebrows.Renderer.Enabled = true;
		Eyes.Renderer.Enabled = true;
		Nose.Renderer.Enabled = true;
		Mouth.Renderer.Enabled = true;
	}

	protected override void OnStart()
	{

		base.OnStart(); 

		GameObject.Name = "Face";
		GameObject.WorldScale = 9;

		if ( IsProxy ) { return; }


	}

	public void UpdateRenderer()
	{
		Eyebrows.UpdateRenderer();
		Eyes.UpdateRenderer();
		Nose.UpdateRenderer();
		Mouth.UpdateRenderer();		
	}

	public void Save()
	{

		string fullPath = Path.Combine( NPC.FILE_PATH, FILE_NAME );

		List<FaceFeature.FaceFeatureData> data = new List<FaceFeature.FaceFeatureData>()
		{
			Eyebrows.Data,
			Eyes.Data,
			Nose.Data,
			Mouth.Data,
		};

		Sandbox.FileSystem.Data.CreateDirectory( NPC.FILE_PATH );
		Sandbox.FileSystem.Data.WriteJson( fullPath, data );

	}

	public bool Load()
	{
		string fullPath = Path.Combine( NPC.FILE_PATH, FILE_NAME );


		if ( !Sandbox.FileSystem.Data.FileExists( fullPath ) ) { return false; }

		List<FaceFeature.FaceFeatureData> data = Sandbox.FileSystem.Data.ReadJson<List<FaceFeature.FaceFeatureData>>( fullPath );

		Eyebrows.SetTextureID( data[0].ID );
		Eyes.SetTextureID( data[1].ID );
		Nose.SetTextureID( data[2].ID );
		Mouth.SetTextureID( data[3].ID );

		return true;
	}

	protected override void OnUpdate()
	{
		if ( !Nose.Renderer.IsValid() )
		{
			Nose.Renderer.FlipHorizontal = Nose.WorldPosition.x < Transform.Parent.WorldPosition.x; //Nose.WorldRotation.Yaw() < 0;
		}

		if(!Owner.Enabled ) { GameObject.LocalPosition = 0; return; }

		float sin = MathF.Sin( Time.Now * 2 );
		float a = sin * 20;
		GameObject.WorldRotation = Rotation.FromYaw( a );

		Transform _ref = Owner.ForwardReference.Value;
		Gizmo.Draw.Color = Color.Green;
		Gizmo.Draw.Line( _ref.Position, _ref.Position + _ref.Forward * 24 );
		Vector3 scale = _ref.Scale;
		Transform.Position = _ref.Position + (_ref.Forward.Normal * 17f * scale);	
		Transform.Scale = 13 * scale;

		// Blinking
		if(TimeSinceBlink > blinkTimer)
		{
			blinkTimer = Game.Random.Float( 0.2f, 7f );
			TimeSinceBlink = 0;											
			Eyes.Blink();
		}

		//if ( IsProxy ) { return; }

	}

}
