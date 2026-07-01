public interface IState
{
    // 상태에 진입할 때 1회 호출 
    void Enter();

    // 상태에 머무는 동안 매 프레임 호출 
    void Update();

    // 상태를 빠져나갈 때 1회 호출 
    void Exit();
}