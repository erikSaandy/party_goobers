using System;

public class LevelObjectiveHandler : Component
{

	/// <summary>
	/// List of potential objectives for this level.
	/// </summary>
	private List<LevelObjective> Objectives { get; set; }

	[Sync][Property] private int ObjectiveId { get; set; } = 0;
	public LevelObjective CurrentObjective => Objectives[ObjectiveId];

	[Sync][Property] public int PlayerCount { get; set; } = 0;

	protected override void OnAwake()
	{
		base.OnAwake();

		if(IsProxy) { return; }

		Objectives = GameObject.Components.GetAll<LevelObjective>( FindMode.InSelf ).ToList();
		ObjectiveId = Objectives.GetRandomId();

		PlayerCount = PartyFacesManager.PlayersAlive.Count();

	}

	[Broadcast]
	public void OnPlayerCompletedObjective( Guid playerId )
	{
		Player player = Scene.Directory.FindByGuid( playerId )?.Components.Get<Player>();

		if (player == null) { Log.Error( "Player that completed objective is null? fuck off." ); return; }

		player.LifeState = Player.PlayerLifeState.Safe;

		if(IsProxy) { return; }

		//Check for full completion

		int aliveCount = PartyFacesManager.PlayersAlive.Count();

		if( aliveCount == 0 )
		{
			PartyFacesManager.Instance.ExitRound();
		}

	}

}
