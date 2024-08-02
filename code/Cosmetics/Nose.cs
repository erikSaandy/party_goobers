using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

[Serializable]
public class Nose : FaceFeature
{
	[JsonIgnore] public override List<Texture> TextureCollection => FaceData.Instance.NoseTextures;
	[JsonIgnore] public override float ZDepth => -.3f;
	[JsonIgnore] public override Vector2 BaseOffset => new Vector2( 0f, 0.1f );
	public Nose() : base() { }

}
