using Sandbox;
using Sandbox.UI;
using System;
using System.Threading.Tasks;

public class PartyFacesManager : SingletonComponent<PartyFacesManager>
{

	// Hide Screen with "loading-screen"
	// Load random map prefab
	// Get objective from list of objectives in loaded map prefab.
	// Show Screen with objective
	// Ask NPCBuffer to place a valid set of unique NPCs on available spawn points.
	// Hide objective screen (screen wipe?)
	// Set players time to 10 seconds
	// Wait for players to succeed or fail, or for time to run out
	// ... Hide screen with "loading screen"


	// Timer is always set to 10
	// Score is added based on how fast you beat the level
	// Score = (int)( 10f - timeLeft ) * 1000

	// Players can find debuffs to add to opponents game.

	// 2X speed next few rounds
	// 3X speed next few rounds
	// Obscuring cloud moving from face to face, hiding them.
	// Inverted mouse next round
	// Juggle can

	public static IEnumerable<Player> Players => Instance.Scene.GetAllComponents<Player>();
	public static IEnumerable<Player> PlayersAlive => Players.Where(x => x.LifeState == Player.PlayerLifeState.Alive );
	public static IEnumerable<Player> PlayersSafe => Players.Where( x => x.LifeState == Player.PlayerLifeState.Safe );

	public static TimeSince TimeSinceRoundStart { get; private set; } = new TimeSince();

	public List<DeathInstance> RoundDeaths { get; private set; } = new();

	public Action OnRoundEnter { get; set; }
	public Action OnRoundExit { get; set; }

	[Sync] public static bool RoundIsOn { get; private set; } = false;

	public LevelDataComponent CurrentLevelData { get; set; } = null;

	protected override void OnStart()
	{
		base.OnStart();

		if(IsProxy) { return; }

		//FadeScreen.Show();
		ExitRound();

		LevelTimer.OnTimerDepleted += OnTimerDepleted;
		LevelTimer.OnTimerStarted += OnTimerStarted;

	}

	[Broadcast]
	public void KillAllPlayers()
	{
		IEnumerable<Player> players = Players;
		foreach(Player player in players)
		{
			player?.Kill( "Game has ended!" );
		}

	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if(IsProxy) { return; }

		if ( Input.Pressed( "Jump" ) )
		{
			//	//LevelTimer.Start( 10 );

			//	if ( ScoreBoard.Visible ) { ScoreBoard.Hide(); }
			//	else { ScoreBoard.Show(); }

			//NPCIconGenerator.Instance.RequestNPCHeadshot( Scene.Components.GetAll<NPC>().Shuffle().First().GameObject.Id );

			//if ( ObjectiveDisplay.Visible ) { ObjectiveDisplay.Hide(); }
			//else { ObjectiveDisplay.Show(); }

		}

	}

	[Broadcast]
	public void OnPlayerDeath( Guid player, string source )
	{
		RoundDeaths.Add( new DeathInstance( player, source ) );
	}


	private void OnTimerStarted()
	{
		Log.Info( "> TIMER STARTED" );
	}

	private void OnTimerDepleted()
	{
		Log.Info( "> TIMER ENDED");
	}

	[Broadcast]
	public void EnterRound()
	{
		OnRoundEnter?.Invoke();

		RoundDeaths?.Clear();

		if ( IsProxy ) { return; }

		EnterRoundAsync();

	}

	public async void EnterRoundAsync()
	{

		await Task.Delay( 500 );

		FadeScreen.Hide();
		LevelTimer.Start();

		RoundIsOn = true;

	}

	[Broadcast]
	public void ExitRound()
	{
		OnRoundExit?.Invoke();

		if(IsProxy) { return; }

		ExitRoundAsync();


	}

	public async void ExitRoundAsync()
	{
		LevelTimer.Stop();

		await Task.Delay( 1000 );

		FadeScreen.Show();

		await Task.Delay( 600 );

		RoundIsOn = false;

		LevelHandler.Instance.UnloadCurrentLevel();

		ScoreBoard.UpdateScoreBoard();
		ScoreBoard.Show();

		NPCBuffer.Instance.HideNPCs();

		await Task.Delay( 500 );

		LevelHandler.Instance.LoadRandomLevel();

		await Task.Delay( 2000 );

		ScoreBoard.Hide();

		await Task.Delay( 400 );

		ObjectiveDisplay.Show();

		await Task.Delay( 2500 );

		ObjectiveDisplay.Hide();

		await Task.Delay( 250 );

		EnterRound();

	}

	[Broadcast]
	public static void EnableGameobject( Guid gameObjectId, bool enabled )
	{
		var gameObject = Game.ActiveScene.Directory.FindByGuid( gameObjectId );
		gameObject.Enabled = enabled;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		FadeScreen.Visible = true;

	}

	public struct DeathInstance
	{
		public string source;
		public Guid Player;

		public DeathInstance( Guid player, string source)
		{
			this.Player = player;
			this.source = source;
		}

	}



}
