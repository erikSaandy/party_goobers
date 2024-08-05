
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

 	[JsonIgnore] public abstract List<Texture> TextureCollection { get; }
	[JsonIgnore] public Texture WantedTexture => TextureCollection[Data.ID];

	[JsonIgnore] public bool IsSpawned { get; private set; } = false;

	[JsonIgnore] public SpriteRenderer Renderer => GameObject.Components.Get<SpriteRenderer>(true);

	[JsonIgnore] public abstract float ZDepth { get; }

	public FaceFeature() { }

	protected override void OnAwake()
	{
		base.OnAwake();

		Owner = Components.GetInParentOrSelf<Face>( true );

		Data = new();

		if ( IsProxy ) { return; }

		GameObject.Name = GetType().Name;
		GameObject.Parent = Owner.GameObject;
		GameObject.Transform.LocalPosition = 0;

		Data.Offset = 0;
	}

	private void UpdateRenderer()
	{
		if(Renderer == null) { return; }

		Renderer.Texture = WantedTexture;
		//Owner.Transform.Parent.Transform.Position = new Vector3( Game.Random.Float( -0, 0 ), Game.Random.Float( -0, 00 ) );
	}

	protected override void OnUpdate()
	{
		// Apply offsets relative to camera.
		Vector3 off = (BaseOffset + Data.Offset);

		//Vector3 dir = Owner.Transform.Position - Owner.Owner.LookAtObject.Transform.Position;

		Rotation rot = Owner.Owner.ForwardReference.Value.Rotation;
		//Rotation rot = Scene.Camera.Transform.Rotation;

		Transform.LocalPosition = 
			Vector3.Up * off.y 
			+ rot.Left * off.x + 
			rot.Backward * ZDepth;
		
	}

	[Broadcast]
	public void SetColor(Color color)
	{
		if ( Renderer == null ) { return; }

		Renderer.Color = color;
	}

	[Authority]
	public void Randomize()
	{
		if(IsProxy) { return; }

		SetOffset( GetRandomOffset() );
		SetTextureID( TextureCollection.GetRandomId() );
	}

	[Authority]
	private void SetOffset( Vector2 offset )
	{
		Data.Offset = offset;
	}

	public virtual Vector2 GetRandomOffset()
	{
		return new Vector2( Game.Random.Float( -0.2f, 0.2f ), Game.Random.Float( -0.2f, 0.2f ) ) * 0.1f;
	}

	[Serializable]
	public class FaceFeatureData
	{
		[Property][JsonInclude] public Vector2 Offset { get; set; }
		[Property][JsonInclude] public int ID { get; set; }

		public FaceFeatureData()
		{
			Offset = 0;
			ID = 0;
		}

	}

}
