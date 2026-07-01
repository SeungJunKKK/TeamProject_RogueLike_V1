using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class PlayerIdleState : IState
{
    private PlayerController player;

    public PlayerIdleState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        Debug.Log("시스템: 대기(Idle) 상태 진입 완료.");
         player.Anim.Play("Idle_Down"); 

        player.Rb.linearVelocity = Vector2.zero;
    }

    public void Update()
    {
        if(player.MovementInput.sqrMagnitude > 0.01f)
        {
            player.ChangeState(new PlayerRunState(player));
        }
    }

    public void Exit()
    {
        Debug.Log("시스템: 대기(Idle) 상태 해제.");
    }
}