using Avalon.Common.Templates;
using Avalon.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Avalon.Projectiles.Melee;

public class Cell : FlailTemplate
{
	public override int LaunchTimeLimit => 16;
	public override float LaunchSpeed => 14f;
	public override float MaxLaunchLength => 700f;
	public override float RetractAcceleration => 3f;
	public override float MaxRetractSpeed => 13f;
	public override float ForcedRetractAcceleration => 6f;
	public override float MaxForcedRetractSpeed => 16f;
	public override int DefaultHitCooldown => 10;
	public override int SpinHitCooldown => 20;
	public override int MovingHitCooldown => 10;

	public override void SetStaticDefaults()
	{
		// These lines facilitate the trail drawing
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 2;

		base.SetStaticDefaults();
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
		Projectile.width = 20;
		Projectile.height = 20;
	}

	public override bool EmitDust(int dustType, Vector2? posMod, Vector2? velMod, float velMaxRadians, float velMult, int antecedent, int consequent, float fadeIn, bool noGravity, float scale, byte alpha)
	{
		dustType = ModContent.DustType<ContagionWeapons>();
		scale = 1.5f;
		alpha = 128;
		if (CurrentAIState == AIState.Spinning) // The base method does not specify conditions for spawning the dust, so you are able to specify anything here
		{
			consequent = 2;
		}
		else if (Projectile.velocity.Length() <= 3)
		{
			velMaxRadians = MathHelper.TwoPi;
			velMult = 1.5f;
			consequent = 5;
		}
		return base.EmitDust(dustType, posMod, velMod, velMaxRadians, velMult, antecedent, consequent, fadeIn, noGravity, scale, alpha);
	}
}
