﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FindAlikeObjective : LevelObjective
{

	public override bool Disabled { get; set; } = false;
	public override int Weight => 100;


	public override int RequiredSpawnCount => NumberOfAlike;

	public override int MaxSelectedNPCs => NumberOfAlike;

	[Property][Range(1, 5)] public int NumberOfAlike { get; private set; } = 2;

	[Sync] public Guid TargetNPCId { get; private set; }
	[Sync] public List<Guid> AlikeNPCIds { get; private set; }

	public override IEnumerable<NPC> GetNPCPool( List<NPC> set, LevelDataComponent level )
	{	
		IEnumerable<NPC> npcs = base.GetNPCPool( set, level );

		int targetId = npcs.GetRandomId();
		TargetNPCId = npcs.ElementAt( targetId ).GameObject.Id;

		AlikeNPCIds = new();

		int id = targetId;
		while (true)
		{
			id++;
			id %= npcs.Count();

			npcs.ElementAt( id ).GameObject.Components.Get<NPC>(true).CopyFrom( TargetNPCId );

			AlikeNPCIds.Add( npcs.ElementAt( id ).GameObject.Id );

			if(AlikeNPCIds.Count >= NumberOfAlike - 1)
			{
				break;
			}

		}

		npcs.ElementAt( targetId ).GameObject.Components.Get<NPC>( true ).CopyFrom( AlikeNPCIds[0] );

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
		if ( AlikeNPCIds == null || AlikeNPCIds.Count == 0 || TargetNPCId == default ) { return; }

		Gizmo.Draw.LineSphere( Scene.Directory.FindByGuid( TargetNPCId ).WorldPosition + Vector3.Up * 16, 16 );

		foreach (Guid id in AlikeNPCIds )
		{
			//Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.LineSphere( Scene.Directory.FindByGuid( id ).WorldPosition + Vector3.Up * 16, 16 );
		}
#endif

	}

	protected override void ClientSelectedNPC( Guid player, NPC npc )
	{

	}

	protected override void CompletedObjective( Guid player )
	{
		base.CompletedObjective( player );

		if(IsProxy) { return; }


	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		if(IsProxy) { return; }

		if(AlikeNPCIds == null) { return; }

		// randomize alike npcs on level unload.
		foreach ( Guid npcId in AlikeNPCIds )
		{
			if ( !Scene.Directory.FindByGuid( npcId ).Components.TryGet<NPC>( out NPC npc, FindMode.EverythingInSelf ) ) { continue; }

			npc.Randomize();
		}

	}

	protected override bool ObjectiveIsSatisfied()
	{
		if(SelectedNPCs.Count < NumberOfAlike) { return false; }

		NPC target = Scene.Directory.FindByGuid( TargetNPCId ).Components.Get<NPC>( true );

		for(int i = 0; i < SelectedNPCs.Count; i++ )
		{
			// Selected is target
			if( SelectedNPCs[i].GameObject.Id == TargetNPCId ) { continue; }

			// Selected npc is not alike target
			if(!SelectedNPCs[i].IsAlike( target )) { return false; }
		}

		return true;
	}
}
