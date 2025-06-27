using UnityEngine;

public class Scene : MonoBehaviour
{
    public int Time = 0;
    public float TimePool = 0;

    public Stage _stage;
    public Player _player;

    [Header("Player Movement Details")] 
    public Vector2 PlayerSpeed;
    public float PlayerGravity;

    public GameObject playerPrefab;
    public GameObject blookPrefab;

    internal const int numRows = 10, numCols = 10;
    internal const float cellSize = 0.5f;

    public const float Sqrt2 = 1.414213562373095f;
    public const float Sqrt21 = 0.7071067811865475f;
    public const int FPS = 60;

    // 逻辑帧率间隔时长
    public const float FrameDelay = 1.0f / FPS;

    public Color gridColor = Color.white;

    private void Start()
    {
        _player = new Player(this);
        _player.InitInputAction();

        _stage = new Stage(this);
    }

    private void Update()
    {
        _player.HandlePlayerInput();

        // 按设计帧率驱动游戏逻辑
        TimePool += UnityEngine.Time.deltaTime;
        if (TimePool > FrameDelay)
        {
            TimePool -= FrameDelay;
            ++Time;
            _stage.Update();
        }

        _stage.Draw();
    }

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