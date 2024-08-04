using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FindSpecificObjective : LevelObjective
{
	public override int MaxSelectedNPCs => 1;

	[Sync] public Guid TargetNPCId { get; private set; }

	public override IEnumerable<NPC> GetNPCPool( List<NPC> set, LevelDataComponent level )
	{	
		IEnumerable<NPC> npcs = base.GetNPCPool( set, level );

		TargetNPCId = npcs.GetRandom().GameObject.Id;

		return npcs;

	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		
		if(IsProxy) { return; }

		Gizmo.Draw.Color = Color.Green;
		Gizmo.Draw.LineSphere( Scene.Directory.FindByGuid( TargetNPCId ).Transform.Position + Vector3.Up * 16, 16 );

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
