using Microsoft.VisualBasic;
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

	protected override void OnStart()
	{
		base.OnStart();

		if(IsProxy) { return; }

		NPCs = new();

		for ( int i = 0; i < NPC_COUNT; i++ )
		{
			GameObject npc = SceneUtility.GetPrefabScene( Prefab ).Clone();

			npc.BreakFromPrefab();
			npc.NetworkSpawn();

			NPC c = npc.Components.Get<NPC>();

			NPCs.Add( c );
			c.Face.Hide();
			c.Hide();

		}

	}

	[Authority]
	public void PossessFreeNPC( Guid playerId )
	{
		if(IsProxy) { return; }

		IEnumerable<NPC> FreeNPCs = NPCs.Where( x => x.Owner == null );
		NPC npc = FreeNPCs.GetRandom();
		npc.SetOwner( playerId );

		Player player = Instance.Scene.Directory.FindByGuid( playerId ).Components.Get<Player>();
		player.SetNPC( npc.GameObject.Id );

		//Network.AssignOwnership( owner.Network.OwnerConnection );
		Log.Info( $"{player.Network.OwnerConnection.DisplayName} posessed npc" );
		npc.GameObject.Name = $"NPC ({player.Network.OwnerConnection.DisplayName})";

		// Load client face if excists on file.
		npc.Face.Load();

	}

	/// <summary>
	/// Place NPCs into current level.
	/// </summary>
	[Authority]
	public void PlaceNPCs()
	{

		if(IsProxy) { return; }

		LevelDataComponent level = LevelHandler.Instance.CurrentLevelData;
		if(level == null) {
			Log.Error( "Can't spawn NPCs because there is no level loaded." );
			return;
		}

		IEnumerable<NPC> pool = level.Objective.GetNPCPool( NPCs, level );

		for ( int i = 0; i < pool.Count(); i++ ) 
		{
			NPC npc = pool.ElementAt( i );


			if ( LevelHandler.Instance.FindSpawnLocation( out Transform tr ) )
			{
				npc.Spawn( tr );
			}
			else
			{
				continue;
			}

			// Is there a valid path?
			if ( level.GetRandomNodePath( out NodePathComponent path ) )
			{
				Vector3 pointOnPath = path.ClosestPointOnPath( npc.Transform.Position );
				int nextTargetId = path.GetNextTargetFromPos( pointOnPath );
				Vector3 nextTargetPos = path.GetTargetPosition( nextTargetId );

				Vector3 dirToNext = (nextTargetPos - pointOnPath).Normal;
				npc.Transform.Position = pointOnPath;
				//NPCs[i].Transform.Rotation = Rotation.FromYaw( Vector3.VectorAngle( dirToNext ).yaw );

				//Log.Info( NPCs[i].GameObject.Name + ": " +	 nextTargetPos );
				npc.MoveTowards( nextTargetPos );
			}

			npc.Show();

		}

	}

	/// <summary>
	/// Hide all NPCs.
	/// </summary>
	[Authority]
	public void HideNPCs()
	{
		foreach(NPC npc in NPCs)
		{
			npc.Hide();
		}
	}

}
