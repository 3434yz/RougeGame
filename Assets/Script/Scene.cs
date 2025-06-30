using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Scene : MonoBehaviour
{
    public int Time = 0;
    public float TimePool = 0;

    public Stage _stage;
    public Player _player;

    [Header("Prefab")] 
    public GameObject playerPrefab;
    public GameObject blookPrefab;
    
    [Header("Player Movement Details")] 
    public Vector2 PlayerSpeed;
    public float PlayerGravity;
    public float PlayerAirSpeedScale;
    public float PlayerWallSlideSpeedScale;
    
    [Header("Player Dash Details")]
    public float PlayerDashSpeed;
    public float PlayerDashSeconds;
    public float PlayerDashCDSeconds;

    [Header("Player Attack Details")] 
    public int PlayerAttackCastFrames;
    public int PlayerAttackCalFrames;
    
    [Header("Collision Details")] 
    public LayerMask GroundLayer;

    internal const int numRows = 10, numCols = 10;
    internal const float cellSize = 0.5f;

    public const float Sqrt2 = 1.414213562373095f;
    public const float Sqrt21 = 0.7071067811865475f;
    public const int FPS = 60;

    // 逻辑帧率间隔时长
    public const float FrameDelay = 1.0f / FPS;

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

    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // Gizmos.DrawLine(_player.CurrentPosition,
        //     _player.CurrentPosition + new Vector2(0.5f * _player.FaceDirection, 0));
    }
}