using Sandbox;
using System;

public class WhackamoleComponent : Component
{

	 
	IEnumerable<NPC> NPCs => LevelHandler.Instance.CurrentLevelData.Objective.NPCs;

	private TimeSince TimeSinceUpdate { get; set; }

	[Broadcast]
	public void OnInitiated()
	{

		if(IsProxy) { return; }

		foreach(NPC npc in NPCs )
		{
			npc.Crouch();
		}

		TimeSinceUpdate = 0;

	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if( IsProxy ) { return; }

		if(TimeSinceUpdate > 3)
		{
			foreach ( NPC npc in NPCs )
			{
				if ( Game.Random.Int( 0, 2 ) >= 1 )
				{
					npc.ToggleCrouch();
				}

			}

			TimeSinceUpdate = 0;

		}

	}

}
