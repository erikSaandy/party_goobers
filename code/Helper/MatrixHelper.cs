using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

public static class MatrixHelper
{
	static Vector3 res = 0;

	// Transforms a direction by this matrix.
	public static Vector3 MultiplyVector( this Matrix4x4 matrix, Vector3 vector )
	{
		res = 0;
		res.x = matrix.M11 * vector.x + matrix.M12 * vector.y + matrix.M13 * vector.z;
		res.y = matrix.M21 * vector.x + matrix.M22 * vector.y + matrix.M23 * vector.z;
		res.z = matrix.M31 * vector.x + matrix.M32 * vector.y + matrix.M33 * vector.z;
		return res;
	}

	// Transforms a position by this matrix, with a perspective divide. (generic)
	public static Vector3 MultiplyPoint( this Matrix4x4 matrix, Vector3 point )
	{
		res = 0;

		float w;
		res.x = matrix.M11 * point.x + matrix.M12 * point.y + matrix.M13 * point.z + matrix.M14;
		res.y = matrix.M21 * point.x + matrix.M22 * point.y + matrix.M23 * point.z + matrix.M24;
		res.z = matrix.M31 * point.x + matrix.M32 * point.y + matrix.M33 * point.z + matrix.M34;
		w = matrix.M41 * point.x + matrix.M42 * point.y + matrix.M43 * point.z + matrix.M44;

		w = 1F / w;
		res.x *= w;
		res.y *= w;
		res.z *= w;
		return res;
	}


}
