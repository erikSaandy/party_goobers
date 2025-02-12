using Sandbox;
using Sandbox.UI;
using System;
using System.Numerics;
using System.Threading.Channels;

public class LobbyComponent : Component, Component.INetworkListener
{

	[Sync] public int PlayersInLobby { get; set; } = 0;

	[Sync] public NetList<Guid> NPCs { get; set; } = new();

	protected override void OnAwake()
	{
		base.OnAwake();
		FadeScreen.Hide();
	}

	protected override void OnStart()
	{
		base.OnStart();

		if(IsProxy) { return; }

		NPCs.Clear();
		PlayersInLobby = 0;

		IEnumerable<Player> players = PartyFacesManager.Players;

		foreach ( Player player in players )
		{
			PlayersInLobby++;
			NPC npc = NPCBuffer.Instance.PlaceLobbyNPC( player.Network.OwnerConnection.Id );
			//npc.SetAnimationBehaviour( NPC.AnimationBehaviour.Cheer );
			NPCs.Add( npc.GameObject.Id );
		}
	}

	protected override void OnUpdate()
	{
	}

	/// <summary>
	/// A client is fully connected to the server. This is called on the host.
	/// </summary>
	public void OnActive( Connection channel )
	{
		if( IsProxy ) { return; }

		Log.Info( $"Player '{channel.DisplayName}' joined lobby." );

		PlayersInLobby++;
		NPC npc = NPCBuffer.Instance.PlaceLobbyNPC( channel.Id );
		NPCs.Add( npc.GameObject.Id );

		npc.Speak( "sounds/npc_hello.sound" );
		npc.Wave();

		//npc.SetAnimationBehaviour( NPC.AnimationBehaviour.Cheer );

	}

	public void OnDisconnected( Connection channel )
	{
		if(channel == null ) { return; }
		Log.Info( $"Player '{channel.DisplayName}' left lobby." );

		PlayersInLobby--;

		if(NPCs == null) { return; }

		for(int i = 0; i < NPCs.Count; i++ )
		{

			NPC npc = Scene.Directory.FindByGuid( NPCs[i] ).Components.Get<NPC>();

			if ( !npc.IsValid() ) { continue; }

			if ( npc.ConnectionId == channel.Id )
			{
				npc.ConnectionId = default;
				PartyFacesManager.EnableGameobject( npc.GameObject.Id, false );
				NPCs?.RemoveAt( i );
				break;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if(IsProxy) { return; }

		foreach ( Guid npcId in NPCs )
		{
			NPC npc = Scene.Directory.FindByGuid( npcId ).Components.Get<NPC>(true);
			npc?.Randomize();
		}

		NPCBuffer.Instance?.HideNPCs();
	}

}


