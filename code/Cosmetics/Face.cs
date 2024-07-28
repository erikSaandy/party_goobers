using Sandbox.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class Face : Component, Component.INetworkListener
{
	public const float SIZE = 100;

	[JsonInclude][Property] public Eyebrows Eyebrow { get; set; }
	[JsonInclude][Property] public Eyes Eyes { get; set; }
	[JsonInclude][Property] public Nose Nose { get; set; }
	[JsonInclude][Property] public Mouth Mouth { get; set; }

	public SpriteRenderer EyesRenderer { get; set; }

	public void Randomize()
	{
		Eyebrow.Randomize();
		Eyes.Randomize();
		Nose.Randomize();
		Mouth.Randomize();
	}

	protected override void OnStart()
	{

		base.OnStart();

		Connection owner = GameObject.Network.OwnerConnection;

		GameObject.Name = $"Face - {owner.DisplayName}";

		Log.Info( "spawned face for " + owner.DisplayName );

		Transform.LocalPosition = 0;

	}

	protected override void OnUpdate()
	{
		Nose.Renderer.FlipHorizontal = Nose.Renderer.GameObject.Transform.Position.x < Transform.Position.x;

		float sin = MathF.Sin( Time.Now * 5 );
		float a = sin * 20;
		GameObject.Transform.Rotation = Rotation.FromPitch( a );

		if (IsProxy) { return; }

		if(Input.Pressed("Jump"))
		{	
			Randomize();
		}
	}

}
