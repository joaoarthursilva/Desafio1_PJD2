using UnityEngine;

public static class Extensions
{
    public static float[] ToFloatArray(this Vector3 vector)
    {
        var result = new float[3];
        result[0] = vector.x;
        result[1] = vector.y;
        result[2] = vector.z;

        return result;
    }

    public static Vector3 ToVector3(this float[] floatArray)
    {
        var result = new Vector3
        {
            x = floatArray[0],
            y = floatArray[1],
            z = floatArray[2]
        };

        return result;
    }
}