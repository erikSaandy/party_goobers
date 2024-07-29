using Sandbox;
using System;

public class Player : Component
{
	[Property] public Guid NPCGuid { get; private set; }
	[Property] public NPC NPC => Scene.Directory.FindByGuid( NPCGuid ).Components.Get<NPC>();

	[Property] public string Name { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		if ( IsProxy ) { return; }

		NPCBuffer.Instance.Free( out Guid id );
		NPCGuid = id;

		NPC.GetPosessedBy( this );


	}

	protected override void OnDestroy()
	{
		base.OnDestroy();



	}

}
