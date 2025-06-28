using UnityEngine;

public abstract class EntityState
{
    protected Player _player;
    protected StateMachine _stateMachine;
    protected string _animBoolName;

    public EntityState(Player player_, StateMachine stateMachine_, string animBoolName_)
    {
        _player = player_;
        _stateMachine = stateMachine_;
        _animBoolName = animBoolName_;
    }

    public virtual void Enter()
    {
        _player.mAnimator.SetBool(_animBoolName, true);
    }

    public virtual int Update()
    {
        _player.mAnimator.SetFloat("yVelocity", _player.CurrentVelocity.y);
        return 0;
    }

    public virtual void Exit()
    {
        _player.mAnimator.SetBool(_animBoolName, false);
    }
}