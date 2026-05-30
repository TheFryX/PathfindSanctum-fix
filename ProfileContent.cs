using System;
using System.Collections.Generic;
using System.Linq;

namespace PathfindSanctum;

public class ProfileContent
{
    public Dictionary<string, float> RoomTypeWeights { get; set; }
    public Dictionary<string, float> AfflictionWeights { get; set; }
    public Dictionary<string, float> RewardWeights { get; set; }

    private static ProfileContent CreateBaseProfile()
    {
        return new ProfileContent
        {
            RoomTypeWeights = new()
            {
                [Constants.RoomTypes.Gauntlet] = -1000, // Annoying
                [Constants.RoomTypes.Hourglass] = -200, // Dense mobs but manageable with defenses
                [Constants.RoomTypes.Chalice] = 0, // Neutral
                [Constants.RoomTypes.Ritual] = 0, // Neutral
                [Constants.RoomTypes.Escape] = 100, // Safe, controlled environment
                [Constants.RoomTypes.Boss] = 0, // Required
            },
            AfflictionWeights = new()
            {
                // Can get trapped
                [Constants.AfflictionTypes.GlassShard] = -4000, // The next [Boons|Boon] you gain is converted into a random Minor [Afflictions|Affliction] ||| (Average weight of all afflictions you don't have)
                [Constants.AfflictionTypes.GhastlyScythe] = -4000, // Losing [Honour] ends the Trial (removed after 3 rooms)
                [Constants.AfflictionTypes.VeiledSight] = -4000, // Rooms are unknown on the Trial Map
                [Constants.AfflictionTypes.MyriadAspersions] = -4000, // When you gain an [Afflictions|Affliction], gain an additional random Minor [Afflictions|Affliction]
                [Constants.AfflictionTypes.DeceptiveMirror] = -4000, // You are not always taken to the room you select
                [Constants.AfflictionTypes.PurpleSmoke] = -4000, // [Afflictions] are unknown on the Trial Map
                [Constants.AfflictionTypes.GoldenSmoke] = -400, // Rewards are unknown on the Trial Map
                [Constants.AfflictionTypes.RedSmoke] = -4000, // Room types are unknown on the Trial Map
                [Constants.AfflictionTypes.BlackSmoke] = -4000, // You can see one fewer room ahead on the Trial Map
                // Quickly makes you lose honour
                [Constants.AfflictionTypes.RapidQuicksand] = -1000, // Traps are faster
                [Constants.AfflictionTypes.DeadlySnare] = -1000, // Traps deal Triple Damage
                // Less profit (worse for no-hit runs)
                [Constants.AfflictionTypes.ForgottenTraditions] = -1000, // 50% reduced Effect of your Non-[ItemRarity|Unique] [Relic|Relics]
                [Constants.AfflictionTypes.SeasonOfFamine] = -1000, // The Merchant offers 50% fewer choices
                [Constants.AfflictionTypes.OrbOfNegation] = -1000, // Non-[ItemRarity|Unique] [Relic|Relics] have no Effect
                [Constants.AfflictionTypes.WinterDrought] = -1000, // Lose all [SacredWater|Sacred Water] on floor completion
                // Problematic if build is weak
                [Constants.AfflictionTypes.BrandedBalbalakh] = -1000, // Cannot restore [Honour]
                [Constants.AfflictionTypes.ChiselledStone] = -1000, // Monsters [Petrify] on Hit
                [Constants.AfflictionTypes.WeakenedFlesh] = -100, // 25% less Maximum [Honour]
                [Constants.AfflictionTypes.Untouchable] = -1000, // You are [Curse|Cursed] with [Enfeeble]
                [Constants.AfflictionTypes.CostlyAid] = -900, // Gain a random Minor [Afflictions|Affliction] when you venerate a Maraketh Shrine
                [Constants.AfflictionTypes.BluntSword] = -1000, // You and your Minions deal 40% less Damage
                [Constants.AfflictionTypes.SpikedShell] = -1000, // Monsters have 50% increased Maximum Life
                [Constants.AfflictionTypes.SuspectedSympathiser] = -200, // 50% reduced [Honour] restored
                [Constants.AfflictionTypes.Haemorrhage] = -100, // You cannot restore [Honour] (removed after killing the next Boss)
                // Problematic for certain builds (handled Dynamically)
                [Constants.AfflictionTypes.CorrosiveConcoction] = 0, // You have no [Defences] ||| Only matters if you're EV or ES based
                [Constants.AfflictionTypes.IronManacles] = 0, // You have no [Evasion] ||| Only matters if you're EV based
                [Constants.AfflictionTypes.ShatteredShield] = 0, // You have no [EnergyShield|Energy Shield] ||| Only matters if you're ES based
                // Sucks
                [Constants.AfflictionTypes.UnquenchedThirst] = -200, // You cannot gain [SacredWater|Sacred Water] ||| Floor 4 this is nearly-free, depends on how many merchants you got left
                [Constants.AfflictionTypes.UnassumingBrick] = -1000, // You cannot gain any more [Boons] ||| Floor 4 this is nearly-free, depends on how many merchants you got left
                [Constants.AfflictionTypes.TraditionsDemand] = -800, // The Merchant only offers one choice
                [Constants.AfflictionTypes.FiendishWings] = -400, // Monsters' Action Speed cannot be slowed below base ||| Matters more if you're freezing/electrocuting the target
                [Constants.AfflictionTypes.HungryFangs] = -600, // Monsters remove 5% of your Life, Mana and [EnergyShield|Energy Shield] on [HitDamage|Hit]
                [Constants.AfflictionTypes.WornSandals] = -400, // 25% reduced Movement Speed
                [Constants.AfflictionTypes.TradeTariff] = -300, // 50% increased Merchant prices
                // Nearly Free
                [Constants.AfflictionTypes.DeathToll] = -400, // Take {0} [Physical] Damage after completing the next Room || ? Monsters no longer drop [SacredWater|Sacred Water]
                [Constants.AfflictionTypes.SpikedExit] = -300, // Take {0} [Physical] Damage on Room Completion
                [Constants.AfflictionTypes.ExhaustedWells] = 0, // Chests no longer grant [SacredWater|Sacred Water]
                [Constants.AfflictionTypes.GateToll] = -100, // Lose 30 [SacredWater|Sacred Water] on room completion
                [Constants.AfflictionTypes.LeakingWaterskin] = -100, // Lose 20 [SacredWater|Sacred Water] when you take Damage from an Enemy [HitDamage|Hit]
                [Constants.AfflictionTypes.LowRivers] = -100, // 50% less [SacredWater|Sacred Water] found
                // Free
                [Constants.AfflictionTypes.SharpenedArrowhead] = 0, // You have no [Armour]
                [Constants.AfflictionTypes.RustedMallet] = 0, // Monsters always [Knockback]
                [Constants.AfflictionTypes.ChainsOfBinding] = 0, // Monsters inflict [BindingChains|Binding Chains] on [HitDamage|Hit]
                [Constants.AfflictionTypes.DishonouredTattoo] = 0, // 100% increased Damage Taken while on [LowLife|Low Life]
                [Constants.AfflictionTypes.TatteredBlindfold] = 0, // 90% reduced Light Radius
                [Constants.AfflictionTypes.DarkPit] = 0, // Traps deal 100% increased Damage
                [Constants.AfflictionTypes.HonedClaws] = 0, // Monsters deal 30% more Damage
                // Free Non-Melee
            },
            RewardWeights = new() 
            {
                [Constants.RewardTypes.GoldKey] = 0,
                [Constants.RewardTypes.SilverKey] = 0,
                [Constants.RewardTypes.BronzeKey] = 0,
                [Constants.RewardTypes.GoldenCache] = 0,
                [Constants.RewardTypes.SilverCache] = 0,
                [Constants.RewardTypes.BronzeCache] = 0,
                [Constants.RewardTypes.LargeFountain] = 100,
                [Constants.RewardTypes.Fountain] = 50,
                [Constants.RewardTypes.PledgeToKochai] = 20,
                [Constants.RewardTypes.HonourHalani] = 8,
                [Constants.RewardTypes.HonourAhkeli] = -1,
                [Constants.RewardTypes.HonourOrbala] = 50,
                [Constants.RewardTypes.HonourGalai] = 300, // Restore Honour, Random Boon Cleanse Affliction
                [Constants.RewardTypes.HonourTabana] = 0,
                [Constants.RewardTypes.Merchant] = 20 // Not important if sacred water is below ~360 (less with relics)
            }
        };
    }

    private static void MapWeightsToProfile<T>(Dictionary<string, float> targetWeights, T sourceWeights, Func<T, string, (bool success, int weight)> getWeight, Action<string, float> setWeight) {
        foreach (var key in targetWeights.Keys) {
            var result = getWeight(sourceWeights, key);
            if (result.success) {
                setWeight(key, result.weight);
            }
        }
    }

    private static void MapWeightsToSettings<T>(T targetWeights, Dictionary<string, float> sourceWeights, Action<T, string, float> setWeight) {
        foreach (var key in sourceWeights.Keys.ToList()) {
            setWeight(targetWeights, key, sourceWeights[key]);
        }
    }

    public static ProfileContent CreateDefaultProfile()
    {
        var profile = CreateBaseProfile();
        return profile;
    }

    public static ProfileContent CreateNoHitProfile()
    {
        var profile = CreateBaseProfile();

        profile.RoomTypeWeights[Constants.RoomTypes.Gauntlet] = -200; // Predictable traps
        profile.RoomTypeWeights[Constants.RoomTypes.Hourglass] = -1000; // Dangerous mob density

        profile.AfflictionWeights[Constants.AfflictionTypes.DeathToll] = -500000; // Run-Ending
        profile.AfflictionWeights[Constants.AfflictionTypes.SpikedExit] = -600000; // Run-Ending
        profile.AfflictionWeights[Constants.AfflictionTypes.DeceptiveMirror] = -400000; // Run-Ending

        profile.AfflictionWeights[Constants.AfflictionTypes.GlassShard] = -50000; // ~3/58 chance of ending your run
        profile.AfflictionWeights[Constants.AfflictionTypes.MyriadAspersions] = -50000; // ~3/58 chance of ending your run every time you mess up

        // Free
        profile.AfflictionWeights[Constants.AfflictionTypes.GhastlyScythe] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.DeadlySnare] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.BrandedBalbalakh] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.ChiselledStone] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.WeakenedFlesh] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.CostlyAid] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.SuspectedSympathiser] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.Haemorrhage] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.LeakingWaterskin] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.RustedMallet] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.ChainsOfBinding] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.DishonouredTattoo] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.DarkPit] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.HonedClaws] = 0;
        profile.AfflictionWeights[Constants.AfflictionTypes.HungryFangs] = 0;

        // Depends if the relic strategy works
        // [WeightKeys.ForgottenTraditions] = -1000, // 50% reduced Effect of your Non-[ItemRarity|Unique] [Relic|Relics]
        // [WeightKeys.ofNegation] = -1000, // Non-[ItemRarity|Unique] [Relic|Relics] have no Effect

        // Depends if you can't get your combo off in tim
        // [WeightKeys.FiendishWings] = -600, // Monsters' Action Speed cannot be slowed below base ||| Matters more if you're freezing/electrocuting the target
        return profile;
    }

    public static ProfileContent CreateNewProfileFromSettings(PathfindSanctumSettings settings) {
        var profile = CreateBaseProfile();

        MapWeightsToProfile(
            profile.RoomTypeWeights, 
            settings.Weights.RoomTypeWeights,
            (weights, key) => {
                bool result = weights.GetWeight(key, out int value);
                return (result, value);
            },
            (key, value) => profile.RoomTypeWeights[key] = value
        );
        
        MapWeightsToProfile(
            profile.AfflictionWeights,
            settings.Weights.AfflictionWeights,
            (weights, key) => {
                bool result = weights.GetWeight(key, out int value);
                return (result, value);
            },
            (key, value) => profile.AfflictionWeights[key] = value    
        );

        MapWeightsToProfile(
            profile.RewardWeights,
            settings.Weights.RewardWeights,
            (weights, key) => {
                bool result = weights.GetWeight(key, out int value);
                return (result, value);
            },
            (key, value) => profile.RewardWeights[key] = value
        );

        return profile;
    }

    public void LoadProfile(PathfindSanctumSettings settings) {
        MapWeightsToSettings(settings.Weights.RoomTypeWeights, RoomTypeWeights, (weights, key, value) => weights.SetWeight(key, value));
        MapWeightsToSettings(settings.Weights.AfflictionWeights, AfflictionWeights, (weights, key, value) => weights.SetWeight(key, value));
        MapWeightsToSettings(settings.Weights.RewardWeights, RewardWeights, (weights, key, value) => weights.SetWeight(key, value));
    }

    public ProfileContent Clone()
    {
        return new ProfileContent
        {
            RoomTypeWeights = new Dictionary<string, float>(RoomTypeWeights),
            AfflictionWeights = new Dictionary<string, float>(AfflictionWeights),
            RewardWeights = new Dictionary<string, float>(RewardWeights)
        };
    }
}
