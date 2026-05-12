public class SimpleEnemyController : IEnemyController
{
    public int AttackDamage;
    public EnemyIntent CurrentIntent { get; private set; }

    public SimpleEnemyController(int attackDamage = 6)
    {
        AttackDamage = attackDamage;
    }

    public void PlanNextAction(CharacterInstance self)
    {
        CurrentIntent = new EnemyIntent { Type = EnemyIntentType.Attack, Value = AttackDamage };
    }

    public void ExecuteAction(CharacterInstance self, CharacterInstance target)
    {
        target.TakeDamage(CurrentIntent.Value);
    }
}
