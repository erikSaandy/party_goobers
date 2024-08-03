using Sandbox;

public class LevelObjectiveHandler : Component
{

	/// <summary>
	/// List of potential objectives for this level.
	/// </summary>
	private List<LevelObjective> Objectives { get; set; }

	[Sync][Property] private int ObjectiveId { get; set; } = 0;
	public LevelObjective CurrentObjective => Objectives[ObjectiveId];

	protected override void OnAwake()
	{
		base.OnAwake();

		if(IsProxy) { return; }

		Objectives = GameObject.Components.GetAll<LevelObjective>( FindMode.InSelf ).ToList();
		ObjectiveId = Objectives.GetRandomId();

	}

	protected override void OnUpdate()
	{

	}
}
