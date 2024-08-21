using Sandbox;

public sealed class WorldLevelTimer : Component
{
	[RequireComponent] public TextRenderer Renderer { get; private set; }

	protected override void OnUpdate()
	{
		if( Renderer == null ) { return; }
				
		Renderer.Text = LevelTimer.TimeNowDisplay.ToString("0000");
	}
}
