using System.Collections.Generic;
using System.Text;
using ExileCore2;
using ExileCore2.Shared.Enums;

namespace PathfindSanctum;

public class WeightCalculator(GameController gameController, PathfindSanctumSettings settings) {
    private readonly GameController gameController = gameController;
    private readonly PathfindSanctumSettings settings = settings;
    private readonly StringBuilder debugText = new();

    public (double weight, string debug) CalculateRoomWeight(RoomState room)
    {
        if (room == null)
            return (0, string.Empty);

        debugText.Clear();
        var profile = settings.Profiles[settings.CurrentProfile];
        double weight = 1000000;

        weight += CalculateRoomTypeWeight(room, profile);
        weight += CalculateAfflictionWeights(room, profile);
        weight += CalculateRewardWeights(room, profile);
        weight += CalculateConnectivityBonus(room);

        return (weight, debugText.ToString());
    }

    private double CalculateRoomTypeWeight(RoomState room, ProfileContent profile)
    {
        var roomType = room.RoomType;
        if (roomType == null)
            return 0;

        if (settings.Weights.RoomTypeWeights.GetWeight(roomType, out int typeWeight))
        {
            debugText.AppendLine($"{roomType}:{typeWeight}");
            return typeWeight;
        }

        debugText.AppendLine($"Room Type ({roomType}): 0 (not found in weights)");
        return 0;
    }

    private double CalculateAfflictionWeights(RoomState room, ProfileContent profile)
    {
        var affliction = room.Affliction;
        if (affliction == null)
            return 0;

        var afflictionName = affliction.ToString();
        var dynamicWeight = CalculateDynamicAfflictionWeight(afflictionName);
        if (dynamicWeight != null)
        {
            if (settings.DebugSettings.DebugEnable.Value)
                debugText.AppendLine($"{afflictionName}:{dynamicWeight}");
            return (double)dynamicWeight;
        }
        else if (settings.Weights.AfflictionWeights.GetWeight(afflictionName, out int afflictionWeight))
        {
            if (settings.DebugSettings.DebugEnable.Value)
                debugText.AppendLine($"{afflictionName}:{afflictionWeight}");
            return afflictionWeight;
        }

        debugText.AppendLine($"Affliction ({afflictionName}): 0 (not found in weights)");
        return 0;
    }

    private double? CalculateDynamicAfflictionWeight(string afflictionName)
    {
        return afflictionName switch
        {
            "Iron Manacles" => settings.AdvancedSettings.DynamicEVWeight ? CalculateIronManaclesWeight(): null,
            "Shattered Shield" => settings.AdvancedSettings.DynamicESWeight ? CalculateShatteredShieldWeight() : null,
            "Worn Sandals" => QueenOfTheForestWeight(),
            "Corrosive Concoction"
                => ((settings.AdvancedSettings.DynamicEVWeight ? CalculateIronManaclesWeight() : null) ?? 0)
                + ((settings.AdvancedSettings.DynamicESWeight ? CalculateShatteredShieldWeight() : null) ?? 0),
            _ => null // No dynamic modification
        };
    }

    private double? QueenOfTheForestWeight()
    {
        var hasQueenOfTheForest = gameController.Player.Stats.GetValueOrDefault(
            GameStat.MovementSpeedIsOnlyBase1PctPerXEvasionRating,
            0
        );

        if (hasQueenOfTheForest > 0)
        {
            return 0;
        }

        return null;
    }

    private double? CalculateIronManaclesWeight()
    {
        var playerEvasion = gameController.Player.Stats.GetValueOrDefault(
            GameStat.EvasionRating,
            0
        );

        if (playerEvasion > settings.AdvancedSettings.DynamicEVUpperThreshold)
        {
            return settings.AdvancedSettings.DynamicEVUpperWeight;
        }
        else if (playerEvasion > settings.AdvancedSettings.DynamicEVLowerThreshold)
        {
            return settings.AdvancedSettings.DynamicEVLowerWeight;
        }

        return null;
    }

    private double? CalculateShatteredShieldWeight()
    {
        var playerEnergyShield = gameController.Player.Stats.GetValueOrDefault(
            GameStat.MaximumEnergyShield,
            0
        );

        if (playerEnergyShield > settings.AdvancedSettings.DynamicESUpperThreshold)
        {
            return settings.AdvancedSettings.DynamicESUpperWeight;
        }
        else if (playerEnergyShield > settings.AdvancedSettings.DynamicESLowerThreshold)
        {
            return settings.AdvancedSettings.DynamicESLowerWeight;
        }

        return null;
    }

    private double CalculateRewardWeights(RoomState room, ProfileContent profile)
    {
        if (
            room?.Reward != null
            && settings.Weights.RewardWeights.GetWeight(room.Reward, out int rewardWeight)
        )
        {
            if (settings.DebugSettings.DebugEnable.Value)
                debugText.AppendLine($"{room.Reward}:{rewardWeight}");
            return rewardWeight;
        }

        return 0;
    }

    private double CalculateConnectivityBonus(RoomState room)
    {
        var connectionBonus = room.Connections > 1 ? 100 : 0;
        if (settings.DebugSettings.DebugEnable.Value)
            debugText.AppendLine($"Connectivity:{connectionBonus}");
        return connectionBonus;
    }
}
