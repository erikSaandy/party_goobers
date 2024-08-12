using System;

public class NPCIconGenerator : SingletonComponent<NPCIconGenerator>
{
	private static Color BG_COLOR = new Color( 0, 0, 0, 0 );

	public static Texture RenderTexture { get; private set; } = null;
	[Property] private CameraComponent Camera { get; set; }

	[Property] public NPC DisplayNPC { get; private set; }	

	protected override void OnAwake()
	{
		base.OnAwake();

		RenderTexture = null;
		RenderTexture = Texture.CreateRenderTarget()
			.WithSize( 256, 256 )
			.WithFormat( ImageFormat.RGBA32323232F )
			.Create( "icon_texture" );

	}

	protected override void OnStart()
	{
		base.OnStart();
		DisplayNPC.GameObject.Enabled = true;
		//PartyFacesManager.EnableGameobject( DisplayNPC.GameObject.Id, true );
	}

	protected override void OnUpdate()
	{
	}

	[Broadcast]
	public void RequestNPCHeadshot( Guid npcGuid )
	{
		RequestNPCHeadshotAsync( npcGuid );
	}

	private async void RequestNPCHeadshotAsync(Guid npcGuid)
	{
		await Task.Delay( 100 );

		await Task.RunInThreadAsync( () =>
		{

			NPC npc = Scene.Directory.FindByGuid( npcGuid ).Components.Get<NPC>( true );

			DisplayNPC.CopyFrom( npc.GameObject.Id );
			DisplayNPC.GameObject.Enabled = true;

			Camera.BackgroundColor = BG_COLOR;
			Camera.OrthographicHeight = 35;
			Camera.ZFar = 128;
			Camera.ZNear = 32;
			Camera.Transform.Rotation = Vector3.VectorAngle( DisplayNPC.ForwardReference.Value.Rotation.Backward );
			Camera.Transform.Position = DisplayNPC.Face.Transform.Position - DisplayNPC.ForwardReference.Value.Rotation.Backward * 64;

		} );

		await Task.Delay( 250 );

		Camera.RenderToTexture( RenderTexture );

	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		RenderTexture.Dispose();
	}

}
