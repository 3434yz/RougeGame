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

    private GameObject _playerGameObject;
    private Transform _animTransform;
    public Animator mAnimator;
    public SpriteRenderer mSR;
    public BoxCollider2D mCollider2D;

    public Vector2 mSize;

    public bool OnFloor { set; get; } // 
    public int WallPushDir; // 面对墙壁移动
    private int _faceDirection = 1;
    public Vector2 CurrentVelocity;
    private Vector2 _currentPosition;

    public Player(Scene scene)
    {
        Scene = scene;

        StateMachine = new StateMachine();
        Idle = new PlayerIdleState(this, StateMachine, "idle");
        Move = new PlayerMoveState(this, StateMachine, "moving");
        Jump = new PlayerJumpState(this, StateMachine, "jumpFall");
        JumpFall = new PlayerJumpFallState(this, StateMachine, "jumpFall");
    }

    public void OnEnterStage(Stage stage, Vector2 position)
    {
        _stage = stage;

        _currentPosition = position;
        _playerGameObject = Object.Instantiate(Scene.playerPrefab, _currentPosition, Quaternion.identity);
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

        UpdateVelocity();
        Flip();

        var blocks = _stage.blocks;
        var bakPosition = _currentPosition;
        var velocityX = Mathf.Abs(CurrentVelocity.x);
        var velocityY = Mathf.Abs(CurrentVelocity.y);
        var stepCount = velocityX > velocityY
            ? Mathf.CeilToInt(velocityX / Stage.WALL_WIDTH)
            : Mathf.CeilToInt(velocityY / Stage.WALL_WIDTH);
        var incrVelocityX = CurrentVelocity.x / stepCount;
        var incrVelocityY = CurrentVelocity.y / stepCount;
        var finalPosition =
            new Vector2(_currentPosition.x + CurrentVelocity.x, _currentPosition.y + CurrentVelocity.y);
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
                    if (Tools.Cover(xMoveDir, _currentPosition.x + incrVelocityX, finalPosition.x))
                        _currentPosition.x = finalPosition.x;
                    else _currentPosition.x += incrVelocityX;
                }

                if (0 != CurrentVelocity.y)
                {
                    if (Tools.Cover(yMoveDir, _currentPosition.y + incrVelocityY, finalPosition.y))
                        _currentPosition.y = finalPosition.y;
                    else _currentPosition.y += incrVelocityY;
                }

                PathDetection(blocks);
            }

            var cx = Tools.Cover(xMoveDir, _currentPosition.x, finalPosition.x) is false;
            var cy = Tools.Cover(yMoveDir, _currentPosition.y, finalPosition.y) is false;
            if (cx || cy)
            {
                var more = false;
                if (cx && WallPushDir == 0)
                {
                    _currentPosition.x = finalPosition.x;
                    more = true;
                }

                if (cy && OnFloor is false)
                {
                    _currentPosition.y = finalPosition.y;
                    more = true;
                }

                if (more) PathDetection(blocks);
            }
        }
        else
        {
            _currentPosition = finalPosition;
            PathDetection(blocks);
        }

        return 0;
    }

    private int PathDetection(SpaceIndexFloat<Block> blocks)
    {
        var posLT = GetPosLT();
        var posRB = new Vector2
        {
            x = posLT.x + mSize.x - 0.01f,
            y = posLT.y - mSize.y + 0.01f
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
            _currentPosition.x = posLT.x + mSize.x / 2;
            _currentPosition.y = posLT.y - mSize.y;
            if ((pushOutWays & (int)PushOutWay.Up) != 0)
            {
                CurrentVelocity.y = 0;
                OnFloor = true;
            }
            else if ((pushOutWays & (int)PushOutWay.Down) != 0) CurrentVelocity.y = 0;
            else OnFloor = false;

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

        return pushOutWays;
    }

    public Vector2 GetPosLT()
    {
        var pos = _currentPosition;
        pos.x -= mSize.x / 2;
        pos.y += mSize.y;
        return pos;
    }

    public void Flip()
    {
        if (_faceDirection != (int)playerLastMoveValue.x)
        {
            _faceDirection = (int)playerLastMoveValue.x;
            _animTransform.Rotate(0, 180, 0);
        }
    }

    public void SetVelocity(Vector2 velocity)
    {
        CurrentVelocity = velocity;
    }

    public void UpdateVelocity()
    {
        if (WallPushDir + _faceDirection == 0) CurrentVelocity.x = 0;
        else CurrentVelocity.x = Scene.PlayerSpeed.x * playerMoveValue.x;
        if (OnFloor) CurrentVelocity.y = 0;
        else CurrentVelocity.y -= Scene.PlayerGravity;
    }

    public void Draw()
    {
        // Debug.Log($"Draw Position: {mPos.x}, {mPos.y}");
        _playerGameObject.transform.position = _currentPosition;
    }
}