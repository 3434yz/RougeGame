public class PlayerWallSlideState : EntityState
{
    public PlayerWallSlideState(Player player_, StateMachine stateMachine_, string animBoolName_) : base(player_,
        stateMachine_, animBoolName_)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override int Update()
    {
        base.Update();
        if (_player.OnGround)
        {
            _player.StateMachine.ChangeState(_player.Idle);
            _player.Flip();
        }

        if (_player.CurrentVelocity.x != 0)
            _player.StateMachine.ChangeState(_player.JumpFall);

        // if (_player.KeepingPushWall() is false)
        //     _player.StateMachine.ChangeState(_player.JumpFall);

        var scale = _player.playerMoveValue.y switch
        {
            > 0 => 1.0f,
            < 0 => 0.5f,
            _ => .7f
        };

        _player.UpdateVelocity(0, scale);
        return 0;
    }

    public override void Exit()
    {
        base.Exit();
    }
}