using UnityEngine;

public class PlayerJumpState : PlayerAirState
{
    public PlayerJumpState(Player player_, StateMachine stateMachine_, string animBoolName_) : base(player_,
        stateMachine_, animBoolName_)
    {
    }

    public override void Enter()
    {
        base.Enter();

        _player.SetVelocity(new Vector2(_player.CurrentVelocity.x, _player.Scene.PlayerSpeed.y));
        _player.OnGround = false;
    }

    public override int Update()
    {
        base.Update();

        if (_player.CurrentVelocity.y <= 0)
            _player.StateMachine.ChangeState(_player.JumpFall);

        _player.UpdateVelocity();
        return 0;
    }

    public override void Exit()
    {
        base.Exit();
    }
}