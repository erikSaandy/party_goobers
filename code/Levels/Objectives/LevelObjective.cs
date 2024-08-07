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

	protected virtual GameObject NPCLookAt => null;

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

	protected virtual void ClientDeselectedNPC( Guid player, NPC npc )
	{

	}

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
		if ( !AcceptSelection ) { return; }


		if ( SelectedNPCs.Contains( npc ) )
		{
			SelectedNPCs.Remove( npc );
			ClientDeselectedNPC( player, npc );

			npc.SetClientAnimationBehaviour( player, NPC.AnimationBehaviour.Default );
			Sound.Play( "sounds/npc_deselect.sound" );
			return;
		}

		if( SelectedNPCs.Count >= MaxSelectedNPCs ) { return; }


		SelectedNPCs.Add( npc );

		npc.SetClientAnimationBehaviour( player, NPC.AnimationBehaviour.Wave );
		Sound.Play( "sounds/npc_select.sound" );


		ClientSelectedNPC( player, npc );

		Log.Info( "Selected NPC count: " + SelectedNPCs.Count );
		bool evaluate = SelectedNPCs.Count >= MaxSelectedNPCs;

		if (evaluate)
		{
			AcceptSelection = false;

			if( ObjectiveIsSatisfied() )
			{
				Scene.Directory.FindByGuid( player ).Components.Get<Player>().MarkAsSafe();

				await Task.Delay( 700 );

				Handler.OnPlayerCompletedObjective( player );

				// ON CORRECT SELECTION
				OnCompletedObjective( player );
			}
			else
			{
				await Task.Delay( 1000 );

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

		set = set.Shuffle().ToList();

		GameObject lookAt = NPCLookAt == null ? Scene.Camera.GameObject : NPCLookAt;

		IEnumerable<NPC> npcs = set.Take( spawnCount );

		// Make sure npc looks at Objective lookAt (defaults to scene camera).
		foreach (NPC npc in npcs )
		{
			PartyFacesManager.EnableGameobject( npc.GameObject.Id, true );
			npc.LookAt( lookAt.Id );
		}

		//TODO: Make sure player NPCs are always chosen.
		return npcs;

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

		Log.Info( "Cleared objective!" );

	}

}
