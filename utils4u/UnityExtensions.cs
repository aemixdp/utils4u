using UnityEngine;

public static class UnityExtensions
{
    public static Vector2 Rotate(this Vector2 vec, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        float tx = vec.x;
        float ty = vec.y;
        return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
    }

    public static GameObject ChildWithTag(this GameObject parent, string tag)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.tag == tag)
            {
                return child.gameObject;
            }
        }
        return null;
    }
}
