using UnityEngine;

public class PlayerAnimationLogic
{
    private Animator _anim;

    public PlayerAnimationLogic(Animator anim)
    {
        _anim = anim;
    }

    public void Fight()
    {
        _anim.SetTrigger("Fight");
    }

    public void Jump()
    {
        _anim.SetTrigger("Jump");
    }
    
    public void Land()
    {
        _anim.SetTrigger("Land");
    }
    
    public void Block()
    {
        _anim.SetTrigger("Block");
    }
    
    public void Walk(bool active)
    {
        _anim.SetBool("IsWalking", active);
    }

    public void Flip(bool active)
    {
        var spt = _anim.GetComponent<SpriteRenderer>();
        spt.flipX = active;
    }
}
