using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public int width = 20;
    public int height = 36;

    public Tilemap groundMap;
    public Tilemap objectMap;

    public Tile grassTile;
    public Tile sandTile;
    public Tile waterTile;
    public Tile treeTile;
    public Tile fallenTreeTile;
    public Tile flowersTileDec;
    public Tile plantTileDec;
    public Tile rockTileDec;
    public Tile enemyTile;

    private int[,] mapData;

    void Start()
    {
        GenerateMap();
        RenderMap();
    }

    void GenerateMap()
    {
        mapData = new int[height, width];

        FillWithGrass();
        GenerateBiome();
        PlaceObstacles();
    }

    void FillWithGrass()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                mapData[y, x] = 0; // �����
            }
        }
    }
    void GenerateBiome()
    {
        HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>(); // ����� ����� ������� ������

        // ���������� ����
        PlaceWaterPatches(ref occupiedTiles);

        // ���������� ����� �����
        PlaceSandPatches(ref occupiedTiles);
    }

    void PlaceWaterPatches(ref HashSet<Vector2Int> occupiedTiles)
    {
        int waterPatches = Random.Range(0, 2);

        for (int i = 0; i < waterPatches; i++)
        {
            HashSet<Vector2Int> newWaterPatch;
            bool validPosition;

            do
            {
                Vector2Int startPos = new Vector2Int(Random.Range(1, width - 1), Random.Range(1, height - 1));
                newWaterPatch = new HashSet<Vector2Int>(GenerateRandomWaterShape(startPos, 10, 20));
                validPosition = !IsOverlapping(newWaterPatch, occupiedTiles);
            }
            while (!validPosition); // ���� ������������, ������� �����

            // ��������� ��� ������ �� ������ ����� � �������
            foreach (var tile in newWaterPatch)
            {
                if (tile.x >= 0 && tile.x < width && tile.y >= 0 && tile.y < height)
                {
                    occupiedTiles.Add(tile);
                    mapData[tile.y, tile.x] = 3; // �����
                }
            }
        }
    }

    void PlaceSandPatches(ref HashSet<Vector2Int> occupiedTiles)
    {
        int sandPatches = Random.Range(2, 4);

        for (int i = 0; i < sandPatches; i++)
        {
            bool validPosition;
            HashSet<Vector2Int> newSandPatch;

            do
            {
                Vector2Int startPos = new Vector2Int(Random.Range(1, width - 13), Random.Range(1, height - 4)); // ��������� ������ �����
                newSandPatch = GenerateConnectedSandShape(startPos);
                validPosition = !IsOverlapping(newSandPatch, occupiedTiles); // �������� �� ����������� � ������� � ������� ���������
                validPosition = validPosition && !IsSandOverlappingWithOtherSand(newSandPatch, occupiedTiles); // �������� �� ���������� � ������� ������� �����
            }
            while (!validPosition); // ���� ������������, ������� �����

            // ��������� ��� ������ �� ������ ����� ����� � �������
            foreach (var tile in newSandPatch)
            {
                if (tile.x >= 0 && tile.x < width && tile.y >= 0 && tile.y < height)
                {
                    occupiedTiles.Add(tile);
                    mapData[tile.y, tile.x] = 1; // �����
                }
            }
        }
    }

    bool IsOverlapping(HashSet<Vector2Int> newPatch, HashSet<Vector2Int> occupiedTiles)
    {
        foreach (var tile in newPatch)
        {
            if (occupiedTiles.Contains(tile))
            {
                return true; // ����� ����������� ������� ������
            }
        }
        return false; // ����� �� �����������
    }

    bool IsSandOverlappingWithOtherSand(HashSet<Vector2Int> newSandPatch, HashSet<Vector2Int> occupiedTiles)
    {
        foreach (var tile in newSandPatch)
        {
            // ���������, �� ������� �� ������ �� ������� �������
            if (tile.x >= 0 && tile.x < width && tile.y >= 0 && tile.y < height)
            {
                // ���������, �������� �� ��� ������ ������ � ���� ��� ��� ������ ������ ������ �����
                if (occupiedTiles.Contains(tile) && mapData[tile.y, tile.x] == 1) // 1 � ��� �����
                {
                    return true; // ����� ����� ������������ � ������ ������ �����
                }
            }
        }
        return false; // ����� ����� �� ������������
    }

    HashSet<Vector2Int> GenerateConnectedSandShape(Vector2Int start)
    {
        HashSet<Vector2Int> sandPatch = new HashSet<Vector2Int>();
        int patchSize = Random.Range(3, 13); // ������ ����� ����� �� 3 �� 12 ������

        Vector2Int currentPos = start;
        sandPatch.Add(currentPos); // �������� � ����� ������

        // ��������� ����� �����, ���������� �� ����� �� ����
        for (int i = 1; i < patchSize; i++)
        {
            Vector2Int nextPos = currentPos;
            List<Vector2Int> possibleDirections = new List<Vector2Int>();

            // ��������� ��������� ����������� ��� ��������� ������ (�������������� � ������������)
            possibleDirections.Add(new Vector2Int(1, 0));  // ������
            possibleDirections.Add(new Vector2Int(-1, 0)); // �����
            possibleDirections.Add(new Vector2Int(0, 1));  // ����
            possibleDirections.Add(new Vector2Int(0, -1)); // �����

            // ��������� ������� �������� ���� �� �����������
            Vector2Int direction = possibleDirections[Random.Range(0, possibleDirections.Count)];
            nextPos = new Vector2Int(currentPos.x + direction.x, currentPos.y + direction.y);

            // ���������, �� ������� �� ������ �� ������� �����
            if (nextPos.x < 0 || nextPos.x >= width || nextPos.y < 0 || nextPos.y >= height)
            {
                continue; // ���� ������� �� ������� �����, ������� ������ �����������
            }

            // ��������� ����� ������ � ����� �����
            sandPatch.Add(nextPos);
            currentPos = nextPos; // ��������� ������� �������

            // ���������������, ����� ��������� ������������ ������ �����
            if (sandPatch.Count >= patchSize)
            {
                break;
            }
        }

        // ��������, ��� ����� �� ������ � ������������� ������������ �������
        return sandPatch.Count >= 3 ? sandPatch : new HashSet<Vector2Int>();
    }


    HashSet<Vector2Int> GenerateRandomWaterShape(Vector2Int start, int minSize, int maxSize)
    {
        HashSet<Vector2Int> waterTiles = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        waterTiles.Add(start);

        int targetSize = Random.Range(minSize, maxSize + 1);

        while (waterTiles.Count < targetSize && queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            // ��������� ����� ������ ��������� �������
            foreach (Vector2Int dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, new Vector2Int(1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1), new Vector2Int(-1, 1) })
            {
                Vector2Int next = current + dir;

                // ���������, ��� ������ ��������� � �������� ����� � ��� �� ���� ���������
                if (!waterTiles.Contains(next) && waterTiles.Count < targetSize &&
                    next.x >= 0 && next.x < width && next.y >= 0 && next.y < height)
                {
                    waterTiles.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        return new HashSet<Vector2Int>(waterTiles);
    }


    void PlaceObstacles()
    {
        PlaceObjects(treeTile, 2, 8, 2);       // ������� (������������)
        PlaceObjects(fallenTreeTile, 0, 2, 4); // ���������� ������� (����������)
        PlaceEnemy(enemyTile, 1, 1, 1);
    }

    void PlaceObjects(Tile tile, int minCount, int maxCount, int type)
    {
        int count = Random.Range(minCount, maxCount + 1);
        HashSet<Vector2Int> placedObjects = new HashSet<Vector2Int>();

        for (int i = 0; i < count; i++)
        {
            Vector2Int position;
            bool validPosition;

            do
            {
                position = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
                validPosition = mapData[position.y, position.x] == 0 && !HasNearbyObject(position, placedObjects);
            }
            while (!validPosition);

            mapData[position.y, position.x] = type;
            placedObjects.Add(position);
        }
    }
    
    void PlaceEnemy(Tile tile, int minCount, int maxCount, int type)
    {
        int count = Random.Range(minCount, maxCount + 1);
        HashSet<Vector2Int> placedObjects = new HashSet<Vector2Int>();

        for (int i = 0; i < count; i++)
        {
            Vector2Int position;
            bool validPosition;

            do
            {
                position = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
                validPosition = mapData[position.y, position.x] == 1 && !HasNearbyObject(position, placedObjects);
            }
            while (!validPosition);

            mapData[position.y, position.x] = type;
            placedObjects.Add(position);
        }
    }

    bool HasNearbyObject(Vector2Int pos, HashSet<Vector2Int> placedObjects)
    {
        foreach (Vector2Int dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        {
            if (placedObjects.Contains(pos + dir))
            {
                return true;
            }
        }
        return false;
    }

    Tile DecorateGrass()
    {
        int weight = Random.Range(1, 21);
        if (weight ==1)
        {
            return flowersTileDec;
        }
        else if (weight ==2)
        {;
            return plantTileDec;
        }
        else if (weight == 3)
        {
            return rockTileDec;
        }
        else
        {
            return null;
        }
    }

    Tile EnemyDraw()
    {
        int weight = Random.Range(1, 10);
        if(weight==1)
        {
            return enemyTile;
        }
        else
        {
            return null;
        }
    }

    void RenderMap()
    {
        groundMap.ClearAllTiles();
        objectMap.ClearAllTiles();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Tile groundTile = null;
                Tile objectTile = null;

                switch (mapData[y, x])
                {
                    case 0:
                        groundTile = grassTile;
                        objectTile = DecorateGrass();
                        break;
                    case 1:
                        groundTile = sandTile;
                        objectTile = EnemyDraw();
                        break;
                    case 2:
                        groundTile = grassTile;
                        objectTile = treeTile;
                        break;
                    case 3:
                        groundTile = waterTile;
                        break;
                    case 4:
                        groundTile = grassTile;
                        objectTile = fallenTreeTile;
                        break;
                }

                groundMap.SetTile(new Vector3Int(x, y, 0), groundTile);

                if (objectTile != null)
                {
                    objectMap.SetTile(new Vector3Int(x, y, 1), objectTile);
                }
            }
        }
    }
}