using System;
using System.Net;

public static class IntExtension
{
	enum Firsts
	{
		twenty = 2,
		thirty = 3,
		fourty = 4,
		fifty = 5,
		sixty = 6,
		seventy = 7,
		eighty = 8,
		ninety = 9,

	}

	enum Seconds
	{
		ten = 1,
		twenty = 2,
		thirty = 3,
		fourty = 4,
		fifty = 5,
		sixty = 6,
		seventy = 7,
		eighty = 8,
		ninety = 9,

	}

	/// <summary>
	/// Takes a number between 0 and 999, and writes it out in english.
	/// </summary>
	/// <param name="nr">number between 0 and 999</param>
	/// <returns></returns>
	public static string ToEnglish(this int nr)
	{
		int lastDigit = LastDigit( nr );

		string result = "";
		if (nr == 0) { return "zero"; }

		if( nr.ToString().Length == 3)
		{
			result += GetFirst( FirstDigit( nr ) ) + "-hundred";

			if( nr % 100 == 0 ) { return result; }

			result += " and ";

		}

		if(nr.ToString().Length >= 2) {

			int twoLast = (nr > 99) ? (nr % 100) : nr;
			int second = ( twoLast - ( twoLast % 10) ) / 10 ;

			if(second != 0)
			{
				if ( twoLast < 20 && twoLast > 9 )
				{
					return result + GetTeens( twoLast );
				}

				result += ((Seconds)second).ToString() + (lastDigit == 0 ? "" : "-");
			}

		}

		return result + GetFirst( lastDigit );

		string GetFirst(int j)
		{
			switch ( j )
			{
				case 1:
					return "one";
				case 2:
					return "two";
				case 3:
					return "three";
				case 4:
					return "four";
				case 5:
					return "five";
				case 6:
					return "six";
				case 7:
					return "seven";
				case 8:
					return "eight";
				case 9:
					return "nine";
				default: 
					return "";
			}


		}

		string GetTeens(int j)
		{
			switch ( j )
			{
				case 10: return "ten";
				case 11: return "eleven";
				case 12: return "twelve";
				case 13: return "thirteen";
				case 14: return "fourteen";
				case 15: return "fifteen";
				case 16: return "sixteen";
				case 17: return "seventeen";
				case 18: return "eighteen";
				case 19: return "nineteen";
				default: return "";

			}

		}

	}

	public static int FirstDigit( this int i )
	{

		while ( i > 9 )
		{
			i /= 10;
		}
		return i;
	}

	public static int LastDigit(this int i)
	{
		i %= 100;
		i %= 10;
		return i;
	}

}
