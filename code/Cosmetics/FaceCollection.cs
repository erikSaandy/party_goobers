using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

[GameResource("Face Collection", "faceCol", "A Collection of face parts.")]
public class FaceCollection : GameResource
{
	[Property][ResourceType("texture")] public string EyeBlinkTexture { get; set; }
	[Property][ResourceType( "texture" )] public List<string> EyebrowTextures { get; set; }
	[Property][ResourceType( "texture" )] public List<string> EyeTextures { get; set; }
	[Property][ResourceType( "texture" )] public List<string> NoseTextures { get; set; }
	[Property][ResourceType( "texture" )] public List<string> MouthTextures { get; set; }

}
