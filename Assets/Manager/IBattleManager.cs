using System;
using System.Collections.Generic;

public interface IBattleManager
{
    event Action<BattleResult> OnBattleEnd;

    BattleState CurrentState { get; }
    bool IsPlayerTurn { get; }

    void StartBattle(CharacterInstance player, List<CharacterInstance> enemies);
    void EndPlayerTurn();
    List<CharacterInstance> GetAliveEnemies();
}
