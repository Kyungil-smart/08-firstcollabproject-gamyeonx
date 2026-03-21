using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "ScriptableObjects/MonsterData")]
public class MonsterDataSO : ScriptableObject
{
    [field: SerializeField] public int Id { get; private set; }
    [field: SerializeField] public string EName { get; private set; }
    [field: SerializeField] public string KName { get; private set; }
    [field: SerializeField][field: TextArea] public string Description { get; private set; }
    [field: SerializeField] public int Health { get; private set; }
    [field: SerializeField] public int Damage { get; private set; }
    [field: SerializeField] public int Defense { get; private set; }
    [field: SerializeField] public bool CanChasePlayer { get; private set; }

    public void SetData(string[] datas)
    {
        Id = int.Parse(datas[0]);
        EName = datas[1];
        KName = datas[2];
        Description = datas[3];
        Health = int.Parse(datas[4]);
        Damage = int.Parse(datas[5]);
        Defense = int.Parse(datas[6]);
        CanChasePlayer = bool.Parse(datas[7]);
    }
}
