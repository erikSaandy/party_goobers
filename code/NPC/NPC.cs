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

	public NPC() { }

	protected override void OnStart()
	{
		base.OnStart();

		Color = Color.Random;
		Face.SetColor( Color );
		Renderer.Tint = Color;

	}

	public void GetPosessedBy(Player owner)
	{
		this.Owner = owner;
		Network.AssignOwnership( owner.Network.OwnerConnection );

		GameObject.Name = $"NPC ({owner.Network.OwnerConnection.DisplayName})";

		Face.Load();

	}

}
