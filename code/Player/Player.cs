using Sandbox;

public class Player : Component
{
	[Property] public Face Face { get; set; }

	[Property] public string Name { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		if ( IsProxy ) { return; }

	}

}
