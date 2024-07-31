﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

[Serializable]
public class Eyes : FaceFeature
{
	[JsonIgnore] public override List<Texture> TextureCollection => FaceData.Instance.EyeTextures;
	[JsonIgnore] public override float ZDepth => 0.01f;
	[JsonIgnore] public override Vector2 BaseOffset => new Vector2( 0f, 0.2f );

	public Eyes() : base( ) { }

}
