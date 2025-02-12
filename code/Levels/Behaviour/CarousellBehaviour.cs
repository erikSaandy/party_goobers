using Saandy;
using System;
using System.Numerics;

namespace Sandbox;

public class CarousellBehaviour : Component
{
	float rad = 0;
	float radius = 0;
	float z = 0;

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		Gizmo.Draw.Color = Color.Red;
		float r = Transform.Position.x;
		float z = Transform.Position.z;
		Gizmo.Draw.LineCircle( -Transform.Position.WithZ(0), Vector3.Up, r );

	}

	protected override void OnAwake()
	{
		base.OnAwake();

		radius = Transform.Position.x;
		z = Transform.Position.z;

	}

	protected override void OnUpdate()
	{

		rad += Time.Delta * 0.65f;
		if( rad > MathF.Tau ) { rad -= MathF.Tau; }
		Vector3 dir = new Vector2( MathF.Cos( rad ), MathF.Sin( rad ) );

		Gizmo.Draw.Line( 0, dir * 100 );

		Scene.Camera.Transform.Position = (dir * radius).WithZ( z );

		Scene.Camera.WorldRotation = Scene.WorldRotation.RotateAroundAxis( Vector3.Up, rad * Math2d.Rad2Deg ) * WorldRotation;

		//matrix *= Matrix4x4.CreateTranslation( Vector3.One );
		//Transform.Position += (Vector3)matrix.Translation;
		//Log.Info( (Vector3)matrix.Translation );
	}

}
