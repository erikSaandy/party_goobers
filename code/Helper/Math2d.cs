using System.Collections;
using System.Collections.Generic;
using Sandbox;
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Saandy
{

	public static class Math2d
	{

		public const float kEpsilonNormalSqrt = 1e-15F;
		public const float PI = 3.14159265358979f;

		public const float Deg2Rad = PI / 180f;
		public const float Rad2Deg = 180f / PI;

		// A representation of positive infinity (RO).
		public const float Infinity = Single.PositiveInfinity;

		public static int Sign( float value ) { return value >= 0.0f ? 1 : -1; }
		public static bool SameSign( float a, float b ) { return a * b >= 0.0f; }

		#region v3

		public static float SqrMagnitude( Vector3 vector ) { return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z; }

		// The smaller of the two possible angles between the two vectors is returned, therefore the result will never be greater than 180 degrees or smaller than -180 degrees.
		// If you imagine the from and to vectors as lines on a piece of paper, both originating from the same point, then the /axis/ vector would point up out of the paper.
		// The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction.
		public static float SignedAngle( Vector3 from, Vector3 to, Vector3 axis )
		{
			float unsignedAngle = Angle( from, to );

			float cross_x = from.y * to.z - from.z * to.y;
			float cross_y = from.z * to.x - from.x * to.z;
			float cross_z = from.x * to.y - from.y * to.x;
			float sign = Math.Sign( axis.x * cross_x + axis.y * cross_y + axis.z * cross_z );
			return unsignedAngle * sign;
		}

		public static float SignedAngle( Vector2 from, Vector2 to )
		{
			float unsigned_angle = Angle( from, to );
			float sign = MathF.Sign( from.x * to.y - from.y * to.x );
			return unsigned_angle * sign;
		}

		public static float Angle( Vector3 from, Vector3 to )
		{
			// sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
			float denominator = (float)Math.Sqrt( from.LengthSquared * to.LengthSquared );
			if ( denominator < kEpsilonNormalSqrt )
				return 0F;

			float dot = MathX.Clamp( Dot( from, to ) / denominator, -1F, 1F );
			return ((float)Math.Acos( dot )) * Rad2Deg;
		}

		public static float Dot( Vector3 lhs, Vector3 rhs ) { return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z; }
		#endregion

		public static float SolveVerticalLaunchSpeed( float intendedApexHeight, float gravity = 9.81f )
		{
			return (float)Math.Sqrt( 2f * gravity * intendedApexHeight );
		}

		public static Vector2 RotateByAngle( Vector2 vector, float angle )
		{
			float a = (float)Math.Atan2( vector.y, vector.x );
			a -= angle * Deg2Rad;
			return new Vector2( (float)Math.Cos( a ), (float)Math.Sin( a ) );
		}

		public static float Angle( Vector2 vector )
		{
			float a = (float)Math.Atan2( vector.y, vector.x );
			a = (180 * a) / PI - 90f; // deg
			return (360 - (float)Math.Round( a )) % 360;
		}

		public static float Angle3D( Vector3 v1, Vector3 v2, Vector3 up )
		{
			var cross = Vector3.Cross( v1, v2 );
			var dot = Vector3.Dot( v1, v2 );
			var angle = Math.Atan2( cross.Length, dot );

			var test = Vector3.Dot( up, cross );
			if ( test < 0.0 ) angle = -angle;
			return (float)angle * Rad2Deg;
		}

		public static Vector3 RotateVector3D( Vector3 vector, Vector3 axis, float radians )
		{
			Vector3 vxp = Vector3.Cross( axis, vector );
			Vector3 vxvxp = Vector3.Cross( axis, vxp );
			return vector + MathF.Sin( radians ) * vxp + (1 - MathF.Cos( radians )) * vxvxp;
		}


		/// <summary>
		/// Calculate roll (x) and pitch (y) between two vectors, and return the vector.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns></returns>
		public static Vector3 Angle3D( Vector3 v1, Vector3 v2 )
		{
			return Angle3D( v1, v2, out float yaw, out float pitch );
		}

		/// <summary>
		/// Calculate roll (x) and pitch (y) between two vectors, and return the vector.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="yaw"></param>
		/// <param name="pitch"></param>
		/// <returns></returns>
		public static Vector3 Angle3D( Vector3 v1, Vector3 v2, out float yaw, out float pitch )
		{
			float dX = v1.x - v2.x;
			float dY = v1.y - v2.y;
			float dZ = v1.z - v2.z;

			//radians
			yaw = MathF.Atan2( dZ, dX );
			pitch = MathF.Atan2( MathF.Sqrt( (dZ * dZ) + (dX * dX) ), dY ) + MathF.PI;

			float X = MathF.Sin( pitch ) * MathF.Cos( yaw );
			float Y = MathF.Sin( pitch ) * MathF.Sin( yaw );
			float Z = MathF.Cos( pitch );

			return new Vector3( X, Y, Z );

		}

		/// <summary>
		/// Get x and y coordinates from index.
		/// </summary>
		public static void FlattenedArrayIndex( int i, int w, out int x, out int y )
		{
			y = i / w;
			x = i % w;
		}

		/// <summary>
		/// 0 based array, width(0,1,2,3) = 4
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public static int ArrayIndex( int x, int y, int width, int height )
		{
			return (y * width) + x;
		}

		public static void DrawCircle( Vector2 center, float r, Color c, float duration = 5f )
		{
			Vector2 dO = new Vector2( 0, 1 );
			Vector2 dC = new Vector2();

			Log.Warning( "CANNOT DRAW CIRCLE. REFACTOR!" );

			float step = 360.0f / 45;
			for ( int i = 0; i <= 45; i++ )
			{
				float a = step * i * Deg2Rad;
				dC = new Vector2( (float)Math.Sin( a ), (float)Math.Cos( a ) );

				//DebugOverlay.Line(center + (dO * r), center + (dC * r), c, duration);
				dO = dC;
			}
		}

		public static void DrawCircleGizmo( Vector2 center, float r )
		{
			Vector2 dO = new Vector2( 0, 1 );
			Vector2 dC = new Vector2();

			float step = 360.0f / 45;
			for ( int i = 0; i <= 45; i++ )
			{
				float a = step * i * Deg2Rad;
				dC = new Vector2( (float)Math.Sin( a ), (float)Math.Cos( a ) );

				Gizmo.Draw.Line( center + (dO * r), center + (dC * r) );
				dO = dC;
			}
		}

		public static void DrawPoint( Vector3 p, Color c, float duration = 5f, float size = 0.025f )
		{
			Log.Warning( "CANNOT DRAW POINT! REFACTOR." );
			BBox b = new BBox( p - (Vector3.One * size) / 2, p + (Vector3.One * size) / 2 );
			//DebugOverlay.Line( new Vector3(b.Mins.x, b.Mins.y, b.Center.z), new Vector3( b.Maxs.x, b.Maxs.y, b.Center.z ), c, duration );
			//DebugOverlay.Line( new Vector3(b.Mins.x, b.Maxs.y, b.Center.z), new Vector3( b.Maxs.x, b.Mins.y, b.Center.z ), c, duration );
		}

		public static bool PointIsOnLine( Vector2 point, Vector2 l_start, Vector2 l_end, float precision = .05f )
		{
			float id = GetLineIntersectionDistance( point, l_start, l_end );

			if ( id <= precision )
				return true;

			return false;
		}

		public static float GetLineIntersectionDistance( Vector2 point, Vector2 l_start, Vector2 l_end )
		{
			Vector2 pointCross = (l_start - l_end).Normal;

			Vector2 cross = (Vector2)Vector3.Cross( pointCross, Vector3.Forward );
			Vector2 l2_start = point - cross;
			Vector2 l2_end = point + cross;

			Vector2 intersectionPoint;
			GetLineSegmentIntersection( l_start, l_end, l2_start, l2_end, out intersectionPoint );

			return new Vector2( point.x - intersectionPoint.x, point.y - intersectionPoint.y ).Length;
		}

		public static bool LineSegmentsIntersection( Line a, Line b )
		{
			return GetLineSegmentIntersection( a.pointA, a.pointB, b.pointA, b.pointB, out Vector2 i );
		}

		public static bool GetLineSegmentIntersection( Line l1, Line l2, out Vector2 intersectionPoint )
		{
			return GetLineSegmentIntersection( l1.pointA, l1.pointB, l2.pointA, l2.pointB, out intersectionPoint );
		}

		public static bool GetLineSegmentIntersection( Vector2 l1A, Vector2 l1B, Vector2 l2A, Vector2 l2B, out Vector2 intersectionPoint )
		{
			intersectionPoint = Vector2.Zero;

			var d = (l1B.x - l1A.x) * (l2B.y - l2A.y) - (l1B.y - l1A.y) * (l2B.x - l2A.x);

			if ( d == 0.0f )
			{
				return false;
			}

			var u = ((l2A.x - l1A.x) * (l2B.y - l2A.y) - (l2A.y - l1A.y) * (l2B.x - l2A.x)) / d;
			var v = ((l2A.x - l1A.x) * (l1B.y - l1A.y) - (l2A.y - l1A.y) * (l1B.x - l1A.x)) / d;

			if ( u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f )
			{
				return false;
			}

			intersectionPoint.x = l1A.x + u * (l1B.x - l1A.x);
			intersectionPoint.y = l1A.y + u * (l1B.y - l1A.y);

			return true;
		}

		public static int ClampListIndex( int index, int listSize )
		{
			index = ((index % listSize) + listSize) % listSize;

			return index;
		}

		public static int GetGreatestCommonFactor( int a, int b )
		{
			while ( b != 0 )
			{
				int temp = b;
				b = a % b;
				a = temp;
			}
			return a;
		}

		public static int GetLeastCommonMultiple( int a, int b )
		{
			return (a / GetGreatestCommonFactor( a, b )) * b;
		}

		public static float Clamp01( float value )
		{
			if ( value < 0F )
				return 0F;
			else if ( value > 1F )
				return 1F;
			else
				return value;
		}

		// Compares two floating point values if they are similar.
		public static bool Approximately( float a, float b )
		{
			// If a or b is zero, compare that the other is less or equal to epsilon.
			// If neither a or b are 0, then find an epsilon that is good for
			// comparing numbers at the maximum magnitude of a and b.
			// Floating points have about 7 significant digits, so
			// 1.000001f can be represented while 1.0000001f is rounded to zero,
			// thus we could use an epsilon of 0.000001f for comparing values close to 1.
			// We multiply this epsilon by the biggest magnitude of a and b.
			return MathF.Abs( b - a ) < MathF.Max( 0.000001f * MathF.Max( MathF.Abs( a ), MathF.Abs( b ) ), float.Epsilon * 8 );
		}

		// Euclidian distance between A and B
		public static float Distance( Vector2 A, Vector2 B )
		{
			return (float)Math.Sqrt( (A.x - B.x) * (A.x - B.x) + (A.y - B.y) * (A.y - B.y) );
		}

		public static float LerpAngle( float a, float b, float t )
		{
			float delta = Repeat( (b - a), 360 );
			if ( delta > 180 )
				delta -= 360;

			return a + delta * t;
		}

		/// <summary>
		/// An unclamped inverse lerp.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="v"></param>
		/// <returns></returns>
		public static float InverseLerp( float a, float b, float v )
		{
			return (v - a) / (b - a);
		}

		public static float Lerp( float a, float b, float t )
		{
			return a + (b - a) * t;
		}

		public static Vector3 Lerp( Vector3 a, Vector3 b, float t )
		{
			return a + (b - a) * t;
		}

		public static Vector2 Lerp( Vector2 a, Vector2 b, float t )
		{
			return a + (b - a) * t;
		}

		public static Vector2 QuadraticCurve( Vector2 a, Vector2 b, Vector2 c, float t )
		{
			return QuadraticCurve( (Vector3)a, (Vector3)b, (Vector3)c, t );
		}

		public static Vector3 QuadraticCurve( Vector3 a, Vector3 b, Vector3 c, float t )
		{
			Vector3 p0 = Lerp( a, b, t );
			Vector3 p1 = Lerp( b, c, t );
			return Lerp( p0, p1, t );
		}


		public static Vector2 CubicCurve( Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t )
		{
			Vector2 p0 = QuadraticCurve( a, b, c, t );
			Vector2 p1 = QuadraticCurve( b, c, d, t );
			return Lerp( p0, p1, t );
		}

		public static float Map( float input, float inputMin, float inputMax, float min, float max )
		{
			return min + (input - inputMin) * (max - min) / (inputMax - inputMin);
		}

		#region SmoothDamp


		public static float SmoothDamp( float current, float target, ref float currentVelocity, float smoothTime )
		{
			float deltaTime = Time.Delta;
			float maxSpeed = Infinity;
			return SmoothDamp( current, target, ref currentVelocity, smoothTime, deltaTime, maxSpeed );
		}

		// Gradually changes a value towards a desired goal over time.
		public static float SmoothDamp( float current, float target, ref float currentVelocity, float smoothTime, float deltaTime, float maxSpeed = Infinity )
		{
			// Based on Game Programming Gems 4 Chapter 1.10
			smoothTime = MathF.Max( 0.0001F, smoothTime );
			float omega = 2F / smoothTime;

			float x = omega * deltaTime;
			float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
			float change = current - target;
			float originalTo = target;

			// Clamp maximum speed
			float maxChange = maxSpeed * smoothTime;
			change = Math.Clamp( change, -maxChange, maxChange );
			target = current - change;

			float temp = (currentVelocity + omega * change) * deltaTime;
			currentVelocity = (currentVelocity - omega * temp) * exp;
			float output = target + (change + temp) * exp;

			// Prevent overshooting
			if ( originalTo - current > 0.0F == output > originalTo )
			{
				output = originalTo;
				currentVelocity = (output - originalTo) / deltaTime;
			}

			return output;
		}

		public static float SmoothDampAngle( float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed = Infinity )
		{
			float deltaTime = Time.Delta;
			return SmoothDampAngle( current, target, ref currentVelocity, smoothTime, deltaTime, maxSpeed );
		}

		public static float SmoothDampAngle( float current, float target, ref float currentVelocity, float smoothTime )
		{
			float deltaTime = Time.Delta;
			float maxSpeed = Infinity;
			return SmoothDampAngle( current, target, ref currentVelocity, smoothTime, deltaTime, maxSpeed );
		}

		// Gradually changes an angle given in degrees towards a desired goal angle over time.
		public static float SmoothDampAngle( float current, float target, ref float currentVelocity, float smoothTime, float deltaTime, float maxSpeed = Infinity )
		{
			target = current + DeltaAngle( current, target );
			return SmoothDamp( current, target, ref currentVelocity, smoothTime, deltaTime, maxSpeed );
		}

		// Calculates the shortest difference between two given angles.
		public static float DeltaAngle( float current, float target )
		{
			float delta = Math2d.Repeat( (target - current), 360.0F );
			if ( delta > 180.0F )
				delta -= 360.0F;
			return delta;
		}

		// Loops the value t, so that it is never larger than length and never smaller than 0.
		public static float Repeat( float t, float length )
		{
			return Math.Clamp( t - (float)Math.Floor( t / length ) * length, 0.0f, length );
		}

		#endregion


		/// <summary>
		/// Returns the closest point on the line pA,pB to other, expressed as a percentage along the line direction.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public static float ClosestPointPercentageAlongLine( Vector3 pA, Vector3 pB, Vector3 other )
		{
			Vector3 line = (pB - pA);
			return MathX.Clamp( Vector3.Dot( other - pA, line.Normal ) / line.Length, 0f, 1f );
		}

		/// <summary>
		/// Get the closest point on an infinite line defined by (pA, pB), to point other.
		/// https://stackoverflow.com/questions/27733634/closest-point-on-a-3-dimensional-line-to-another-point
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public static Vector3 ClosestPointOnLine( Vector3 pA, Vector3 pB, Vector3 other )
		{
			Vector3 dir = (pB - pA).Normal;
			return pA + (dir * Vector3.Dot( other - pA, dir ));
		}

		/// <summary>
		/// Get the closest point on a line segment (finite line) between (pA, pB), to point other.
		/// </summary>
		/// <param name="pA"></param>
		/// <param name="pB"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static Vector3 ClosestPointOnLineSegment( Vector3 pA, Vector3 pB, Vector3 other )
		{
			Vector3 line = (pB - pA);
			float along = MathX.Clamp( Vector3.Dot( other - pA, line.Normal ), 0, line.Length );
			return pA + line.Normal * along;
		}

		public class Line
		{

			public float ax { get; set; }
			public float ay { get; set; }
			public float bx { get; set; }
			public float by { get; set; }

			[NonSerialized][JsonIgnore] private Vector2 tempA = Vector2.Zero;
			[NonSerialized][JsonIgnore] private Vector2 tempB = Vector2.Zero;
			[JsonIgnore] public Vector2 pointA { get { return new Vector2( ax, ay ); } set { ax = value.x; ay = value.y; } }
			[JsonIgnore] public Vector2 pointB { get { return new Vector2( bx, by ); } set { bx = value.x; by = value.y; } }

			[JsonIgnore] public Vector2 Right => Vector3.Cross( Direction, Vector3.Up ).Normal;
			[JsonIgnore] public Vector2 Left => -Right;

			public Line()
			{
				ax = ay = bx = by = 0;
			}

			public Line( Vector2 a, Vector2 b )
			{
				ax = a.x;
				ay = a.y;
				bx = b.x;
				by = b.y;
			}

			/// <summary>
			/// Outline direction on specified side.
			/// </summary>
			[JsonIgnore] public Vector2 Direction { get { return GetDir(); } }
			Vector2 GetDir() { return (pointB - pointA).Normal; }

			[JsonIgnore] public float Magnitude { get { return GetMagnitude(); } }
			float GetMagnitude()
			{
				return (pointB - pointA).Length;
			}
			public Line Shrink( float distance = 1 )
			{
				Vector2 p1 = pointA + Direction * distance;
				Vector2 p2 = pointB - Direction * distance;

				return new Line( p1, p2 );
			}

			/// <summary>
			/// Do these lines overlap, independant of line direction?
			/// </summary>
			/// <param name="line"></param>
			/// <returns></returns>
			public bool OverlapsWith( Math2d.Line line )
			{
				for ( int i = 0; i < 2; i++ )
				{
					if ( line.pointA == pointA && line.pointB == pointB ) return true;
					line.Flip();
				}

				return false;
			}

			public Vector2 Middle()
			{
				return pointA + (Magnitude * Direction * 0.5f);
			}

			public void Flip()
			{
				Vector2 a = pointA;
				pointA = pointB;
				pointB = a;
			}

			/// <summary>
			/// Untested. https://stackoverflow.com/questions/27733634/closest-point-on-a-3-dimensional-line-to-another-point
			/// </summary>
			/// <param name="point"></param>
			/// <returns></returns>
			public Vector2 ClosestPointOnLine( Vector2 point )
			{
				return pointA + (Direction * Vector2.Dot( point - pointA, Direction ));
			}


			//

			public void Draw( Color c = default( Color ), float duration = 0 )
			{
				Gizmo.Draw.Color = c;
				Gizmo.Draw.Line( pointA, pointB );
			}

			public bool Equals( Line other )
			{
				if ( pointA == other.pointA && pointB == other.pointB ||
				   pointA == other.pointB && pointB == other.pointA )
				{
					return true;
				}

				return false;
			}

		}
	}

}
