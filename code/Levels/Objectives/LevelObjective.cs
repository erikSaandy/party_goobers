using Sandbox;
using System;
using System.Numerics;
using System.Threading.Tasks;

public abstract class LevelObjective : Component
{

	public List<NPC> SelectedNPCs { get; set; } = new();

	public virtual int MaxSelectedNPCs => 1;

	protected bool AcceptSelection { get; set; } = true;

	public LevelObjectiveHandler Handler => LevelHandler.Instance.CurrentLevelData.ObjectiveHandler;

	/// <summary>
	/// Does this NPC satisfy the objective?
	/// </summary>
	/// <param name="npc"></param>
	/// <returns></returns>
	protected abstract bool ObjectiveIsSatisfied();

	/// <summary>
	/// Client Clicked on a valid NPC and selected it.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="npc"></param>
	protected abstract void ClientSelectedNPC( Guid player, NPC npc );

	/// <summary>
	/// Client clicked on an NPC in the level.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="npc"></param>
	public virtual void ClientClickedOnNPC( Guid player, NPC npc )
	{
		CheckSuccess( player, npc );
	}

	private async void CheckSuccess( Guid player, NPC npc )
	{

		// Return if doesn't accept selection or player has selected all npcs to trigger objective
		if ( !AcceptSelection || SelectedNPCs.Count >= MaxSelectedNPCs ) { return; }


		SelectedNPCs.Add( npc );
		npc.SetClientAnimationBehaviour( player, NPC.AnimationBehaviour.Wave );

		ClientSelectedNPC( player, npc );

		Log.Info( "Selected NPC count: " + SelectedNPCs.Count );
		bool evaluate = SelectedNPCs.Count >= MaxSelectedNPCs;

		if (evaluate)
		{
			AcceptSelection = false;
			await Task.Delay( 1000 );

			if( ObjectiveIsSatisfied() )
			{
				// ON CORRECT SELECTION
				OnCompletedObjective( player );
				Handler.OnPlayerCompletedObjective( player );
			}
			else
			{
				// ON WRONG SELECTION
				ResetSelection( player );
			}

		}

	}

	private void ResetSelection( Guid player )
	{
		foreach(NPC npc in SelectedNPCs)
		{
			npc.SetClientAnimationBehaviour( player, NPC.AnimationBehaviour.Default );
		}

		SelectedNPCs.Clear();

		AcceptSelection = true;

	}

	/// <summary>
	/// Get a pool of NPCs for use with this objective.
	/// </summary>
	/// <param name="set"></param>
	/// <param name="level"></param>
	/// <returns></returns>
	public virtual IEnumerable<NPC> GetNPCPool( List<NPC> set, LevelDataComponent level )
	{
		if ( IsProxy ) { return null; }

		int spawnCount = LevelHandler.Instance.CurrentLevelData.SpawnPoints.Count();
		Log.Info( "found " + spawnCount + " spawners in level." );

		spawnCount = int.Min( spawnCount, set.Count );

		//TODO: Make sure player NPCs are always chosen.
		return set.Take( spawnCount );

	}


	/// <summary>
	/// Called when player completes the objective.
	/// </summary>
	/// <param name="player"></param>
	protected virtual void OnCompletedObjective( Guid player )
	{
		foreach( NPC npc in SelectedNPCs ) {

			npc.SetClientAnimationBehaviour( player, NPC.AnimationBehaviour.Cheer );

		}
	}

}
