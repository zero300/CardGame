public interface IEnemyController
{
    EnemyIntent CurrentIntent { get; }
    void PlanNextAction(CharacterInstance self);
    void ExecuteAction(CharacterInstance self, CharacterInstance target);
}
