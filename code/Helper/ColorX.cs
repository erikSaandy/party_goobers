using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

public static class ColorX
{

	public static Color[] MiiColors { get; set; } = new Color[99]
	{

			ColorX.FromHex("631D09"),ColorX.FromHex("8C3124"),ColorX.FromHex("6C3F2C"),ColorX.FromHex("905641"),ColorX.FromHex("B61D06"),
			ColorX.FromHex("FD2105"),ColorX.FromHex("FE5946"),ColorX.FromHex("FF8266"),ColorX.FromHex("FFAFA6"),ColorX.FromHex("FFC7BA"),

			ColorX.FromHex("7A373C"),ColorX.FromHex("A12D3D"),ColorX.FromHex("96243F"),ColorX.FromHex("C14944"),ColorX.FromHex("D63357"),
			ColorX.FromHex("B95F82"),ColorX.FromHex("D25E6A"),ColorX.FromHex("FF8298"),ColorX.FromHex("FEB5C9"),ColorX.FromHex("FFD0DC"),

			ColorX.FromHex("342044"),ColorX.FromHex("38293C"),ColorX.FromHex("50204D"),ColorX.FromHex("714EB7"),ColorX.FromHex("8966BD"),
			ColorX.FromHex("C68ECF"),ColorX.FromHex("A998CB"),ColorX.FromHex("C7B2EB"),ColorX.FromHex("EFC7FB"),ColorX.FromHex("D2C7F0"),

			ColorX.FromHex("17203F"),ColorX.FromHex("1D326B"),ColorX.FromHex("0B3D68"),ColorX.FromHex("4459AD"),ColorX.FromHex("2186DF"),
			ColorX.FromHex("4DAFF4"),ColorX.FromHex("74BFDF"),ColorX.FromHex("86A8FC"),ColorX.FromHex("7DBBFC"),ColorX.FromHex("9BDFFF"),
						
			ColorX.FromHex("052B33"),ColorX.FromHex("00383E"),ColorX.FromHex("064A5A"),ColorX.FromHex("1D5F62"),ColorX.FromHex("336A59"),
			ColorX.FromHex("2A768E"),ColorX.FromHex("48A5B1"),ColorX.FromHex("75BC9D"),ColorX.FromHex("76CAC0"),ColorX.FromHex("7FDAB4"),

			ColorX.FromHex("064436"),ColorX.FromHex("426D01"),ColorX.FromHex("006A64"),ColorX.FromHex("2F8F71"),ColorX.FromHex("449C15"),
			ColorX.FromHex("92B103"),ColorX.FromHex("5BB884"),ColorX.FromHex("9DD03F"),ColorX.FromHex("91D27A"),ColorX.FromHex("B8E9A6"),

			ColorX.FromHex("4F3E0F"),ColorX.FromHex("5F5B2D"),ColorX.FromHex("9B8E27"),ColorX.FromHex("AA9561"),ColorX.FromHex("D0B735"),
			ColorX.FromHex("CFB885"),ColorX.FromHex("DAC87E"),ColorX.FromHex("D8D26B"),ColorX.FromHex("D5E07F"),ColorX.FromHex("D8F199"),

			ColorX.FromHex("64390F"),ColorX.FromHex("824602"),ColorX.FromHex("8E5A17"),ColorX.FromHex("B06001"),ColorX.FromHex("D79F45"),
			ColorX.FromHex("ECBC78"),ColorX.FromHex("FFDC44"),ColorX.FromHex("FEDB7C"),ColorX.FromHex("F9E89A"),ColorX.FromHex("FEF39A"),

			ColorX.FromHex("432212"),ColorX.FromHex("803E12"),ColorX.FromHex("AF5219"),ColorX.FromHex("E85B04"),ColorX.FromHex("FF9805"),
			ColorX.FromHex("D69D65"),ColorX.FromHex("F9A075"),ColorX.FromHex("FEB562"),ColorX.FromHex("FFC589"),ColorX.FromHex("E8CFB0"),

			/*ColorX.FromHex("000000")*/ColorX.FromHex("302929"),ColorX.FromHex("414141"),ColorX.FromHex("6B6F73"),ColorX.FromHex("796F67"),
			ColorX.FromHex("767981"),ColorX.FromHex("9B9B9B"),ColorX.FromHex("BEBEBE"),ColorX.FromHex("DFD8CC"),ColorX.FromHex("FFFFFF"),

	};

	public static Color FromHex(string hex)
	{

		if ( hex == null )
		{
			return null;
		}

		if ( hex.Length == 3 )
		{
			string value = hex.Substring( 0, 1 );
			string value2 = hex.Substring( 1, 1 );
			string value3 = hex.Substring( 2, 1 );
			hex = $"{value}{value}{value2}{value2}{value3}{value3}FF";
		}
		else if ( hex.Length == 4 )
		{
			string value4 = hex.Substring( 0, 1 );
			string value5 = hex.Substring( 1, 1 );
			string value6 = hex.Substring( 2, 1 );
			string value7 = hex.Substring( 3, 1 );
			hex = $"{value4}{value4}{value5}{value5}{value6}{value6}{value7}{value7}";
		}
		else if ( hex.Length == 6 ) 
		{
			
			hex = $"{hex}FF";
		
		}

		int.TryParse( hex, NumberStyles.HexNumber, null, out var result );

		byte b = (byte)((uint)(result >> 24) & 0xFFu);
		byte b2 = (byte)((uint)(result >> 16) & 0xFFu);
		byte b3 = (byte)((uint)(result >> 8) & 0xFFu);
		byte b4 = (byte)((uint)result & 0xFFu);

		return Color.FromBytes( b, b2, b3, b4 );

	}


}
