using Saandy;
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

public class NodePathComponent : Component
{
	public enum MoveTypes
	{
		Straight,
		Loop,
	}

	[Property, ToggleGroup( "TargetPointsEnabled", Label = "Target Points" )]
	public bool TargetPointsEnabled { get; set; } = false;
	[Property, ToggleGroup( "TargetPointsEnabled" )] public MoveTypes MoveType { get; set; }
	[Property, ToggleGroup( "TargetPointsEnabled" )] public List<Target> Targets { get; set; }

	public TimeSince TimeSinceUsed { get; set; } = 10;

	private float GetFullPathLength()
	{
		float length = 0;
		for ( int i = 1; i < Targets.Count; i++ )
		{
			length += GetTargetPosition( i ).Length;
		}

		return length;
	}

	private Vector3 TargetSegment( int index ) { return (GetTargetPosition( index + 1 ) - GetTargetPosition( index )); }
	private float TargetLength( int index ) { return TargetSegment( index ).Length; }
	private Vector3 TargetForward( int index ) { return TargetSegment( index ).Normal; }

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( !TargetPointsEnabled ) { return; }
		if ( Targets == null || Targets.Count == 0 ) { return; }

		// Path
		for ( int i = 1; i < Targets.Count; i++ )
		{
			Gizmo.Draw.Color = Color.White;
			Gizmo.Draw.Line( ToGizmoSpace( GetTargetPosition( i - 1 ) ), ToGizmoSpace( GetTargetPosition( i ) ) );

			Gizmo.Draw.Color = Color.Yellow;
			Gizmo.Draw.Arrow( ToGizmoSpace( GetTargetPosition( i - 1 ) ), ToGizmoSpace( GetTargetPosition( i ) ), 24, 8 );

		}

		Gizmo.Draw.Color = Color.White;
		Gizmo.Draw.Line( ToGizmoSpace( GetTargetPosition( Targets.Count - 1 ) ), ToGizmoSpace( GetTargetPosition( 0 ) ) );


		Vector3 ToGizmoSpace( Vector3 pos )
		{
			pos -= Transform.Position;
			pos *= 1 / Transform.Scale;

			return pos;
		}

	}

	protected override void OnAwake()
	{
		base.OnAwake();

		SpawnNodeTriggers();

	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

	}

	[Broadcast]
	private void SpawnNodeTriggers()
	{
		foreach ( GameObject ch in GameObject.Children )
		{
			ch.Destroy();
		}

		for ( int i = 0; i < Targets.Count; i++ )
		{

			GameObject g = new GameObject( true, $"node trigger {i.ToString( "00" )}" );
			SphereCollider col = g.Components.Create<SphereCollider>();
			col.Radius = 32;
			col.GameObject.Parent = GameObject;
			col.Transform.Position = GetTargetPosition( i );
			col.IsTrigger = true;
			col.OnTriggerEnter += NPCEnteredNodeTrigger;

		}

	}

	public void NPCEnteredNodeTrigger( Collider collider )
	{
		if ( IsProxy ) { return; }

		NPC npc = collider.Components.Get<NPC>();
		if ( npc == null ) { return; }


		int nextTargetId = GetNextTargetFromPos( npc.Transform.Position );
		npc.SetWantedPosition( GetTargetPosition( nextTargetId ) );


	}

	public Vector3 ClosestPointOnPath(Vector3 fromPos, out Vector3 nextTargetPos )
	{

		int target1Id = GetClosestTarget( fromPos );

		Vector3 toTarget = GetTargetPosition( target1Id ) - fromPos;
		Vector3 dirToTarget = toTarget.Normal;

		float dot = Vector3.Dot( dirToTarget, TargetForward( target1Id ) );


		int target2Id = target1Id + 1;

		if ( dot > 0 )
		{
			int targetTemp = target1Id;
			target1Id = (target1Id - 1);
			target2Id = targetTemp;
		}

		Vector3 point = Math2d.ClosestPointOnLineSegment( GetTargetPosition( target1Id ), GetTargetPosition( target2Id ), fromPos );

		TimeSinceUsed = 0;

		nextTargetPos = GetTargetPosition( target2Id );
		return point;

	}

	public int GetClosestTarget(Vector2 toPos)
	{

		float dst = 0;
		int target = 0;
		for ( int i = 0; i < Targets.Count; i++ )
		{
			Vector3 pos = GetTargetPosition( i );

			float cdst = Vector3.DistanceBetweenSquared( toPos, pos );

			if ( cdst < dst || i == 0 )
			{
				dst = cdst;
				target = i;
			}

		}

		return target;

	}

	public int GetNextTargetFromPos(Vector2 pos)
	{
		int closest = GetClosestTarget( pos );

		int next = (closest + 1) % (Targets.Count - 1);
		return next;
	}

	public Vector3 GetTargetPosition(int id)
	{

		id = id % Targets.Count;

		if ( id < 0 ) { id += Targets.Count; }
		else if (id == 0) { return GameObject.Transform.Position + Targets[id].Position; }

		return GetTargetPosition(id-1) + Targets[id].Position;
	}

	public struct Target
	{
		[JsonInclude][KeyProperty] public Vector3 Position { get; set; }
		[JsonInclude][KeyProperty][Range(0f, 5f)] public float PauseTime { get; set; }

	}

}
