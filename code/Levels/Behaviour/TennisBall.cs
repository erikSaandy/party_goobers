using Saandy;
using Sandbox;

public class TennisBall : Component
{
	[Property] public GameObject InvertedBall { get; private set; }

	[Property] public GameObject PointA { get; private set; }

	[Property] public GameObject PointB { get; private set; }

	[Property] public GameObject PointC { get; private set; }

	private Vector3 A => PointA.Transform.Position;
	private Vector3 B => PointB.Transform.Position;
	private Vector3 C => PointC.Transform.Position;

	private int Dir = -1;

	private float t = 0;

	private float TurnTime = 2.2f;
	private TimeSince TimeSinceTurn = 0;

	protected override void OnUpdate()
	{
		if(IsProxy) { return; }

		if(FadeScreen.Visible) { return; }

		t += Time.Delta * 1.2f * Dir;
		t = t.Clamp( 0, 1 );

		if( TimeSinceTurn >= TurnTime )
		{
			Turn();
		}

		GameObject.Transform.Position = Math2d.QuadraticCurve( A, C, B, t );
		InvertedBall.Transform.Position = Math2d.QuadraticCurve( A, C, B, 1-t );
	}


	private void Turn()
	{
		TimeSinceTurn = 0;
		Dir *= -1;
		OnBallHit();
	}

	[Broadcast]
	private void OnBallHit()
	{
		Sound.Play( "sounds/tennis_ball_hit.sound" );
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if( PointA == null || PointB == null || PointC == null ) { return; }

		Gizmo.Draw.Color = Color.Yellow;
		Vector3 a = A - Transform.Position;
		Vector3 b = B - Transform.Position;
		Vector3 c = C - Transform.Position;
		Gizmo.Draw.Line( a, b );
		Gizmo.Draw.Line( b, c );
		Gizmo.Draw.Color = Color.Green;
		Gizmo.Draw.LineSphere( a, 16 );
		Gizmo.Draw.Color = Color.Red;
		Gizmo.Draw.LineSphere( b, 16 );
		Gizmo.Draw.Color = Color.Yellow;
		Gizmo.Draw.LineSphere( c, 16 );

		float ti = 0;
		int steps = 10;
		Vector3 pointOld = A - Transform.Position;
		for(int i = 1; i <= steps; i++ )
		{
			ti += 1f / steps;
			Vector3 point = Math2d.QuadraticCurve( A, C, B, ti ) - GameObject.Transform.Position;
			Gizmo.Draw.Line( pointOld, point );
			pointOld = point;
		}


	}

}
