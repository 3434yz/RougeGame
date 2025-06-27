using System.Collections.Generic;
using UnityEngine;

public class SpaceGridCountRadius
{
    public int Count { get; set; }
    public float Radius { get; set; }

    public SpaceGridCountRadius(int count, float radius)
    {
        Radius = radius;
        Count = count;
    }
}

public class SpaceGridRingDiffuseData
{
    public float CellSize;
    public List<SpaceGridCountRadius> Lens = new();
    public List<XYi> Idxes = new();

    public void Init(int gridNumRows, float cellSize)
    {
        CellSize = cellSize;
        Lens.Add(new SpaceGridCountRadius(0, 0f));
        Idxes.Add(new XYi(0, 0));

        HashSet<ulong> set = new();
        set.Add(0);

        for (var radius = cellSize; radius < cellSize * gridNumRows; radius += cellSize)
        {
            var lenBak = Idxes.Count;
            var radians = Mathf.Asin(0.5f / radius) * 2;
            var step = (int)(Mathf.PI * 2 / radians);
            var inc = Mathf.PI * 2 / step;
            for (var i = 0; i < step; i++)
            {
                var a = inc * i;
                var cos = Mathf.Cos(a);
                var sin = Mathf.Sin(a);
                var ix = (int)(cos * radius / cellSize);
                var iy = (int)(sin * radius / cellSize);
                var key = ((ulong)(uint)iy << 32) + (uint)ix;
                if (set.Add(key))
                    Idxes.Add(new XYi(ix, iy));
            }

            if (Idxes.Count > lenBak)
                Lens.Add(new SpaceGridCountRadius(lenBak, radius));
        }
    }
}

public class IntSpaceGridRingDiffuseData
{
    public int CellSize;
    public List<SpaceGridCountRadius> Lens = new();
    public List<XYi> Idxes = new();

    public void Init(int gridNumRows, int cellSize)
    {
        CellSize = cellSize;
        Lens.Add(new SpaceGridCountRadius(0, 0f));
        Idxes.Add(new XYi(0, 0));

        HashSet<ulong> set = new();
        set.Add(0);

        for (var radius = (float)CellSize; radius < CellSize * gridNumRows; radius += CellSize)
        {
            var lenBak = Idxes.Count;
            var radians = Mathf.Asin(0.5f / radius) * 2;
            var step = (int)(Mathf.PI * 2 / radians);
            var inc = Mathf.PI * 2 / step;
            for (var i = 0; i < step; i++)
            {
                var a = inc * i;
                var cos = Mathf.Cos(a);
                var sin = Mathf.Sin(a);
                var ix = (int)(cos * radius / CellSize);
                var iy = (int)(sin * radius / CellSize);
                var key = ((ulong)(uint)iy << 32) + (uint)ix;
                if (set.Add(key))
                    Idxes.Add(new XYi(ix, iy));
            }

            if (Idxes.Count > lenBak)
                Lens.Add(new SpaceGridCountRadius(lenBak, radius));
        }
    }
}