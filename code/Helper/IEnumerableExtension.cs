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

	public static int GetRandomIdWeighted<T>( this IEnumerable<T> pool, bool considerDisabled = false ) where T : IWeighted
	{
		if ( pool == null || pool.Count() == 0 ) { Log.Error( $"weighted pool can not be empty." ); }

		// Only one item in list? select it.
		if ( pool.Count() == 1 ) { return 0; }

		int totalWeight = pool.Sum( x => x.Weight );

		int rnd = Game.Random.Next( totalWeight );

		int i = pool.TakeWhile( x => (rnd -= x.Weight) >= 0 ).Count();

		if ( !pool.ElementAt( i ).Disabled || !considerDisabled )
		{
			return i;
		}

		int n = 0;

		int j = i;
		do
		{
			j++;
			j %= pool.Count();

			if ( !pool.ElementAt( j ).Disabled ) { return i; }

		} while ( n < 10 );

		// Can't find enabled.
		return i;

	}

	public static IEnumerable<T> Shuffle<T>( this IEnumerable<T> source )
	{
		return source.Shuffle( new Random() );
	}

	public static IEnumerable<T> Shuffle<T>( this IEnumerable<T> source, Random rng )
	{
		if ( source == null ) throw new ArgumentNullException( nameof( source ) );
		if ( rng == null ) throw new ArgumentNullException( nameof( rng ) );

		return source.ShuffleIterator( rng );
	}

	private static IEnumerable<T> ShuffleIterator<T>(
		this IEnumerable<T> source, Random rng )
	{
		var buffer = source.ToList();
		for ( int i = 0; i < buffer.Count; i++ )
		{
			int j = rng.Next( i, buffer.Count );
			yield return buffer[j];

			buffer[j] = buffer[i];
		}
	}

}
