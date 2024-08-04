using Sandbox;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

public class NPCIconGenerator : SingletonComponent<NPCIconGenerator>
{
	public static Texture RenderTexture { get; private set; } = null;
	[Property] private CameraComponent Camera { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		RenderTexture = null;
		RenderTexture = Texture.CreateRenderTarget()
			.WithSize( 512, 512 )
			.WithFormat( ImageFormat.RGBA32323232F )
			.Create( "icon_texture" );

	}

	protected override void OnUpdate()
	{

		if(Input.Pressed("Jump"))
		{
			RequestNPCHeadshot( Scene.Components.GetAll<NPC>().Shuffle().First().GameObject.Id );

		}
	}

	public void RequestNPCHeadshot(Guid npcGuid)
	{
		NPC npc = Scene.Directory.FindByGuid( npcGuid ).Components.Get<NPC>();

		//npc.Tags.Add( "npcrt" );
		//Camera.RenderTags.Add( "npcrt" );

		Camera.Orthographic = true;
		Camera.OrthographicHeight = 64;
		Camera.ZFar = 128;
		Camera.ZNear = 32;
		Camera.Transform.Rotation = Vector3.VectorAngle( npc.ForwardReference.Value.Rotation.Backward );
		Camera.Transform.Position = npc.Face.Transform.Position - npc.ForwardReference.Value.Rotation.Backward * 64;

		Camera.RenderToTexture( RenderTexture );

		//npc.Tags.Remove( "npcrt" );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		RenderTexture.Dispose();
	}

}
