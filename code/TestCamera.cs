using Sandbox;

public class TestCamera : Component
{
	TimeSince TimeSinceSpace = 0;
	protected override void OnUpdate()
	{
		if ( Input.Pressed( "Jump" ) )
		{
			TimeSinceSpace = 0;
		}

		if (Input.Down("Jump"))
		{
			Transform.Position += Transform.Rotation.Forward * Time.Delta * -30 * TimeSinceSpace * TimeSinceSpace;
		}

	}
}
