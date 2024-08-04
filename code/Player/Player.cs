using Sandbox;
using System;

public class Player : Component
{
	public enum PlayerLifeState
	{
		/// <summary>
		/// Player is actively playing the game.
		/// </summary>
		Alive,

		/// <summary>
		/// Player is safe and waiting for next round.
		/// </summary>
		Safe,

		/// <summary>
		/// Player has lost the game.
		/// </summary>
		Dead

	}

	[Sync][Property] private Guid NPCId { get; set; } = default;
	public NPC NPC => NPCId == default ? null : Scene.Directory.FindByGuid( NPCId ).Components.Get<NPC>();

	private IInteractable PreviousHit { get; set; } = null;

	public int LifeCount { get; private set; } = 3;
	[Sync] public PlayerLifeState LifeState { get; set; }

	[Sync] public int Score { get; set; } = 1254;

	public Player()
	{

	}

	[Broadcast]
	public void SetNPC(Guid npcId)
	{
		if(IsProxy) { return; }

		// Player already owns other NPC
		if(NPC != null && NPC.Owner == this)
		{
			NPC.Randomize();
			NPC.ClearOwner();
		}


		this.NPCId = npcId;

	}

	protected override void OnStart()
	{
		base.OnStart();

		if(IsProxy) { return; }

		NPCBuffer.Instance.PossessFreeNPC( GameObject.Id );

		LifeState = PlayerLifeState.Safe;

	}

	[Broadcast]
	public void Kill( string source = "" )
	{
		if( LifeState == PlayerLifeState.Dead ) { return; }

		LifeState = PlayerLifeState.Dead;
		LifeCount = 0;

		if( IsProxy ) { return; }

		PartyFacesManager.Instance.OnPlayerDeath( Id, source );

	}

	protected override void OnDestroy()
	{
		Kill( "Disconnected from server" );

		base.OnDestroy();

	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy ) { return; }

		if ( TraceLook( out IInteractable hit ) )
		{
			if ( hit != PreviousHit )
			{
				//Log.Info( "Mouse entered interactable!" );
				hit.OnMouseEnter( GameObject.Id );
			}

			if ( Input.Pressed( "Attack1" ) )
			{
				//Log.Info( "Clicked on interactable!" );
				hit.OnInteract( GameObject.Id );
			}
			else
			{
				//Log.Info( "Hovering on interactable!" );
				hit.OnMouseHover( GameObject.Id );
			}


		}
		
		if( PreviousHit != null && PreviousHit != hit)
		{
			//Log.Info( "Mouse exited interactable!" );
			PreviousHit.OnMouseExit( GameObject.Id );
		}

		if(Input.Pressed("Jump"))
		{
			//LevelTimer.Start( 10 );
			if ( ScoreBoard.Visible ) { ScoreBoard.Hide(); }
			else { ScoreBoard.Show(); }
		}

		PreviousHit = hit;

	}

	private bool TraceLook( out IInteractable interactable )
	{
		interactable = null;

		//Log.Info( FadeScreen.Visible + " :: " + LevelTimer.IsRunning );
		if ( FadeScreen.Visible && LifeState == PlayerLifeState.Alive ) { return false; }

		//Log.Info( CursorHud.CursorPosition );
		Ray ray = Scene.Camera.ScreenPixelToRay( Mouse.Position );

		SceneTraceResult trace = Scene.Trace.Ray( ray, 100000 )
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
