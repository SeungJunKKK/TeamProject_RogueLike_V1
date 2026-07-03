using UnityEngine;

public class PlayerDashState : IState
{
    private PlayerController player;
    private float dashTimer;
    private Vector2 dashDirection;

    public PlayerDashState(PlayerController player) { this.player = player; }

    public void Enter()
    {
        player.SpawnDustEffect(player.IsFacingRight);
        player.Anim.Play("Dash");

        dashTimer = player.DashDuration;
        float dirX = player.MovementInput.x != 0 ? Mathf.Sign(player.MovementInput.x) : (player.IsFacingRight ? 1f : -1f);
        dashDirection = new Vector2(dirX, 0f);

        player.Rb.linearVelocity = dashDirection * player.DashSpeed;
        player.Rb.gravityScale = 0f;
    }

    public void Update()
    {
        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0)
        {
            if (player.MovementInput.x != 0) player.ChangeState(new PlayerRunState(player));
            else player.ChangeState(new PlayerIdleState(player));
        }
    }

    public void Exit()
    {
        player.Rb.gravityScale = 3f;
        player.Rb.linearVelocity = Vector2.zero;
    }
}