using UnityEngine;

public class PlayerWallJumpState : PlayerAirState
{
    public PlayerWallJumpState(Player player_, StateMachine stateMachine_, string animBoolName_) : base(player_,
        stateMachine_, animBoolName_)
    {
    }

    public override void Enter()
    {
        base.Enter();

        _player.SetVelocity(new Vector2(_player.Scene.PlayerSpeed.x * -_player.FaceDirection,
            _player.Scene.PlayerSpeed.y));
    }

    public override int Update()
    {
        base.Update();
        if (_player.OnGround)
            _player.StateMachine.ChangeState(_player.Idle);
        else if (_player.CurrentVelocity.y <= 0)
            _player.StateMachine.ChangeState(_player.JumpFall);
        // if (_player.playerMoveValue.x != 0 && _player.playerMoveValue.x + _player.WallPushDir == 0)
        //     _player.StateMachine.ChangeState(_player.WallSlide);
        return 0;
    }

    public override void Exit()
    {
        base.Exit();
    }
}