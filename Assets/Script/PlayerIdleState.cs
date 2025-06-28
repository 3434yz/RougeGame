using UnityEngine;

public class PlayerIdleState : PlayerGroundState
{
    private bool _stateCondition;

    public PlayerIdleState(Player _player, StateMachine _stateMachine, string animBoolName) : base(_player,
        _stateMachine,
        animBoolName)
    {
    }

    public override int Update()
    {
        base.Update();
        if (_player._moving)
        {
            _stateMachine.ChangeState(_player.Move);
        }

        if (_player.CurrentVelocity.y < 0)
        {
            _stateMachine.ChangeState(_player.JumpFall);
        }
        _player.UpdateVelocity();
        return 0;
    }
}