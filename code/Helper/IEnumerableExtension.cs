using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class IEnumerableExtension
{
	public static T GetRandom<T>( this IEnumerable<T> pool )
	{
		int id = GetRandomId( pool );
		return pool.ElementAt( id );
	}

	public static int GetRandomId<T>( this IEnumerable<T> pool )
	{
		if ( pool == null || pool.Count() == 0 ) { Log.Error( $"Pool can not be empty." ); }

		// Only one item in list? select it.
		if ( pool.Count() == 1 ) { return 0; }

		return Game.Random.Next( pool.Count() );

	}

}
