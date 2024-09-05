using Sandbox;
using System;

public sealed class LevelDataComponent : Component	
{

	// If nodepath is null, NPCs stand still. 
	// Else, NPCs follow NodePath

	// Multiple paths...
	// Pick random

	[Property] public CameraComponent CameraReference { get; set; }
	[Property] public MapInstance MapInstance { get; set; }

	[Property] public LevelObjectiveHandler ObjectiveHandler { get; set; }
	public LevelObjective Objective => ObjectiveHandler?.CurrentObjective;

	[Property] public NodePathComponent[] NodePaths { get; set; }

	[Group( "LookAt" )][Property] public bool OverrideLookAt { get; set; } = false;
	[Group( "LookAt" )][Property] public GameObject NpcLookAtOverride { get; set; }

	[Property] public int? MinSpawnCount { get; set; }

	public List<SpawnPoint> SpawnPoints { get; set; }

	[Property] public Action OnInitiated { get; set; }

	[Property] private Action<Guid> OnNPCSpawned { get; set; }

	public void NPCSpawned(Guid npcId)
	{
		OnNPCSpawned?.Invoke( npcId );
	}

	protected override void OnAwake()
	{
		base.OnAwake();

		if(MapInstance != null)
		{
			SpawnPoints = Components.GetAll<SpawnPoint>( FindMode.InDescendants ).ToList();
			Log.Info( "Map loaded: " + MapInstance.IsLoaded );
		}
		else
		{
			SpawnPoints = Scene.GetAllComponents<SpawnPoint>().ToList();
		}

	}

	protected override void OnStart()
	{
		base.OnStart();

		if(CameraReference == null)
		{
			Log.Warning( "Map has no camera reference." );
			return;
		}

		Scene.Camera.Transform.Position = CameraReference.Transform.Position;
		Scene.Camera.Transform.Rotation = CameraReference.Transform.Rotation;
		Scene.Camera.FieldOfView = CameraReference.FieldOfView;
		Scene.Camera.ZFar = CameraReference.ZFar;
		Scene.Camera.ZNear = CameraReference.ZNear;
		Scene.Camera.BackgroundColor = CameraReference.BackgroundColor;
		Scene.Camera.Orthographic = CameraReference.Orthographic;
		Scene.Camera.OrthographicHeight = CameraReference.OrthographicHeight;

		CameraReference.Enabled = false;

		//if(IsProxy) { return; }

		//CameraReference.GameObject.Destroy();

	}

	public bool GetRandomNodePath( out NodePathComponent nodePath, float cooldown = 0 )
	{
		nodePath = null;
		if( NodePaths?.Count() == 0) { return false; }

		IEnumerable<NodePathComponent> validPaths = NodePaths.Where( x => x.TimeSinceUsed >= ( cooldown - float.Epsilon) );
		
		if(validPaths.Count() == 0) { return false; }

		nodePath = validPaths.GetRandom();
		nodePath.TimeSinceUsed = 0;

		return true;
	}

	public void ClientClickedOnNPC( Guid player, NPC npc )
	{
		Objective.ClientClickedOnNPC( player, npc );
	}

	protected override void OnUpdate()
	{

	}

	public void OnEnterRound()
	{

	}

	public void OnExitRound()
	{

	}

}
