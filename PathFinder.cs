using System.Collections.Generic;
using System.Linq;
using ExileCore2;
using ExileCore2.Shared;

namespace PathfindSanctum;

/// <summary>
/// Handles Dijkstra Pathfinding logic for Sanctum, calculating optimal routes based on room weights.
/// </summary>
public class PathFinder(
    Graphics graphics,
    PathfindSanctumSettings settings,
    SanctumStateTracker sanctumStateTracker,
    WeightCalculator weightCalculator
)
{
    private double[,] roomWeights;
    private readonly Dictionary<(int, int), string> debugTexts = [];

    private List<(int, int)> foundBestPath;

    #region Path Calculation
    public void CreateRoomWeightMap()
    {
        var roomsByLayer = sanctumStateTracker.roomsByLayer;
        if (roomsByLayer == null || roomsByLayer.Count == 0)
        {
            roomWeights = null;
            debugTexts.Clear();
            return;
        }

        var maxRoomsInLayer = roomsByLayer.Max(x => x?.Count ?? 0);
        if (maxRoomsInLayer <= 0)
        {
            roomWeights = null;
            debugTexts.Clear();
            return;
        }

        roomWeights = new double[roomsByLayer.Count, maxRoomsInLayer];

        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            if (roomsByLayer[layer] == null)
                continue;

            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                var sanctumRoom = roomsByLayer[layer][room];
                if (sanctumRoom == null)
                    continue;

                var stateTrackerRoom = sanctumStateTracker.GetRoom(layer, room);
                var (weight, debug) = weightCalculator.CalculateRoomWeight(stateTrackerRoom);
                roomWeights[layer, room] = weight;
                debugTexts[(layer, room)] = debug;
            }
        }
    }

    public List<(int, int)> FindBestPath()
    {
        var roomsByLayer = sanctumStateTracker.roomsByLayer;
        if (roomsByLayer == null || roomsByLayer.Count == 0 || roomWeights == null)
        {
            foundBestPath = new List<(int, int)>();
            return foundBestPath;
        }

        // PoE/EC2 0.5 changed RoomChoices/RoomLayout behaviour.  The safest way is to
        // calculate the route forward from the currently selectable/current layer to the
        // boss.  If FloorData.RoomChoices is stale/invalid, fall back to the first layer
        // so white boxes are still drawn on the suggested route.
        var startNodes = GetStartNodes(roomsByLayer).ToList();
        if (startNodes.Count == 0)
        {
            foundBestPath = new List<(int, int)>();
            return foundBestPath;
        }

        List<(int, int)> bestOverallPath = new();
        double bestOverallCost = double.MinValue;
        int bestOverallDepth = -1;

        foreach (var startNode in startNodes)
        {
            var bestPath = new Dictionary<(int, int), List<(int, int)>>
            {
                { startNode, new List<(int, int)> { startNode } }
            };
            var maxCost = new Dictionary<(int, int), double>();

            for (int i = 0; i < roomsByLayer.Count; i++)
            {
                if (roomsByLayer[i] == null)
                    continue;

                for (int j = 0; j < roomsByLayer[i].Count; j++)
                {
                    maxCost[(i, j)] = double.MinValue;
                }
            }

            maxCost[startNode] = roomWeights[startNode.Item1, startNode.Item2];

            var queue = new SortedSet<(int, int)>(
                Comparer<(int, int)>.Create(
                    (a, b) =>
                    {
                        double costA = maxCost[a];
                        double costB = maxCost[b];
                        if (costA != costB)
                            return costB.CompareTo(costA);
                        return a.CompareTo(b);
                    }
                )
            )
            {
                startNode
            };

            while (queue.Any())
            {
                var currentRoom = queue.First();
                queue.Remove(currentRoom);

                foreach (var neighbor in GetForwardNeighbors(currentRoom, sanctumStateTracker.roomLayout, roomsByLayer))
                {
                    if (!maxCost.ContainsKey(neighbor))
                        continue;

                    double neighborCost = maxCost[currentRoom] + roomWeights[neighbor.Item1, neighbor.Item2];
                    if (neighborCost > maxCost[neighbor])
                    {
                        queue.Remove(neighbor);
                        maxCost[neighbor] = neighborCost;
                        queue.Add(neighbor);
                        bestPath[neighbor] = new List<(int, int)>(bestPath[currentRoom]) { neighbor };
                    }
                }
            }

            var candidate = bestPath
                .OrderByDescending(pair => pair.Value.Last().Item1) // deepest path first
                .ThenByDescending(pair => maxCost.GetValueOrDefault(pair.Key, double.MinValue))
                .FirstOrDefault();

            if (candidate.Value != null)
            {
                var depth = candidate.Value.Last().Item1;
                var cost = maxCost.GetValueOrDefault(candidate.Key, double.MinValue);
                if (depth > bestOverallDepth || (depth == bestOverallDepth && cost > bestOverallCost))
                {
                    bestOverallDepth = depth;
                    bestOverallCost = cost;
                    bestOverallPath = candidate.Value;
                }
            }
        }

        foundBestPath = bestOverallPath ?? new List<(int, int)>();
        return foundBestPath;
    }

    private IEnumerable<(int, int)> GetStartNodes(List<List<RoomsByLayerFromUI.FakeSanctumRoomElement>> roomsByLayer)
    {
        // EC2/PoE2 0.5 makes FloorData.RoomChoices unreliable on the map screen.
        // If we start from that stale value the computed path is often only one room long,
        // so nothing visible is framed.  Prefer the first visible column with outgoing
        // connections; this restores the old "white squares where to go" behaviour.
        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            var rooms = roomsByLayer[layer];
            if (rooms == null || rooms.Count == 0)
                continue;

            var yieldedAny = false;
            for (var room = 0; room < rooms.Count; room++)
            {
                if (GetForwardNeighbors((layer, room), sanctumStateTracker.roomLayout, roomsByLayer).Any())
                {
                    yieldedAny = true;
                    yield return (layer, room);
                }
            }

            if (yieldedAny)
                yield break;
        }

        // Last-resort fallback: first non-empty layer, even if the graph is incomplete.
        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            var rooms = roomsByLayer[layer];
            if (rooms == null || rooms.Count == 0)
                continue;

            for (var room = 0; room < rooms.Count; room++)
                yield return (layer, room);
            yield break;
        }
    }

    private static IEnumerable<(int, int)> GetForwardNeighbors(
        (int, int) currentRoom,
        byte[][][] connections,
        List<List<RoomsByLayerFromUI.FakeSanctumRoomElement>> roomsByLayer
    )
    {
        int currentLayerIndex = currentRoom.Item1;
        int currentRoomIndex = currentRoom.Item2;
        int nextLayerIndex = currentLayerIndex + 1;

        if (connections == null || roomsByLayer == null || nextLayerIndex >= roomsByLayer.Count)
            yield break;

        if (currentLayerIndex < 0 || currentLayerIndex >= connections.Length)
            yield break;

        var currentLayer = connections[currentLayerIndex];
        if (currentLayer == null || currentRoomIndex < 0 || currentRoomIndex >= currentLayer.Length)
            yield break;

        var connectedRooms = currentLayer[currentRoomIndex];
        if (connectedRooms == null)
            yield break;

        foreach (var nextRoom in connectedRooms)
        {
            if (nextRoom >= 0 && nextRoom < roomsByLayer[nextLayerIndex].Count)
                yield return (nextLayerIndex, nextRoom);
        }
    }

    private static IEnumerable<(int, int)> GetNeighbors(
        (int, int) currentRoom,
        byte[][][] connections
    )
    {
        int currentLayerIndex = currentRoom.Item1;
        int currentRoomIndex = currentRoom.Item2;
        int previousLayerIndex = currentLayerIndex - 1;

        if (connections == null || currentLayerIndex <= 0)
        {
            yield break; // brak sąsiadów
        }

        // ✅ zabezpieczenie granic
        if (previousLayerIndex < 0 || previousLayerIndex >= connections.Length)
        {
            yield break;
        }

        byte[][] previousLayer = connections[previousLayerIndex];
        if (previousLayer == null)
        {
            yield break;
        }

        for (int previousLayerRoomIndex = 0; previousLayerRoomIndex < previousLayer.Length; previousLayerRoomIndex++)
        {
            var previousLayerRoom = previousLayer[previousLayerRoomIndex];
            if (previousLayerRoom == null)
            {
                continue;
            }

            if (previousLayerRoom.Contains((byte)currentRoomIndex))
            {
                yield return (previousLayerIndex, previousLayerRoomIndex);
            }
        }
    }
    #endregion

    #region Visualization
    public void DrawDebugInfo()
    {
        if (!settings.DebugSettings.DebugEnable.Value)
            return;

        var roomsByLayer = sanctumStateTracker.roomsByLayer;
        if (roomsByLayer == null || roomsByLayer.Count == 0 || roomWeights == null)
            return;

        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            if (roomsByLayer[layer] == null)
                continue;

            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                var sanctumRoom = sanctumStateTracker.GetRoom(layer, room);
                if (sanctumRoom == null)
                    continue;

                var pos = sanctumRoom.Position;

                var debugText = debugTexts.TryGetValue((layer, room), out var text)
                    ? text
                    : string.Empty;
                var displayText = $"Weight: {roomWeights[layer, room]:F0}\n{debugText}";

                using (graphics.SetTextScale(settings.DebugSettings.DebugFontSizeMultiplier))
                {
                    graphics.DrawTextWithBackground(
                        displayText,
                        pos,
                        settings.StyleSettings.TextColor,
                        settings.StyleSettings.BackgroundColor
                    );
                }
            }
        }
    }

    public void DrawBestPath()
    {
        if (this.foundBestPath == null)
            return;

        if (sanctumStateTracker.roomsByLayer == null || sanctumStateTracker.roomsByLayer.Count == 0)
            return;

        foreach (var room in this.foundBestPath)
        {
            if (room.Item1 < 0
                || room.Item1 >= sanctumStateTracker.roomsByLayer.Count
                || sanctumStateTracker.roomsByLayer[room.Item1] == null
                || room.Item2 < 0
                || room.Item2 >= sanctumStateTracker.roomsByLayer[room.Item1].Count)
                continue;

            var sanctumRoom = sanctumStateTracker.roomsByLayer[room.Item1][room.Item2];
            if (sanctumRoom == null)
                continue;

            var rect = sanctumRoom.GetClientRect();
            var isBossRoom = room.Item1 == sanctumStateTracker.roomsByLayer.Count - 1;
            var isEntranceRoom = room.Item1 == 0;

            // EC2 0.5 reports different rectangles for the special entrance/boss icons
            // than for normal room icons.  Using the TopLeft directly makes the frame
            // land on the upper-left part of the icon, so center these markers on their
            // client rect and use the same fixed node-sized box as the rest of the route.
            if ((isEntranceRoom || isBossRoom) && rect.Width > 0 && rect.Height > 0)
            {
                var centerX = rect.X + rect.Width / 2f;
                var centerY = rect.Y + rect.Height / 2f;
                rect = new RectangleF(centerX - 48f, centerY - 48f, 96f, 96f);
            }
            // EC2 0.5 sometimes reports a parent/column rect for Sanctum nodes. When the
            // rect is too large, draw a sane node-sized square around the saved room
            // position instead, otherwise the old white-box highlight can disappear or
            // cover a whole column.
            else if (rect.Width <= 0 || rect.Height <= 0 || rect.Width > 140 || rect.Height > 140)
            {
                var stateRoom = sanctumStateTracker.GetRoom(room.Item1, room.Item2);
                var pos = stateRoom?.Position ?? sanctumRoom.Position;
                rect = new RectangleF(pos.X - 8, pos.Y - 8, 96, 96);
            }
            else
            {
                // Slightly enlarge sane node rects so the marker is visible around the icon.
                rect = new RectangleF(rect.X - 6, rect.Y - 6, rect.Width + 12, rect.Height + 12);
            }

            graphics.DrawFrame(
                rect,
                settings.StyleSettings.BestPathColor,
                settings.StyleSettings.FrameThickness
            );
        }
    }
    #endregion
}
