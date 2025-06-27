using System;
using System.Collections.Generic;
using UnityEngine;

public interface ISpaceItemFloat<T> where T : ISpaceItemFloat<T>
{
    Vector2 Position { get; }
    int IndexAtItems { get; set; }
    int IndexAtCells { get; set; }
    T? Prev { get; set; }
    T? Next { get; set; }
}


public class SpaceIndexFloat<T> where T : class, ISpaceItemFloat<T>
{
    public Vector2 GridSize;
    public float CellSize;
    public float InvCellSize;
    public int NumRows, NumCols;
    public List<T> Items = new();
    public int CellsLen;
    public T?[] Cells;
    public int[] Counts;
    private readonly bool _enableDoubleLink;
    public SpaceGridRingDiffuseData Rdd;

    public static readonly XYi[] NeighborOffsets = new XYi[]
    {
        new(1, 0), new(1, 1), new(0, 1), new(-1, 1),
        new(-1, 0), new(-1, -1), new(0, -1), new(1, -1)
    };

    public SpaceIndexFloat(bool enableDoubleLink = false)
    {
        _enableDoubleLink = enableDoubleLink;
    }

    public void Init(SpaceGridRingDiffuseData rdd, int numRows, int numCols, float cellSize, int cap = 10000)
    {
        Debug.Assert(Cells == null || Cells.Length == 0);
        Debug.Assert(numCols > 0 && numRows > 0 && cellSize > 0);

        Rdd = rdd;
        NumRows = numRows;
        NumCols = numCols;
        CellSize = cellSize;
        GridSize = new Vector2(cellSize * numCols, cellSize * numRows);
        InvCellSize = 1f / cellSize;

        CellsLen = numRows * numCols;
        Cells = new T[CellsLen];
        Counts = new int[CellsLen];

        Items.Capacity = cap;
    }

    public T Add(T c)
    {
#if UNITY_EDITOR
        Debug.Assert(c is not null && c.IndexAtItems == -1 && c.IndexAtCells == -1);
#endif
        var cri = PosToColRowIndex(c.Position);
        var ci = ColRowIndexToCellIndex(cri);
        if (_enableDoubleLink)
        {
#if UNITY_EDITOR
            Debug.Assert(c.Prev is null && c.Next is null);
            Debug.Assert(Cells[ci] is null || Cells[ci].Prev is null);
#endif
            if (Cells[ci] is not null)
                Cells[ci].Prev = c;
            c.Next = Cells[ci];
            c.IndexAtCells = ci;
            Cells[ci] = c;
#if UNITY_EDITOR
            Debug.Assert(Cells[ci].Prev is null);
            Debug.Assert(c.Next != c && c.Prev != c);
#endif
        }
        else
        {
#if UNITY_EDITOR
            // Debug.Assert(Cells[ci] is null);
            if (Cells[ci] is not null)
            {
                Debug.LogError($"CellIndex: {ci}");
            }
#endif
            c.IndexAtCells = ci;
            Cells[ci] = c;
        }

        Counts[ci]++;

        // store
        c.IndexAtItems = Items.Count;
        Items.Add(c);
        return c;
    }

    public T Add(T c, XYi cri)
    {
#if UNITY_EDITOR
        Debug.Assert(c is not null && c.IndexAtItems == -1 && c.IndexAtCells == -1);
#endif
        var ci = ColRowIndexToCellIndex(cri);
        if (_enableDoubleLink)
        {
#if UNITY_EDITOR
            Debug.Assert(c.Prev is null && c.Next is null);
            Debug.Assert(Cells[ci] is null || Cells[ci].Prev is null);
#endif
            if (Cells[ci] is not null)
                Cells[ci].Prev = c;
            c.Next = Cells[ci];
            c.IndexAtCells = ci;
            Cells[ci] = c;
#if UNITY_EDITOR
            Debug.Assert(Cells[ci].Prev is null);
            Debug.Assert(c.Next != c && c.Prev != c);
#endif
        }
        else
        {
#if UNITY_EDITOR
            // Debug.Assert(Cells[ci] is null);
            if (Cells[ci] is not null)
            {
                Debug.LogError($"CellIndex: {ci}");
            }
#endif
            c.IndexAtCells = ci;
            Cells[ci] = c;
        }

        Counts[ci]++;

        // store
        c.IndexAtItems = Items.Count;
        Items.Add(c);
        return c;
    }

    public void Remove(T c)
    {
#if UNITY_EDITOR
        Debug.Assert(c is not null && c.IndexAtItems == -1 && c.IndexAtCells == -1);
#endif
        if (_enableDoubleLink)
        {
#if UNITY_EDITOR
            Debug.Assert(c.Next != c && c.Prev != c);
            Debug.Assert((c.Prev is null && Cells[c.IndexAtCells] == c) ||
                         (c.Prev is not null && c.Prev.Next == c && Cells[c.IndexAtCells] != c));
            Debug.Assert(c.Next is null || c.Next.Prev == c);
#endif
            if (c.Prev is not null) // isn't header
            {
#if UNITY_EDITOR
                Debug.Assert(Cells[c.IndexAtCells] != c);
#endif
                c.Prev.Next = c.Next;
                if (c.Next is not null)
                {
                    c.Next.Prev = c.Prev;
                    c.Next = null;
                }

                c.Prev = null;
            }
            else
            {
#if UNITY_EDITOR
                Debug.Assert(Cells[c.IndexAtCells] == c);
#endif
                Cells[c.IndexAtCells] = c.Next;
                if (c.Next is not null)
                {
                    c.Next.Prev = null;
                    c.Next = null;
                }
            }
#if UNITY_EDITOR
            Debug.Assert(Cells[c.IndexAtCells] != c);
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.Assert(Cells[c.IndexAtCells] == c);
#endif
            Cells[c.IndexAtCells] = null;
        }

        Counts[c.IndexAtCells]--;

        // Clear
        var ii = c.IndexAtItems;
        var lastIdx = Items.Count - 1;
        if (ii != lastIdx)
        {
            Items[ii] = Items[lastIdx];
            Items[ii].IndexAtItems = ii;
        }

        c.IndexAtCells = -1;
        c.IndexAtItems = -1;
        Items.RemoveAt(lastIdx);
    }

    public void Update(T c)
    {
#if UNITY_EDITOR
        Debug.Assert(c is not null);
        Debug.Assert(c.IndexAtItems != -1 && c.IndexAtCells != -1);
        Debug.Assert(c.Next != c && c.Prev != c);
        Debug.Assert((c.Prev is null && Cells[c.IndexAtCells] == c) ||
                     (c.Prev is not null && c.Prev.Next == c && Cells[c.IndexAtCells] != c));
        Debug.Assert(c.Next is null || c.Next.Prev == c);
#endif
        var cri = PosToColRowIndex(c.Position);
        var ci = ColRowIndexToCellIndex(cri);
        if (ci == c.IndexAtCells)
            return;
#if UNITY_EDITOR
        Debug.Assert(Cells[ci] is null || Cells[ci].Prev is null);
#endif

        // unlink
        if (c.Prev is not null) // isn't header
        {
#if UNITY_EDITOR
            Debug.Assert(Cells[c.IndexAtCells] != c);
#endif
            c.Prev.Next = c.Next;
            if (c.Next is not null)
                c.Next.Prev = c.Prev;
        }
        else
        {
#if UNITY_EDITOR
            Debug.Assert(Cells[c.IndexAtCells] == c);
#endif
            Cells[c.IndexAtCells] = c.Next;
            if (c.Next is not null)
                c.Next.Prev = null;
        }

        Counts[c.IndexAtCells]--;
#if UNITY_EDITOR
        Debug.Assert(Cells[c.IndexAtCells] != c);
        Debug.Assert(ci != c.IndexAtCells);
#endif
        // link
        if (Cells[ci] is not null)
            Cells[ci].Prev = c;
        c.Prev = null;
        c.Next = Cells[ci];
        Cells[ci] = c;
        c.IndexAtCells = ci;
        Counts[ci]++;
#if UNITY_EDITOR
        Debug.Assert(Cells[ci].Prev is null);
        Debug.Assert(Cells[ci].Next != c);
        Debug.Assert(Cells[ci].Prev != c);
#endif
    }

    public XYi PosToColRowIndex(XYi pos)
    {
        var cri = new XYi((int)(pos.X / CellSize), (int)(pos.Y / CellSize));
#if UNITY_EDITOR
        Debug.Assert(!(cri.X < 0 || cri.X >= NumCols || cri.Y < 0 || cri.Y >= NumRows));
#endif
        return cri;
    }

    public XYi PosToColRowIndex(Vector2 pos)
    {
        var x = (int)(pos.x * InvCellSize);
        var y = (int)((GridSize.y - pos.y) * InvCellSize);
        var cri = new XYi(x, y);
#if UNITY_EDITOR
        Debug.Assert(!(cri.X < 0 || cri.X >= NumCols || cri.Y < 0 || cri.Y >= NumRows));
#endif
        return cri;
    }

    public int ColRowIndexToCellIndex(XYi cri)
    {
        var ci = cri.Y * NumCols + cri.X;
#if UNITY_EDITOR
        Debug.Assert(ci >= 0 && ci < CellsLen);
#endif
        return ci;
    }

    public XYi CellIndexToColRowIndex(int ci)
    {
#if UNITY_EDITOR
        Debug.Assert(ci >= 0 && ci < CellsLen);
#endif
        var ri = ci / NumCols;
        return new XYi(ci - ri * NumCols, ri);
    }

    public T TryAt(XYi cri)
    {
        if (cri.X < 0 || cri.X >= NumCols)
            return null;
        if (cri.Y < 0 || cri.Y >= NumRows)
            return null;
        var ci = cri.Y * NumCols + cri.X;
#if UNITY_EDITOR
        Debug.Assert(ci >= 0 && ci < CellsLen);
#endif
        return Cells[ci];
    }

    public T At(XYi cri)
    {
#if UNITY_EDITOR
        Debug.Assert(cri.X >= 0 && cri.X < NumCols && cri.Y >= 0 && cri.Y < NumRows);
#endif
        var ci = cri.Y * NumCols + cri.X;
#if UNITY_EDITOR
        Debug.Assert(ci >= 0 && ci < CellsLen);
#endif
        return Cells[ci];
    }

    public void Clear()
    {
        Items.Clear();
        for (var i = 0; i < Cells.Length; i++)
            Cells[i] = null;
        for (var i = 0; i < CellsLen; i++)
            Counts[i] = 0;
    }

    public void Foreach9All(XYi pos, Func<T, bool> func, T except = null)
    {
        var cIdx = pos.X / CellSize;
        if (cIdx < 0 || cIdx >= NumCols) return;
        var rIdx = pos.Y / CellSize;
        if (rIdx < 0 || rIdx >= NumRows) return;

        var idx = (int)(rIdx * NumCols + cIdx);

        bool InvokeAndCheck(T c) => (c != except) && func(c);

        // 5
        {
            var c = Cells[idx];
            while (c != null)
            {
                var next = c.Next;
                if (InvokeAndCheck(c)) return;
                c = next;
            }
        }

        // 6
        cIdx++;
        if (cIdx >= NumCols) return;
        idx++;
        {
            var c = Cells[idx];
            while (c != null)
            {
                var next = c.Next;
                if (InvokeAndCheck(c)) return;
                c = next;
            }
        }

        // 3
        rIdx++;
        if (rIdx >= NumRows) return;
        idx += NumCols;
        {
            var c = Cells[idx];
            while (c != null)
            {
                var next = c.Next;
                if (InvokeAndCheck(c)) return;
                c = next;
            }
        }

        // 2
        idx--;
        {
            var c = Cells[idx];
            while (c != null)
            {
                var next = c.Next;
                if (InvokeAndCheck(c)) return;
                c = next;
            }
        }

        // 1
        cIdx -= 2;
        if (cIdx < 0) return;
        idx--;
        {
            var c = Cells[idx];
            while (c != null)
            {
                var next = c.Next;
                if (InvokeAndCheck(c)) return;
                c = next;
            }
        }

        // 4
        idx -= NumCols;
        {
            var c = Cells[idx];
            while (c != null)
            {
                var next = c.Next;
                if (InvokeAndCheck(c)) return;
                c = next;
            }
        }

        // 7
        rIdx -= 2;
        if (rIdx < 0) return;
        idx -= NumCols;
        {
            var c = Cells[idx];
            while (c != null)
            {
                var next = c.Next;
                if (InvokeAndCheck(c)) return;
                c = next;
            }
        }

        // 8
        idx++;
        {
            var c = Cells[idx];
            while (c != null)
            {
                var next = c.Next;
                if (InvokeAndCheck(c)) return;
                c = next;
            }
        }

        // 9
        idx++;
        {
            var c = Cells[idx];
            while (c != null)
            {
                var next = c.Next;
                if (InvokeAndCheck(c)) return;
                c = next;
            }
        }
    }

    public void ForeachByRange(XYi pos, Func<T, bool> func, float maxDistance, T except = null)
    {
        var cIdxBase = (int)(pos.X / CellSize);
        if (cIdxBase < 0 || cIdxBase >= NumCols) return;
        var rIdxBase = (int)(pos.Y / CellSize);
        if (rIdxBase < 0 || rIdxBase >= NumRows) return;
        var searchRange = maxDistance + CellSize;

        bool InvokeAndCheck(T c) => (c != except) && func(c);

        var lens = Rdd.Lens;
        var idxes = Rdd.Idxes;
        for (int i = 1, e = lens.Count; i < e; i++)
        {
            var offsets = lens[i - 1].Count;
            var size = lens[i].Count - lens[i - 1].Count;
            for (var j = 0; j < size; j++)
            {
                var tmp = idxes[offsets + j];
                var cIdx = cIdxBase + tmp.X;
                if (cIdx < 0 || cIdx >= NumCols) continue;
                var rIdx = rIdxBase + tmp.Y;
                if (rIdx < 0 || rIdx >= NumRows) continue;
                var idx = rIdx * NumCols + cIdx;

                var c = Cells[idx];
                while (c is not null)
                {
                    var next = c.Next;
                    if (InvokeAndCheck(c))
                        return;
                    c = next;
                }
            }

            if (lens[i].Radius > searchRange) break;
        }
    }
}

