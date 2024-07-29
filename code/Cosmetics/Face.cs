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
	public const float SIZE = 100;

	const string FILE_PATH = "Data/";
	const string FILE_NAME = "Me.face";

	[Property] public Eyebrows Eyebrows { get; set; }
	[Property] public Eyes Eyes { get; set; }
	[Property] public Nose Nose { get; set; }
	[Property] public Mouth Mouth { get; set; }

	public void Randomize()
	{
		Eyebrows.Randomize();
		Eyes.Randomize();
		Nose.Randomize();
		Mouth.Randomize();
		Save();
	}

	protected override void OnStart()
	{

		base.OnStart();

		Connection owner = GameObject.Network.OwnerConnection;

		GameObject.Name = $"Face - {owner.DisplayName}";

		Log.Info( "spawned face for " + owner.DisplayName );

		Transform.LocalPosition = 0;

		if(IsProxy) { return; }

		Load();

	}

	public void Save()
	{

		string fullPath = Path.Combine( FILE_PATH, FILE_NAME );

		List<FaceFeature.FaceFeatureData> data = new List<FaceFeature.FaceFeatureData>()
		{
			Eyebrows.Data,
			Eyes.Data,
			Nose.Data,
			Mouth.Data,
		};

		Sandbox.FileSystem.Data.CreateDirectory( FILE_PATH );
		Sandbox.FileSystem.Data.WriteJson( fullPath, data );

		Log.Info( "Saved face." );

	}

	public bool Load()
	{
		string fullPath = Path.Combine( FILE_PATH, FILE_NAME );


		if ( !Sandbox.FileSystem.Data.FileExists( fullPath ) ) { return false; }

		List<FaceFeature.FaceFeatureData> data = Sandbox.FileSystem.Data.ReadJson<List<FaceFeature.FaceFeatureData>>( fullPath );
		Eyebrows.SetTextureID( data[0].ID );
		Eyes.SetTextureID( data[1].ID );
		Nose.SetTextureID( data[2].ID );
		Mouth.SetTextureID( data[3].ID );

		Log.Info( "Loaded face." );

		return true;
	}

	protected override void OnUpdate()
	{
		if(Nose.Renderer != null)
		{
			Nose.Renderer.FlipHorizontal = Nose.Transform.Position.x < Transform.Position.x;
		}

		float sin = MathF.Sin( Time.Now * 5 );
		float a = sin * 20;
		GameObject.Transform.Rotation = Rotation.FromPitch( a );

		if ( IsProxy ) { return; }

		if(Input.Pressed("Jump"))
		{	
			Randomize();
		}
	}

}
