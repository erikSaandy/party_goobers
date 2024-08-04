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
	public LevelObjective Objective => ObjectiveHandler.CurrentObjective;

	[Property] public NodePathComponent[] NodePaths { get; set; }


	public List<SpawnPoint> SpawnPoints { get; set; }


	protected override void OnAwake()
	{
		base.OnAwake();

		SpawnPoints = MapInstance.Components.GetAll<SpawnPoint>( FindMode.InDescendants ).ToList();

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
