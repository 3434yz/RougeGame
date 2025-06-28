using Common;
using UnityEngine;

public partial class Player
{
    public Scene Scene;
    private Stage _stage;

    public StateMachine StateMachine;
    public PlayerIdleState Idle;
    public PlayerMoveState Move;
    public PlayerJumpState Jump;
    public PlayerJumpFallState JumpFall;
    public PlayerWallSlideState WallSlide;

    private GameObject _playerGameObject;
    private Transform _animTransform;
    public Animator mAnimator;
    public SpriteRenderer mSR;
    public BoxCollider2D mCollider2D;

    public Vector2 mSize;

    public bool OnGround { set; get; } // 
    public int WallPushDir; // 面对墙壁移动
    public int FaceDirection { get; private set; }
    public Vector2 CurrentVelocity;
    public Vector2 CurrentPosition;

    public Player(Scene scene)
    {
        Scene = scene;

        StateMachine = new StateMachine();
        Idle = new PlayerIdleState(this, StateMachine, "idle");
        Move = new PlayerMoveState(this, StateMachine, "moving");
        Jump = new PlayerJumpState(this, StateMachine, "jumpFall");
        JumpFall = new PlayerJumpFallState(this, StateMachine, "jumpFall");
        WallSlide = new PlayerWallSlideState(this, StateMachine, "wallSlide");
        FaceDirection = 1;
    }

    public void OnEnterStage(Stage stage, Vector2 position)
    {
        _stage = stage;

        CurrentPosition = position;
        _playerGameObject = Object.Instantiate(Scene.playerPrefab, CurrentPosition, Quaternion.identity);
        _animTransform = _playerGameObject.transform.Find("Animator");

        mAnimator = _animTransform.GetComponentInChildren<Animator>();
        mSR = _animTransform.GetComponentInChildren<SpriteRenderer>();
        mCollider2D = _animTransform.GetComponentInChildren<BoxCollider2D>();
        mSize = mCollider2D.bounds.size;

        StateMachine.Initialize(Idle);
    }

    public int Update()
    {
        StateMachine.Update();

        var blocks = _stage.blocks;
        var bakPosition = CurrentPosition;
        var velocityX = Mathf.Abs(CurrentVelocity.x);
        var velocityY = Mathf.Abs(CurrentVelocity.y);
        var stepCount = velocityX > velocityY
            ? Mathf.CeilToInt(velocityX / Stage.WALL_WIDTH)
            : Mathf.CeilToInt(velocityY / Stage.WALL_WIDTH);
        var incrVelocityX = CurrentVelocity.x / stepCount;
        var incrVelocityY = CurrentVelocity.y / stepCount;
        var finalPosition =
            new Vector2(CurrentPosition.x + CurrentVelocity.x, CurrentPosition.y + CurrentVelocity.y);
        var xMoveDir = (int)playerMoveValue.x;
        var yMoveDir = CurrentVelocity.y > 0 ? 1 : -1;
        if (velocityX >= Stage.WALL_WIDTH || velocityY >= Stage.WALL_WIDTH)
        {
            for (var i = 0; i < stepCount; i++)
            {
                if (CurrentVelocity is { x: 0, y: 0 })
                    break;

                if (0 != CurrentVelocity.x)
                {
                    if (Tools.Cover(xMoveDir, CurrentPosition.x + incrVelocityX, finalPosition.x))
                        CurrentPosition.x = finalPosition.x;
                    else CurrentPosition.x += incrVelocityX;
                }

                if (0 != CurrentVelocity.y)
                {
                    if (Tools.Cover(yMoveDir, CurrentPosition.y + incrVelocityY, finalPosition.y))
                        CurrentPosition.y = finalPosition.y;
                    else CurrentPosition.y += incrVelocityY;
                }

                PathDetection(blocks);
            }

            var cx = Tools.Cover(xMoveDir, CurrentPosition.x, finalPosition.x) is false;
            var cy = Tools.Cover(yMoveDir, CurrentPosition.y, finalPosition.y) is false;
            if (cx || cy)
            {
                var more = false;
                if (cx && WallPushDir == 0)
                {
                    CurrentPosition.x = finalPosition.x;
                    more = true;
                }

                if (cy && OnGround is false)
                {
                    CurrentPosition.y = finalPosition.y;
                    more = true;
                }

                if (more) PathDetection(blocks);
            }
        }
        else
        {
            CurrentPosition = finalPosition;
            PathDetection(blocks);
        }

        return 0;
    }

    private int PathDetection(SpaceIndexFloat<Block> blocks)
    {
        var posLT = GetPosLT();
        var posRB = new Vector2
        {
            x = posLT.x + mSize.x - 0.001f,
            y = posLT.y - mSize.y + 0.001f
        };

        var pushOutWays = 0;
        var criFrom = blocks.PosToColRowIndex(posLT);
        var criTo = blocks.PosToColRowIndex(posRB);
        for (var rowIdx = criFrom.Y; rowIdx <= criTo.Y; rowIdx++)
        {
            for (var colIdx = criFrom.X; colIdx <= criTo.X; colIdx++)
            {
                var bc = blocks.At(new XYi(colIdx, rowIdx));
                if (bc is not null && bc.IsCrossBox(posLT, mSize))
                {
                    var pushOutBox = bc.CalculatePushOutBox(posLT, mSize);
                    if (pushOutBox.Way != PushOutWay.Unknown)
                    {
                        posLT = pushOutBox.Pos;
                        posRB.x = posLT.x + mSize.x;
                        posRB.y = posLT.y - mSize.y;
                        pushOutWays |= (int)pushOutBox.Way;
                    }
                }
            }
        }

        if (pushOutWays != 0)
        {
            CurrentPosition.x = posLT.x + mSize.x / 2;
            CurrentPosition.y = posLT.y - mSize.y;
            if ((pushOutWays & (int)PushOutWay.Up) != 0)
            {
                CurrentVelocity.y = 0;
                OnGround = true;
            }
            else if ((pushOutWays & (int)PushOutWay.Down) != 0) CurrentVelocity.y = 0;
            else OnGround = false;

            if ((pushOutWays & (int)PushOutWay.Left) != 0)
            {
                CurrentVelocity.x = 0;
                WallPushDir = -1;
            }
            else if ((pushOutWays & (int)PushOutWay.Right) != 0)
            {
                WallPushDir = 1;
                CurrentVelocity.x = 0;
            }
            else WallPushDir = 0;
        }
        else
        {
            WallPushDir = 0;
            OnGround = false;
        }

        return pushOutWays;
    }

    public Vector2 GetPosLT()
    {
        var pos = CurrentPosition;
        pos.x -= mSize.x / 2;
        pos.y += mSize.y;
        return pos;
    }

    // public void CheckFacingDir()
    // {
    //     if (FaceDirection != (int)playerLastMoveValue.x)
    //     {
    //         FaceDirection = (int)playerLastMoveValue.x;
    //         Flip();
    //     }
    // }

    public void Flip()
    {
        FaceDirection *= -1;
        _animTransform.Rotate(0, 180, 0);
    }

    public void SetVelocity(Vector2 velocity)
    {
        CurrentVelocity = velocity;
    }

    public void UpdateVelocity(float scaleX = 1, float scaleY = 1)
    {
        if (WallPushDir + FaceDirection == 0) CurrentVelocity.x = 0;
        else CurrentVelocity.x = Scene.PlayerSpeed.x * playerMoveValue.x;

        if (OnGround) CurrentVelocity.y = 0;
        else CurrentVelocity.y -= Scene.PlayerGravity;

        CurrentVelocity.x *= scaleX;
        CurrentVelocity.y *= scaleY;
        
        HandleFlip(CurrentVelocity.x);
    }

    public void HandleFlip(float xVelocity)
    {
        if (xVelocity == 0 && playerMoveValue.x == 0)
            return;
        if(xVelocity > 0 && FaceDirection == -1)
            Flip();
        else if (xVelocity < 0 && FaceDirection == 1)
            Flip();
    }

    /// <summary>
    /// 是否一直对着墙壁移动
    /// </summary>
    /// <returns></returns>
    public bool KeepingPushWall()
    {
        return playerMoveValue.x != 0 && CurrentVelocity.x == 0;
    }

    public void UpdateGroundVelocity()
    {
    }


    public void Draw()
    {
        _playerGameObject.transform.position = CurrentPosition;
        // if (Physics2D.Raycast(_currentPosition, Vector2.down, 0.5f, Scene.GroundLayer))
        //     Debug.Log($"Draw Position: {_currentPosition.x}, {_currentPosition.y}");
    }
}