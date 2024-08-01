using Sandbox;

public sealed class LevelDataComponent : Component
{

	// If nodepath is null, NPCs stand still. 
	// Else, NPCs follow NodePath

	// Multiple paths...
	// Pick random

	[Property] public CameraComponent CameraReference { get; set; }
	[Property] public MapInstance MapInstance { get; set; }
	[Property] public NodePathComponent NodePath { get; set; }

	protected override void OnUpdate()
	{

	}

	public void OnEnterRound()
	{

	}

	public void OnExitRound()
	{

	}

}
