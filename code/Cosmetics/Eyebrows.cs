using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

[Serializable]
public class Eyebrows : FaceFeature
{
	[JsonIgnore] public override List<Texture> TextureCollection => FaceData.Instance.EyebrowTextures;
	[JsonIgnore] public override float ZDepth => .25f;
	[JsonIgnore] public override Vector2 BaseOffset => new Vector2( 0f, 0.5f );

	public Eyebrows( ) : base() { }

}
