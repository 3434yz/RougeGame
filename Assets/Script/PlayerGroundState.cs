using UnityEngine;


public class PlayerGroundState : EntityState
{
    public PlayerGroundState(Player player_, StateMachine stateMachine_, string animBoolName_) : base(player_,
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
        if (_player._jumping && _player.OnGround) _stateMachine.ChangeState(_player.Jump);
        else if (_player.InvokerAttack()) _stateMachine.ChangeState(_player.BaseAttack);
        return 0;
    }

    public override void Exit()
    {
        base.Exit();
    }
}