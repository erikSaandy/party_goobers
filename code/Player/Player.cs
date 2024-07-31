using Sandbox;
using System;

public class Player : Component
{
	[Sync][Property] public Guid NPCGuid { get; private set; }
	public NPC NPC => Scene.Directory.FindByGuid( NPCGuid ).Components.Get<NPC>();

	private IInteractable PreviousHit { get; set; } = null;

	public Player()
	{

	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( IsProxy ) { return; }

		NPCBuffer.Instance.Free( out Guid id );
		NPCGuid = id;

		NPC.GetPosessedBy( this );


	}

	protected override void OnDestroy()
	{
		base.OnDestroy();



	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy ) { return; }

		if ( TraceLook( out SceneTraceResult trace, out IInteractable hit ) )
		{
			if ( Input.Pressed( "Attack1" ) )
			{
				Log.Info( "Clicked on interactable!" );
				hit.OnInteract( this.Id );
			}
			else
			{
				Log.Info( "Hovering on interactable!" );
				hit.OnMouseHover( this.Id );
			}


			if(hit != PreviousHit)
			{
				Log.Info( "Mouse entered interactable!" );
				hit.OnMouseEnter( this.Id );
			}

		}
		
		if( PreviousHit != null && PreviousHit != hit)
		{
			Log.Info( "Mouse exited interactable!" );
			PreviousHit.OnMouseExit( this.Id );
		}

		PreviousHit = hit;

	}

	private bool TraceLook( out SceneTraceResult trace, out IInteractable interactable )
	{
		interactable = null;

		//Log.Info( CursorHud.CursorPosition );
		Ray ray = Scene.Camera.ScreenPixelToRay( Mouse.Position );
		trace = Scene.Trace.Ray( ray, 100000 )
			.WithoutTags("player")
			.Run();

		Gizmo.Draw.Color = Color.Red;
		Gizmo.Draw.Line( ray.Position, ray.Forward * 100000 );

		if ( trace.GameObject == null ) { return false; }

		interactable = trace.GameObject.Components.GetInAncestorsOrSelf<IInteractable>();

		if ( interactable == null ) { return false; }

		return true;

	}


}
