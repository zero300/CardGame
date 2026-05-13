using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Setup, PlayerTurn, EnemyTurn, End }
public enum BattleResult { Victory, Defeat }

public class BattleManager : MonoBehaviour, IBattleManager
{
    public event Action<BattleResult> OnBattleEnd;

    public BattleState CurrentState => _state;
    public bool IsPlayerTurn => _state == BattleState.PlayerTurn;

    private BattleState _state = BattleState.Setup;
    private CharacterInstance _localPlayer;
    private List<CharacterInstance> _enemies = new List<CharacterInstance>();
    private bool _lastDiedWasEnemy;

    public void StartBattle(CharacterInstance player, List<CharacterInstance> enemies)
    {
        // Unsubscribe previous LocalPlayer to prevent accumulation across battles
        if (_localPlayer != null)
            _localPlayer.OnDeath -= OnLocalPlayerDeath;

        _localPlayer = player;
        _enemies = enemies;
        _state = BattleState.Setup;
        _lastDiedWasEnemy = false;

        _localPlayer.OnDeath += OnLocalPlayerDeath;
        foreach (var enemy in _enemies)
        {
            var captured = enemy;
            enemy.OnDeath += () => OnEnemyDeath(captured);
        }

        ServiceLocator.Instance.Get<CardManager>().BindLocalPlayerHand(player);

        StartPlayerTurn();
    }

    private void OnLocalPlayerDeath()
    {
        _lastDiedWasEnemy = false;
        CheckResult();
    }

    private void OnEnemyDeath(CharacterInstance enemy)
    {
        enemy.OnDeath -= () => OnEnemyDeath(enemy);
        _lastDiedWasEnemy = true;
        CheckResult();
    }

    private void CheckResult()
    {
        bool playerDead = _localPlayer.CurrentHP <= 0;
        bool allEnemiesDead = _enemies.TrueForAll(e => e.CurrentHP <= 0);

        if (!playerDead && !allEnemiesDead) return;

        BattleResult result;
        if (allEnemiesDead && !playerDead)
            result = BattleResult.Victory;
        else if (playerDead && !allEnemiesDead)
            result = BattleResult.Defeat;
        else // 雙方皆死：最後死亡的一方決定結果
            result = _lastDiedWasEnemy ? BattleResult.Victory : BattleResult.Defeat;

        EndBattle(result);
    }

    private void StartPlayerTurn()
    {
        _state = BattleState.PlayerTurn;

        foreach (var enemy in GetAliveEnemies())
            enemy.EnemyController?.PlanNextAction(enemy);

        _localPlayer.TurnStart();
    }

    public void EndPlayerTurn()
    {
        if (_state != BattleState.PlayerTurn) return;
        _localPlayer.TurnEnd();
        StartCoroutine(ExecuteEnemyTurn());
    }

    private IEnumerator ExecuteEnemyTurn()
    {
        _state = BattleState.EnemyTurn;

        foreach (var enemy in GetAliveEnemies())
        {
            enemy.ClearBlock();
            enemy.EnemyController?.ExecuteAction(enemy, _localPlayer);
            yield return new WaitForSeconds(0.5f);

            if (_state == BattleState.End) yield break;
        }

        CheckResult();

        if (_state != BattleState.End)
            StartPlayerTurn();
    }

    public List<CharacterInstance> GetAliveEnemies()
    {
        return _enemies.FindAll(e => e.CurrentHP > 0);
    }

    private void EndBattle(BattleResult result)
    {
        _state = BattleState.End;
        OnBattleEnd?.Invoke(result);
    }
}
