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

	[Sync][Property] public int PlayerCount { get; set; } = 0;

	protected override void OnAwake()
	{
		base.OnAwake();

		Objectives = GameObject.Components.GetAll<LevelObjective>( FindMode.InSelf ).ToList();

		if (IsProxy) { return; }

		ObjectiveId = Objectives.GetRandomId();

		PlayerCount = PartyFacesManager.PlayersAlive.Count();

	}

	[Broadcast]
	public void OnPlayerCompletedObjective( Guid playerId )
	{
		Player player = Scene.Directory.FindByGuid( playerId )?.Components.Get<Player>();

		if (player == null) { Log.Error( "Player that completed objective is null? fuck off." ); return; }

		float time = MathF.Max( LevelTimer.TimeNow + 1, 0 );

		player.AddScore( (int)(time * 1000) );

		Log.Info( player.Network.OwnerConnection.DisplayName + " completed objective.");

		if (IsProxy) { return; }

		//Check for full completion

		// TODO: bandaid fix because player life state won't update in time?
		int aliveCount = PartyFacesManager.PlayersAlive.Where( x => x != player ).Count();


		if ( aliveCount == 0 )
		{
			PartyFacesManager.Instance.ExitRound();
		}

	}

}
