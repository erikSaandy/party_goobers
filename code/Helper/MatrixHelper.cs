using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

public static class MatrixHelper
{
	// Transforms a direction by this matrix.
	public static Vector3 MultiplyVector( this Matrix4x4 matrix, Vector3 vector )
	{
		Vector3 res = 0;
		res.x = matrix.M11 * vector.x + matrix.M12 * vector.y + matrix.M13 * vector.z;
		res.y = matrix.M21 * vector.x + matrix.M22 * vector.y + matrix.M23 * vector.z;
		res.z = matrix.M31 * vector.x + matrix.M32 * vector.y + matrix.M33 * vector.z;
		return res;
	}
}
