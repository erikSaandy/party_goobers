public static class ICollectionExtension
{
	public static T GetRandomWeighted<T>( this ICollection<T> pool ) where T : IWeighted
	{
		return pool.ElementAt( GetRandomIdWeighted<T>( pool ) );

		//if ( pool == null || pool.Count == 0 ) { Log.Error( $"weighted pool can not be empty." ); }

		//// Only one item in list? select it.
		//if ( pool.Count == 1 ) { return pool.ElementAt( 0 ); }

		//int totalWeight = pool.Sum( x => x.Weight );

		//int rnd = Game.Random.Next( totalWeight );

		//return pool.First( x => (rnd -= x.Weight) < 0 );

	}

	public static int GetRandomIdWeighted<T>( this ICollection<T> pool ) where T : IWeighted
	{
		if ( pool == null || pool.Count == 0 ) { Log.Error( $"weighted pool can not be empty." ); }

		// Only one item in list? select it.
		if ( pool.Count == 1 ) { return 0; }

		int totalWeight = pool.Sum( x => x.Weight );

		int rnd = Game.Random.Next( totalWeight );

		return pool.TakeWhile( x => (rnd -= x.Weight) >= 0 ).Count();

	}

	public static T GetRandom<T>(this ICollection<T> pool)
	{
		int id = GetRandomId( pool );
		return pool.ElementAt( id );
	}

	public static int GetRandomId<T>( this ICollection<T> pool )
	{
		if ( pool == null || pool.Count == 0 ) { Log.Error( $"Pool can not be empty." ); }

		// Only one item in list? select it.
		if ( pool.Count == 1 ) { return 0; }

		return Game.Random.Next( pool.Count );

	}
}
