using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Avalon.Items.Consumables;

public class ContagionCrate : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.IsFishingCrate[Type] = true;
        Item.ResearchUnlockCount = 10;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Crates>());
        Item.placeStyle = 0;
        Item.width = 12; //The hitbox dimensions are intentionally smaller so that it looks nicer when fished up on a bobber
        Item.height = 12;
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(0, 2);
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Crates;
    }

    public override bool CanRightClick()
    {
        return true;
    }

    public override void ModifyItemLoot(ItemLoot itemLoot)
    {
        int[] themedDrops = new int[] {
            ModContent.ItemType<Accessories.PreHardmode.BandofStamina>(),
            ModContent.ItemType<Weapons.Ranged.PreHardmode.Blunderblight>(),
            ModContent.ItemType<Weapons.Magic.PreHardmode.Smogscreen>(),
            ModContent.ItemType<Pets.SepticCell>(),
            //ModContent.ItemType<Weapons.Melee.PreHardmode.PotentialContagionBoomerang>(),
        };
        itemLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, themedDrops));

        // Drop coins
        itemLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 4, 5, 13));

        // Drop pre-hm ores, with the addition of the Avalon ones
        IItemDropRule[] oreTypes = new IItemDropRule[] {
            ItemDropRule.Common(ItemID.CopperOre, 1, 20, 36),
            ItemDropRule.Common(ItemID.TinOre, 1, 20, 36),
            ItemDropRule.Common(ModContent.ItemType<Material.Ores.BronzeOre>(), 1, 20, 36),
            ItemDropRule.Common(ItemID.IronOre, 1, 20, 36),
            ItemDropRule.Common(ItemID.LeadOre, 1, 20, 36),
            ItemDropRule.Common(ModContent.ItemType<Material.Ores.NickelOre>(), 1, 20, 36),
            ItemDropRule.Common(ItemID.SilverOre, 1, 20, 36),
            ItemDropRule.Common(ItemID.TungstenOre, 1, 20, 36),
            ItemDropRule.Common(ModContent.ItemType<Material.Ores.ZincOre>(), 1, 20, 36),
            ItemDropRule.Common(ItemID.GoldOre, 1, 20, 36),
            ItemDropRule.Common(ItemID.PlatinumOre, 1, 20, 36),
            ItemDropRule.Common(ModContent.ItemType<Material.Ores.BismuthOre>(), 1, 20, 36),
        };
        itemLoot.Add(new OneFromRulesRule(7, oreTypes));

        // Drop pre-hm bars (except copper/tin), with the addition of the Avalon ones
        IItemDropRule[] oreBars = new IItemDropRule[] {
            ItemDropRule.Common(ItemID.IronBar, 1, 10, 21),
            ItemDropRule.Common(ItemID.LeadBar, 1, 10, 21),
            ItemDropRule.Common(ModContent.ItemType<Material.Bars.NickelBar>(), 1, 10, 21),
            ItemDropRule.Common(ItemID.SilverBar, 1, 10, 21),
            ItemDropRule.Common(ItemID.TungstenBar, 1, 10, 21),
            ItemDropRule.Common(ModContent.ItemType<Material.Bars.ZincBar>(), 1, 10, 21),
            ItemDropRule.Common(ItemID.GoldBar, 1, 10, 21),
            ItemDropRule.Common(ItemID.PlatinumBar, 1, 10, 21),
            ItemDropRule.Common(ModContent.ItemType<Material.Bars.BismuthBar>(), 1, 10, 21),
        };
        itemLoot.Add(new OneFromRulesRule(4, oreBars));

        // Drop an "exploration utility" potion, with the addition of the Avalon ones
        IItemDropRule[] explorationPotions = new IItemDropRule[] {
            ItemDropRule.Common(ItemID.ObsidianSkinPotion, 1, 2, 5),
            ItemDropRule.Common(ItemID.SpelunkerPotion, 1, 2, 5),
            ItemDropRule.Common(ItemID.HunterPotion, 1, 2, 5),
            ItemDropRule.Common(ItemID.GravitationPotion, 1, 2, 5),
            ItemDropRule.Common(ItemID.MiningPotion, 1, 2, 5),
            ItemDropRule.Common(ItemID.HeartreachPotion, 1, 2, 5),
        };
        itemLoot.Add(new OneFromRulesRule(4, explorationPotions));

        // Drop (pre-hm) resource potion
        IItemDropRule[] resourcePotions = new IItemDropRule[] {
            ItemDropRule.Common(ItemID.HealingPotion, 1, 5, 18),
            ItemDropRule.Common(ItemID.ManaPotion, 1, 5, 18),
            ItemDropRule.Common(ModContent.ItemType<Potions.Other.StaminaPotion>(), 1, 5, 18),
        };
        itemLoot.Add(new OneFromRulesRule(2, resourcePotions));

        // Drop (high-end) bait
        IItemDropRule[] highendBait = new IItemDropRule[] {
            ItemDropRule.Common(ItemID.JourneymanBait, 1, 2, 7),
            ItemDropRule.Common(ItemID.MasterBait, 1, 2, 7),
        };
        itemLoot.Add(new OneFromRulesRule(2, highendBait));
    }
}
