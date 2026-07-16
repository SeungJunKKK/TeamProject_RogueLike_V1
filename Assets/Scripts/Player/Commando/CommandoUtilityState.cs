using UnityEngine;

namespace Player.Commando
{
  
public class CommandoUtilityState : PlayerAttackState
    {
        public CommandoUtilityState(PlayerController player, float duration)
            : base(player, "C_Utility", duration, SkillType.Utility_C) { }

        protected override void ExecuteShoot()
        {

        }
    }
}
