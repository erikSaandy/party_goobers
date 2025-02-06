using System;
using System.Threading;

public class LevelObjectiveHandler : Component
{

	/// <summary>
	/// List of potential objectives for this level.
	/// </summary>
	private List<LevelObjective> Objectives { get; set; }

	[Sync][Property] public int ObjectiveId { get; set; } = 0;
	public LevelObjective CurrentObjective => Objectives[ObjectiveId];

	protected override void OnStart()
	{
		base.OnStart();

		if (IsProxy) { return; }

	}

	public IEnumerable<NPC> GetNPCPool( List<NPC> set, LevelDataComponent level )
	{
		GetObjectives();

		int spawnCount = level.SpawnPoints.Count();
		if(level.MinSpawnCount.HasValue) { spawnCount = level.MinSpawnCount.Value; }

		foreach(LevelObjective obj in Objectives)
		{
			// If there isn't enough spawners, disable objective.
			if ( obj.RequiredSpawnCount > spawnCount )
			{

				if ( Objectives.Find( x => x is FindYourselfObjective ) != null )
				{
					int fyobj = Objectives.TakeWhile( x => (x is not FindYourselfObjective) ).Count();
					Objectives[fyobj].Disabled = true;
					Log.Info( "Disabled FindYourselfObjective." );
				}

			}
		}

		ObjectiveId = Objectives.GetRandomIdWeighted( considerDisabled: true );

		foreach ( LevelObjective objective in Objectives )
		{
			if ( objective == CurrentObjective ) { continue; }
			objective.Enabled = false;
		}

		Log.Info( $"Selected Objective: {CurrentObjective.GetType()}." );

		return CurrentObjective.GetNPCPool( set, level );

	}

	[Rpc.Broadcast]
	private void GetObjectives()
	{
		Objectives = GameObject.Components.GetAll<LevelObjective>( FindMode.InSelf ).ToList();
	}


	[Rpc.Broadcast]
	public void OnPlayerCompletedObjective( Guid playerId )
	{
		Player player = Scene.Directory.FindByGuid( playerId )?.Components.Get<Player>();

		if (!player.IsValid()) { Log.Error( "Player that completed objective is null? fuck off." ); return; }

		Log.Info( player.Network.Owner.DisplayName + " completed objective.");

		if (IsProxy) { return; }

		float time = MathF.Max( LevelTimer.TimeNow + 1, 0 );
		player.AddScore( (int)(time * 1000) );

		// TODO: bandaid fix because player life state won't update in time?
		int aliveCount = PartyFacesManager.PlayersAlive.Count();

		if ( aliveCount == 0 )
		{
			PartyFacesManager.Instance.ExitRound();
		}

	}

}
