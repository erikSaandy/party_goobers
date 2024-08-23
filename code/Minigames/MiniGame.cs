using Saandy;
using Sandbox;
using System;
using System.Threading.Tasks;

public class MiniGame : SingletonComponent<MiniGame>
{

	public const int DEPTH = 60;

	public static CameraComponent Camera { get; private set; }

	bool AwaitingMinigame = false;
	TimeSince TimeSinceTimerStarted = 0;
	float MinigameTime = 0;

	[Property] public PrefabFile TargetObject { get; private set; }
	[Property] public PrefabFile BalloonObject { get; private set; }
	[Property] public PrefabFile CanObject { get; private set; }
	[Property] public DistractionComponent Distraction { get; private set; }

	[Property] public float Margin { get; private set; } = 0.5f;
	public static float Aspect => Screen.Width / Screen.Height;
	public static Vector2 ScreenSize => new Vector2( Camera.OrthographicHeight * Aspect, Camera.OrthographicHeight );
	public static Vector2 ScreenSizeWithMargin => ScreenSize - Instance.Margin;
	public static Vector3 Min => (Instance.Transform.Rotation.Forward * DEPTH) + (ScreenSizeWithMargin.x * 0.5f * Instance.Transform.Rotation.Left) + (ScreenSizeWithMargin.y * 0.5f * Instance.Transform.Rotation.Down);
	public static Vector3 Max => (Instance.Transform.Rotation.Forward * DEPTH) + (ScreenSizeWithMargin.x * 0.5f * Instance.Transform.Rotation.Right) + (ScreenSizeWithMargin.y * 0.5f * Instance.Transform.Rotation.Up);

	public static Vector3 TopLeft => Max + (Instance.Transform.Rotation.Left * ScreenSizeWithMargin.x);
	public static Vector3 TopRight => Max;
	public static Vector3 BottomLeft => Min;
	public static Vector3 BottomRight => Min + ( Instance.Transform.Rotation.Right * ScreenSizeWithMargin.x );

	private static int RoundNumber => PartyFacesManager.Instance.RoundNumber;
	public float InitChance( int round ) { return Math2d.Clamp01( (round / 4f) * 0.5f ); }

	private static List<MiniEvent> Events = new List<MiniEvent>()
	{
		new TargetPracticeEvent(),
		new BalloonEvent(),
		new DistractionEvent(),
		new CanEvent()
	};

	public static Vector3 GetRandomPositionOnScreen()
	{
		Vector3 hori = Vector3.Lerp( BottomLeft, BottomRight, Game.Random.Float( 0f, 1f ) );
		Vector3 vert = Vector3.Lerp( BottomLeft, TopLeft, Game.Random.Float( 0f, 1f ) );
		return hori + vert + Max + Instance.Transform.Position;
	}


	public static Vector3 GetRandomPositionBelowScreen()
	{
		Vector3 hori = Vector3.Lerp( BottomLeft, BottomRight, Game.Random.Float( 0f, 1f ) );
		Vector3 vert = Instance.Transform.Rotation.Down * 50;

		return hori + vert + Instance.Transform.Position;

	}

	public static Vector3 GetPositionBesideScreen( bool right = false )
	{

		return Instance.Transform.Position + (right ? (BottomRight.WithZ(0) + Vector3.Right * 50) : (BottomLeft.WithZ(0) + Vector3.Left * 50));
	}

	[Broadcast]
	public void SpawnClientPrefabs( Vector3 position, PrefabFile prefab )
	{
		GameObject obj = SceneUtility.GetPrefabScene( prefab ).Clone( GetRandomPositionBelowScreen() );
	}

	protected override void OnAwake()
	{
		base.OnAwake();

		Camera = Components.Get<CameraComponent>();

		LevelTimer.OnTimerStarted += OnTimerStarted;
		LevelTimer.OnTimerStopped += OnTimerStopped;
	}

	void OnTimerStarted()
	{
		if( Instance.IsProxy ) { return; }

		float chance = 11;// InitChance( RoundNumber );
		Log.Info( "chance for minigame: " + chance );
		if ( Game.Random.Float( 0f, 1f ) > chance) { return; }

		AwaitingMinigame = true;
		TimeSinceTimerStarted = 0;
		MinigameTime = 1;
		Log.Info( "awaiting minigame..." );

	}

	void OnTimerStopped()
	{
		if ( Instance.IsProxy ) { return; }

		AwaitingMinigame = false;

	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if(Instance.IsProxy) { return; }

		//Gizmo.Draw.Color = Color.Blue;
		//Gizmo.Draw.SolidSphere( TopLeft + Instance.Transform.Position, 8 );
		//Gizmo.Draw.SolidSphere( TopRight + Instance.Transform.Position, 8 );
		//Gizmo.Draw.SolidSphere( BottomLeft + Instance.Transform.Position, 8 );
		//Gizmo.Draw.SolidSphere( BottomRight + Instance.Transform.Position, 8 );

		if (!AwaitingMinigame) { return; }

		// It's Minigame time!
		if ( TimeSinceTimerStarted >= MinigameTime )
		{
			AwaitingMinigame = false;

			Events.GetRandomWeighted().Invoke();

		}

	}

	private abstract class MiniEvent : IWeighted
	{
		public virtual bool Disabled { get; set; }
		public virtual int Weight { get; }

		public MiniEvent() { }

		public abstract void Invoke();

	}

	private class TargetPracticeEvent : MiniEvent
	{
		public override int Weight => 500;

		public override async void Invoke()
		{
			int minCount = (int)(MiniGame.RoundNumber * 0.2f) + 1;
			int count = Game.Random.Int( minCount, minCount + 1 );

			for ( int i = 0; i < count; i++ )
			{
				GameObject target = SceneUtility.GetPrefabScene( MiniGame.Instance.TargetObject ).Clone( GetRandomPositionOnScreen() );
				target.Transform.Rotation = Rotation.FromRoll( 90 );
				target.NetworkSpawn();

				await MiniGame.Instance.Task.Delay( 300 );
			}
		}

	}

	private class DistractionEvent : MiniEvent
	{
		public override int Weight => 80;

		public override void Invoke()
		{
			MiniGame.Instance.Distraction.Distract();
		}

	}

	private class BalloonEvent : MiniEvent
	{
		public override int Weight => 300;

		public override async void Invoke()
		{
			int minCount = (int)(MiniGame.RoundNumber * 0.15f) + 1;
			int count = Game.Random.Int( minCount, minCount + 1 );

			for ( int i = 0; i < count; i++ )
			{
				GameObject balloon = SceneUtility.GetPrefabScene( MiniGame.Instance.BalloonObject ).Clone( GetRandomPositionBelowScreen() );
				balloon.NetworkSpawn();

				await MiniGame.Instance.Task.Delay( 600 );
			}

		}

	}

	private class CanEvent : MiniEvent
	{
		public override int Weight => 300;

		public override void Invoke()
		{
			bool right = Game.Random.Int( 0, 1 ) == 1;

			Vector3 pos = MiniGame.GetPositionBesideScreen( right );
			MiniGame.Instance.SpawnClientPrefabs( pos, MiniGame.Instance.CanObject );

		}

	}

}
