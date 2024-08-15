public class EscalatrorComponent : Component
{

	[Property] private SkinnedModelRenderer Renderer { get; set; }

	[Property][Range(0, 6)] public float Speed { get; set; } = 1f;

	private Transform? Step => Renderer.GetAttachment( "step_reference" );
	private Vector3 PositionOld { get; set; }
	private Vector3 PositionStart { get; set; }
	private Vector3 StepSize { get; set; }

	[Property] private List<NPC> Passengers { get; set; }

	[Property] private BoxCollider TriggerVolume { get; set; }
	[Property] private BoxCollider LoopTriggerVolume { get; set; }

	private Vector3 LoopPosition { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		TriggerVolume.OnTriggerEnter += OnTriggerEnter;
		LoopTriggerVolume.OnTriggerEnter += OnEnterLoopTrigger;
	}

	protected override void OnStart()
	{
		base.OnStart();

		if(IsProxy) { return; }

		PositionOld = Step.Value.Position;
		PositionStart = PositionOld;

		Passengers = new();

		LoopPosition = Step.Value.Position;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		Renderer.Set( "f_speed", Speed );

		if ( IsProxy ) { return; }

		if( !Step.HasValue ) { return; }

		Vector3 delta = Step.Value.Position - PositionOld;

		if ( delta.z < 0f )
		{
			StepSize = Step.Value.Position - PositionStart;
			delta -= StepSize;
		}

		PositionOld = Step.Value.Position;

		foreach ( NPC passenger in Passengers )
		{

			//if(!passenger.Controller.IsOnGround) { continue; }
			//passenger.Transform.Position += delta;
			//passenger.Controller.MoveTo( passenger.Transform.Position + delta, false );
			passenger.Teleport( passenger.Transform.Position + delta );

		}

	}

	void OnTriggerEnter( Collider other )
	{
		if(IsProxy) { return; }

		if( !other.GameObject.Enabled ) { return; }

		other.GameObject.Components.TryGet( out NPC npc );

		if(npc == null) { return; }

		if(!Passengers.Contains( npc ) )
		{
			Log.Info( "enter" );
			Passengers.Add( npc );
		}
	}

	void OnEnterLoopTrigger( Collider other )
	{
		if ( IsProxy ) { return; }

		other.GameObject.Components.TryGet( out NPC npc );

		if ( npc == null ) { return; }

		if ( !Passengers.Contains( npc ) ) { return; }

		Log.Info( "exit" );
		npc.Teleport( LoopPosition );
		//Passengers.RemoveAt( id );

	}

}

