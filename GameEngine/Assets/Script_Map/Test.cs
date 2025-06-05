using UnityEngine;
using System.Collections.Generic;

public class BSPBlockMapGenerator : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject baseBlockPrefab;
    [SerializeField] private GameObject platformPrefab;

    [Header("Map Settings")]
    [SerializeField] private int mapWidth = 64;
    [SerializeField] private int mapHeight = 64;
    [SerializeField] private int minRoomSize = 8;
    [SerializeField] private int maxDepth = 5;

    [Header("Corridor Settings")]
    [SerializeField] private int corridorWidth = 3;

    [Header("Platform Settings")]
    [SerializeField] private int platformMinWidth = 4;

    private List<RectInt> rooms = new List<RectInt>();
    private List<(RectInt, RectInt)> connectedRooms = new List<(RectInt, RectInt)>();
    private HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();

    void Start()
    {
        RectInt rootRoom = new RectInt(0, 0, mapWidth, mapHeight);
        SplitRoom(rootRoom, 0);

        foreach (var pair in connectedRooms)
        {
            CreateCorridor(pair.Item1, pair.Item2);
        }

        foreach (var room in rooms)
        {
            GenerateRoom(room);
        }
    }

    void SplitRoom(RectInt room, int depth)
    {
        if (depth >= maxDepth || (room.width < minRoomSize * 2 && room.height < minRoomSize * 2))
        {
            rooms.Add(room);
            return;
        }

        bool splitHorizontally = room.width < room.height;
        if (room.width > room.height && Random.value > 0.5f) splitHorizontally = false;

        if (splitHorizontally)
        {
            int splitY = Random.Range(minRoomSize, room.height - minRoomSize);
            var top = new RectInt(room.x, room.y + splitY, room.width, room.height - splitY);
            var bottom = new RectInt(room.x, room.y, room.width, splitY);
            SplitRoom(top, depth + 1);
            SplitRoom(bottom, depth + 1);
            connectedRooms.Add((top, bottom));
        }
        else
        {
            int splitX = Random.Range(minRoomSize, room.width - minRoomSize);
            var left = new RectInt(room.x, room.y, splitX, room.height);
            var right = new RectInt(room.x + splitX, room.y, room.width - splitX, room.height);
            SplitRoom(left, depth + 1);
            SplitRoom(right, depth + 1);
            connectedRooms.Add((left, right));
        }
    }

    void CreateCorridor(RectInt a, RectInt b)
    {
        Vector2Int centerA = new Vector2Int(a.x + a.width / 2, a.y + a.height / 2);
        Vector2Int centerB = new Vector2Int(b.x + b.width / 2, b.y + b.height / 2);

        if (Random.value < 0.5f)
        {
            CreateHorizontalCorridor(centerA.x, centerB.x, centerA.y);
            CreateVerticalCorridor(centerA.y, centerB.y, centerB.x);
        }
        else
        {
            CreateVerticalCorridor(centerA.y, centerB.y, centerA.x);
            CreateHorizontalCorridor(centerA.x, centerB.x, centerB.y);
        }
    }

    void CreateHorizontalCorridor(int xStart, int xEnd, int y)
    {
        if (xStart > xEnd) (xStart, xEnd) = (xEnd, xStart);
        for (int x = xStart; x <= xEnd; x++)
        {
            for (int i = -corridorWidth / 2; i <= corridorWidth / 2; i++)
            {
                corridorTiles.Add(new Vector2Int(x, y + i));
            }
        }
    }

    void CreateVerticalCorridor(int yStart, int yEnd, int x)
    {
        if (yStart > yEnd) (yStart, yEnd) = (yEnd, yStart);
        for (int y = yStart; y <= yEnd; y++)
        {
            for (int i = -corridorWidth / 2; i <= corridorWidth / 2; i++)
            {
                corridorTiles.Add(new Vector2Int(x + i, y));
            }
        }
    }

    void GenerateRoom(RectInt room)
    {
        for (int x = room.x; x < room.xMax; x++)
        {
            for (int y = room.y; y < room.yMax; y++)
            {
                Vector2Int tilePos = new Vector2Int(x, y);

                if (corridorTiles.Contains(tilePos)) continue;

                bool isEdge = (x == room.x || x == room.xMax - 1 || y == room.y || y == room.yMax - 1);
                if (!isEdge) continue;

                Vector3 pos = new Vector3(x, y, 0);
                GameObject block = Instantiate(baseBlockPrefab, pos, Quaternion.identity, transform);
                block.layer = LayerMask.NameToLayer("BaseMap");

                // È¸Àü ¼³Á¤
                bool hasLeft = (x > room.x);
                bool hasRight = (x < room.xMax - 1);
                bool hasUp = (y < room.yMax - 1);
                bool hasDown = (y > room.y);

                if (!hasLeft) block.transform.rotation = Quaternion.Euler(0, 0, -90);
                else if (!hasRight) block.transform.rotation = Quaternion.Euler(0, 0, 90);
                else if (!hasUp) block.transform.rotation = Quaternion.Euler(0, 0, 180);
                else block.transform.rotation = Quaternion.identity;
            }
        }

        // Áß¾Ó ÇÃ·§Æû ¼³Ä¡
        if (room.width >= platformMinWidth && room.height >= 3)
        {
            int platformY = room.y + room.height / 2;
            int startX = room.x + (room.width - platformMinWidth) / 2;

            for (int i = 0; i < platformMinWidth; i++)
            {
                Vector2Int platformPos = new Vector2Int(startX + i, platformY);
                if (corridorTiles.Contains(platformPos)) continue;

                Vector3 pos = new Vector3(platformPos.x, platformPos.y, 0);
                GameObject platform = Instantiate(platformPrefab, pos, Quaternion.identity, transform);
                platform.layer = LayerMask.NameToLayer("Map");
            }
        }
    }
}
