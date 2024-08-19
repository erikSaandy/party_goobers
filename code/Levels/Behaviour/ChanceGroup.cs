using Sandbox;

public class ChangeGroup : Component
{
	[Property][Range( 0f, 1f )] public float TriggerChance { get; set; } = 0.2f;

	protected override void OnStart()
	{
		base.OnStart();

		if ( Game.Random.Float( 0f, 1f ) > TriggerChance ) { Destroy(); return; }

	}


	protected override void OnUpdate()
	{

	}
}
