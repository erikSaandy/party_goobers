using Sandbox.Citizen;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NPCFootsteps : Component
{
	[Property] private SkinnedModelRenderer Owner { get; set; }

	private TimeSince TimeSinceLastStep { get; set; }

	protected override void OnEnabled()
	{
		if ( !Owner.IsValid() )
			return;

		Owner.OnFootstepEvent += OnEvent;

		Owner.OnSoundEvent += OnSoundEvent;
	}

	protected override void OnDisabled()
	{
		if ( !Owner.IsValid() )
			return;

		Owner.OnFootstepEvent -= OnEvent;
	}

	private void OnEvent( SceneModel.FootstepEvent e )
	{
		if ( TimeSinceLastStep < 0.2f || FadeScreen.Visible )
			return;

		var trace = Scene.Trace
			.Ray( e.Transform.Position + Vector3.Up * 20f, e.Transform.Position + Vector3.Up * -20f )
			.Run();

		if ( !trace.Hit )
			return;

		if ( trace.Surface is null )
			return;

		TimeSinceLastStep = 0f;

		var sound = e.FootId == 0 ? trace.Surface.Sounds.FootLeft : trace.Surface.Sounds.FootRight;
		if ( sound is null ) return;

		var handle = Sound.Play( sound, trace.HitPosition + trace.Normal * 5f );
		handle.Volume *= e.Volume * 8;

	}

	private void OnSoundEvent( SceneModel.SoundEvent e )
	{
		//Log.Info( e.Name );
		//Sound.Play( "", e.Position );
	}

}
