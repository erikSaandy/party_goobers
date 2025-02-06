using Sandbox;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;

public class LevelHandler : SingletonComponent<LevelHandler>
{

	[Sync] private Guid CurrentLevelDataId { get; set; } = default;
	[Property] public LevelDataComponent CurrentLevelData => GetCurrentLevelData();
	private LevelDataComponent GetCurrentLevelData()
	{
		if( !CurrentLevelData.IsValid() ) { return null; }
		if( CurrentLevelDataId == default ) { return null; }

		Scene.Directory.FindByGuid( CurrentLevelDataId ).Components.TryGet<LevelDataComponent>(out LevelDataComponent level, FindMode.InSelf);
		return level;
	}

	[Sync] public bool LevelIsLoaded { get; private set; } = false;

	[ResourceType( "prefab" )]
	[Property] public string LobbyPrefab { get; set; }

	[ResourceType( "prefab" )]
	[Property] public string[] LevelPrefabs { get; set; }

	public async Task LoadRandomLevel()
	{
		if(IsProxy) { return; }

		UnloadCurrentLevel();

		string level = LevelPrefabs.GetRandom();

#if DEBUG
		for ( int i = 1; i <= 10; i++ )
		{
			if ( Input.Down( $"Slot{i}" ) )
			{
				level = LevelPrefabs[(int)MathF.Min( i - 1, LevelPrefabs.Count() )];
				break;
			}
		}
#endif

		LoadLevel( level );

		await NPCBuffer.Instance.PlaceNPCs();

		await Task.Delay( 500 );

		CurrentLevelData.OnInitiated?.Invoke();

		Instance.LevelIsLoaded = true;

	}

	public void LoadLobbyLevel()
	{
		if ( IsProxy ) { return; }

		UnloadCurrentLevel();

		LoadLevel( LobbyPrefab );

		Instance.LevelIsLoaded = true;

	}

	private void LoadLevel(string level)
	{
		GameObject levelObject = SceneUtility.GetPrefabScene( ResourceLibrary.Get<PrefabFile>( level ) ).Clone( Vector3.Zero );

		levelObject.BreakFromPrefab();
		levelObject.NetworkSpawn();
		CurrentLevelDataId = levelObject.Id;

	}

	[Rpc.Broadcast]
	public void UpdateSceneCamera(Vector3 position, Rotation rotation, float fov, float zFar, float zNear )
	{

	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

	}

	[Rpc.Broadcast]
	public void UnloadCurrentLevel()
	{

		if ( CurrentLevelData != null )
		{
			CurrentLevelData.GameObject.Destroy();
			CurrentLevelDataId = default;
			Instance.LevelIsLoaded = false;
		}

	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		LevelIsLoaded = false;
	}

	public bool FindSpawnLocation( out Transform transform )
	{
		transform = Transform.World;

		LevelDataComponent level = CurrentLevelData;
		if ( !level.IsValid() ) { return false; }

		if( level.SpawnPoints?.Count() == 0)
		{
			Log.Error( "Level does not have any defined spawn points!" );
			return false;
		}

		if ( level.SpawnPoints.Count > 0 )
		{
			SpawnPoint sp = Random.Shared.FromList( level.SpawnPoints );
			transform = sp.Transform.World;
			level.SpawnPoints.Remove( sp );
			sp.Destroy();
			return true;
		}

		// Failing that, spawn where we are
		return false;
	}

}
