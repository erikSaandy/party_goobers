using Saandy;
using System;
using System.Numerics;

namespace Sandbox;

public class StarParticleController : ParticleController
{
	[Property] public Curve RadiusOverTime { get; set; }
	[Property] public float BaseRadius { get; set; } = 16;

	//int ParticleCount => ParticleEffect.Particles.Count;

	protected override void OnParticleStep( Particle particle, float delta )
	{

		//i = ParticleEffect.Particles.TakeWhile( x => x != particle ).Count();

		//float rad = ( (MathF.Tau / ParticleCount) * i ) + particle.Age;

		//Vector2 dir = new Vector2( MathF.Sin( rad ), MathF.Cos( rad ) );
		//float radius = 24 * RadiusOverTime.Evaluate( particle.LifeDelta );
		//particle.Position = Transform.Position + (Vector3)(dir * radius);

		// //

		Vector2 dir = Transform.World.PointToLocal( particle.Position );
		dir = Math2d.RotateByAngle( dir, Time.Delta * 120 );

		float radius = BaseRadius * RadiusOverTime.Evaluate( particle.LifeDelta );

		particle.Position = Transform.Local.PointToWorld( (Vector3)(dir * radius) );

	}

	int i = 0;
	protected override void OnParticleCreated( Particle particle )
	{
		base.OnParticleCreated( particle );

		i++;
		i %= 10;

		particle.Size = .3f;
		if(i % 2 == 0) { particle.Size *= 2f; }

		float rad = ((MathF.Tau / 10) * i);

		Vector2 dir = new Vector2( MathF.Sin( rad ), MathF.Cos( rad ) );
		particle.Position = Transform.Local.PointToWorld( (Vector3)(dir * BaseRadius) );
		//particle.Position = 0;
		particle.Velocity = 0;

	}

}
