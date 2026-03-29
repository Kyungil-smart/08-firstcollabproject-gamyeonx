using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder
{
    private List<Node> _openList = new List<Node>(); // 탐색할 노드들 저장
    
    // 이미 탐색 끝난 노드의 좌표 저장
    private HashSet<Vector3Int> _closedSet = new HashSet<Vector3Int>(); 
    
    //좌표로 노드 관리, 좌표에 Node가 이미 있는지 없는지 확인하는 용도
    private Dictionary<Vector3Int, Node> _allNodes = new Dictionary<Vector3Int, Node>();

    // GetNeighbors 메서드(이웃 Node를 불러올 때)에 사용할 List
    private List<Vector3Int> _direction = new List<Vector3Int>();
    
    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int target)
    {
        DataStructInit(); // A* 사용할때마다 필드 자료구조 초기화
        
        Node startNode = new Node(start); // 시작 노드
        startNode.GCost = 0; // 시작 노드니까 GCost는 0
        _openList.Add(startNode); // 시작 노드 openList에 추가
        
        _allNodes[start] = startNode;

        while (_openList.Count > 0)
        {
            Node current = _openList[0];
            // 가장 낮은 FCost 찾기
            for (int i = 1; i < _openList.Count; i++)
            {
                // current Node와 비교해서 FCost가 더 낮거나, FCost가 같을 경우는 HCost가 더 작은 Node 선택 
                if (_openList[i].FCost < current.FCost ||
                   (_openList[i].FCost == current.FCost && _openList[i].HCost < current.HCost))
                    current = _openList[i];
            }
            
            // current Node가 선택 됐다면 방문한 Node로 취급(openList에서 삭제, closedSet에 추가) 
            _openList.Remove(current); 
            _closedSet.Add(current.Position); 

            // 현재 Node가 도착점이라면, RetracePath 메서드 실행 후 반환
            if (current.Position == target)
            {
                return RetracePath(current);
            }
            
            // 현재 Node로 부터 4방향(상하좌우)의 Node들을 탐색
            foreach (Vector3Int neighborPos in GetNeighbors(current.Position, _direction))
            {
                // 이미 방문한 Node라면 무시
                if (_closedSet.Contains(neighborPos)) 
                    continue;
                
                // 길이 아닌 Node라면 무시
                if (!IsWalkable(neighborPos))
                    continue;
                
                // 이웃 Node의 GCost는 현재 Node의 GCost에 10을 더한 값
                int newGCost = current.GCost + 10;

                
                // 이웃 Node가 allNodes에 있는지(없으면 true, 있으면 false),즉 처음 생성 된 Node인지
                Node neighbor;
                bool isNew = !_allNodes.TryGetValue(neighborPos, out neighbor);
                
                // 이웃 Node가 없었다면 생성해주고 allNodes에 넣어줌
                if (isNew)
                {
                    neighbor = new Node(neighborPos);
                    _allNodes[neighborPos] = neighbor;
                }
                
                // Node가 새로 생성됐거나, 이미 생성 됐지만 newGCost값이 기존 GCost보다 작을 경우 G,H,Parent 갱신
                if (isNew || newGCost < neighbor.GCost)
                {
                    neighbor.GCost = newGCost;
                    neighbor.HCost = GetDistance(neighbor.Position, target);
                    neighbor.Parent = current;
                    
                    // openList에 포함돼 있지 않다면, 포함시켜 준다.
                    if (!_openList.Contains(neighbor))
                        _openList.Add(neighbor);
                }
            }
        }
        return null; // 도착할 수 없다면 null 반환
    }

    // 도착점에 도달했을 때 실행되는 메서드
    List<Vector3Int> RetracePath(Node endNode)
    {
        List<Vector3Int> path = new List<Vector3Int>();

        Node current = endNode;
        
        // Node의 Parents를 차례대로 path에 담아준다.(도착점에서 시작점 순으로 담김)
        while (current != null)
        {
            path.Add(current.Position);
            current = current.Parent;
        }
        
        // 도착점부터 시작점까지 path에 저장돼 있으므로 역순으로 다시 담아줌(시작점 부터 도착점)
        path.Reverse();
        return path;
    }
    
    // 현재 Node의 상하좌우의 Node들의 좌표를 얻어오는 메서드
    List<Vector3Int> GetNeighbors(Vector3Int pos, List<Vector3Int> direction)
    {
        direction.Clear();
        
        direction.Add(pos + Vector3Int.up);
        direction.Add(pos + Vector3Int.down);
        direction.Add(pos + Vector3Int.left);
        direction.Add(pos + Vector3Int.right);
        
        return direction;
    }
    
    // 시작점부터 도작첨까지의 거리를 계산해주는 메서드
    int GetDistance(Vector3Int a, Vector3Int b)
    {
        // 시작점과 도착점의 x축,y축의 차를 철대값으로 계산 후 더한 값
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
    
    // Node의 TileType이 Road인지 아닌지 판별
    bool IsWalkable(Vector3Int pos)
    {
        return GridBuildingSystem.Instance.GetTileType(pos) == TileType.Road;
    }

    // 필드 자료구조 초기화
    void DataStructInit()
    {
        _openList.Clear();
        _closedSet.Clear();
        _allNodes.Clear();
    }
}