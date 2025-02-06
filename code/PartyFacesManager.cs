using Sandbox;
using Sandbox.UI;
using System;
using System.Numerics;
using System.Threading.Tasks;

public class PartyFacesManager : SingletonComponent<PartyFacesManager>
{

	public static IEnumerable<Player> Players => Instance.Scene.GetAllComponents<Player>();
	public static IEnumerable<Player> PlayersAlive => Players.Where( x => x.LifeState == Player.PlayerLifeState.Alive );
	public static IEnumerable<Player> PlayersSafe => Players.Where( x => x.LifeState == Player.PlayerLifeState.Safe );
	public static IEnumerable<Player> PlayersDead => Players.Where( x => x.LifeState == Player.PlayerLifeState.Dead );
	public static IEnumerable<Player> PlayersNotDead => Players.Where( x => x.LifeState != Player.PlayerLifeState.Dead );

	public static TimeSince TimeSinceRoundStart { get; private set; } = new TimeSince();

	public List<DeathInstance> RoundDeaths { get; private set; } = new();

	public Action OnGameStart { get; set; }
	public Action OnGameEnd { get; set; }
	public Action OnRoundEnter { get; set; }
	public Action OnRoundExit { get; set; }

	[Sync] public bool RoundIsOn { get; private set; } = false;
	[Sync] public bool GameIsOn { get; private set; }

	public LevelDataComponent CurrentLevelData { get; set; } = null;

	[Property] public ScreenLabelHandler LabelHandler { get; private set; }

	[Property] public GameObject ConfettiParticles { get; private set; }
	[Property] public GameObject StarParticles { get; private set; }

	[ResourceType("vmdl")]
	[Property] public string CrownModel { get; set; }

	[Sync] public int RoundNumber { get; private set; }

	public void ThrowConfettiClient()
	{
		Vector3 pos = Scene.Camera.Transform.Position + Scene.Camera.WorldRotation.Down * 150 + Scene.Camera.WorldRotation.Forward * 600;
		GameObject conf = ConfettiParticles.Clone( pos, Vector3.VectorAngle( Scene.Camera.WorldRotation.Up ) );
		conf.Enabled = true;
		Sound.Play( "sounds/confetti_throw.sound" );
	}

	public static void SpawnStarParticlesClient( Vector3 position )
	{
		GameObject stars = Instance.StarParticles.Clone();
		stars.Enabled = true;
		stars.Transform.Position = position;
		stars.WorldRotation = Rotation.FromRoll( Instance.Scene.Camera.WorldRotation.Pitch() - 15 );
	}

	protected override void OnStart()
	{
		base.OnStart();

		if(IsProxy) { return; }

		OpenLobby();
		//StartGame();

		LevelTimer.OnTimerDepleted += OnTimerDepleted;
		LevelTimer.OnTimerStarted += OnTimerStarted;

	}

	private async void OpenLobby()
	{

		await Task.Delay( 1000 );

		LevelHandler.Instance.LoadLobbyLevel();

		await Task.Delay( 1000 );

		FadeScreen.Hide();

	}

	[Rpc.Broadcast]
	public void KillAllPlayers()
	{
		IEnumerable<Player> players = Players;
		foreach(Player player in players)
		{
			player?.Kill( "Game has ended!" );
		}

	}


	public static void PlaySoundClient( string path )
	{
		Sound.Play( path );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if(IsProxy) { return; }

		if ( Input.Pressed( "Jump" ) )
		{
			//	//LevelTimer.Start( 10 );

			//if ( ScoreBoard.Visible ) { ScoreBoard.Hide(); }
			//else { ScoreBoard.Show(); }

			//NPCIconGenerator.Instance.RequestNPCHeadshot( Scene.Components.GetAll<NPC>().Shuffle().First().GameObject.Id );

			//if ( ObjectiveDisplay.Visible ) { ObjectiveDisplay.Hide(); }
			//else { ObjectiveDisplay.Show(); }

		}

	}

	[Rpc.Broadcast]
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

		ExitRound();

	}

	public async void StartGame()
	{
		if(IsProxy) { Log.Warning( "Only the host can start a game." ); return; }

		if (GameIsOn) { Log.Warning( "Can't start game as the game is already going on." ); return; }

		GameIsOn = true;

		ScoreBoard.Clear();
		RoundNumber = 0;

		OnGameStart?.Invoke();

		FadeScreen.Show();

		await Task.Delay( 1000 );

		LevelHandler.Instance.UnloadCurrentLevel();

		await Task.Delay( 1000 );

		EnterRound();
	}

	[Authority]
	private void EnterRound()
	{

		if ( IsProxy ) { return; }

		if ( RoundIsOn ) { Log.Warning( "Can't enter round as round is already going on." ); return; }

		RoundIsOn = true;
		RoundNumber++;

		Log.Info( "Entering round " + RoundNumber );

		RoundDeaths?.Clear();
		EnterRoundAsync();
	}

	private async void EnterRoundAsync()
	{

		FadeScreen.Show();

		await Task.Delay( 200 );

		OnRoundEnter?.Invoke();

		await LevelHandler.Instance.LoadRandomLevel();

		await Task.Delay( 600 );

		ObjectiveDisplay.Show();

		await Task.Delay( 2500 );

		ObjectiveDisplay.Hide();

		await Task.Delay( 250 );

		await Task.Delay( 500 );

		FadeScreen.Hide();
		LevelTimer.Start();

	}

	[Authority]
	public void ExitRound()
	{

		if ( IsProxy ) { return; }

		if ( !RoundIsOn ) { Log.Warning( "Can't exit round as no round is going on." ); return; }

		RoundIsOn = false;

		ExitRoundAsync();


	}

	private async void ExitRoundAsync()
	{
		LevelTimer.Stop();

		await Task.Delay( 2500 );

		OnRoundExit?.Invoke();

		FadeScreen.Show();

		await Task.Delay( 500 );

		RoundIsOn = false;

		LevelHandler.Instance.UnloadCurrentLevel();

		NPCBuffer.Instance.HideNPCs();

		await Task.Delay( 200 );

		if ( PlayersNotDead.Count() == 0 )
		{
			ExitGame();
			return;
		}

		ScoreBoard.Refresh();
		ScoreBoard.Show();

		await Task.Delay( 3500 );

		ScoreBoard.Hide();

		await Task.Delay( 200 );

		EnterRound();

	}

	private async void ExitGame()
	{

		await Task.Delay( 100 );

		GameIsOn = false;

		OnGameEnd?.Invoke();

		ScoreBoard.Refresh();
		ScoreBoard.Show();

		Player winner = ScoreBoard.GetLeadingPlayer();
		winner?.OnWonGame();


		await Task.Delay( 3500 );
	
		ScoreBoard.Hide();

		await Task.Delay( 200 );

		OpenLobby();

	}

	[Rpc.Broadcast]
	public static void EnableGameobject( Guid gameObjectId, bool enabled )
	{
		var gameObject = Game.ActiveScene.Directory.FindByGuid( gameObjectId );
		if(!gameObject.IsValid()) { return; }

		gameObject.Enabled = enabled;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		FadeScreen.Visible = true;
		RoundIsOn = false;

		LevelTimer.OnTimerDepleted = null;
		LevelTimer.OnTimerStarted = null;

		OnGameEnd?.Invoke();

		OnGameEnd = null;
		OnGameStart = null;
		OnRoundEnter = null;
		OnRoundExit = null;

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
