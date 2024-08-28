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

	protected override void OnAwake()
	{
		base.OnAwake();

		Objectives = GameObject.Components.GetAll<LevelObjective>( FindMode.InSelf ).ToList();

		if (IsProxy) { return; }

		//ObjectiveId = Objectives.TakeWhile( x => (x is not FindOddObjective) ).Count();
		ObjectiveId = Objectives.GetRandomIdWeighted<LevelObjective>();

		foreach(LevelObjective objective in Objectives)
		{
			if(objective == CurrentObjective) { continue; }
			objective.Enabled = false;
		}

		Log.Info( $"Selected Objective: {CurrentObjective.GetType()}." );

	}

	[Broadcast]
	public void OnPlayerCompletedObjective( Guid playerId )
	{
		Player player = Scene.Directory.FindByGuid( playerId )?.Components.Get<Player>();

		if (player == null) { Log.Error( "Player that completed objective is null? fuck off." ); return; }

		Log.Info( player.Network.OwnerConnection.DisplayName + " completed objective.");

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
