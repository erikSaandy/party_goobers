using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

[Serializable]
public class Eyes : FaceFeature
{
	[JsonIgnore] public override List<Texture> TextureCollection => FaceData.Instance.EyeTextures;
	[JsonIgnore] public override float ZDepth => 0f;
	[JsonIgnore] public override Vector2 BaseOffset => new Vector2( 0f, 0.2f );

	public Eyes() : base( ) { }

	public async void Blink()
	{
		if(Owner.Owner.Tags.Has("npcdisplay")) { return; }

		Renderer.Texture = FaceData.Instance.EyeBlinkTexture;

		await Task.Delay( 100 );

		Renderer.Texture = FaceData.Instance.EyeTextures[Data.ID];

	}

	[Broadcast]
	public override void SetColor( Color color ) {

		ClientSetColor( color );
	}

	public override void ClientSetColor( Color color )
	{
		if ( Renderer == null ) { return; }

		Renderer.Color = Color.White * 2;
	}

}
