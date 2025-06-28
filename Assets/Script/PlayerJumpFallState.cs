public class PlayerJumpFallState : PlayerAirState
{
    public PlayerJumpFallState(Player player_, StateMachine stateMachine_, string animBoolName_) : base(player_,
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
        if (_player.CurrentVelocity.y == 0)
            _player.StateMachine.ChangeState(_player.Idle);
        if (_player.playerMoveValue.x != 0 && _player.playerMoveValue.x + _player.WallPushDir == 0)
            _player.StateMachine.ChangeState(_player.WallSlide);

        _player.UpdateVelocity();
        return 0;
    }

    public override void Exit()
    {
        base.Exit();
    }
}