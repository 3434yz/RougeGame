public class PlayerBaseAttackState : EntityState
{
    public PlayerBaseAttackState(Player player_, StateMachine stateMachine_, string animBoolName_) : base(player_,
        stateMachine_, animBoolName_)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _player.CastAttack();
    }

    public override int Update()
    {
        base.Update();
        if (_player.Attacking() is false)
            _player.StateMachine.ChangeState(_player.Idle);

        return 0;
    }

    public override void Exit()
    {
        base.Exit();
        _player.AttackCheckTrigger();
    }
}