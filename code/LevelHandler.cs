using Sandbox;
using System;
using System.ComponentModel.DataAnnotations;

public class LevelHandler : SingletonComponent<LevelHandler>
{
	[Sync] private Guid CurrentLevelDataId { get; set; } = default;
	[Property] public LevelDataComponent CurrentLevelData => CurrentLevelDataId == default ? null : Scene.Directory.FindByGuid( CurrentLevelDataId ).Components.Get<LevelDataComponent>();

	[Sync] public bool LevelIsLoaded { get; private set; } = false;

	[ResourceType( "prefab" )]
	[Property] public string[] LevelPrefabs { get; set; }

	public void LoadRandomLevel()
	{
		if(IsProxy) { return; }

		UnloadCurrentLevel();

		GameObject levelObject = SceneUtility.GetPrefabScene( ResourceLibrary.Get<PrefabFile>( LevelPrefabs.GetRandom() ) ).Clone( Vector3.Zero );
		levelObject.BreakFromPrefab();
		levelObject.NetworkSpawn();
		LevelDataComponent levelData = levelObject.Components.Get<LevelDataComponent>();
		Scene.Camera.Transform.Position = levelData.CameraReference.Transform.Position;
		Scene.Camera.Transform.Rotation = levelData.CameraReference.Transform.Rotation;
		Scene.Camera.FieldOfView = levelData.CameraReference.FieldOfView;
		Scene.Camera.ZFar = levelData.CameraReference.ZFar;
		Scene.Camera.ZNear = levelData.CameraReference.ZNear;

		levelData.CameraReference.GameObject.Destroy();

		levelObject.BreakFromPrefab();
		//levelObject.NetworkSpawn();

		CurrentLevelDataId = levelObject.Id;

		NPCBuffer.Instance.PlaceNPCs();

		Instance.LevelIsLoaded = true;

	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

	}

	[Broadcast]
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
		if ( level == null ) { return false; }

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
