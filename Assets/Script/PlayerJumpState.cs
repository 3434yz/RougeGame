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
        if (_player.Dashing())
            return 0;

        if (_player.CurrentVelocity.y <= 0) _player.StateMachine.ChangeState(_player.JumpFall);
        else _player.UpdateVelocity(0.5f);
        return 0;
    }

    public override void Exit()
    {
        base.Exit();
    }
}