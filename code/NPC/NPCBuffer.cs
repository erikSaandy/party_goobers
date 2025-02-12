﻿using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NPCBuffer : SingletonComponent<NPCBuffer>
{

	public const int NPC_COUNT = 50; // TODO: Tie to max player count.

	[Property] public PrefabFile Prefab { get; private set; }

	// HOST ONLY
	[Property] public List<NPC> NPCs { get; private set; }

	public static Action OnNPCsPlaced { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		if(IsProxy) { return; }

		NPCs = new();

		for ( int i = 0; i < NPC_COUNT; i++ )
		{
			GameObject npc = SceneUtility.GetPrefabScene( Prefab ).Clone();

			NPC c = npc.Components.Get<NPC>( true );
			c.Randomize();

			NPCs.Add( c );

			npc.BreakFromPrefab();
			npc.NetworkSpawn();

			PartyFacesManager.EnableGameobject( c.GameObject.Id, false );

		}

	}

	//[Authority]
	//public void PossessFreeNPC( Guid playerId )
	//{
	//	if(IsProxy) { return; }

	//	IEnumerable<NPC> FreeNPCs = NPCs.Where( x => x.Owner == null );
	//	NPC npc = FreeNPCs.GetRandom();
	//	npc.SetOwner( playerId );

	//	Player player = Instance.Scene.Directory.FindByGuid( playerId ).Components.Get<Player>();
	//	player.SetNPC( npc.GameObject.Id );

	//	//Network.AssignOwnership( owner.Network.OwnerConnection );
	//	Log.Info( $"{player.Network.OwnerConnection.DisplayName} posessed npc" );
	//	npc.GameObject.Name = $"NPC ({player.Network.OwnerConnection.DisplayName})";

	//	// Load client face if excists on file.
	//	npc.Face.Load();

	//}

	/// <summary>
	/// Place NPCs into current level.
	/// </summary>
	public async Task PlaceNPCs()
	{

		if(IsProxy) { return; }

		LevelDataComponent level = LevelHandler.Instance.CurrentLevelData;

		if(!level.IsValid()) {
			Log.Error( "Can't spawn NPCs because there is no level loaded." );
			return;
		}

		IEnumerable<NPC> pool = level.ObjectiveHandler.GetNPCPool( NPCs, level );

		for ( int i = 0; i < pool.Count(); i++ ) 
		{

			NPC npc = pool.ElementAt( i );

			if ( LevelHandler.Instance.FindSpawnLocation( out Transform tr ) )
			{

				PartyFacesManager.EnableGameobject( npc.GameObject.Id, true );
				npc.Spawn( tr );
			}
			else
			{
				continue;
			}

			if( level.NodePaths?.Length > 0)
			{

				// Get closest path point among all level paths.
				NodePathComponent path = null;
				float dstClosest = 99999999;
				Vector3 closestPathPoint = 0;

				foreach(NodePathComponent pPath in level.NodePaths)
				{
					Vector3 pointOnPath = pPath.ClosestPointOnPath( npc.WorldPosition );

					float dst = Vector3.DistanceBetweenSquared( npc.WorldPosition, pointOnPath );

					if ( dst < dstClosest )
					{
						dstClosest = dst;
						closestPathPoint = pointOnPath;
						path = pPath;
					}

				}


				int nextTargetId = path.GetNextTargetFromPos( closestPathPoint );
				Vector3 nextTargetPos = path.GetTargetPosition( nextTargetId );
				Vector3 dirToNext = (nextTargetPos - closestPathPoint).Normal;

				npc.WorldPosition = closestPathPoint;
				npc.MoveTowards( nextTargetPos );

			}
			else
			{
				npc.StopMoving();
			}

			LevelHandler.Instance.CurrentLevelData.NPCSpawned( npc.GameObject.Id );

			await Task.Yield();

		}

		PostNPCsPlaced();

	}

	[Rpc.Broadcast]
	private void PostNPCsPlaced()
	{
		OnNPCsPlaced?.Invoke();
	}

	public NPC PlaceLobbyNPC( Guid connectionId )
	{
		Connection con = Connection.Find( connectionId );
		Log.Info( "placing " + con.DisplayName + " lobby npc..." );

		NPC npc = NPCs.Where( x => x.Enabled == true ).GetRandom();
		npc.StopMoving();
		npc.LookAt( Scene.Camera.GameObject.Id );
		PartyFacesManager.EnableGameobject( npc.GameObject.Id, true );

		SpawnPoint sp = LevelHandler.Instance.CurrentLevelData.SpawnPoints.Where( x => !x.Tags.Contains( "taken" ) ).GetRandom();
		sp.Tags.Add( "taken" );
		// Place NPC on a spawnpoint in lobby that is not taken.
		npc.Spawn( sp.Transform.World );
		npc.SetOwner( connectionId );
		npc.LoadFromConnection( connectionId );

		return npc;

	}

	/// <summary>
	/// Hide all NPCs.
	/// </summary>
	[Rpc.Owner]
	public void HideNPCs()
	{
		foreach(NPC npc in NPCs)
		{
			if( !npc.IsValid() || npc.Tags == null ) { continue; }
			if(npc.Tags.Has( "npcdisplay" ) ) { continue; }

			if(npc.GameObject.Enabled)
			{
				PartyFacesManager.EnableGameobject( npc.GameObject.Id, false );
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		OnNPCsPlaced = null;

	}

}
