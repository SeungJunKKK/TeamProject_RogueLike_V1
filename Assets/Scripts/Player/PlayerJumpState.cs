using UnityEngine;

public class PlayerJumpState : IState
{
    private PlayerController player;

    public PlayerJumpState(PlayerController player) { this.player = player; }

    public void Enter()
    {
        player.SpawnDustEffect(player.IsFacingRight);
        player.Anim.Play(player.IsFacingRight ? "Jump_Right" : "Jump_Left");
        player.Rb.linearVelocity = new Vector2(player.Rb.linearVelocity.x, player.JumpForce);
    }

    public void Update()
    {
        player.Rb.linearVelocity = new Vector2(player.MovementInput.x * player.MoveSpeed, player.Rb.linearVelocity.y);

        if (player.Rb.linearVelocity.y <= 0.01f && player.Rb.linearVelocity.y >= -0.01f)
        {
            if (player.MovementInput.x != 0) player.ChangeState(new PlayerRunState(player));
            else player.ChangeState(new PlayerIdleState(player));
        }
    }

    public void Exit() { }
}