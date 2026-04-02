using System.Collections.Generic;
using UnityEngine;


public class GuestRoadWanderSelector : MonoBehaviour
{
    [Header("배회 길 선택 설정")]
    [SerializeField] private int _wanderSearchRadius = 6;
    [SerializeField] private int _wanderMinCellDistance = 2;
    [SerializeField] private int _wanderRandomPickTryCount = 10;


    public bool TryGetRandomRoadCell(out Vector3Int resultCell)
    {
        resultCell = Vector3Int.zero;

        if(GridBuildingSystem.Instance == null)
        {
            return false;
        }

        Vector3Int currentCell = GridBuildingSystem.Instance.gridLayout.WorldToCell(transform.position);

        if(GridBuildingSystem.Instance.GetTileType(currentCell) != TileType.Road)
        {
            return false;
        }

        List<Vector3Int> candidateCells = CollectWanderRoadCandidates(currentCell);

        if(candidateCells.Count == 0)
        {
            return false;
        }

        for(int i = 0; i < _wanderRandomPickTryCount; i++)
        {
            int randomIndex = Random.Range(0, candidateCells.Count);
            Vector3Int picked = candidateCells[randomIndex];

            if(picked == currentCell)
            {
                continue;
            }

            resultCell = picked;
            return true;
        }

        resultCell = candidateCells[0];
        return true;
    }

    private List<Vector3Int> CollectWanderRoadCandidates(Vector3Int centerCell)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        for(int x = -_wanderSearchRadius; x <= _wanderSearchRadius; x++)
        {
            for(int y = -_wanderSearchRadius; y <= _wanderSearchRadius; y++)
            {
                Vector3Int cell = new Vector3Int(centerCell.x + x, centerCell.y + y, 0);

                if(cell == centerCell)
                {
                    continue;
                }

                if(!IsFarEnoughForWander(centerCell, cell))
                {
                    continue;
                }

                if(GridBuildingSystem.Instance.GetTileType(cell) != TileType.Road)
                {
                    continue;
                }

                result.Add(cell);
            }
        }

        return result;
    }

    private bool IsFarEnoughForWander(Vector3Int origin, Vector3Int target)
    {
        int distance = Mathf.Abs(origin.x - target.x) + Mathf.Abs(origin.y - target.y);
        return distance >= _wanderMinCellDistance;
    }
}