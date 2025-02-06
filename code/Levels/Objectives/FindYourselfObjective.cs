using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FindYourselfObjective : LevelObjective
{
	public override bool Disabled { get; set; } = false;
	public override int Weight => 100;

	public override int RequiredSpawnCount => PartyFacesManager.Players.Count();

	public override int MaxSelectedNPCs => 1;

	public Guid TargetNPCId { get; private set; }

	public override IEnumerable<NPC> GetNPCPool( List<NPC> set, LevelDataComponent level )
	{	
		IEnumerable<NPC> npcs = base.GetNPCPool( set, level );

		TargetNPCId = npcs.GetRandom().GameObject.Id;

		int i = 0;
		foreach ( Connection con in Connection.All )
		{
			Log.Info( "creating npc for con " + con.DisplayName + "..." );
			CreateConnectionNPC( npcs.ElementAt(i).GameObject.Id, con.Id );
			i++;
		}

		//NPCIconGenerator.Instance.RequestNPCHeadshot( TargetNPCId );

		return npcs;

	}

	[Rpc.Broadcast]
	private void CreateConnectionNPC( Guid npcId, Guid conId )
	{
		if(Connection.Local.Id != conId) { return; }

		NPC npc = Scene.Directory.FindByGuid( npcId ).Components.Get<NPC>( true );
		if(!npc.IsValid()) { Log.Error( "Couldn't find NPC for your client. Saandy is moron." ); }

		TargetNPCId = npc.GameObject.Id;
		npc.LoadFromConnection( Connection.Local.Id );

		NPCIconGenerator.Instance.RequestNPCHeadshotLocal( npc.GameObject.Id );
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

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if( TargetNPCId == default) { return; }

		GameObject npcObj = Scene.Directory.FindByGuid( TargetNPCId );
		if(!npcObj.IsValid()) { return; }

		if ( !Scene.Directory.FindByGuid( TargetNPCId ).Components.TryGet<NPC>( out NPC npc, FindMode.EverythingInSelf ) ) { return; }

		npc?.Randomize();

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

		NPC target = Scene.Directory.FindByGuid( TargetNPCId ).Components.Get<NPC>( true );
		if( !target.IsValid() ) { return false; }

		return SelectedNPCs[0].IsAlike( target );
	}
}
