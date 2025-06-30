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
    public PlayerWallJumpState WallJump;
    public PlayerDashState Dash;
    public PlayerBaseAttackState BaseAttack;

    private GameObject _playerGameObject;
    private Transform _animTransform;
    public Animator Animator;
    public SpriteRenderer SpriteRenderer;
    public BoxCollider2D Collider2D;

    public Vector2 Size;

    public bool OnGround { set; get; } // 地板推力
    public int WallPushDir; // 墙壁推力方向
    public int FaceDirection { get; private set; }
    public Vector2 CurrentVelocity;
    public Vector2 CurrentPosition;

    private int _dashCastingFrames; // 冲刺持续帧数
    private int _dashColdDownFrames; // 冲刺CD
    private int _dashCastDelayFrames; // 冲刺结束的帧数
    private int _dashReadyFrames; // 冲刺就绪的帧数

    private int _attackCastingFrames; // 攻击动画持续帧
    private int _attackCalFrames;
    private int _attackCastingDelayFrames; // 攻击动画结束帧

    private AnimationClip testClip;

    public Player(Scene scene)
    {
        Scene = scene;

        StateMachine = new StateMachine();
        Idle = new PlayerIdleState(this, StateMachine, "idle");
        Move = new PlayerMoveState(this, StateMachine, "moving");
        Jump = new PlayerJumpState(this, StateMachine, "jumpFall");
        JumpFall = new PlayerJumpFallState(this, StateMachine, "jumpFall");
        WallSlide = new PlayerWallSlideState(this, StateMachine, "wallSlide");
        WallJump = new PlayerWallJumpState(this, StateMachine, "jumpFall");
        Dash = new PlayerDashState(this, StateMachine, "dash");
        BaseAttack = new PlayerBaseAttackState(this, StateMachine, "baseAttack");

        _dashCastingFrames = (int)(Scene.PlayerDashSeconds * Scene.FPS);
        _dashColdDownFrames = (int)(Scene.PlayerDashCDSeconds * Scene.FPS);

        _attackCastingFrames = Scene.PlayerAttackCastFrames;
        _attackCalFrames = Scene.PlayerAttackCalFrames;

        FaceDirection = 1;
    }

    public void OnEnterStage(Stage stage, Vector2 position)
    {
        _stage = stage;

        CurrentPosition = position;
        _playerGameObject = Object.Instantiate(Scene.playerPrefab, CurrentPosition, Quaternion.identity);
        _animTransform = _playerGameObject.transform.Find("Animator");

        Animator = _animTransform.GetComponentInChildren<Animator>();
        SpriteRenderer = _animTransform.GetComponentInChildren<SpriteRenderer>();
        Collider2D = _animTransform.GetComponentInChildren<BoxCollider2D>();
        Size = Collider2D.bounds.size;

        var controller = Animator.runtimeAnimatorController;
        var clips = controller.animationClips;
        foreach (var clip in clips)
        {
            if (clip.name == "playerBaseAttack")
            {
                Debug.Log($"playerBaseAttack 动画一轮播放时长为:{clip.length}");
                break;
            }
        }

        StateMachine.Initialize(Idle);
    }

    public int Update()
    {
        CollisionCheck();
        StateMachine.Update();
        return 0;
    }

    private void CollisionCheck()
    {
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
            if (!cx && !cy) return;
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
        else
        {
            CurrentPosition = finalPosition;
            PathDetection(blocks);
        }
    }

    private int PathDetection(SpaceIndexFloat<Block> blocks)
    {
        var posLT = GetPosLT();
        var posRB = new Vector2
        {
            x = posLT.x + Size.x - 0.001f,
            y = posLT.y - Size.y + 0.001f
        };

        var pushOutWays = 0;
        var criFrom = blocks.PosToColRowIndex(posLT);
        var criTo = blocks.PosToColRowIndex(posRB);
        for (var rowIdx = criFrom.Y; rowIdx <= criTo.Y; rowIdx++)
        {
            for (var colIdx = criFrom.X; colIdx <= criTo.X; colIdx++)
            {
                var bc = blocks.At(new XYi(colIdx, rowIdx));
                if (bc is not null && bc.IsCrossBox(posLT, Size))
                {
                    var pushOutBox = bc.CalculatePushOutBox(posLT, Size);
                    if (pushOutBox.Way != PushOutWay.Unknown)
                    {
                        posLT = pushOutBox.Pos;
                        posRB.x = posLT.x + Size.x;
                        posRB.y = posLT.y - Size.y;
                        pushOutWays |= (int)pushOutBox.Way;
                    }
                }
            }
        }

        if (pushOutWays != 0)
        {
            CurrentPosition.x = posLT.x + Size.x / 2;
            CurrentPosition.y = posLT.y - Size.y;
            if ((pushOutWays & (int)PushOutWay.Up) != 0)
            {
                CurrentVelocity.y = 0;
                OnGround = true;
            }
            else if ((pushOutWays & (int)PushOutWay.Down) != 0)
                CurrentVelocity.y = 0;
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
        pos.x -= Size.x / 2;
        pos.y += Size.y;
        return pos;
    }

    public void Flip()
    {
        FaceDirection *= -1;
        _animTransform.Rotate(0, 180, 0);
    }

    #region Velocity

    public void SetVelocity(Vector2 velocity)
    {
        CurrentVelocity = velocity;
        HandleFlip(CurrentVelocity.x);
    }

    public void UpdateVelocity(float scaleX = 1, float scaleY = 1)
    {
        if (WallPushDir + FaceDirection == 0) CurrentVelocity.x = 0;
        else CurrentVelocity.x = Scene.PlayerSpeed.x * playerMoveValue.x;

        if (OnGround) CurrentVelocity.y = 0;
        else UpdateGravityVelocity();

        HandleFlip(CurrentVelocity.x);

        if (Attacking()) CurrentVelocity.x = 0;

        CurrentVelocity.x *= scaleX;
        CurrentVelocity.y *= scaleY;
    }

    public void UpdateGravityVelocity()
    {
        CurrentVelocity.y -= Scene.PlayerGravity;
    }

    public void UpdateGroundVelocity()
    {
    }

    #endregion


    public void HandleFlip(float xVelocity)
    {
        if (xVelocity == 0 && playerMoveValue.x == 0)
            return;
        if (xVelocity > 0 && FaceDirection == -1)
            Flip();
        else if (xVelocity < 0 && FaceDirection == 1)
            Flip();
    }

    #region Dash

    public void CastDash()
    {
        if (!CanDash()) return;
        _dashReadyFrames = Scene.Time + _dashColdDownFrames;
        _dashCastDelayFrames = Scene.Time + _dashCastingFrames;
        SetVelocity(new Vector2(Scene.PlayerDashSpeed * FaceDirection, 0));
    }

    public bool CanDash()
    {
        return Scene.Time > _dashReadyFrames;
    }

    public bool Dashing()
    {
        if (WallPushDir != 0) _dashCastDelayFrames = Scene.Time;
        return _dashCastDelayFrames > Scene.Time;
    }

    #endregion

    #region Attack

    public void CastAttack()
    {
        _attackCastingDelayFrames = Scene.Time + _attackCastingFrames;
        SetVelocity(new Vector2(0, CurrentVelocity.y));
    }

    public bool Attacking()
    {
        return Scene.Time <= _attackCastingDelayFrames;
    }

    #endregion


    /// <summary>
    /// 是否一直对着墙壁移动
    /// </summary>
    /// <returns></returns>
    public bool KeepingPushWall()
    {
        return playerMoveValue.x != 0 && CurrentVelocity.x == 0;
    }

    public void AttackCheckTrigger()
    {
        Debug.Log("---------------------------------------------------------------------------");
    }


    public void Draw()
    {
        _playerGameObject.transform.position = CurrentPosition;
        // if (Physics2D.Raycast(_currentPosition, Vector2.down, 0.5f, Scene.GroundLayer))
        //     Debug.Log($"Draw Position: {_currentPosition.x}, {_currentPosition.y}");
    }
}