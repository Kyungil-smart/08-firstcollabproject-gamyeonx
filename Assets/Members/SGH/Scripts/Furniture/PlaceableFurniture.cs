using UnityEngine;

public class PlaceableFurniture : MonoBehaviour
{
    private Camera _mainCamera;
    private LayerMask _placementLayer;
    private bool _isPlacing = false; // 설치 중 상태 분기
    private Furniture _currentFurniture; // 이동/설치/취소 대상 참조

    private void Awake()
    {
        _mainCamera = Camera.main;
        _placementLayer = LayerMask.GetMask("InBuildingTile");
    }

    public void Creation(GameObject furniturePrefab)
    {
        if (_isPlacing) return;

        _currentFurniture = Instantiate(furniturePrefab, Vector3.zero, Quaternion.identity).GetComponent<Furniture>();
        _isPlacing = true;
    }

    private void Update()
    {
        Vector2 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if (_isPlacing && _currentFurniture != null)
        {
            _currentFurniture.transform.position = mouseWorld;

            Collider2D targetTile = Physics2D.OverlapPoint(mouseWorld, _placementLayer);

            if (targetTile != null)
            {
                bool occupied = false;
                foreach (Transform child in targetTile.transform)
                {
                    if (child.GetComponent<Furniture>() != null && child != _currentFurniture.transform)
                    {
                        occupied = true;
                        break;
                    }
                }

                if (!occupied)
                {
                    _currentFurniture.transform.position = targetTile.transform.position;

                    if (Input.GetMouseButtonDown(0))
                        PlaceFurniture(targetTile);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
                CancelPlacement();
        }
        else
        {
            Collider2D hit = Physics2D.OverlapPoint(mouseWorld, _placementLayer);

            if (hit != null)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    Furniture f = hit.GetComponentInChildren<Furniture>();
                    if (f != null)
                        Destroy(f.gameObject);
                }

                if (Input.GetMouseButtonDown(2))
                {
                    Furniture f = hit.GetComponentInChildren<Furniture>();
                    if (f != null)
                    {
                        _currentFurniture = f;
                        _isPlacing = true;
                        _currentFurniture.transform.parent = null;
                    }
                }
            }
        }
    }

    // 설치 확정
    private void PlaceFurniture(Collider2D tile)
    {
        _isPlacing = false;

        if (_currentFurniture != null)
            _currentFurniture.transform.parent = tile.transform; // 가구를 설치 타일에 자식으로

        _currentFurniture = null;
    }

    // 설치 취소
    private void CancelPlacement()
    {
        _isPlacing = false;

        if (_currentFurniture != null)
        {
            Destroy(_currentFurniture.gameObject);
            _currentFurniture = null;
        }
    }
}