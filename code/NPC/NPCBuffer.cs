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
	public static List<NPC> NPCs { get; private set; }

	protected override void OnStart()
	{
		base.OnAwake();

		if(IsProxy) { return; }

		SpawnBuffer();

	}

	[Authority]
	private void SpawnBuffer()
	{
		if(IsProxy) { return; }

		NPCs = new();

		for(int i = 0; i < NPC_COUNT; i++ )
		{
			GameObject npc = SceneUtility.GetPrefabScene( Prefab ).Clone();
			npc.BreakFromPrefab();

			npc.NetworkSpawn();

			NPC c = npc.Components.Get<NPC>();
			c.Hide();
			NPCs.Add( c );

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

}
