using Sandbox;

public class VariationGroup : Component
{
	[Property][Range( 0f, 1f )] public float TriggerChance { get; set; } = 0.2f;

	protected override void OnStart()
	{
		base.OnStart();

		if(IsProxy) { return; }

		if ( Game.Random.Float( 0f, 1f ) > TriggerChance ) { GameObject.Destroy(); return; }

	}


	protected override void OnUpdate()
	{

	}
}
