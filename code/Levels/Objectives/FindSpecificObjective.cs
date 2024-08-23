using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FindSpecificObjective : LevelObjective
{
	public override bool Disabled { get; set; } = false;
	public override int Weight => 220;

	public override int MaxSelectedNPCs => 1;

	[Sync] public Guid TargetNPCId { get; private set; }

	public override IEnumerable<NPC> GetNPCPool( List<NPC> set, LevelDataComponent level )
	{	
		IEnumerable<NPC> npcs = base.GetNPCPool( set, level );

		TargetNPCId = npcs.GetRandom().GameObject.Id;

		NPCIconGenerator.Instance.RequestNPCHeadshot( TargetNPCId );

		return npcs;

	}

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		
		if(IsProxy) { return; }

#if DEBUG
		if( TargetNPCId != default)
		{
			Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.LineSphere( Scene.Directory.FindByGuid( TargetNPCId ).Transform.Position + Vector3.Up * 16, 16 );
		}
#endif

	}

	protected override void ClientSelectedNPC( Guid player, NPC npc )
	{

	}

	protected override void CompletedObjective( Guid player )
	{
		base.CompletedObjective( player );

	}

	protected override bool ObjectiveIsSatisfied()
	{
		if(SelectedNPCs.Count == 0) { return false; }

		return SelectedNPCs[0].GameObject.Id == TargetNPCId;
	}
}
