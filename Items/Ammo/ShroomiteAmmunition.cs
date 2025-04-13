using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Avalon.Items.Ammo;

public class ShroomiteAmmunition : ModItem
{
	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 297;
	}

	public override void SetDefaults()
	{
		Item.damage = 14;
		Item.shootSpeed = 7.5f;
		Item.ammo = AmmoID.Arrow;
		Item.DamageType = DamageClass.Ranged;
		Item.consumable = true;
		Item.rare = ItemRarityID.Blue;
		Item.width = 16;
		Item.knockBack = 3f;
		Item.shoot = ModContent.ProjectileType<Projectiles.Ranged.ShroomiteArrow>();
		Item.value = Item.sellPrice(0, 0, 0, 8);
		Item.maxStack = 9999;
		Item.height = 16;
	}
	public override bool? CanBeChosenAsAmmo(Item weapon, Player player)
	{
		return weapon.useAmmo == AmmoID.Arrow || weapon.useAmmo == AmmoID.Bullet || weapon.useAmmo == AmmoID.Rocket;
	}
}
