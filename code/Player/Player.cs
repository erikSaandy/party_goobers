using Sandbox;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;

public class Player : Component
{
	public const int MAX_HEALTH = 3;

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

	public Action OnLifeTaken { get; set; } 

	[Sync][Property] public int Lives { get; private set; } = MAX_HEALTH;

	[Broadcast]
	public void TakeLife()
	{
		if(IsProxy) { return; }

		if(LifeState == PlayerLifeState.Dead ) { return; }

		Lives--;
		OnLifeTaken?.Invoke();
		PartyFacesManager.Instance.LabelHandler.SpawnLabel( "❤️", Mouse.Position / Screen.Size, Vector2.Up * 250 + Vector2.Left * 50, true );

		if (Lives <= 0)
		{
			Lives = 0;
			Kill( "Ran out of lives!" );
		}

	}

	[Sync][Property] public PlayerLifeState LifeState { get; private set; } = PlayerLifeState.Safe;

	[Sync] public int Score { get; set; } = 0;

	[Authority]
	public void AddScore(int score) {
		this.Score += score;

		if(IsProxy) { return; }

		Sandbox.Services.Stats.SetValue( "score", this.Score );

	}

	[Broadcast]
	public void OnWonGame()
	{
		Log.Info( $"Winner is {Network.OwnerConnection.DisplayName}!" );

		if(IsProxy) { return; }

		Sandbox.Services.Stats.Increment( "gameswon", 1);

	}

	public Player() { }

	protected override void OnStart()
	{
		base.OnStart();

		PartyFacesManager.Instance.OnRoundEnter += OnRoundEnter;
		PartyFacesManager.Instance.OnRoundExit += OnRoundExit;

		PartyFacesManager.Instance.OnGameStart += OnGameStart;
		PartyFacesManager.Instance.OnGameEnd += OnGameEnd;

		LevelTimer.OnTimerDepleted += OnTimerDepleted;

		if (IsProxy) { return; }

		//NPCBuffer.Instance.PossessFreeNPC( GameObject.Id );


		if( PartyFacesManager.Instance.RoundIsOn )
		{
			ClientInfo.Show( "Hold on, \nThe round has already started!", new Func<bool>( () => !PartyFacesManager.Instance.RoundIsOn ) );
		}

	}

	[Broadcast]
	void OnTimerDepleted()
	{
		if(IsProxy) { return; }

		if(LifeState == PlayerLifeState.Alive)
		{
			TakeLife();
		}
	}

	[Broadcast]
	void OnGameStart()
	{
		if(IsProxy) { return; }

		Score = 0;
		Lives = MAX_HEALTH;
		LifeState = PlayerLifeState.Safe;

		Sandbox.Services.Stats.Increment( "gamesplayed", 1 );

	}

	[Broadcast]
	void OnGameEnd()
	{
		if(IsProxy) { return; }

		LifeState = PlayerLifeState.Safe;

		Sandbox.Services.Stats.SetValue( "gamelevelcount", PartyFacesManager.Instance.RoundNumber );

	}

	[Broadcast]
	void OnRoundExit()
	{
		if ( IsProxy ) { return; }

	}

	[Broadcast]
	void OnRoundEnter()
	{
		if(IsProxy) { return; }

		if(LifeState == PlayerLifeState.Safe )
		{
			Log.Info( Network.OwnerConnection.DisplayName + " entered round!" );
			LifeState = PlayerLifeState.Alive;
		}

	}

	[Broadcast]
	public void Kill( string source = "" )
	{
		if( LifeState == PlayerLifeState.Dead ) { return; }

		LifeState = PlayerLifeState.Dead;
		Lives = 0;

		if( IsProxy ) { return; }

		PartyFacesManager.Instance.OnPlayerDeath( Id, source );

	}

	[Broadcast]
	public void MarkAsSafe()
	{
		LifeState = PlayerLifeState.Safe;
	}

	protected override void OnDestroy()
	{
		Kill( "Disconnected from server" );

		base.OnDestroy();

		PartyFacesManager.Instance.OnRoundEnter -= OnRoundEnter;
		PartyFacesManager.Instance.OnRoundExit -= OnRoundExit;

		PartyFacesManager.Instance.OnGameStart -= OnGameStart;
		PartyFacesManager.Instance.OnGameEnd -= OnGameEnd;

		LevelTimer.OnTimerDepleted -= OnTimerDepleted;

	}

	protected override void OnUpdate()
	{

		base.OnUpdate();

		if ( IsProxy ) { return; }

		if ( TraceLook( out IInteractable hit, out SceneTraceResult trace ) )
		{

			if ( hit != PreviousHit )
			{
				//Log.Info( "Mouse entered interactable!" );
				hit.OnMouseEnter( GameObject.Id );
			}

			if ( Input.Pressed( "Attack1" ) )
			{
				hit.OnInteract( GameObject.Id, trace );
			}
			else
			{
				//Log.Info( $"Hovering over {hit}!" );
				hit.OnMouseHover( GameObject.Id );
			}

		}
		
		if( PreviousHit != null && PreviousHit != hit)
		{
			//Log.Info( "Mouse exited interactable!" );
			PreviousHit.OnMouseExit( GameObject.Id );
		}

		PreviousHit = hit;

	}

	private bool TraceLook( out IInteractable interactable, out SceneTraceResult trace )
	{
		interactable = null;
		trace = default;
		Ray ray = default;

		if( FadeScreen.Visible ) { return false; }

		// Minigame trace first

		if ( MiniGame.Camera != null)
		{
			ray = MiniGame.Camera.ScreenPixelToRay( Mouse.Position );

			trace = Scene.Trace.Ray( ray, 2000 )
			.WithoutTags( "player" )
			.UseHitboxes()
			.Run();

			if ( trace.GameObject != null )
			{

				interactable = trace.GameObject.Components.GetInAncestorsOrSelf<IInteractable>();

				if ( interactable != null ) { return true; }
			}
		}

		if ( LifeState != PlayerLifeState.Alive || !LevelTimer.IsRunning ) { return false; }

		// Check game scene

		ray = Scene.Camera.ScreenPixelToRay( Mouse.Position );

		trace = Scene.Trace.Ray( ray, 100000 )
			.WithoutTags( "player" )
			.UseHitboxes()
			.Run();

		Gizmo.Draw.Color = Color.Red;
		Gizmo.Draw.Line( ray.Position, ray.Forward * 100000 );

		if ( trace.GameObject == null ) { return false; }
		interactable = trace.GameObject.Components.GetInAncestorsOrSelf<IInteractable>();
		if ( interactable == null ) { return false; }

		return true;

	}

}
