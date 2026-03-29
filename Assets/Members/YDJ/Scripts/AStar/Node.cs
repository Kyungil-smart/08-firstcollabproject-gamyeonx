using UnityEngine;

public class Node
{
    public Vector3Int Position; // 현재 노드의 위치 좌표
    
    // Cost 기준은 10
    public int GCost; // 시작점에서 현재 노드까지 이동 비용
    public int HCost; // 현재 노드에서 도착점까지 이동 비용
    
    public int FCost { get => GCost + HCost; } // GCost와 HCost의 합

    public Node Parent; // 현재 노드의 부모노드가 누군지

    public Node(Vector3Int position)
    {
        this.Position = position;
    }
}