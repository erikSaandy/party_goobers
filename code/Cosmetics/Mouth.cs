﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

[Serializable]
public class Mouth : FaceFeature
{
	[JsonIgnore] public override List<Texture> TextureCollection => FaceData.Instance.MouthTextures;
	[JsonIgnore] public override float ZDepth => -0.25f;
	[JsonIgnore] public override Vector2 BaseOffset => new Vector2( 0f, -0.15f );

	public Mouth() : base() { }

	[Rpc.Broadcast]
	public override void SetColor( Color color )
	{
		ClientSetColor( color );
	}

	public override void ClientSetColor( Color color )
	{
		if ( !Renderer.IsValid() ) { return; }

		Renderer.Color = Color.White * 2;
	}

}
