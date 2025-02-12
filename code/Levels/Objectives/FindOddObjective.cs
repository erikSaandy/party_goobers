﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

public class FindOddObjective : LevelObjective
{
	[Flags]
	public enum OddityFlags
	{
		OddRunner = 1,
		OddSleeper = 2,
		OddTennis = 4,
	}

	public override bool Disabled { get; set; } = false;
	public override int Weight => TargetNPCCount;

	public override int RequiredSpawnCount => MaxTargetNPCs;

	public override int MaxSelectedNPCs => TargetNPCCount;

	[Sync] public int TargetNPCCount { get; private set; }
	[Property][Range(1, 5)] public int MaxTargetNPCs { get; set; } = 3;

	[Sync] public List<Guid> TargetNPCIds { get; private set; }

	[Property] public OddityFlags OddityMask { get; set; }

	public override IEnumerable<NPC> GetNPCPool( List<NPC> set, LevelDataComponent level )
	{
		IEnumerable<NPC> npcs = base.GetNPCPool( set, level );

		TargetNPCCount = Game.Random.Int( 1, MaxTargetNPCs );
		TargetNPCIds = GetTargetNPCs( npcs, TargetNPCCount );

		CreateOddity();

		return npcs;

	}

	private void CreateOddity()
	{
		OddityFlags[] flags = Enum.GetValues( typeof( OddityFlags ) )
		.Cast<OddityFlags>()
		.Where( x => (OddityMask & x) == x )
		.ToArray();

		OddityFlags flag = flags[Game.Random.Next( flags.Length )];

		Log.Info( $"Selected oddity { flag.ToString() }" );

		if (flag == OddityFlags.OddRunner)
		{
			foreach(Guid npcId in TargetNPCIds )
			{
				NPC npc = Scene.Directory.FindByGuid( npcId ).Components.Get<NPC>();

				npc.Jog( true );
			}
		}
		else if(flag == OddityFlags.OddSleeper)
		{
			foreach ( Guid npcId in TargetNPCIds )
			{
				NPC npc = Scene.Directory.FindByGuid( npcId ).Components.Get<NPC>();
				npc.LookAt( npc.GameObject.Children.Find(x => x.Name == "sad_lookat" ).Id );
			}
		}
		else if(flag == OddityFlags.OddTennis)
		{
			TennisBall ball = Scene.GetAllComponents<TennisBall>().First();
			if(!ball.IsValid()) { Log.Error( "can't find tennis ball, Saandy is dumb." ); return; }

			foreach ( Guid npcId in TargetNPCIds )
			{
				NPC npc = Scene.Directory.FindByGuid( npcId ).Components.Get<NPC>();
				npc.LookAt( ball.InvertedBall.Id );
			}

		}

	}

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy ) { return; }

#if DEBUG
		foreach(Guid target in TargetNPCIds)
		{
			Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.LineSphere( Scene.Directory.FindByGuid( target ).Transform.Position + Vector3.Up * 16, 16 );
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
		if ( SelectedNPCs.Count < TargetNPCCount ) { return false; }

		for ( int i = 0; i < SelectedNPCs.Count; i++ )
		{
			// Selected is wrong
			if ( !TargetNPCIds.Contains( SelectedNPCs[i].GameObject.Id ) ) { return false; }
		}

		return true;

	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsProxy ) { return; }

		if ( TargetNPCIds == null ) { return; }

		foreach ( Guid npcId in TargetNPCIds )
		{
			if(!Scene.Directory.FindByGuid( npcId ).Components.TryGet<NPC>( out NPC npc, FindMode.EverythingInSelf )) { continue; }

			npc.Jog( false );
		}

	}

}
