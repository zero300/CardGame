using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    public void HandleNodeEntered(string nodeId)
    {
        var mapManager = ServiceLocator.Instance.Get<MapManager>();
        var node = mapManager?.GetNode(nodeId);
        if (node == null) return;

        switch (node.Type)
        {
            case NodeType.Combat:
            case NodeType.Boss:
                StartCombat();
                break;

            case NodeType.Rest:
                // Slice 4: show RestPanel — stub completes immediately
                mapManager.CompleteCurrentNode(nodeId);
                break;

            default:
                // Elite / Shop / RandomEvent — Slice 5 stub
                mapManager.CompleteCurrentNode(nodeId);
                break;
        }
    }

    private void StartCombat()
    {
        var runManager = ServiceLocator.Instance.Get<RunManager>();
        var characterManager = ServiceLocator.Instance.Get<CharacterManager>();
        var battleManager = ServiceLocator.Instance.Get<IBattleManager>();
        var localPlayer = runManager?.LocalPlayer;

        if (localPlayer == null)
        {
            Debug.LogWarning("[EventManager] LocalPlayer is null — cannot start combat.");
            return;
        }

        // Reset battle piles; HP and Deck persist across nodes
        localPlayer.BattleStart();

        CharacterInstance enemy = characterManager.CreateCharacter("Enemy1");
        if (enemy != null)
        {
            enemy.EnemyController = new SimpleEnemyController();
            characterManager.CreateCharacterUI(enemy, isEnemy: true);
        }

        runManager.SetMode(RunMode.Battle);
        battleManager?.StartBattle(localPlayer, new List<CharacterInstance> { enemy });
    }
}
