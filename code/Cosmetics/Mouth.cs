using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class Mouth : FaceFeature
{
	[JsonIgnore] public override List<Texture> TextureCollection => FaceData.Instance.MouthTextures;
	[JsonIgnore] public override float ZDepth => -0.1f;
	[JsonIgnore] public override Vector2 BaseOffset => new Vector2( 0f, -0.15f );

	public Mouth() : base() { }

}
