using UnityEngine;

public class PlayerRunState : IState
{
    private readonly PlayerController player; 

    public PlayerRunState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.Anim.Play("Walk");
    }

    public void Update()
    {
        player.Rb.linearVelocity = new Vector2(player.MovementInput.x * player.MoveSpeed, player.Rb.linearVelocity.y);

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            player.ChangeState(new PlayerDashState(player));
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.ChangeState(new PlayerJumpState(player));
            return;
        }

        if (player.MovementInput.x == 0)
        {
            player.ChangeState(new PlayerIdleState(player));
        }
    }

    public void Exit() { }
}