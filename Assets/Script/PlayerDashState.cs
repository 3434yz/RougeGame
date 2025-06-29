public class PlayerDashState : EntityState
{
    public PlayerDashState(Player player_, StateMachine stateMachine_, string animBoolName_) : base(player_,
        stateMachine_, animBoolName_)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _player.CastDash();
    }

    public override int Update()
    {
        base.Update();
        if (_player.Dashing() is false)
            _player.StateMachine.ChangeState(_player.Idle);
        return 0;
    }

    public override void Exit()
    {
        base.Exit();
    }
}