using System.Collections.Generic;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;

namespace PathfindSanctum;

public class PathfindSanctumSettings : ISettings {

    public ToggleNode Enable { get; set; } = new ToggleNode(true);

    public Dictionary<string, ProfileContent> Profiles = new() {
        [Constants.Profiles.Default] = ProfileContent.CreateDefaultProfile(),
        [Constants.Profiles.NoHit] = ProfileContent.CreateNoHitProfile()
    };
    // We need this here to remember selected profile
    public int selectedProfileIndex = 0;
    internal string CurrentProfile { get { return ProfileManager.ProfileNames[selectedProfileIndex]; } }

    [JsonIgnore]
    [Menu("Weight Settings")]
    public CustomWeightSettings Weights { get; set; } = new CustomWeightSettings();

    [Menu("Advanced Settings")]
    public AdvancedSettings AdvancedSettings { get; set; } = new AdvancedSettings();

    [Menu("Style Settings")]
    public StyleSettings StyleSettings { get; set; } = new StyleSettings();

    [Menu("Debug Settings")]
    public DebugSettings DebugSettings { get; set; } = new DebugSettings();

    [JsonIgnore]
    internal ProfileManager ProfileManager { get; private set; }

    public PathfindSanctumSettings() {
        ProfileManager = new ProfileManager(this);
        Profiles = new() {
            [Constants.Profiles.Default] = ProfileContent.CreateDefaultProfile(),
            [Constants.Profiles.NoHit] = ProfileContent.CreateNoHitProfile(),
        };
        LoadSavedSettingsAfterStart();
    }

    private async void LoadSavedSettingsAfterStart() {
        await Task.Delay(1000);
        ProfileManager.LoadSelectedProfile();
    }
}

[Submenu(CollapsedByDefault = true)]
public class CustomWeightSettings {
    [Menu("Room Type Weights")]
    public RoomTypeWeightSettings RoomTypeWeights { get; set; } = new RoomTypeWeightSettings();
        
    [Menu("Affliction Weights")]
    public AfflictionWeightSettings AfflictionWeights{ get; set; } = new AfflictionWeightSettings();
        
    [Menu("Reward Weights")]
    public RewardWeightSettings RewardWeights{ get; set; } = new RewardWeightSettings();

}

[Submenu(CollapsedByDefault = true)]
public class RoomTypeWeightSettings : WeightSettings {
    [Menu(Constants.RoomTypes.Gauntlet, "Levers, Traps, large layout. Generally annoying.")]
    public RangeNode<int> GauntletWeight { get; set; } = new RangeNode<int>(-1000, -1000, 1000);

    [Menu(Constants.RoomTypes.Hourglass, "Dense mobs but manageable with strong character")]
    public RangeNode<int> HourglassWeight { get; set; } = new RangeNode<int>(-200, -1000, 1000);

    [Menu(Constants.RoomTypes.Chalice, "Kill all rare mobs. Quick and easy.")]
    public RangeNode<int> ChaliceWeight { get; set; } = new RangeNode<int>(0, -1000, 1000);

    [Menu(Constants.RoomTypes.Ritual, "Clear all ritual sites. Quick and easy.")]
    public RangeNode<int> RitualWeight { get; set; } = new RangeNode<int>(0, -1000, 1000);

    [Menu(Constants.RoomTypes.Escape, "Crystals. Safe, controlled environment. Just run through. Quick and easy.")]
    public RangeNode<int> EscapeWeight { get; set; } = new RangeNode<int>(100, -1000, 1000);

    [Menu(Constants.RoomTypes.Boss)]
    public RangeNode<int> BossWeight { get; set; } = new RangeNode<int>(0, -1000, 1000);
}

[Submenu(CollapsedByDefault = true)]
public class AfflictionWeightSettings : WeightSettings {
    [Menu(Constants.AfflictionTypes.GlassShard, "The next Boon you gain is converted into a random Minor Affliction")]
    public RangeNode<int> GlassShardWeight { get; set; } = new RangeNode<int>(-4000, -5000, 0);

    [Menu(Constants.AfflictionTypes.GhastlyScythe, "Losing Honour ends the Trial (removed after 3 rooms)")]
    public RangeNode<int> GhastlyScytheWeight { get; set; } = new RangeNode<int>(-4000, -5000, 0);

    [Menu(Constants.AfflictionTypes.VeiledSight, "Rooms are unknown on the Trial Map")]
    public RangeNode<int> VeiledSightWeight { get; set; } = new RangeNode<int>(-4000, -5000, 0);

    [Menu(Constants.AfflictionTypes.MyriadAspersions, "When you gain an Affliction, gain an additional random Minor Affliction")]
    public RangeNode<int> MyriadAspersionsWeight { get; set; } = new RangeNode<int>(-4000, -5000, 0);

    [Menu(Constants.AfflictionTypes.DeceptiveMirror, "You are not always taken to the room you select")]
    public RangeNode<int> DeceptiveMirrorWeight { get; set; } = new RangeNode<int>(-4000, -5000, 0);

    [Menu(Constants.AfflictionTypes.PurpleSmoke, "Afflictions are unknown on the Trial Map")]
    public RangeNode<int> PurpleSmokeWeight { get; set; } = new RangeNode<int>(-4000, -5000, 0);

    [Menu(Constants.AfflictionTypes.GoldenSmoke, "Rewards are unknown on the Trial Map")]
    public RangeNode<int> GoldenSmokeWeight { get; set; } = new RangeNode<int>(-400, -5000, 0);

    [Menu(Constants.AfflictionTypes.RedSmoke, "Room types are unknown on the Trial Map")]
    public RangeNode<int> RedSmokeWeight { get; set; } = new RangeNode<int>(-4000, -5000, 0);

    [Menu(Constants.AfflictionTypes.BlackSmoke, "You can see one fewer room ahead on the Trial Map")]
    public RangeNode<int> BlackSmokeWeight { get; set; } = new RangeNode<int>(-4000, -5000, 0);

    [Menu(Constants.AfflictionTypes.RapidQuicksand, "Traps are faster")]
    public RangeNode<int> RapidQuicksandWeight { get; set; } = new RangeNode<int>(-1000, -5000, 0);

    [Menu(Constants.AfflictionTypes.DeadlySnare, "Traps deal Triple Damage")]
    public RangeNode<int> DeadlySnareWeight { get; set; } = new RangeNode<int>(-1000, -5000, 0);

    [Menu(Constants.AfflictionTypes.ForgottenTraditions, "50%% reduced Effect of your Non-Unique Relics")]
    public RangeNode<int> ForgottenTraditionsWeight { get; set; } = new RangeNode<int>(-1000, -5000, 0);

    [Menu(Constants.AfflictionTypes.SeasonOfFamine, "The Merchant offers 50%% fewer choices")]
    public RangeNode<int> SeasonOfFamineWeight { get; set; } = new RangeNode<int>(-1000, -5000, 0);

    [Menu(Constants.AfflictionTypes.OrbOfNegation, "Non-Unique Relics have no Effect")]
    public RangeNode<int> OrbOfNegationWeight { get; set; } = new RangeNode<int>(-1000, -5000, 0);

    [Menu(Constants.AfflictionTypes.WinterDrought, "Lose all Sacred Water on floor completion")]
    public RangeNode<int> WinterDroughtWeight { get; set; } = new RangeNode<int>(-1000, -5000, 0);

    [Menu(Constants.AfflictionTypes.BrandedBalbalakh, "Cannot restore Honour")]
    public RangeNode<int> BrandedBalbalakhWeight { get; set; } = new RangeNode<int>(-1000, -5000, 0);

    [Menu(Constants.AfflictionTypes.ChiselledStone, "Monsters Petrify on Hit")]
    public RangeNode<int> ChiselledStoneWeight { get; set; } = new RangeNode<int>(-1000, -5000, 0);

    [Menu(Constants.AfflictionTypes.WeakenedFlesh, "25%% less Maximum Honour")]
    public RangeNode<int> WeakenedFleshWeight { get; set; } = new RangeNode<int>(-100, -5000, 0);

    [Menu(Constants.AfflictionTypes.Untouchable, "You are cursed with Enfeeble")]
    public RangeNode<int> UntouchableWeight { get; set; } = new RangeNode<int>(-1000, -5000, 0);

    [Menu(Constants.AfflictionTypes.CostlyAid, "Gain a random Minor Affliction when you venerate a Maraketh Shrine")]
    public RangeNode<int> CostlyAidWeight { get; set; } = new RangeNode<int>(-900, -5000, 0);

    [Menu(Constants.AfflictionTypes.BluntSword, "You and your Minions deal 40%% less Damage")]
    public RangeNode<int> BluntSwordWeight { get; set; } = new RangeNode<int>(-1000, -5000, 0);

    [Menu(Constants.AfflictionTypes.SpikedShell, "Monsters have 50%% increased Maximum Life")]
    public RangeNode<int> SpikedShellWeight { get; set; } = new RangeNode<int>(-1000, -5000, 0);

    [Menu(Constants.AfflictionTypes.SuspectedSympathiser, "50%% reduced Honour restored")]
    public RangeNode<int> SuspectedSympathiserWeight { get; set; } = new RangeNode<int>(-200, -5000, 0);

    [Menu(Constants.AfflictionTypes.Haemorrhage, "You cannot restore Honour (removed after killing the next Boss)")]
    public RangeNode<int> HaemorrhageWeight { get; set; } = new RangeNode<int>(-100, -5000, 0);

    [Menu(Constants.AfflictionTypes.CorrosiveConcoction, "You have no Defences (Only matters if you're EV or ES based)")]
    public RangeNode<int> CorrosiveConcoctionWeight { get; set; } = new RangeNode<int>(0, -5000, 0);

    [Menu(Constants.AfflictionTypes.IronManacles, "You have no Evasion")]
    public RangeNode<int> IronManaclesWeight { get; set; } = new RangeNode<int>(0, -5000, 0);

    [Menu(Constants.AfflictionTypes.ShatteredShield, "You have no Energy Shield")]
    public RangeNode<int> ShatteredShieldWeight { get; set; } = new RangeNode<int>(0, -5000, 0);

    [Menu(Constants.AfflictionTypes.UnquenchedThirst, "You cannot gain Sacred Water (Floor 4 this is nearly-free, depends on how many merchants you got left)")]
    public RangeNode<int> UnquenchedThirstWeight { get; set; } = new RangeNode<int>(-200, -5000, 0);

    [Menu(Constants.AfflictionTypes.UnassumingBrick, "You cannot gain any more Boons (Floor 4 this is nearly-free, depends on how many merchants you got left)")]
    public RangeNode<int> UnassumingBrickWeight { get; set; } = new RangeNode<int>(-1000, -5000, 0);

    [Menu(Constants.AfflictionTypes.TraditionsDemand, "The Merchant only offers one choice")]
    public RangeNode<int> TraditionsDemandWeight { get; set; } = new RangeNode<int>(-800, -5000, 0);

    [Menu(Constants.AfflictionTypes.FiendishWings, "Monsters' Action Speed cannot be slowed below base (Matters more if you're freezing/electrocuting the target)")]
    public RangeNode<int> FiendishWingsWeight { get; set; } = new RangeNode<int>(-400, -5000, 0);

    [Menu(Constants.AfflictionTypes.HungryFangs, "Monsters remove 5%% of your Life, Mana and Energy Shield on Hit")]
    public RangeNode<int> HungryFangsWeight { get; set; } = new RangeNode<int>(-600, -5000, 0);

    [Menu(Constants.AfflictionTypes.WornSandals, "25%% reduced Movement Speed")]
    public RangeNode<int> WornSandalsWeight { get; set; } = new RangeNode<int>(-400, -5000, 0);

    [Menu(Constants.AfflictionTypes.TradeTariff, "50%% increased Merchant prices")]
    public RangeNode<int> TradeTariffWeight { get; set; } = new RangeNode<int>(-300, -5000, 0);

    [Menu(Constants.AfflictionTypes.DeathToll, "Take flat amount of Physical Damage after completing eight rooms")]
    public RangeNode<int> DeathTollWeight { get; set; } = new RangeNode<int>(-400, -5000, 0);

    [Menu(Constants.AfflictionTypes.SpikedExit, "Take flat amount of Physical Damage on Room Completion")]
    public RangeNode<int> SpikedExitWeight { get; set; } = new RangeNode<int>(-300, -5000, 0);

    [Menu(Constants.AfflictionTypes.ExhaustedWells, "Chests no longer grant Sacred Water")]
    public RangeNode<int> ExhaustedWellsWeight { get; set; } = new RangeNode<int>(0, -5000, 0);

    [Menu(Constants.AfflictionTypes.GateToll, "Lose 30 Sacred Water on room completion")]
    public RangeNode<int> GateTollWeight { get; set; } = new RangeNode<int>(-100, -5000, 0);

    [Menu(Constants.AfflictionTypes.LeakingWaterskin, "Lose 20 Sacred Water when you take Damage from an Enemy Hit")]
    public RangeNode<int> LeakingWaterskinWeight { get; set; } = new RangeNode<int>(-100, -5000, 0);

    [Menu(Constants.AfflictionTypes.LowRivers, "50%% less Sacred Water found")]
    public RangeNode<int> LowRiversWeight { get; set; } = new RangeNode<int>(-100, -5000, 0);

    [Menu(Constants.AfflictionTypes.SharpenedArrowhead, "You have no Armour")]
    public RangeNode<int> SharpenedArrowheadWeight { get; set; } = new RangeNode<int>(0, -5000, 0);

    [Menu(Constants.AfflictionTypes.RustedMallet, "Monsters always Knockback")]
    public RangeNode<int> RustedMalletWeight { get; set; } = new RangeNode<int>(0, -5000, 0);

    [Menu(Constants.AfflictionTypes.ChainsOfBinding, "Monsters inflict Binding Chains on Hit")]
    public RangeNode<int> ChainsOfBindingWeight { get; set; } = new RangeNode<int>(0, -5000, 0);

    [Menu(Constants.AfflictionTypes.DishonouredTattoo, "100%% increased Damage Taken while on Low Life")]
    public RangeNode<int> DishonouredTattooWeight { get; set; } = new RangeNode<int>(0, -5000, 0);

    [Menu(Constants.AfflictionTypes.TatteredBlindfold, "90%% reduced Light Radius")]
    public RangeNode<int> TatteredBlindfoldWeight { get; set; } = new RangeNode<int>(0, -5000, 0);

    [Menu(Constants.AfflictionTypes.DarkPit, "Traps deal 100%% increased Damage")]
    public RangeNode<int> DarkPitWeight { get; set; } = new RangeNode<int>(0, -5000, 0);

    [Menu(Constants.AfflictionTypes.HonedClaws, "Monsters deal 30%% more Damage")]
    public RangeNode<int> HonedClawsWeight { get; set; } = new RangeNode<int>(0, -5000, 0);
}

[Submenu(CollapsedByDefault = true)]
public class RewardWeightSettings : WeightSettings {
    [Menu(Constants.RewardTypes.GoldKey)]
    public RangeNode<int> GoldKeyWeight { get; set; } = new RangeNode<int>(0, -1000, 1000);

    [Menu(Constants.RewardTypes.SilverKey)]
    public RangeNode<int> SilverKeyWeight { get; set; } = new RangeNode<int>(0, -1000, 1000);

    [Menu(Constants.RewardTypes.BronzeKey)]
    public RangeNode<int> BronzeKeyWeight { get; set; } = new RangeNode<int>(0, -1000, 1000);

    [Menu(Constants.RewardTypes.GoldenCache)]
    public RangeNode<int> GoldenCacheWeight { get; set; } = new RangeNode<int>(0, -1000, 1000);

    [Menu(Constants.RewardTypes.SilverCache)]
    public RangeNode<int> SilverCacheWeight { get; set; } = new RangeNode<int>(0, -1000, 1000);

    [Menu(Constants.RewardTypes.BronzeCache)]
    public RangeNode<int> BronzeCacheWeight { get; set; } = new RangeNode<int>(0, -1000, 1000);

    [Menu(Constants.RewardTypes.LargeFountain, "Large fountain containing Sacred Water")]
    public RangeNode<int> LargeFountainWeight { get; set; } = new RangeNode<int>(100, -1000, 1000);

    [Menu(Constants.RewardTypes.Fountain, "Regular fountain containing Sacred Water")]
    public RangeNode<int> FountainWeight { get; set; } = new RangeNode<int>(50, -1000, 1000);

    [Menu(Constants.RewardTypes.PledgeToKochai, "Select one of three random Pledges")]
    public RangeNode<int> PledgeToKochaiWeight { get; set; } = new RangeNode<int>(20, -1000, 1000);

    [Menu(Constants.RewardTypes.HonourHalani, "Restore Honour and gain Sacred Water")]
    public RangeNode<int> HonourHalaniWeight { get; set; } = new RangeNode<int>(8, -1000, 1000);

    [Menu(Constants.RewardTypes.HonourAhkeli, "Restore 25%% of your Honour and gain a random Minor Affliction")]
    public RangeNode<int> HonourAhkeliWeight { get; set; } = new RangeNode<int>(-1, -1000, 1000);

    [Menu(Constants.RewardTypes.HonourOrbala, "Restore 10%% of your Honour and gain a random Boon")]
    public RangeNode<int> HonourOrbalaWeight { get; set; } = new RangeNode<int>(50, -1000, 1000);

    [Menu(Constants.RewardTypes.HonourGalai, "Restore 50%% of your Honour, Gain a random Boon and cleanse a random Affliction")]
    public RangeNode<int> HonourGalaiWeight { get; set; } = new RangeNode<int>(300, -1000, 1000);

    [Menu(Constants.RewardTypes.HonourTabana, "Restore 10%% of your Honour")]
    public RangeNode<int> HonourTabanaWeight { get; set; } = new RangeNode<int>(0, -1000, 1000);

    [Menu(Constants.RewardTypes.Merchant, "Not important if sacred water is below ~360 (less with relics)")]
    public RangeNode<int> MerchantWeight { get; set; } = new RangeNode<int>(20, -1000, 1000);
}

[Submenu(CollapsedByDefault = true)]
public class AdvancedSettings {
    [Menu("Dynamic Evasion Rating Weights", "Dynamically adjust weights of [Iron Manacles] and [Corrosive Concoction] afflictions based on your character's evasion rating\nThis will ignore the weight set in the profile")]
    public ToggleNode DynamicEVWeight { get; set; } = new ToggleNode(true);

    [Menu("Evasion Lower Threshold")]
    public RangeNode<int> DynamicEVLowerThreshold { get; set; } = new RangeNode<int>(6000, 0, 50000);

    [Menu("Evasion Lower Weight", "This weight will be used when your evasion rating is above the lower threshold")]
    public RangeNode<int> DynamicEVLowerWeight { get; set; } = new RangeNode<int>(-750, -10000, 0);

    [Menu("Evasion Upper Threshold")]
    public RangeNode<int> DynamicEVUpperThreshold { get; set; } = new RangeNode<int>(20000, 0, 50000);

    [Menu("Evasion Upper Weight", "This weight will be used when your when your evasion rating is above the upper threshold")]
    public RangeNode<int> DynamicEVUpperWeight { get; set; } = new RangeNode<int>(-5000, -10000, 0);


    [Menu("Dynamic Energy Shield Weights", "Dynamically adjust weights of [Shattered Shield] and [Corrosive Concoction] afflictions based on your character's energy shield\nThis will ignore the weight set in the profile")]
    public ToggleNode DynamicESWeight { get; set; } = new ToggleNode(true);

    [Menu("ES Lower Threshold")]
    public RangeNode<int> DynamicESLowerThreshold { get; set; } = new RangeNode<int>(1000, 0, 20000);

    [Menu("ES Lower Weight", "This weight will be used when your maximum energy shield surpasses the lower threshold")]
    public RangeNode<int> DynamicESLowerWeight { get; set; } = new RangeNode<int>(-750, -10000, 0);

    [Menu("ES Upper Threshold")]
    public RangeNode<int> DynamicESUpperThreshold { get; set; } = new RangeNode<int>(6000, 0, 20000);

    [Menu("ES Lower Weight", "This weight will be used when your maximum energy shield surpasses the upper threshold")]
    public RangeNode<int> DynamicESUpperWeight { get; set; } = new RangeNode<int>(-5000, -10000, 0);
}

[Submenu(CollapsedByDefault = true)]
public class StyleSettings {
    public ColorNode TextColor { get; set; } = new ColorNode(Color.White);
    public ColorNode BackgroundColor { get; set; } = new ColorNode(Color.FromArgb(128, 0, 0, 0));
    public ColorNode BestPathColor { get; set; } = new(Color.Cyan);
    public RangeNode<int> FrameThickness { get; set; } = new RangeNode<int>(5, 0, 10);
}

[Submenu(CollapsedByDefault = true)]
public class DebugSettings {
    [Menu("Enable Debug Text", "Draw debug information on the floor map")]
    public ToggleNode DebugEnable { get; set; } = new ToggleNode(false);

    [Menu("Debug Font Size")]
    public RangeNode<float> DebugFontSizeMultiplier { get; set; } = new RangeNode<float>(1.0f, 0.5f, 2f);
}
