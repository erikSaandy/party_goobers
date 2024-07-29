
using Microsoft.VisualBasic;
using System;
using System.Text.Json.Serialization;

public abstract class FaceFeature : Component
{

	[JsonIgnore] public Face Owner { get; private set; }

	[JsonIgnore] public abstract Vector2 BaseOffset { get; }

	public FaceFeatureData Data { get; set; } 

	[Broadcast]
	public void SetTextureID(int value)
	{
		Data.ID = value % TextureCollection.Count;
		UpdateRenderer();
	}

	public void SetFeatureData(FaceFeatureData data)
	{
		this.Data.Offset = data.Offset;
		SetTextureID(data.ID);
	}

 	[JsonIgnore] public abstract List<Texture> TextureCollection { get; }
	[JsonIgnore] public Texture WantedTexture => TextureCollection[Data.ID];

	[JsonIgnore] public bool IsSpawned { get; private set; } = false;

	[JsonIgnore] public SpriteRenderer Renderer { get; private set; }

	[JsonIgnore] public abstract float ZDepth { get; }

	public FaceFeature( ) { }

	protected override void OnAwake()
	{
		base.OnAwake();

		Owner = Components.GetInParentOrSelf<Face>();

		Data = new();

		GameObject.Name = GetType().Name;
		GameObject.Parent = Owner.GameObject;
		GameObject.Transform.LocalPosition = 0;
		Renderer = GameObject.Components.GetOrCreate<SpriteRenderer>();
		Renderer.Size = Face.SIZE;
		Renderer.Opaque = true;

		UpdatePosition();

		if (IsProxy) { return; }

		SetTextureID( TextureCollection.GetRandomId() );
		Data.Offset = 0;
	}

	private void UpdateRenderer()
	{

		Renderer.Texture = WantedTexture;

		UpdatePosition();

		Owner.Transform.Parent.Transform.Position = new Vector3( Game.Random.Float( -400, 400 ), Game.Random.Float( -400, 400 ) );

	}

	private void UpdatePosition()
	{
		Transform.LocalPosition =
		(Vector3)(BaseOffset + Data.Offset) * Face.SIZE // 2D offset
		+ (Vector3.Up * ZDepth * Face.SIZE); // Z offset
	}

	public void Randomize()
	{
		if(IsProxy) { return; }

		Data.Offset = GetRandomOffset();
		SetTextureID( TextureCollection.GetRandomId() );
	}

	public virtual Vector2 GetRandomOffset()
	{
		return new Vector2( Game.Random.Float( -0.2f, 0.2f ), Game.Random.Float( -0.2f, 0.2f ) ) * 0.1f;
	}

	public class FaceFeatureData
	{
		[Property] public Vector2 Offset { get; set; }
		[Property] public int ID { get; set; }

		public FaceFeatureData()
		{
			Offset = 0;
			ID = 0;
		}

	}

}
