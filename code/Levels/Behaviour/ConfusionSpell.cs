using Saandy;
using Sandbox;
using System;

public sealed class ConfusionSpell : Component
{
	[Property] [Range( 0f, 1f )] public float TriggerChance { get; set; } = 0.2f;

	IEnumerable<NPC> NPCs => LevelHandler.Instance.CurrentLevelData.Objective.NPCs;
	List<GameObject> LookAt { get; set; }

	private bool IsActive { get; set; } = false;

	[Broadcast]
	public void OnInitiated()
	{

		if ( IsProxy ) { return; }

		// Don't initiate confusion if objective is find odd.
		if(LevelHandler.Instance.CurrentLevelData.Objective is FindOddObjective) { return; }

		if ( Game.Random.Float( 0f, 1f ) > TriggerChance ) { Destroy(); return; }

		IsActive = true;

		LookAt = new();

		foreach ( NPC npc in NPCs )
		{
			GameObject look = new GameObject( true, $"{npc.GameObject.Name} LookAt temp");
			look.Transform.Position = npc.Transform.Position + (npc.WorldRotation.Forward * 40) + npc.WorldRotation.Up * 64; 
			LookAt.Add( look );
			look.NetworkSpawn();

			npc.LookAt( look.Id );
		}

	}


	NPC npc = null;
	GameObject look = null;
	Transform fwd;
	float focalDst = 128;
	float evenA = 0;
	float oddA = MathF.PI;
	float speed = 3;
	float swing = 24;
	float evenSin;
	float oddSin;
	protected override void OnUpdate()
	{
		if(IsProxy) { return; }
		
		if(!IsActive) { return; }

		evenA += Time.Delta;
		oddA += Time.Delta;
		if(evenA >= MathF.Tau) { evenA -= MathF.Tau; }
		if ( oddA >= MathF.Tau ) { oddA -= MathF.Tau; }

		//float evenSin = MathF.Sin( evenA * speed ) * swing * Math2d.Deg2Rad;
		//float oddSin = MathF.Sin( oddA * speed ) * swing * Math2d.Deg2Rad;


		evenSin = MathX.Clamp( MathF.Sin( evenA * speed ) * 2, -1f, 1f ) * Math2d.Deg2Rad * swing;
		oddSin = MathX.Clamp( MathF.Sin( oddA * speed ) * 2, -1f, 1f ) * Math2d.Deg2Rad * swing;

		for (int i = 0; i < NPCs.Count(); i++ )
		{
			npc = NPCs.ElementAt(i);
			look = LookAt.ElementAt( i );

			fwd = npc.ForwardReference.Value;
			Vector3 dir = Math2d.RotateVector3D( npc.WorldRotation.Forward, Vector3.Up, (i % 2 == 0) ? evenSin : oddSin );


			look.Transform.Position = fwd.Position + dir * focalDst + (Vector3.Up*8);


		}


	}
}
