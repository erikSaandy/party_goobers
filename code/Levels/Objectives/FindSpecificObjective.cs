using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FindSpecificObjective : LevelObjective
{

	public override int MaxSelectedNPCs => 1;

	[Sync][Property] public Guid TargetNPCId { get; private set; }

	public override IEnumerable<NPC> GetNPCPool( List<NPC> set, LevelDataComponent level )
	{	
		IEnumerable<NPC> npcs = base.GetNPCPool( set, level );

		TargetNPCId = npcs.GetRandom().GameObject.Id;

		return npcs;

	}

	protected override void ClientSelectedNPC( Guid player, NPC npc )
	{

	}

	protected override void OnCompletedObjective( Guid player )
	{
		base.OnCompletedObjective( player );

		Log.Info( "Cleared objective!" );

	}

	protected override bool ObjectiveIsSatisfied()
	{
		if(SelectedNPCs.Count == 0) { return false; }

		return SelectedNPCs[0].GameObject.Id == TargetNPCId;
	}
}
