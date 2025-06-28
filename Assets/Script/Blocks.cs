using UnityEngine;

public enum PushOutWay
{
    Unknown = 0,
    Up = 1,
    Right = 2,
    Down = 4,
    Left = 8
}

public struct PushOutBox
{
    public Vector2 Pos;
    public PushOutWay Way;
}

public class BlockWayOut
{
    public bool up { get; set; }
    public bool right { get; set; }
    public bool down { get; set; }
    public bool left { get; set; }

    public bool IsZero()
    {
        return !up && !right && !down && !left && !up;
    }
}

public class Block : ISpaceItemFloat<Block>
{
    public GameObject Go;
    public BlockWayOut WayOut;
    public bool AtEdge;
    public Vector2 Size;
    public BoxCollider2D Collider;

    public Vector2 Position { get; set; }
    public int IndexAtItems { get; set; }
    public int IndexAtCells { get; set; }
    public Block Prev { get; set; }
    public Block Next { get; set; }

    public Block(GameObject go)
    {
        Go = go;
        WayOut = new BlockWayOut();
        AtEdge = true;
        Collider = go.GetComponent<BoxCollider2D>();
        var xSize = Collider.bounds.size.x;
        var ySize = Collider.bounds.size.y;

        Size = new Vector2(xSize, ySize);
        Position = Go.transform.position;
        IndexAtItems = -1;
        IndexAtCells = -1;
    }

    public Vector2 GetLTPosition()
    {
        var posLT = new Vector2
        {
            x = Position.x,
            y = Position.y + Size.y
        };
        return posLT;
    }

    public Vector2 GetRBPosition()
    {
        var posRB = new Vector2
        {
            x = Position.x + Size.x,
            y = Position.y - Size.y
        };
        return posRB;
    }

    public void FillWayOut(SpaceIndexFloat<Block> blocks)
    {
        var cri = blocks.PosToColRowIndex(Position);
#if UNITY_EDITOR
        Debug.Assert(blocks.TryAt(cri) == this);
#endif
        if (cri.Y == 0)
        {
            WayOut.up = false;
            AtEdge = true;
        }
        else
        {
            WayOut.up = blocks.TryAt(new XYi(cri.X, cri.Y - 1)) is null;
            AtEdge = false;
        }

        if (cri.Y + 1 == blocks.NumRows)
        {
            WayOut.down = false;
            AtEdge = true;
        }
        else
        {
            WayOut.down = blocks.TryAt(new XYi(cri.X, cri.Y + 1)) is null;
            AtEdge = false;
        }

        if (cri.X == 0)
        {
            WayOut.left = false;
            AtEdge = true;
        }
        else
        {
            WayOut.left = blocks.TryAt(new XYi(cri.X - 1, cri.Y)) is null;
            AtEdge = false;
        }

        if (cri.X + 1 == blocks.NumCols)
        {
            WayOut.right = false;
            AtEdge = true;
        }
        else
        {
            WayOut.right = blocks.TryAt(new XYi(cri.X + 1, cri.Y)) is null;
            AtEdge = false;
        }

        if (AtEdge && WayOut.IsZero())
        {
            if (cri.Y != 0) WayOut.up = true;
            if (cri.Y + 1 != blocks.NumRows) WayOut.down = true;
            if (cri.X != 0) WayOut.left = true;
            if (cri.X + 1 != blocks.NumCols) WayOut.right = true;
        }

        // Debug.Log($"Cri: ({cri.X}, {cri.Y}),WayOut:{WayOut.up},{WayOut.down},{WayOut.left},{WayOut.right}");
    }

    public PushOutBox CalculatePushOutBox(Vector2 cPosLT, Vector2 cSize)
    {
        var bPosRb = GetRBPosition();
        var bHalfSize = new Vector2
        {
            x = Size.x / 2,
            y = Size.y / 2,
        };
        var bCenter = new Vector2
        {
            x = Position.x + bHalfSize.x,
            y = Position.y - bHalfSize.y
        };
        var cPosRB = new Vector2
        {
            x = cPosLT.x + cSize.x,
            y = cPosLT.y - cSize.y
        };
        var cCenter = new Vector2
        {
            x = cPosLT.x + cSize.x / 2,
            y = cPosLT.y - cSize.y / 2,
        };
        float dLeft, dRight, dUp, dDown;
        if (cCenter.x >= bCenter.x)
        {
            if (WayOut.left) dLeft = cCenter.x - Position.x + cSize.x;
            else dLeft = Mathf.Infinity;
            if (WayOut.right) dRight = bPosRb.x - cPosLT.x;
            else dRight = Mathf.Infinity;
            if (cCenter.y <= bCenter.y)
            {
                if (WayOut.up) dUp = Position.y - cCenter.y + cSize.y;
                else dUp = Mathf.Infinity;
                if (WayOut.down) dDown = cPosLT.y - bPosRb.y;
                else dDown = Mathf.Infinity;
            }
            else
            {
                if (WayOut.up) dUp = Position.y - cPosRB.y;
                else dUp = Mathf.Infinity;
                if (WayOut.down) dDown = cPosLT.y - Position.y + cSize.y;
                else dDown = Mathf.Infinity;
            }
        }
        else
        {
            if (WayOut.left) dLeft = cPosRB.x - Position.x;
            else dLeft = Mathf.Infinity;
            if (WayOut.right) dRight = bPosRb.x - cPosRB.x;
            else dRight = Mathf.Infinity;
            if (cCenter.y <= bCenter.y)
            {
                if (WayOut.up) dUp = Position.y - cCenter.y + cSize.y;
                else dUp = Mathf.Infinity;
                if (WayOut.down) dDown = cPosLT.y - bPosRb.y;
                else dDown = Mathf.Infinity;
            }
            else
            {
                if (WayOut.up) dUp = Position.y - cPosRB.y;
                else dUp = Mathf.Infinity;
                if (WayOut.down) dDown = cPosLT.y - Position.y + cSize.y;
                else dDown = Mathf.Infinity;
            }
        }

        if (float.IsPositiveInfinity(dLeft) && float.IsPositiveInfinity(dRight) && float.IsPositiveInfinity(dUp) &&
            float.IsPositiveInfinity(dDown))
        {
            return new PushOutBox
            {
                Pos = cPosLT,
                Way = PushOutWay.Unknown
            };
        }

        var pos = new Vector2();
        if (dRight <= dLeft && dRight <= dUp && dRight <= dDown)
        {
            if (dRight > bHalfSize.x) dRight = 0.01f;
            pos.x = bPosRb.x;
            pos.y = cPosLT.y;
            return new PushOutBox
            {
                Pos = pos,
                Way = PushOutWay.Right,
            };
        }
        else if (dLeft <= dRight && dLeft <= dUp && dLeft <= dDown)
        {
            if (dLeft > bHalfSize.x) dLeft = 0.01f;
            pos.x = Position.x - cSize.x;
            pos.y = cPosLT.y;
            return new PushOutBox
            {
                Pos = pos,
                Way = PushOutWay.Left,
            };
        }
        else if (dUp <= dLeft && dUp <= dRight && dUp <= dDown)
        {
            if (dUp > bHalfSize.y) dUp = 0.01f;
            pos.x = cPosLT.x;
            pos.y = Position.y + cSize.y;
            return new PushOutBox
            {
                Pos = pos,
                Way = PushOutWay.Up,
            };
        }
        else
        {
            if (dDown > bHalfSize.y) dDown = 0.01f;
            pos.x = cPosLT.x;
            pos.y = bPosRb.y - cSize.y;
            return new PushOutBox
            {
                Pos = new Vector2
                {
                    x = cPosLT.x,
                    y = cPosLT.y - dDown,
                },
                Way = PushOutWay.Down,
            };
        }
    }

    public bool PushOutCircle(Vector2 targetPos, float tarRadius)
    {
        return false;
    }

    public bool IsCrossBox(Vector2 cPosLT, Vector2 cSize)
    {
        return Physics.IsAABBIntersect(Position, Size, cPosLT, cSize);
    }
}