/*
 * A class to hold generic vetor scripts to clean up some of the vector code
 * created 11-4-19
 * version: .02
 * Author William Jones
 */

using UnityEngine;

public class VectorUtilities
{
    /// <summary>
    /// Rounds all 3 components of a vector
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3 VectorRound(Vector3 vec)
    {
        return new Vector3(Mathf.Round(vec.x), Mathf.Round(vec.y), Mathf.Round(vec.z));
    }
    /// <summary>
    /// Floors all 3 components of a vector3
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3 VectorFloor(Vector3 vec)
    {
        return new Vector3(Mathf.Floor(vec.x), Mathf.Floor(vec.y), Mathf.Floor(vec.z));
    }

    /// <summary>
    /// Adds the given float to all 3 components of a vector3
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="add"></param>
    /// <returns></returns>
    public static Vector3 VectorAdd(Vector3 vec, float add)
    {
        return new Vector3(vec.x + add, vec.y + add, vec.z + add);
    }

    /// <summary>
    /// Clamps all 3 components of a vector3
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static Vector3 VectorClamp(Vector3 vec, float min, float max)
    {
        return new Vector3(
            Mathf.Clamp(vec.x,min,max),
            Mathf.Clamp(vec.y,min,max),
            Mathf.Clamp(vec.z,min,max));
    }

    /// <summary>
    /// Returns the maximum value from within a vector
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static float VectorMax(Vector3 vec)
    {
        return Mathf.Max(vec.x, vec.y, vec.z);
    }

    /// <summary>
    /// Returns the minimum value from within a vector
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static float VectorMin(Vector3 vec)
    {
        return Mathf.Min(vec.x, vec.y, vec.z);
    }

    /// <summary>
    /// Returns a vector with all positive components
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3 Abs(Vector3 vec)
    {
        vec.x = Mathf.Abs(vec.x);
        vec.y = Mathf.Abs(vec.y);
        vec.z = Mathf.Abs(vec.z);
        return vec;
    }

    /// <summary>
    /// Returns a vector composed of the sign of each component
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3 Sign(Vector3 vec)
    {
        return new Vector3(Mathf.Sign(vec.x), Mathf.Sign(vec.y), Mathf.Sign(vec.z));
    }

    /// <summary>
    /// Smooths out the vector by converging all values to the smallest value
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="smoothness">1 = all values are the same.. 0 = nothing changes</param>
    /// <returns></returns>
    public static Vector3 VectorSmooth(Vector3 vec, float smoothness)
    {
        Vector3 smoothedVector = vec;
        Vector3 signVector = VectorUtilities.Sign(vec);
        smoothedVector = VectorUtilities.Abs(smoothedVector);

        float minValue = VectorUtilities.VectorMin(smoothedVector);
        smoothedVector.x -= (smoothedVector.x - minValue) * .5f;
        smoothedVector.y -= (smoothedVector.y - minValue) * .5f;
        smoothedVector.z -= (smoothedVector.z - minValue) * .5f;

        smoothedVector.x = signVector.x * smoothedVector.x;
        smoothedVector.y = signVector.y * smoothedVector.y;
        smoothedVector.z = signVector.z * smoothedVector.z;

        return Vector3.Lerp(vec,smoothedVector,smoothness);
    }

    /// <summary>
    /// Multiplies the corresponding components in each vector
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Vector3 Multiply(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
}
