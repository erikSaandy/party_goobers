using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NPC : Component
{
	public Player Owner { get; set; } = null;

	[Property] public SkinnedModelRenderer Renderer { get; set; }

	[Property] public Face Face { get; set; }

	public Color Color { get; private set; }

	[Property] GameObject LookAt { get; set; }

	private Transform? ForwardReference => Renderer.GetAttachment( "forward_reference" );

	public NPC() { }

	protected override void OnStart()
	{
		base.OnStart();

		Color = Color.Random;
		Color = Color.Desaturate( 0.1f );
		Color = Color.Darken( 0.6f );
		Face.SetColor( Color );
		Renderer.Tint = Color;

		LookAt = Scene.Camera.GameObject;

		Renderer.Set( "b_walking", true );

	}

	public void GetPosessedBy(Player owner)
	{
		this.Owner = owner;
		Network.AssignOwnership( owner.Network.OwnerConnection );

		GameObject.Name = $"NPC ({owner.Network.OwnerConnection.DisplayName})";

		Face.Load();

	}

	protected override void OnUpdate()
	{
		if(LookAt != null) {
			LookAtPosition( LookAt.Transform.Position );	
		}

		Vector3 fwd = ForwardReference?.Forward ?? 0;
		Face.Transform.Position = ( ForwardReference?.Position ?? 0 ) + (fwd.Normal * 14f);

	}

	private void LookAtPosition( Vector3 pos )
	{
		Vector3 from = ForwardReference?.Position ?? 0;
		Vector3 dir = (pos - from);
		Renderer.SetLookDirection( "aim_head", dir );

	}

}
