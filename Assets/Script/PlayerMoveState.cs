public class PlayerMoveState : PlayerGroundState
{
    public PlayerMoveState(Player _player, StateMachine _stateMachine, string animBoolName) : base(_player,
        _stateMachine,
        animBoolName)
    {
    }

    public override int Update()
    {
        base.Update();
        if (false == _player._moving)
            _player.StateMachine.ChangeState(_player.Idle);

        _player.UpdateVelocity();
        return 0;
    }
}