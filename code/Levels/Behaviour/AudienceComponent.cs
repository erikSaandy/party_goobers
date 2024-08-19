using Saandy;

public sealed class AudienceComponent : Component
{

	[Property] public ModelRenderer Renderer { get; set; }
	[Property] public Material CheerMaterial { get; set; }

	float basePosZ = 0;


	float jumpTimer = 0;
	float velocity = 0;
	float startScale = 1;

	bool grounded = false;

	protected override void OnStart()
	{
		base.OnStart();

		basePosZ = GameObject.Transform.Position.z;
		startScale = Transform.Scale.x;

		LevelObjective.OnCompletedObjective += Cheer;

	}

	float jumpTime = 1;
	protected override void OnUpdate()
	{

		jumpTimer += Time.Delta;

		if( grounded && jumpTimer >= jumpTime )
		{
			// jump
			jumpTimer = 0;
			velocity += 1;
			Transform.Scale = 1.02f * startScale;
		}
			
		velocity -= Time.Delta * 8f;

		if(velocity <= 0 && Transform.Position.z < basePosZ)
		{
			Transform.Position = Transform.Position.WithZ( basePosZ ) + 0.0000001f;
			velocity = 0;
			grounded = true;
		}

		Transform.Position = Transform.Position.WithZ( Transform.Position.z + velocity );


		if ( Transform.Scale.x > 1f )
		{
			Transform.Scale = Math2d.Lerp( Transform.Scale.x, startScale, Time.Delta * 4 );
		}

	}

	private void Cheer()
	{
		Renderer.MaterialOverride = CheerMaterial;
		jumpTime = 0.3f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		LevelObjective.OnCompletedObjective -= Cheer;

	}

}
