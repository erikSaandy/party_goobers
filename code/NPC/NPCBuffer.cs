using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NPCBuffer : SingletonComponent<NPCBuffer>
{

	public const int NPC_COUNT = 150; // TODO: Tie to max player count.

	[Property] public PrefabFile Prefab { get; private set; }

	public static List<Guid> NPCs { get; private set; }
	public static Stack<Guid> FreeNPCs { get; private set; }

	protected override void OnStart()
	{
		base.OnAwake();

		SpawnBuffer();

	}

	private void SpawnBuffer()
	{
		NPCs = new();
		FreeNPCs = new();

		for(int i = 0; i < NPC_COUNT; i++ )
		{
			GameObject npc = SceneUtility.GetPrefabScene( Prefab ).Clone();
			npc.BreakFromPrefab();

			NPC c = npc.Components.Get<NPC>();

			npc.NetworkSpawn();
			NPCs.Add( npc.Id );
			FreeNPCs.Push( npc.Id );

			int step = 27;
			int count = 15;
			float start = ((count * 0.5f) * -step) + ( (i / count % 2) == 0 ? step*0.5f : 0 );
			float posX = start + (i % count) * step;
			float posY = ( -(int)(i / count) * step ) + 256;

			npc.Transform.Position = new Vector3( -posY, posX, posY );

		}

	}

	public bool Free(out Guid id)
	{
		if ( FreeNPCs.TryPop( out id ) ) 
		{
			return true;
		}

		return false;
	}

}
