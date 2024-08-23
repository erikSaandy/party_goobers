using Sandbox;
using System;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;

public abstract class LevelObjective : Component, IWeighted
{
	public virtual bool Disabled { get; set; }
	public virtual int Weight { get; }


	public List<NPC> SelectedNPCs { get; set; } = new();

	public virtual int MaxSelectedNPCs => 1;

	protected bool AcceptSelection { get; set; } = true;

	public LevelObjectiveHandler Handler => LevelHandler.Instance.CurrentLevelData.ObjectiveHandler;

	public LevelDataComponent LevelData => LevelHandler.Instance.CurrentLevelData;

	public IEnumerable<NPC> NPCs { get; set; } = null;

	public static Action OnCompletedObjective { get; set; }

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

	private async void CheckSuccess( Guid playerId, NPC npc )
	{

		// Return if doesn't accept selection or player has selected all npcs to trigger objective
		if ( !AcceptSelection ) { return; }


		if ( SelectedNPCs.Contains( npc ) )
		{
			SelectedNPCs.Remove( npc );
			ClientDeselectedNPC( playerId, npc );

			npc.SetClientAnimationBehaviour( playerId, NPC.AnimationBehaviour.Default );
			PartyFacesManager.PlaySoundClient( "sounds/npc_deselect.sound" );
			return;
		}

		if( SelectedNPCs.Count >= MaxSelectedNPCs ) { return; }


		SelectedNPCs.Add( npc );

		npc.SetClientAnimationBehaviour( playerId, NPC.AnimationBehaviour.Wave );
		Sound.Play( "sounds/npc_select.sound" );
		PartyFacesManager.PlaySoundClient( "sounds/npc_select.sound" );


		ClientSelectedNPC( playerId, npc );

		//Log.Info( "Selected NPC count: " + SelectedNPCs.Count );
		bool evaluate = SelectedNPCs.Count >= MaxSelectedNPCs;

		if (evaluate)
		{
			AcceptSelection = false;

			Player player = Scene.Directory.FindByGuid( playerId ).Components.Get<Player>();

			if ( ObjectiveIsSatisfied() )
			{
				Scene.Directory.FindByGuid( playerId ).Components.Get<Player>().MarkAsSafe();

				await Task.Delay( 700 );

				PartyFacesManager.Instance.ThrowConfettiClient();
				Handler.OnPlayerCompletedObjective( playerId );

				// ON CORRECT SELECTION
				CompletedObjective( playerId );
			}
			else
			{
				await Task.Delay( 700 );

				foreach(NPC npcSelected in SelectedNPCs)
				{
					int score = -1000;
					PartyFacesManager.Instance.LabelHandler.SpawnLabel( score.ToString(), Scene.Camera.PointToScreenNormal( npcSelected.Transform.Position + Vector3.Up * 32), Vector2.Up * 250, true );
					player.AddScore( score );

				}

				// ON WRONG SELECTION
				ResetSelection( playerId );
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

		int spawnCount = LevelData.MinSpawnCount.HasValue ? Game.Random.Int(LevelData.MinSpawnCount.Value, LevelData.SpawnPoints.Count()) : LevelData.SpawnPoints.Count();
		Log.Info( "found " + spawnCount + " spawners in level." );

		spawnCount = int.Min( spawnCount, set.Count );

		set = set.Shuffle().ToList();

		NPCs = set.Take( spawnCount );

		// Get lookAt
	   GameObject lookAt = LevelData.OverrideLookAt ? LevelData.NpcLookAtOverride : Scene.Camera.GameObject;

		// Make sure npc looks at Objective lookAt (defaults to scene camera).
		foreach (NPC npc in NPCs )
		{
			PartyFacesManager.EnableGameobject( npc.GameObject.Id, true );

			if(lookAt == null) { continue; }

			npc.LookAt( lookAt.Id );
		}

		//TODO: Make sure player NPCs are always chosen.
		return NPCs;

	}

	protected List<Guid> GetTargetNPCs( IEnumerable<NPC> npcs, int count)
	{
		int startId = npcs.GetRandomId();

		List<Guid> targets = new();

		int id = startId;
		while ( true )
		{
			targets.Add( npcs.ElementAt( id ).GameObject.Id);

			id++;
			id %= npcs.Count();

			if ( targets.Count > count - 1 )
			{
				break;
			}

		}

		return targets;
	}


	/// <summary>
	/// Called when player completes the objective.
	/// </summary>
	/// <param name="player"></param>
	protected virtual void CompletedObjective( Guid player )
	{
		OnCompletedObjective?.Invoke();

		foreach ( NPC npc in SelectedNPCs ) {

			PartyFacesManager.SpawnStarParticlesClient( npc.Transform.Position + Vector3.Up * 55 );
			npc.SetClientAnimationBehaviour( player, NPC.AnimationBehaviour.Cheer );

		}

		Log.Info( "Cleared objective!" );

	}

}
