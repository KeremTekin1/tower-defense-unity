using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "TowerDefense/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float speed;
    public float maxHealth;
    public int moneyReward;
    public Color color;
}
