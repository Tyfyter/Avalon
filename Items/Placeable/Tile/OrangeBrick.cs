using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Avalon.Items.Placeable.Tile;

public class OrangeBrick : ModItem
{
	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 100;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.OrangeBrick>());
	}
	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.StoneBlock)
			.AddIngredient(ItemID.Bone, 2)
			.AddTile(TileID.BoneWelder)
			.Register();
	}
}
