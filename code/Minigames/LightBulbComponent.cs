using Sandbox;

public class LightBulbComponent : Component
{

	[Property][RequireComponent] public SkinnedModelRenderer Renderer { get; private set; }

	[Property] public bool DefaultIsOff { get; set; } = false;
	[Property] public bool StartOn { get; set; } = false;

	[Property] public Light LightSource { get; private set; }

	private string MaterialGroupDefault => Renderer.Model.GetMaterialGroupName( 0 );
	private string MaterialGroupOther => Renderer.Model.GetMaterialGroupName( 1 );

	private string MaterialGroupOn => DefaultIsOff ? MaterialGroupOther : MaterialGroupDefault;
	private string MaterialGroupOff => DefaultIsOff ? MaterialGroupDefault : MaterialGroupOther;

	public bool IsOn => Renderer.MaterialGroup == ( DefaultIsOff ? MaterialGroupOther : MaterialGroupDefault );

	[Property] public float? BlinkInterval { get; set; } = 1f;
	[Group( "Blinking" )] private TimeSince TimeSinceBlink { get; set; } = 0;

	protected override void OnAwake()
	{

		if(StartOn)
		{
			TurnOn();
		}
		else
		{
			TurnOff();
		}

	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if(!BlinkInterval.HasValue) { return; }

		if(TimeSinceBlink >= BlinkInterval)
		{
			TimeSinceBlink = 0;
			Toggle();
		}

	}

	public void TurnOn()
	{
		Renderer.MaterialGroup = MaterialGroupOn;

		if(LightSource != null)
		{
			LightSource.Enabled = true;
		}

	}

	public void TurnOff()
	{
		Renderer.MaterialGroup = MaterialGroupOff;

		if ( LightSource != null )
		{
			LightSource.Enabled = true;
		}

	}

	public void Toggle()
	{
		if(IsOn) { TurnOff(); }
		else { TurnOn(); }
	}

}
