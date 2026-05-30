using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ExileCore2.Shared;
using ExileCore2.PoEMemory.Elements.Sanctum;
using static PathfindSanctum.RoomsByLayerFromUI;

namespace PathfindSanctum;

public class SanctumStateTracker
{
    private uint? currentAreaHash;
    private Dictionary<(int Layer, int Room), RoomState> roomStates = new();

    public List<List<FakeSanctumRoomElement>> roomsByLayer;
    public byte[][][] roomLayout;

    public int PlayerLayerIndex = -1;
    public int PlayerRoomIndex = -1;

    public bool HasRoomData()
    {
        return roomStates.Count > 0;
    }

    public bool IsSameSanctum(uint newAreaHash)
    {
        if (currentAreaHash == null)
        {
            currentAreaHash = newAreaHash;
            return false;
        }
        return currentAreaHash == newAreaHash;
    }

    public void UpdateRoomStates(SanctumFloorWindow floorWindow)
    {
        // FIXME: Use floorWindow.RoomsByLayer once it is available.
        this.roomsByLayer = RoomsByLayerFromUI.GetRoomsByLayer(floorWindow);
        if (roomsByLayer == null || roomsByLayer.Count == 0)
        {
            return;
        }

        // Update Layout Data. In PoE 2 / ExileCore2 0.5 FloorData.RoomLayout is often empty,
        // while room UI nodes are still present. Fall back to rebuilding the graph from
        // visible node positions so the best-path frames can still be drawn.
        this.roomLayout = floorWindow?.FloorData?.RoomLayout;
        if (!HasUsableRoomLayout(this.roomLayout, roomsByLayer))
        {
            this.roomLayout = BuildLayoutFromUI(roomsByLayer);
        }

        // Update Player Data
        var roomChoices = floorWindow?.FloorData?.RoomChoices;
        PlayerLayerIndex = roomChoices != null ? roomChoices.Count - 1 : -1;
        PlayerRoomIndex =
            roomChoices != null && roomChoices.Count > 0
                ? roomChoices.Last()
                : -1;

        // Update Room Data
        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                var sanctumRoom = roomsByLayer[layer][room];
                if (sanctumRoom == null)
                {
                    continue;
                }

                var key = (layer, room);
                if (!roomStates.ContainsKey(key))
                {
                    int numConnections = 0;

                    // ✅ zabezpieczenie przed IndexOutOfRange
                    if (roomLayout != null
                        && layer < roomLayout.Length
                        && roomLayout[layer] != null
                        && room < roomLayout[layer].Length)
                    {
                        numConnections = roomLayout[layer][room].Length;
                    }

                    roomStates[key] = new RoomState(sanctumRoom, numConnections);
                }
                else
                {
                    roomStates[key].UpdateRoom(sanctumRoom);
                    roomStates[key].Connections = GetConnectionCount(layer, room);
                }
            }
        }
    }

    private int GetConnectionCount(int layer, int room)
    {
        if (roomLayout != null
            && layer >= 0
            && layer < roomLayout.Length
            && roomLayout[layer] != null
            && room >= 0
            && room < roomLayout[layer].Length
            && roomLayout[layer][room] != null)
        {
            return roomLayout[layer][room].Length;
        }

        return 0;
    }

    private static bool HasUsableRoomLayout(byte[][][] layout, List<List<FakeSanctumRoomElement>> roomsByLayer)
    {
        if (layout == null || roomsByLayer == null || roomsByLayer.Count < 2)
            return false;

        if (layout.Length < roomsByLayer.Count - 1)
            return false;

        for (var layer = 0; layer < roomsByLayer.Count - 1; layer++)
        {
            if (layout[layer] == null || layout[layer].Length < roomsByLayer[layer].Count)
                return false;

            if (layout[layer].Any(x => x != null && x.Length > 0))
                return true;
        }

        return false;
    }

    private static byte[][][] BuildLayoutFromUI(List<List<FakeSanctumRoomElement>> roomsByLayer)
    {
        var result = new byte[Math.Max(roomsByLayer.Count - 1, 0)][][];

        for (var layer = 0; layer < roomsByLayer.Count - 1; layer++)
        {
            var currentLayer = roomsByLayer[layer];
            var nextLayer = roomsByLayer[layer + 1];
            result[layer] = new byte[currentLayer.Count][];

            for (var room = 0; room < currentLayer.Count; room++)
            {
                var current = currentLayer[room];
                var connections = new List<byte>();

                if (current == null)
                {
                    result[layer][room] = connections.ToArray();
                    continue;
                }

                var currentCenter = GetCenter(current.GetClientRect());
                var allDistances = nextLayer
                    .Select((target, index) => new
                    {
                        Index = index,
                        Target = target,
                        DistanceY = target == null ? float.MaxValue : Math.Abs(GetCenter(target.GetClientRect()).Y - currentCenter.Y),
                    })
                    .Where(x => x.Target != null)
                    .OrderBy(x => x.DistanceY)
                    .ToList();

                if (allDistances.Count() == 0)
                {
                    result[layer][room] = connections.ToArray();
                    continue;
                }

                // Sanctum links are only between adjacent visible columns. The first 0.5
                // fallback connected up to three nearest rows in every column, which can
                // create fake edges and make the recommended route jump to a room that is
                // not reachable in game. Normal Sanctum rooms visually branch to at most two
                // rooms in the next column; only the special entrance/boss-side fan-out may
                // have more.
                var roomHeight = Math.Max(20f, current.GetClientRect().Height);
                var rowSpacing = EstimateRowSpacing(currentLayer, nextLayer);
                var threshold = Math.Max(roomHeight * 1.45f, rowSpacing * 1.15f);

                // Always include the closest row, then include only one additional neighbour
                // for normal columns. This follows the drawn non-crossing Sanctum lanes much
                // better than the old Take(3) heuristic.
                var isEntranceFanOut = currentLayer.Count == 1 || layer == 0;
                var maxConnections = isEntranceFanOut ? Math.Min(3, nextLayer.Count) : 2;

                foreach (var target in allDistances.Where(x => x.DistanceY <= threshold).Take(maxConnections))
                {
                    connections.Add((byte)target.Index);
                }

                result[layer][room] = connections.Distinct().OrderBy(x => x).ToArray();
            }
        }

        return result;
    }


    private static float EstimateRowSpacing(params List<FakeSanctumRoomElement>[] layers)
    {
        var ys = layers
            .Where(layer => layer != null)
            .SelectMany(layer => layer)
            .Where(room => room != null)
            .Select(room => GetCenter(room.GetClientRect()).Y)
            .OrderBy(y => y)
            .ToList();

        var gaps = new List<float>();
        for (var i = 1; i < ys.Count; i++)
        {
            var gap = ys[i] - ys[i - 1];
            if (gap > 20f)
                gaps.Add(gap);
        }

        if (gaps.Count == 0)
            return 90f;

        gaps.Sort();
        return gaps[gaps.Count / 2];
    }

    private static Vector2 GetCenter(RectangleF rect)
    {
        return new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
    }

    public void Reset(uint newAreaHash)
    {
        currentAreaHash = newAreaHash;
        roomStates.Clear();
    }

    public RoomState GetRoom(int layer, int room)
    {
        return roomStates.TryGetValue((layer, room), out var state) ? state : null;
    }
}

public class RoomState
{
    public string RoomType { get; private set; }
    public string Affliction { get; private set; }
    public string Reward { get; private set; }
    public int Connections { get; internal set; }

    public Vector2 Position { get; internal set; }

    public RoomState(FakeSanctumRoomElement room, int numConnections)
    {
        Connections = numConnections;
        UpdateRoom(room);
    }

    public void UpdateRoom(FakeSanctumRoomElement newRoom)
    {
        var newRoomType = newRoom.Data.FightRoom?.RoomType.Id;
        var newAffliction = newRoom.Data?.RoomEffect?.ReadableName;
        var newReward = newRoom.Data.RewardRoom?.RoomType.Id;

        // Only update each field if we're getting new information (not null/empty)
        if (!string.IsNullOrEmpty(newRoomType))
            RoomType = newRoomType;
        if (!string.IsNullOrEmpty(newAffliction))
            Affliction = newAffliction;
        if (!string.IsNullOrEmpty(newReward))
            Reward = newReward;
        Position = newRoom.Position;
    }

    public override string ToString()
    {
        return $"Type: {RoomType}, Affliction: {Affliction}, Reward: {Reward}, Connections: {Connections}";
    }
}
