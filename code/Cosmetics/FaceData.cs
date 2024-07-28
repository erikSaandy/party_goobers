using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FaceData : SingletonComponent<FaceData>
{

	[Property] public List<Texture> EyebrowTextures { get; set; }
	[Property] public List<Texture> EyeTextures { get; set; }
	[Property] public List<Texture> NoseTextures { get; set; }
	[Property] public List<Texture> MouthTextures { get; set; }

}
