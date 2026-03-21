using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public SheetData _monsterSheet;

    [SerializeField] private List<MonsterDataSO> _monsterDataList;
    private Dictionary<string, MonsterDataSO> _monsterDataDictionary;

    private void Awake() => InitMonsterDataDictionary();

    private void Start() => StartCoroutine(_monsterSheet.Load(SetMonsterDatas));

    public void SetMonsterDatas(char splitSymbol, string[] linse)
    {
        if (linse == null) return;

        for (int i = 3; i < linse.Length; i++)
        {
            string[] cols = linse[i].Split(splitSymbol);

            MonsterDataSO monster;

            if (_monsterDataDictionary.ContainsKey(cols[1]))
            {
                monster = _monsterDataDictionary[cols[1]];
            }
            else
            {
                monster = ScriptableObject.CreateInstance<MonsterDataSO>();
                monster.name = cols[1];
                _monsterDataDictionary.Add(cols[1], monster);
                Debug.LogWarning($"<color=yellow>MonsterSO 누락, {cols[1]} 추가해야 함.</color>");

                _monsterDataList.Add(monster);
            }

            monster.SetData(cols);
        }
    }

    private void InitMonsterDataDictionary()
    {
        _monsterDataDictionary = _monsterDataList.ToDictionary(mon => mon.name);
        _monsterDataList.Clear();
        _monsterDataList = null;
    }
}
