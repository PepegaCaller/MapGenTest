using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class TilemapGenerator : MonoBehaviour
{
    [SerializeField] private Tile grassTile;
    [SerializeField] private Tile sandTile;
    [SerializeField] private Tile waterTile;
    [SerializeField] private Tile treeTile;
    [SerializeField] private Tile enemyTile;
    [SerializeField] private Tile rockTileDec;
    [SerializeField] private Tile flowersTileDec;
    [SerializeField] private Tile plantTileDec;
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _obstacleTilemap;
    private int width = 20;
    private int height = 36;
    [SerializeField] private int minSandHeight = 3;
    [SerializeField] private int maxSandHeight = 12;


    [SerializeField] private List<WaterPreset> waterPresets;
    [SerializeField] private int minDistanceBetweenLakes = 3;
    private List<HashSet<Vector2Int>> waterBodies = new List<HashSet<Vector2Int>>();

    private int[,] mapData;  // 0 - grass , 10-19  - sand , 20-29 - water , 1 - grass with tree , 2 - grass with obstacle 

    [System.Serializable]
    public class WaterPreset
    {
        public string name;
        public Vector2Int[] pattern; // local transf param
        public int spawnWeight = 1;
    }
    void Awake()
    {
        mapData = new int[height, width];
        GenerateMap();
        RenderMap();

    }

    void GenerateMap()
    {
        FillWithGrass();
        GenerateBiome();
        FillWalls();
    }
    void FillWithGrass()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                mapData[y, x] = 0; 
            }
        }
    }

    void FillWalls()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if ((x < 4 || x > 15) && mapData[y, x] == 0)
                {
                    mapData[y, x] = 1;
                }
            }
        }
    }

    void GenerateBiome()
    {
        HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>(); 

        
        GenerateSandStrips(ref occupiedTiles);
        GenerateWaterBodies(ref occupiedTiles);
    }
    void GenerateWaterBodies(ref HashSet<Vector2Int> occupiedTiles)
    {
        int attempts = 0;
        int maxAttempts = 50;
        int waterBodyCount = Random.Range(1, 3);

        while (waterBodies.Count < waterBodyCount && attempts < maxAttempts)
        {
            attempts++;
            TrySpawnWaterPreset(ref occupiedTiles);
        }
    }

    bool TrySpawnWaterPreset(ref HashSet<Vector2Int> occupiedTiles)
    {
            WaterPreset preset = GetRandomPreset();
            Vector2Int center = GetRandomWaterPosition(preset);
            if (CanPlaceWater(center, preset, ref occupiedTiles))
            {
                PlaceWater(center, preset, ref occupiedTiles);
                return true;
            }
        return false;
    }

    WaterPreset GetRandomPreset()
    {
        int totalWeight = 0;
        foreach (var preset in waterPresets)
            totalWeight += preset.spawnWeight;

        int random = Random.Range(0, totalWeight);
        foreach (var preset in waterPresets)
        {
            if (random < preset.spawnWeight)
                return preset;
            random -= preset.spawnWeight;
        }
        return waterPresets[0];
    }

    Vector2Int GetRandomWaterPosition(WaterPreset preset)
    {
        int minX = 5;
        int maxX = 15;
        int minY = 5;
        int maxY = 30;

        return new Vector2Int(
            Random.Range(minX, maxX + 1),
            Random.Range(minY, maxY + 1)
        );
    }

    bool CanPlaceWater(Vector2Int center, WaterPreset preset, ref HashSet<Vector2Int> occupiedTiles)
    {
        var potentialTiles = new HashSet<Vector2Int>();

        foreach (Vector2Int offset in preset.pattern)
        {
            Vector2Int pos = center + offset;
            if (!IsInMapBounds(pos) || occupiedTiles.Contains(pos))
                return false;
            potentialTiles.Add(pos);
        }

        foreach (var body in waterBodies)
        {
            if (IsColliding(body, potentialTiles))
                return false;
        }

        return true;
    }
    bool IsColliding(HashSet<Vector2Int> existingBody, HashSet<Vector2Int> newBody)
    {
        foreach (var existingPos in existingBody)
        {
            foreach (var newPos in newBody)
            {
                if (Vector2Int.Distance(existingPos, newPos) < minDistanceBetweenLakes)
                    return true;
            }
        }
        return false;
    }
    
    
    void PlaceWater(Vector2Int center, WaterPreset preset, ref HashSet<Vector2Int> occupiedTiles)
    {
        var newBody = new HashSet<Vector2Int>();
        foreach (Vector2Int offset in preset.pattern)
        {
            Vector2Int pos = center + offset;
            occupiedTiles.Add(pos);
            mapData[pos.y, pos.x] = 20;
        }
        waterBodies.Add(newBody);
    }

    bool IsInMapBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
  
   
    void RenderMap()
    {
        _groundTilemap.ClearAllTiles();
        _obstacleTilemap.ClearAllTiles();

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
                        groundTile = grassTile;
                        objectTile = treeTile;
                        break;
                    case 10:
                        groundTile = sandTile;
                        objectTile = EnemyDraw();
                        break;
                    case 20:
                        groundTile = waterTile;
                        break;
                }

                _groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);

                if (objectTile != null)
                {
                    _obstacleTilemap.SetTile(new Vector3Int(x, y, 1), objectTile);
                }
            }
        }
    }
    


    
    void GenerateSandStrips(ref HashSet<Vector2Int> occupiedTiles)
    {
        int attempts = 0;
        int maxAttempts = 50;
        int stripCount = Random.Range(1, 4);
        int stripReady = 0;
        do
        {
            bool success = TryGenerateSandStrip(ref occupiedTiles);
            if (success) stripReady++;
            attempts++;
        } while (attempts < maxAttempts && stripReady<stripCount);
        
    }

    
    bool TryGenerateSandStrip(ref HashSet<Vector2Int> occupiedTiles)
    {
        var spawnZones = new List<(int min, int max)>
    {
        (4, 7), (8, 11), (12, 15)
    };

        var selectedZone = spawnZones[Random.Range(0, spawnZones.Count)];
        int zoneMin = selectedZone.min;
        int zoneMax = selectedZone.max;

        int stripWidth = Random.Range(2, 5);
        stripWidth = Mathf.Min(stripWidth, zoneMax - zoneMin + 1);

        int stripHeight = Random.Range(3, 13);
        int maxPossibleStartX = zoneMax - stripWidth + 1;
        int startX = Random.Range(zoneMin, Mathf.Max(zoneMin, maxPossibleStartX) + 1);
        int startY = Random.Range(0, height - stripHeight + 1);

        
        HashSet<Vector2Int> tempPositions = new HashSet<Vector2Int>();
        int currentCenterX = startX + stripWidth / 2;
        int halfWidth = stripWidth / 2;

        
        for (int y = startY; y < startY + stripHeight; y++)
        {
            currentCenterX += Random.Range(-1, 2);
            currentCenterX = Mathf.Clamp(
                currentCenterX,
                zoneMin + halfWidth,
                zoneMax - (stripWidth % 2 == 0 ? halfWidth - 1 : halfWidth)
            );

            int left = Mathf.Max(currentCenterX - halfWidth, zoneMin);
            int right = Mathf.Min(currentCenterX + (stripWidth % 2 == 0 ? halfWidth - 1 : halfWidth), zoneMax);

            for (int x = left; x <= right; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (occupiedTiles.Contains(pos))
                    return false;

                tempPositions.Add(pos);
            }
        }
        foreach (var pos in tempPositions)
        {
            occupiedTiles.Add(pos);
            mapData[pos.y, pos.x] = 10;
        }

        return true;
    }
    Tile EnemyDraw()
    {
        int weight = Random.Range(1, 10);
        if (weight == 1)
        {
            return enemyTile;
        }
        else
        {
            return null;
        }
    }
    Tile DecorateGrass()
    {
        int weight = Random.Range(1, 41);
        if (weight == 1)
        {
            return flowersTileDec;
        }
        else if (weight == 2)
        {
            ;
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
}