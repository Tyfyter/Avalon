using Avalon.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Avalon.Projectiles.Melee;

public class AeonStar : ModProjectile
{
	public override void SetDefaults()
	{
		Projectile.width = 24;
		Projectile.height = 24;
		Projectile.aiStyle = -1;
		Projectile.alpha = 0;
		Projectile.penetrate = -1;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.friendly = true;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 25;
		Projectile.tileCollide = false;
		//DrawOriginOffsetX= -4;
		DrawOriginOffsetY = 2;
		DrawOffsetX = 4;
		Projectile.extraUpdates = 1;
	}
	//public override Color? GetAlpha(Color lightColor)
	//{
	//    return new Color(255, 255, 255, 64);
	//}
	Vector2 LastStarPos;
	public override void OnSpawn(IEntitySource source)
	{
		int J = (Projectile.ai[0] != -255) ? Main.projectile[(int)Projectile.ai[0]].whoAmI : Projectile.whoAmI;
		LastStarPos = Main.projectile[J].Center;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		int frameHeight = TextureAssets.Projectile[Type].Value.Height / Main.projFrames[Projectile.type];
		Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, TextureAssets.Projectile[Type].Value.Width, frameHeight);
		Vector2 frameOrigin = frame.Size() / 2f;
		Color color = Color.Lerp(new Color(255, 255, 255, 0), new Color(128, 128, 128, 64), Projectile.ai[1] * 0.03f);
		for (int i = 0; i < 6; i++)
		{
			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.position + frameOrigin - Main.screenPosition + new Vector2(0, (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 12f) * 4).RotatedBy(i * MathHelper.PiOver2 + (Main.timeForVisualEffects * 0.03f)), frame, color * 0.2f, Projectile.rotation, frameOrigin, Projectile.scale, SpriteEffects.None);
		}
		Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.position + frameOrigin - Main.screenPosition, frame, color, Projectile.rotation, frameOrigin, Projectile.scale, SpriteEffects.None);
		return false;
	}
	public override void AI()
	{
		//int F = Dust.NewDust(Projectile.Center, 0, 0, DustID.UnusedWhiteBluePurple, 0, 0, 0, default, 1);
		//Main.dust[F].noGravity = true;
		//Main.dust[F].velocity *= 0;

		float Seed = Projectile.ai[2];
		Projectile lastStar = (Projectile.ai[0] != -255) ? Main.projectile[(int)Projectile.ai[0]] : Projectile;
		float distanceToStar = Projectile.Center.Distance(lastStar.Center);
		if (!lastStar.active)
		{
			lastStar = Projectile;
		}
		Projectile.ai[1]--;
		if (Projectile.ai[1] == 100 && lastStar.ai[2] == Seed && lastStar.whoAmI != Projectile.whoAmI == true)
		{
			SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);
		}
		if (lastStar.ai[2] == Seed && lastStar.whoAmI != Projectile.whoAmI && Projectile.ai[1] < 100)
		{
			for (int i = 0; i < distanceToStar; i += 6)
			{
				int D = Dust.NewDust(Projectile.Center + new Vector2(i, 0).RotatedBy(Projectile.Center.AngleTo(lastStar.Center)), 0, 0, DustID.UnusedWhiteBluePurple, 0, 0, 0, default, 1);
				Main.dust[D].noGravity = true;
				Main.dust[D].velocity *= 0;
			}
		}
		//if (Projectile.ai[1] > 100)
		//{
		//    int BlasfahSaidThisShouldHomeSoItDoesNow = ClassExtensions.FindClosestNPC(Projectile, 300, npc => !npc.active || npc.townNPC || npc.dontTakeDamage || npc.lifeMax <= 5 || npc.type == NPCID.TargetDummy || npc.type == NPCID.CultistBossClone || npc.friendly);
		//    if (BlasfahSaidThisShouldHomeSoItDoesNow != -1)
		//    {
		//        Projectile.velocity += Projectile.Center.DirectionTo(Main.npc[BlasfahSaidThisShouldHomeSoItDoesNow].Center) * 0.2f;
		//    }
		//}
		Projectile.velocity *= 0.95f;
		Projectile.rotation += Projectile.velocity.Length() / 30;
		Projectile.rotation += 0.007f;


		if (Projectile.ai[1] > 30)
		{
			LastStarPos = lastStar.Center;
		}

		if (Projectile.ai[1] == 10)
		{
			//SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}

		if (Projectile.ai[1] < 10)
		{
			int D = Dust.NewDust(Vector2.Lerp(Projectile.Center, LastStarPos, Projectile.ai[1] / 10), 0, 0, DustID.UnusedWhiteBluePurple, 0, 0, 0, default, 2);
			Main.dust[D].color = new Color(255, 255, 255, 0);
			Main.dust[D].noGravity = true;
			Main.dust[D].velocity *= 0;
			Main.dust[D].noLightEmittence = true;
		}
		if (Projectile.ai[1] < 0)
		{
			Projectile.Kill();
		}
	}
	public override void OnKill(int timeLeft)
	{
		SoundEngine.PlaySound(SoundID.Item110, Projectile.Center);

		//for (int i = 0; i < 30; i++)
		//{
		//    int D = Dust.NewDust(Projectile.Center, 0, 0, DustID.UnusedWhiteBluePurple, 0, 0, 0, default, 3);
		//    Main.dust[D].color = new Color(255, 255, 255, 0);
		//    Main.dust[D].noGravity = !Main.rand.NextBool(5);
		//    Main.dust[D].noLightEmittence = true;
		//    Main.dust[D].fadeIn = Main.rand.NextFloat(0f, 2f);
		//    Main.dust[D].velocity = new Vector2(Main.rand.NextFloat(2, 5), 0).RotatedBy(MathHelper.Pi / 15 * i);
		//}

		for (int i = 0; i < 30; i++)
		{
			int D = Dust.NewDust(Projectile.Center, 0, 0, DustID.GoldCoin, 0, 0, 0, default, 3);
			Main.dust[D].color = new Color(255, 255, 255, 0);
			Main.dust[D].noGravity = true;
			Main.dust[D].noLightEmittence = true;
			Main.dust[D].fadeIn = Main.rand.NextFloat(0.5f, 1.5f);
			Main.dust[D].velocity = new Vector2(Main.rand.NextFloat(3, 8), 0).RotatedBy(MathHelper.Pi / 15 * i);
		}

		if (Main.myPlayer == Projectile.owner)
		{
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<AeonExplosion>(), Projectile.damage * 5, Projectile.knockBack * 2, Projectile.owner);
		}
		//for (int i = 0; i < 1; i++)
		//{
		//    ParticleOrchestraSettings particleOrchestraSettings = default(ParticleOrchestraSettings);
		//    particleOrchestraSettings.PositionInWorld = Projectile.Center;
		//    particleOrchestraSettings.MovementVector = Main.rand.NextVector2Circular(7, 7);
		//    ParticleOrchestraSettings settings = particleOrchestraSettings;
		//    ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.StardustPunch, settings, Projectile.owner);
		//    particleOrchestraSettings.MovementVector = Main.rand.NextVector2Circular(7, 7);
		//    settings = particleOrchestraSettings;
		//    ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.PrincessWeapon, settings, Projectile.owner);
		//    particleOrchestraSettings.MovementVector = Vector2.Zero;
		//    ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.Excalibur, settings, Projectile.owner);
		//}

		Projectile lastStar = (Projectile.ai[0] != -255) ? Main.projectile[(int)Projectile.ai[0]] : Projectile;
		if (!lastStar.active)
		{
			lastStar = Projectile;
		}
		ParticleSystem.AddParticle(new AeonStarburst(), Projectile.Center, Vector2.Zero, Color.Yellow, Projectile.rotation, 2);
		if (lastStar == Projectile)
		{
			ParticleSystem.AddParticle(new AeonStarburst(), Projectile.Center, Vector2.Zero, Color.Red, Projectile.rotation + MathHelper.Pi, 3);
		}
	}
}

public class AeonExplosion : ModProjectile
{
	public override void SetDefaults()
	{
		Projectile.Size = new Vector2(128);
		Projectile.aiStyle = -1;
		Projectile.alpha = 0;
		Projectile.penetrate = -1;
		Projectile.scale = 1f;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.friendly = true;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 21;
		Projectile.tileCollide = false;
		Projectile.timeLeft = 1;
	}
	public override void OnSpawn(IEntitySource source)
	{
		//for(int i = 0; i < 600; i++)
		//{
		//    int D = Dust.NewDust(Projectile.position, 128, 128, DustID.UnusedWhiteBluePurple, 0, 0, 0, default, 3);
		//    Main.dust[D].color = new Color(255, 255, 255, 0);
		//    Main.dust[D].noGravity = !Main.rand.NextBool(3);
		//    Main.dust[D].velocity *= 0;
		//    Main.dust[D].noLightEmittence = true;
		//}
	}
	public override bool PreDraw(ref Color lightColor)
	{
		return false;
	}
	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
	{
		modifiers.HitDirectionOverride = (target.Center.X <= Projectile.Center.X) ? -1 : 1;
	}
}
