public interface ITower
{
    int Level { get; }
    bool CanUpgrade();
    int GetUpgradeCost();
    bool TryUpgrade();
}
