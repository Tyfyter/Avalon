using Avalon.Common.Extensions;
using Avalon.Particles;
using Avalon.Projectiles.Melee;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace Avalon.Items.Weapons.Melee.PreHardmode;

public class AeonsEternity : ModItem
{
	public override Color? GetAlpha(Color lightColor)
	{
		return new Color(255, 255, 255, 128);
	}
	public override void SetDefaults()
	{
		Item.DefaultToProjectileSword(ModContent.ProjectileType<AeonBeam>(), 40, 5, 8f, 81, 20);
		Item.rare = ItemRarityID.Pink;
		Item.value = Item.sellPrice(0, 5);
	}
	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		ParticleSystem.AddParticle(new AeonStarburst(), Main.rand.NextVector2FromRectangle(target.Hitbox), Vector2.Zero, Color.Cyan, Main.rand.NextFloat(MathHelper.TwoPi), 1.5f);
	}
	public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
	{
		ParticleSystem.AddParticle(new AeonStarburst(), Main.rand.NextVector2FromRectangle(target.Hitbox), Vector2.Zero, Color.Cyan, Main.rand.NextFloat(MathHelper.TwoPi), 1.5f);
	}
	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		//TimesSwung++;
		//if (TimesSwung == 4)
		//{
		int lastStar = -255;
		SoundEngine.PlaySound(SoundID.Item9, player.Center);
		for (int i = 0; i < Main.rand.Next(4, 8); i++)
		{
			Vector2 velRand = velocity.RotatedByRandom(Math.PI / 6) * Main.rand.NextFloat(0.3f, 2.4f);

			Vector2 dirToMouse = player.SafeDirectionTo(Main.MouseWorld);
			Vector2 velMult = player.velocity * new Vector2(MathF.Abs(dirToMouse.X), MathF.Abs(dirToMouse.Y)); // The player's current velocity, multiplied by the unsigned cosine & sine of the angle to the mouse

			Projectile p = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), position, velRand + velMult * 0.8f + player.velocity * 0.2f, ModContent.ProjectileType<AeonStar>(), damage / 5, knockback, player.whoAmI, lastStar, 160 + (i * 10), (float)Main.timeForVisualEffects);
			p.scale = Main.rand.NextFloat(0.9f, 1.1f);
			p.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
			lastStar = p.whoAmI;
		}
		for (int i = 0; i < 2; i++)
		{
			ParticleOrchestraSettings particleOrchestraSettings = default;
			particleOrchestraSettings.PositionInWorld = player.Center;
			particleOrchestraSettings.MovementVector = velocity.RotatedByRandom(0.2f) * 2 + player.velocity;
			ParticleOrchestraSettings settings = particleOrchestraSettings;
			ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.StardustPunch, settings, player.whoAmI);
			//particleOrchestraSettings.PositionInWorld = player.Center;
			//particleOrchestraSettings.MovementVector = velocity.RotatedByRandom(0.2f) * 2;
			//settings = particleOrchestraSettings;
			//ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.PaladinsHammer, settings, player.whoAmI);
			particleOrchestraSettings.PositionInWorld = player.Center;
			particleOrchestraSettings.MovementVector = velocity.RotatedByRandom(0.2f) * 2 + player.velocity;
			settings = particleOrchestraSettings;
			ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.PrincessWeapon, settings, player.whoAmI);
		}
		// TimesSwung = 0;
		//}
		//if (TimesSwung is 2 or 4 or 0)
		//{
		//    return true;
		//}
		return false;
	}
	public override void MeleeEffects(Player player, Rectangle hitbox)
	{

		ClassExtensions.GetPointOnSwungItemPath(60f, 60f, 0.2f + 0.8f * Main.rand.NextFloat(), Item.scale, out var location2, out var outwardDirection2, player);
		Vector2 vector2 = outwardDirection2.RotatedBy((float)Math.PI / 2f * (float)player.direction * player.gravDir);
		int num15 = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.GemDiamond, player.velocity.X * 0.2f + (float)(player.direction * 3), player.velocity.Y * 0.2f, 140, default(Color), 0.7f);
		Main.dust[num15].position = location2;
		//Main.dust[num15].fadeIn = 1.2f;
		Main.dust[num15].noGravity = true;
		Main.dust[num15].velocity *= 0.25f;
		Main.dust[num15].velocity += vector2 * 5f;

		//if (Main.rand.NextBool(2))
		//{
		//    int num208 = Main.rand.Next(3);
		//    if (num208 == 0)
		//    {
		//        num208 = 15;
		//    }
		//    else if (num208 == 1)
		//    {
		//        num208 = 57;
		//    }
		//    else if (num208 == 2)
		//    {
		//        num208 = 58;
		//    }

		//    for (int j = 0; j < 2; j++)
		//    {
		//        ClassExtensions.GetPointOnSwungItemPath(60f, 60f, 0.2f + 0.8f * Main.rand.NextFloat(), Item.scale, out var location2, out var outwardDirection2, player);
		//        Vector2 vector2 = outwardDirection2.RotatedBy((float)Math.PI / 2f * (float)player.direction * player.gravDir);
		//        Dust.NewDustPerfect(location2, num208, vector2 * 2f, 100, default(Color), 0.7f + Main.rand.NextFloat() * 0.6f);
		//        if (Main.rand.NextBool(20))
		//        {
		//            int num15 = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, num208, player.velocity.X * 0.2f + (float)(player.direction * 3), player.velocity.Y * 0.2f, 140, default(Color), 0.7f);
		//            Main.dust[num15].position = location2;
		//            Main.dust[num15].fadeIn = 1.2f;
		//            Main.dust[num15].noGravity = true;
		//            Main.dust[num15].velocity *= 0.25f;
		//            Main.dust[num15].velocity += vector2 * 5f;
		//        }
		//    }
		//}
		//if (player.itemTime % 2 == 0)
		//{
		//    //int type = Main.rand.Next(2);
		//    //ClassExtensions.GetPointOnSwungItemPath(60f, 60f, 0.2f + 0.8f * Main.rand.NextFloat(), Item.scale, out var location2, out var outwardDirection2, player);
		//    //Vector2 vector2 = outwardDirection2.RotatedBy((float)Math.PI / 2f * (float)player.direction * player.gravDir);
		//    //ParticleOrchestraSettings particleOrchestraSettings = default(ParticleOrchestraSettings);
		//    //particleOrchestraSettings.PositionInWorld = location2;
		//    //particleOrchestraSettings.MovementVector = Vector2.Zero;
		//    //ParticleOrchestraSettings settings = particleOrchestraSettings;
		//    //if (type == 0)
		//    //    ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.StardustPunch, settings, player.whoAmI);
		//    //else
		//    //    ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.PrincessWeapon, settings, player.whoAmI);
		//}
	}
	public override void AddRecipes()
	{
		CreateRecipe(1)
			.AddIngredient(ItemID.Starfury)
			.AddIngredient(ItemID.IceBlade)
			.AddIngredient(ModContent.ItemType<MinersSword>())
			.AddIngredient(ItemID.AntlionClaw)
			.AddIngredient(ModContent.ItemType<OsmiumGreatsword>())
			.AddTile(TileID.DemonAltar)
			.Register();
		CreateRecipe(1)
			.AddIngredient(ItemID.Starfury)
			.AddIngredient(ItemID.IceBlade)
			.AddIngredient(ModContent.ItemType<MinersSword>())
			.AddIngredient(ModContent.ItemType<DesertLongsword>())
			.AddIngredient(ModContent.ItemType<OsmiumGreatsword>())
			.AddTile(TileID.DemonAltar)
			.Register();

		CreateRecipe(1)
			.AddIngredient(ItemID.Starfury)
			.AddIngredient(ItemID.IceBlade)
			.AddIngredient(ModContent.ItemType<MinersSword>())
			.AddIngredient(ItemID.AntlionClaw)
			.AddIngredient(ModContent.ItemType<RhodiumGreatsword>())
			.AddTile(TileID.DemonAltar)
			.Register();
		CreateRecipe(1)
			.AddIngredient(ItemID.Starfury)
			.AddIngredient(ItemID.IceBlade)
			.AddIngredient(ModContent.ItemType<MinersSword>())
			.AddIngredient(ModContent.ItemType<DesertLongsword>())
			.AddIngredient(ModContent.ItemType<RhodiumGreatsword>())
			.AddTile(TileID.DemonAltar)
			.Register();

		CreateRecipe(1)
			.AddIngredient(ItemID.Starfury)
			.AddIngredient(ItemID.IceBlade)
			.AddIngredient(ModContent.ItemType<MinersSword>())
			.AddIngredient(ItemID.AntlionClaw)
			.AddIngredient(ModContent.ItemType<IridiumGreatsword>())
			.AddTile(TileID.DemonAltar)
			.Register();
		CreateRecipe(1)
			.AddIngredient(ItemID.Starfury)
			.AddIngredient(ItemID.IceBlade)
			.AddIngredient(ModContent.ItemType<MinersSword>())
			.AddIngredient(ModContent.ItemType<DesertLongsword>())
			.AddIngredient(ModContent.ItemType<IridiumGreatsword>())
			.AddTile(TileID.DemonAltar)
			.Register();
	}
}
