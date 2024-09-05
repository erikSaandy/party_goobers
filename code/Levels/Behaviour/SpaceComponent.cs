using Saandy;
using System;

namespace Sandbox;

public class SpaceComponent : Component
{
	const float MAX_ANGLE = 0.6f;

	IEnumerable<NPC> NPCs => LevelHandler.Instance.CurrentLevelData.Objective.NPCs;

	List<float> Angles = new List<float>();

	[Broadcast]
	public void OnNPCSpawned(Guid npcId)
	{
		if ( IsProxy ) { return; }

		NPC npc = null;
		Scene.Directory.FindByGuid( npcId ).Components.TryGet<NPC>( out npc, FindMode.EverythingInSelf);

		if ( npc == null || !npc.IsValid ) { return; }

		npc.Float();
		Angles.Add( Game.Random.Float( -MathF.PI * 1f, MathF.PI * 1f ) );

	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if(IsProxy) { return; }

		for ( int i = 0; i < NPCs.Count(); i++ )
		{
			NPC npc = NPCs.ElementAt( i );

			if(Angles.Count < i + 1 ) { return; }

			float dir = i % 2 == 1 ? -1 : 1;
			Angles[i] += Time.Delta * dir * .3f;

			float yaw = MathF.Cos( Angles[i] ) * Math2d.Rad2Deg * 0.5f;

			npc.Transform.Rotation = Rotation.FromAxis( Scene.Camera.Transform.Rotation.Forward, yaw );

		}

	}

}
