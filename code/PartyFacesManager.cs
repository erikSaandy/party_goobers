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
	public static IEnumerable<Player> PlayersAlive => Players.Where(x => !x.IsDead);
	public static TimeSince TimeSinceRoundStart { get; private set; } = new TimeSince();

	public List<DeathInstance> RoundDeaths { get; private set; } = new();

	public Action OnRoundEnter { get; set; }
	public Action OnRoundExit { get; set; }

	public LevelDataComponent CurrentLevelData { get; set; } = null;

	protected override void OnStart()
	{
		base.OnStart();

		if(IsProxy) { return; }

		Test();

		LevelTimer.OnTimerDepleted += OnTimerDepleted;
		LevelTimer.OnTimerStarted += OnTimerStarted;

	}

	/// <summary>
	/// All this stuff can't happen first frame of the game, and it won't, but for testing purposes...
	/// </summary>
	public async void Test()
	{
		FadeScreen.Show();
		await Task.Delay( 200 );
		LevelHandler.Instance.LoadRandomLevel();
		NPCBuffer.Instance.PlaceNPCs();


		await Task.Delay( 500 );

		FadeScreen.Hide();
		LevelTimer.Start();

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

	[Broadcast]
	public void OnPlayerDeath( Guid player, string source )
	{
		RoundDeaths.Add( new DeathInstance( player, source ) );
	}


	private void OnTimerStarted()
	{
		Log.Warning( "TIMER STARTED" );
	}

	private void OnTimerDepleted()
	{
		Log.Warning("TIMER ENDED");
	}

	[Broadcast]
	public void EnterRound()
	{
		OnRoundEnter?.Invoke();

		RoundDeaths?.Clear();

	}

	[Broadcast]
	public void ExitRound()
	{
		OnRoundExit?.Invoke();
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
