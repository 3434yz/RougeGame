using UnityEngine;

public struct XYi
{
    public int X, Y;

    public XYi(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static XYi operator /(XYi a, int b) => new XYi(a.X / b, a.Y / b);

    public static XYi operator /(XYi a, float b)
    {
        var x = a.X / b;
        var y = a.Y / b;
        return new XYi((int)x, (int)y);
    }

    public static XYi operator *(XYi a, int b) => new XYi(a.X * b, a.Y * b);
}


public static class Physics
{
    public static bool IsAABBIntersect(Vector2 aPos, Vector2 aSize, Vector2 bPos, Vector2 bSize)
    {
        if (bPos.x >= aPos.x) // b right of a
        {
            if (bPos.y <= aPos.y) // b down of a
            {
                if (bPos.x < aPos.x + aSize.x)
                    return bPos.y > aPos.y - aSize.y;
            }
            else // b up of a
            {
                if (bPos.x < aPos.x + aSize.x)
                    return bPos.y - bSize.y < aPos.y;
            }
        }
        else // b left of a 
        {
            if (bPos.y <= aPos.y) // down of a
            {
                if (bPos.x + bSize.x > aPos.x)
                    return bPos.y > aPos.y - aSize.y;
            }
            else // b up of a
            {
                if (bPos.x + bSize.x > aPos.x)
                    return bPos.y - bSize.y < aPos.y;
            }
        }

        return false;
    }
}