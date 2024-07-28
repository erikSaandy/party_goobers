
using Microsoft.VisualBasic;
using System;
using System.Text.Json.Serialization;

public abstract class FaceFeature : Component
{

	[JsonIgnore] public Face Owner { get; private set; }

	[JsonIgnore] public abstract Vector2 BaseOffset { get; }
	[JsonInclude] public Vector2 Offset { get; set; }

	[Sync][JsonInclude] public int ID { get; private set; }

	[Broadcast]
	public virtual void SetTextureID(int value)
	{
		this.ID = value % TextureCollection.Count;
		UpdateRenderer();
	}

 	[JsonIgnore] public abstract List<Texture> TextureCollection { get; }
	[JsonIgnore] public Texture WantedTexture => TextureCollection[ID];

	[JsonIgnore] public bool IsSpawned { get; private set; } = false;

	[JsonIgnore] public SpriteRenderer Renderer { get; private set; }

	[JsonIgnore] public abstract float ZDepth { get; }

	public FaceFeature( ) { }

	protected override void OnAwake()
	{
		base.OnAwake();

		Owner = Components.GetInParentOrSelf<Face>();

		if(IsProxy) { return; }

		SetTextureID( TextureCollection.GetRandomId() );
		this.Offset = 0;
	}

	[Broadcast]
	public void UpdateRenderer()
	{
		if(IsSpawned)
		{
			Renderer.Texture = WantedTexture;
		}
		else
		{
			GameObject.Name = GetType().Name;
			GameObject.Parent = Owner.GameObject;
			GameObject.Transform.LocalPosition = 0;
			Renderer = GameObject.Components.GetOrCreate<SpriteRenderer>();
			Renderer.Texture = WantedTexture;
			Renderer.Size = Face.SIZE;
			Renderer.Opaque = true;

			IsSpawned = true;
		}

		Renderer.GameObject.Transform.LocalPosition =
			(Vector3)(BaseOffset + Offset) * Face.SIZE // 2D offset
			+ (Vector3.Up * ZDepth * Face.SIZE); // Z offset

		Owner.Transform.Parent.Transform.Position = new Vector3( Game.Random.Float( -400, 400 ), Game.Random.Float( -400, 400 ) );

	}

	public void Randomize()
	{
		if(IsProxy) { return; }

		Offset = GetRandomOffset();
		SetTextureID( TextureCollection.GetRandomId() );
	}

	public virtual Vector2 GetRandomOffset()
	{
		return new Vector2( Game.Random.Float( -0.2f, 0.2f ), Game.Random.Float( -0.2f, 0.2f ) ) * 0.1f;
	}

}
