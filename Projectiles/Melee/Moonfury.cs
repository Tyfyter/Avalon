using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace Avalon.Projectiles.Melee;

public class Moonfury : ModProjectile
{
    private const string ChainTexturePath = "Avalon/Projectiles/Melee/Moonfury_Chain"; // The folder path to the flail chain sprite
    private const string ChainTextureExtraPath = "Avalon/Projectiles/Melee/Moonfury_Chain";  // This texture and related code is optional and used for a unique effect

    private enum AIState
    {
        Spinning,
        LaunchingForward,
        Retracting,
        UnusedState,
        ForcedRetracting,
        Ricochet,
        Dropping
    }

    // These properties wrap the usual ai and localAI arrays for cleaner and easier to understand code.
    private AIState CurrentAIState
    {
        get => (AIState)Projectile.ai[0];
        set => Projectile.ai[0] = (float)value;
    }
    public ref float StateTimer => ref Projectile.ai[1];
    public ref float CollisionCounter => ref Projectile.localAI[0];
    public ref float SpinningStateTimer => ref Projectile.localAI[1];

    public override void SetStaticDefaults()
    {
        // These lines facilitate the trail drawing
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Rectangle dims = this.GetDims();
        Projectile.netImportant = true; // This ensures that the projectile is synced when other players join the world.
        Projectile.width = 40; // The width of your projectile
        Projectile.height = 40; // The height of your projectile
        Projectile.friendly = true; // Deals damage to enemies
        Projectile.penetrate = -1; // Infinite pierce
        Projectile.DamageType = DamageClass.Melee; // Deals melee damage
        Projectile.usesLocalNPCImmunity = true; // Used for hit cooldown changes in the ai hook
        Projectile.localNPCHitCooldown = 10; // This facilitates custom hit cooldown logic
        //DrawOffsetX = -(int)((dims.Width / 2) - (Projectile.Size.X / 2));
        //DrawOriginOffsetY = -(int)((dims.Width / 2) - (Projectile.Size.Y / 2));

        // Vanilla flails all use aiStyle 15, but the code isn't customizable so an adaption of that aiStyle is used in the AI method
    }

    // This AI code was adapted from vanilla code: Terraria.Projectile.AI_015_Flails()
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        #region AI
        // Kill the projectile if the player dies or gets crowd controlled
        if (!player.active || player.dead || player.noItems || player.CCed || Vector2.Distance(Projectile.Center, player.Center) > 900f)
        {
            Projectile.Kill();
            return;
        }
        if (Main.myPlayer == Projectile.owner && Main.mapFullscreen)
        {
            Projectile.Kill();
            return;
        }

        Vector2 mountedCenter = player.MountedCenter;
        bool doFastThrowDust = false;
        bool shouldOwnerHitCheck = false;
        int launchTimeLimit = 19;  // How much time the projectile can go before retracting (speed and shootTimer will set the flail's range)
        float launchSpeed = 18f; // How fast the projectile can move
        float maxLaunchLength = 800f; // How far the projectile's chain can stretch before being forced to retract when in launched state
        float retractAcceleration = 5f; // How quickly the projectile will accelerate back towards the player while retracting
        float maxRetractSpeed = 16; // The max speed the projectile will have while retracting
        float forcedRetractAcceleration = 6f; // How quickly the projectile will accelerate back towards the player while being forced to retract
        float maxForcedRetractSpeed = maxRetractSpeed; // The max speed the projectile will have while being forced to retract
        float unusedRetractAcceleration = 1f;
        float unusedMaxRetractSpeed = 14f;
        int unusedChainLength = 60;
        int defaultHitCooldown = 10; // How often your flail hits when resting on the ground, or retracting
        int spinHitCooldown = 20; // How often your flail hits when spinning
        int movingHitCooldown = 10; // How often your flail hits when moving
        int ricochetTimeLimit = launchTimeLimit + 5;

        // Scaling these speeds and accelerations by the players melee speed makes the weapon more responsive if the player boosts it or general weapon speed
        float meleeSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Melee);
        launchSpeed *= meleeSpeedMultiplier;
        unusedRetractAcceleration *= meleeSpeedMultiplier;
        unusedMaxRetractSpeed *= meleeSpeedMultiplier;
        retractAcceleration *= meleeSpeedMultiplier;
        maxRetractSpeed *= meleeSpeedMultiplier;
        forcedRetractAcceleration *= meleeSpeedMultiplier;
        maxForcedRetractSpeed *= meleeSpeedMultiplier;
        float launchRange = launchSpeed * launchTimeLimit;
        float maxDroppedRange = launchRange + 160f;
        Projectile.localNPCHitCooldown = defaultHitCooldown;

        switch (CurrentAIState)
        {
            case AIState.Spinning:
                {
                    shouldOwnerHitCheck = true;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 unitVectorTowardsMouse = mountedCenter.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX * player.direction);
                        player.ChangeDir((unitVectorTowardsMouse.X > 0f).ToDirectionInt());
                        if (!player.channel) // If the player releases then change to moving forward mode
                        {
                            CurrentAIState = AIState.LaunchingForward;
                            StateTimer = 0f;
                            Projectile.velocity = unitVectorTowardsMouse * launchSpeed + player.velocity;
                            Projectile.Center = mountedCenter;
                            Projectile.netUpdate = true;
                            Projectile.ResetLocalNPCHitImmunity();
                            Projectile.localNPCHitCooldown = movingHitCooldown;
                            break;
                        }
                    }
                    SpinningStateTimer++;
                    // This line creates a unit vector that is constantly rotated around the player. 10f controls how fast the projectile visually spins around the player
                    Vector2 offsetFromPlayer = new Vector2(player.direction).RotatedBy((float)Math.PI * 10f * (SpinningStateTimer / 60f) * player.direction);

                    offsetFromPlayer.Y *= 0.8f;
                    if (offsetFromPlayer.Y * player.gravDir > 0f)
                    {
                        offsetFromPlayer.Y *= 0.5f;
                    }
                    Projectile.Center = mountedCenter + offsetFromPlayer * 30f;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.localNPCHitCooldown = spinHitCooldown; // set the hit speed to the spinning hit speed
                    break;
                }
            case AIState.LaunchingForward:
                {
                    doFastThrowDust = true;
                    bool shouldSwitchToRetracting = StateTimer++ >= launchTimeLimit;
                    shouldSwitchToRetracting |= Projectile.Distance(mountedCenter) >= maxLaunchLength;
                    if (player.controlUseItem) // If the player clicks, transition to the Dropping state
                    {
                        CurrentAIState = AIState.Dropping;
                        StateTimer = 0f;
                        Projectile.netUpdate = true;
                        Projectile.velocity *= 0.2f;
                        if (Main.myPlayer == Projectile.owner)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<MoonfuryBlade>(), Projectile.damage, Projectile.knockBack, Main.myPlayer, 0, Projectile.velocity.X, Projectile.velocity.Y);
                        break;
                    }
                    if (shouldSwitchToRetracting)
                    {
                        CurrentAIState = AIState.Retracting;
                        StateTimer = 0f;
                        Projectile.netUpdate = true;
                        Projectile.velocity *= 0.3f;
                        if (Main.myPlayer == Projectile.owner)
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<MoonfuryBlade>(), Projectile.damage, Projectile.knockBack, Main.myPlayer,0,Projectile.velocity.X,Projectile.velocity.Y);
                        // This is also where Drippler Crippler spawns its projectile, see above code.
                    }
                    player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                    Projectile.localNPCHitCooldown = movingHitCooldown;
                    break;
                }
            case AIState.Retracting:
                {
                    Vector2 unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                    if (Projectile.Distance(mountedCenter) <= maxRetractSpeed)
                    {
                        Projectile.Kill(); // Kill the projectile once it is close enough to the player
                        return;
                    }
                    if (player.controlUseItem) // If the player clicks, transition to the Dropping state
                    {
                        CurrentAIState = AIState.Dropping;
                        StateTimer = 0f;
                        Projectile.netUpdate = true;
                        Projectile.velocity *= 0.2f;
                    }
                    else
                    {
                        Projectile.velocity *= 0.98f;
                        Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * maxRetractSpeed, retractAcceleration);
                        player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                    }
                    break;
                }
            case AIState.UnusedState: // Projectile.ai[0] == 3; This case is actually unused, but maybe a Terraria update will add it back in, or maybe it is useless, so I left it here.
                {
                    if (!player.controlUseItem)
                    {
                        CurrentAIState = AIState.ForcedRetracting; // Move to super retracting mode if the player taps
                        StateTimer = 0f;
                        Projectile.netUpdate = true;
                        break;
                    }
                    float currentChainLength = Projectile.Distance(mountedCenter);
                    Projectile.tileCollide = StateTimer == 1f;
                    bool flag3 = currentChainLength <= launchRange;
                    if (flag3 != Projectile.tileCollide)
                    {
                        Projectile.tileCollide = flag3;
                        StateTimer = Projectile.tileCollide ? 1 : 0;
                        Projectile.netUpdate = true;
                    }
                    if (currentChainLength > unusedChainLength)
                    {

                        if (currentChainLength >= launchRange)
                        {
                            Projectile.velocity *= 0.5f;
                            Projectile.velocity = Projectile.velocity.MoveTowards(Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero) * unusedMaxRetractSpeed, unusedMaxRetractSpeed);
                        }
                        Projectile.velocity *= 0.98f;
                        Projectile.velocity = Projectile.velocity.MoveTowards(Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero) * unusedMaxRetractSpeed, unusedRetractAcceleration);
                    }
                    else
                    {
                        if (Projectile.velocity.Length() < 6f)
                        {
                            Projectile.velocity.X *= 0.96f;
                            Projectile.velocity.Y += 0.2f;
                        }
                        if (player.velocity.X == 0f)
                        {
                            Projectile.velocity.X *= 0.96f;
                        }
                    }
                    player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                    break;
                }
            case AIState.ForcedRetracting:
                {
                    Projectile.tileCollide = false;
                    Vector2 unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                    if (Projectile.Distance(mountedCenter) <= maxForcedRetractSpeed)
                    {
                        Projectile.Kill(); // Kill the projectile once it is close enough to the player
                        return;
                    }
                    Projectile.velocity *= 0.98f;
                    Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * maxForcedRetractSpeed, forcedRetractAcceleration);
                    Vector2 target = Projectile.Center + Projectile.velocity;
                    Vector2 value = mountedCenter.DirectionFrom(target).SafeNormalize(Vector2.Zero);
                    if (Vector2.Dot(unitVectorTowardsPlayer, value) < 0f)
                    {
                        Projectile.Kill(); // Kill projectile if it will pass the player
                        return;
                    }
                    player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                    break;
                }
            case AIState.Ricochet:
                if (StateTimer++ >= ricochetTimeLimit)
                {
                    CurrentAIState = AIState.Dropping;
                    StateTimer = 0f;
                    Projectile.netUpdate = true;
                }
                else
                {
                    Projectile.localNPCHitCooldown = movingHitCooldown;
                    Projectile.velocity.Y += 0.6f;
                    Projectile.velocity.X *= 0.95f;
                    player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                }
                break;
            case AIState.Dropping:
                if (!player.controlUseItem || Projectile.Distance(mountedCenter) > maxDroppedRange)
                {
                    CurrentAIState = AIState.ForcedRetracting;
                    StateTimer = 0f;
                    Projectile.netUpdate = true;
                }
                else
                {
                    Projectile.velocity.Y += 0.8f;
                    Projectile.velocity.X *= 0.95f;
                    player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                }
                break;
        }

        // This is where Flower Pow launches projectiles. Decompile Terraria to view that code.

        Projectile.direction = (Projectile.velocity.X > 0f).ToDirectionInt();
        Projectile.spriteDirection = Projectile.direction;
        Projectile.ownerHitCheck = shouldOwnerHitCheck; // This prevents attempting to damage enemies without line of sight to the player. The custom Colliding code for spinning makes this necessary.

        //This rotation code is unique to this flail, since the sprite isn't rotationally symmetric and has tip.
        bool freeRotation = CurrentAIState == AIState.Ricochet || CurrentAIState == AIState.Dropping;
        if (freeRotation)
        {
            if (Projectile.velocity.Length() > 1f)
                Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.velocity.X * 0.1f; // skid
            else
                Projectile.rotation += Projectile.velocity.X * 0.1f; // roll
        }
        else
        {
            Vector2 vectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
            Projectile.rotation = vectorTowardsPlayer.ToRotation() + MathHelper.PiOver2;
        }

        // If you have a ball shaped flail, you can use this simplified rotation code instead

        //if (Projectile.velocity.Length() > 1f)
        //    Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.velocity.X * 0.1f; // skid
        //else
        //    Projectile.rotation += Projectile.velocity.X * 0.1f; // roll


        Projectile.timeLeft = 2; // Makes sure the flail doesn't die (good when the flail is resting on the ground)
        player.heldProj = Projectile.whoAmI;
        player.SetDummyItemTime(2); //Add a delay so the player can't button mash the flail
        player.itemRotation = Projectile.DirectionFrom(mountedCenter).ToRotation();
        if (Projectile.Center.X < mountedCenter.X)
        {
            player.itemRotation += (float)Math.PI;
        }
        player.itemRotation = MathHelper.WrapAngle(player.itemRotation);

        #endregion AI

        //for(int i = 0; i < 2; i++)
        //{
        //    if (Projectile.ai[0] != 0)
        //    {
        //        if (Projectile.velocity.Length() >= 1)
        //        {
        //            Dust d = Dust.NewDustPerfect(Projectile.Center + new Vector2(0, 20 * Main.masterColor).RotatedBy(Projectile.velocity.ToRotation()).RotatedBy(i * MathHelper.Pi), DustID.ShadowbeamStaff, Projectile.velocity);
        //            d.scale = 1.5f;
        //            d.noGravity = true;
        //        }
        //    }
        //    else
        //    {
        //        Dust d = Dust.NewDustPerfect(Projectile.Center + new Vector2(10 * Main.masterColor, 0).RotatedBy(Projectile.Center.DirectionTo(player.Center).ToRotation()).RotatedBy(i * MathHelper.Pi), DustID.ShadowbeamStaff, Vector2.Zero);
        //        d.velocity = new Vector2(0, -2 * player.direction).RotatedBy(Projectile.Center.DirectionTo(player.Center).ToRotation());
        //        d.scale = 1.5f;
        //        d.noGravity = true;
        //    }
        //}
        if (Projectile.velocity.Length() > 3 || CurrentAIState == AIState.Spinning)
        {
            Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(22, 22), DustID.Shadowflame, Projectile.velocity.RotatedByRandom(0.1f) * 0.7f, 128);
            d.fadeIn = 1.5f;
            d.noGravity = true;
            d.scale = 1.5f;
            Dust d2 = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(22, 22), DustID.ShadowbeamStaff, Projectile.velocity.RotatedByRandom(0.1f), 128);
            d2.fadeIn = 1.5f;
            d2.noGravity = true;
            d2.scale = 1.5f;
            if (Projectile.ai[0] == 0)
            {
                d.velocity = new Vector2(0, -3 * player.direction).RotatedBy(Projectile.Center.DirectionTo(player.Center).ToRotation()).RotatedByRandom(0.1f);
                d2.velocity = new Vector2(0, -5 * player.direction).RotatedBy(Projectile.Center.DirectionTo(player.Center).ToRotation()).RotatedByRandom(0.1f);
            }
        }
        Projectile.rotation = Projectile.Center.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        int defaultLocalNPCHitCooldown = 10;
        int impactIntensity = 0;
        Vector2 velocity = Projectile.velocity;
        float bounceFactor = 0.2f;
        if (CurrentAIState == AIState.LaunchingForward || CurrentAIState == AIState.Ricochet)
        {
            bounceFactor = 0.4f;
        }

        if (CurrentAIState == AIState.Dropping)
        {
            bounceFactor = 0f;
        }

        if (oldVelocity.X != Projectile.velocity.X)
        {
            if (Math.Abs(oldVelocity.X) > 4f)
            {
                impactIntensity = 1;
            }

            Projectile.velocity.X = (0f - oldVelocity.X) * bounceFactor;
            CollisionCounter += 1f;
        }

        if (oldVelocity.Y != Projectile.velocity.Y)
        {
            if (Math.Abs(oldVelocity.Y) > 4f)
            {
                impactIntensity = 1;
            }

            Projectile.velocity.Y = (0f - oldVelocity.Y) * bounceFactor;
            CollisionCounter += 1f;
        }

        // Here the tiles spawn dust indicating they've been hit
        if (impactIntensity > 0)
        {
            Projectile.netUpdate = true;
            for (int i = 0; i < impactIntensity; i++)
            {
                Collision.HitTiles(Projectile.position, velocity, Projectile.width, Projectile.height);
            }
            if (CurrentAIState == AIState.LaunchingForward)
                Projectile.CreateImpactExplosion2_FlailTileCollision(Projectile.Center, true, Projectile.oldVelocity);
            Projectile.position -= Projectile.oldVelocity;

            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        }

        // Force retraction if stuck on tiles while retracting
        if (CurrentAIState != AIState.UnusedState && CurrentAIState != AIState.Spinning && CurrentAIState != AIState.Ricochet && CurrentAIState != AIState.Dropping && CollisionCounter >= 10f)
        {
            CurrentAIState = AIState.ForcedRetracting;
            Projectile.netUpdate = true;
        }

        // tModLoader currently does not provide the wetVelocity parameter, this code should make the flail bounce back faster when colliding with tiles underwater.
        //if (Projectile.wet)
        //	wetVelocity = Projectile.velocity;

        return false;
    }

    public override bool? CanDamage()
    {
        // Flails in spin mode won't damage enemies within the first 12 ticks. Visually this delays the first hit until the player swings the flail around for a full spin before damaging anything.
        if (CurrentAIState == AIState.Spinning && SpinningStateTimer <= 12f)
        {
            return false;
        }
        return base.CanDamage();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        // Flails do special collision logic that serves to hit anything within an ellipse centered on the player when the flail is spinning around the player. For example, the projectile rotating around the player won't actually hit a bee if it is directly on the player usually, but this code ensures that the bee is hit. This code makes hitting enemies while spinning more consistant and not reliant of the actual position of the flail projectile.
        if (CurrentAIState == AIState.Spinning)
        {
            Vector2 mountedCenter = Main.player[Projectile.owner].MountedCenter;
            Vector2 shortestVectorFromPlayerToTarget = targetHitbox.ClosestPointInRect(mountedCenter) - mountedCenter;
            shortestVectorFromPlayerToTarget.Y /= 0.8f; // Makes the hit area an ellipse. Vertical hit distance is smaller due to this math.
            float hitRadius = 55f; // The length of the semi-major radius of the ellipse (the long end)
            return shortestVectorFromPlayerToTarget.Length() <= hitRadius;
        }
        // Regular collision logic happens otherwise.
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        // Flails do a few custom things, you'll want to keep these to have the same feel as vanilla flails.

        // The hitDirection is always set to hit away from the player, even if the flail damages the npc while returning
        modifiers.HitDirectionOverride = (Main.player[Projectile.owner].Center.X < target.Center.X).ToDirectionInt();

        // Knockback is only 25% as powerful when in spin mode
        if (CurrentAIState == AIState.Spinning)
        {
            modifiers.Knockback *= 0.25f;
        }
        // Knockback is only 50% as powerful when in drop down mode
        else if (CurrentAIState == AIState.Dropping)
        {
            modifiers.Knockback *= 0.5f;
        }
        
        // Flails do 20% more damage while spinning
        if (CurrentAIState == AIState.Spinning)
        {
            modifiers.FinalDamage *= 1.2f;
        }
        // Flails do 100% more damage while launched or retracting. This is the damage the item tooltip for flails aim to match, as this is the most common mode of attack. This is why the item has ItemID.Sets.ToolTipDamageMultiplier[Type] = 2f;
        else if (CurrentAIState == AIState.LaunchingForward || CurrentAIState == AIState.Retracting)
        {
            modifiers.FinalDamage *= 2f;
        }
        base.ModifyHitNPC(target, ref modifiers);
    }

    // PreDraw is used to draw a chain and trail before the projectile is drawn normally.
    public override bool PreDraw(ref Color lightColor)
    {
        Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);

        // This fixes a vanilla GetPlayerArmPosition bug causing the chain to draw incorrectly when stepping up slopes. The flail itself still draws incorrectly due to another similar bug. This should be removed once the vanilla bug is fixed.
        playerArmPosition.Y -= Main.player[Projectile.owner].gfxOffY;

        Asset<Texture2D> chainTexture = ModContent.Request<Texture2D>(ChainTexturePath);
        Asset<Texture2D> chainTextureExtra = ModContent.Request<Texture2D>(ChainTextureExtraPath); // This texture and related code is optional and used for a unique effect

        Rectangle? chainSourceRectangle = null;
        // Drippler Crippler customizes sourceRectangle to cycle through sprite frames: sourceRectangle = asset.Frame(1, 6);
        float chainHeightAdjustment = 0f; // Use this to adjust the chain overlap. 

        Vector2 chainOrigin = chainSourceRectangle.HasValue ? (chainSourceRectangle.Value.Size() / 2f) : (chainTexture.Size() / 2f);
        Vector2 chainDrawPosition = Projectile.Center;
        Vector2 vectorFromProjectileToPlayerArms = playerArmPosition.MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
        Vector2 unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayerArms.SafeNormalize(Vector2.Zero);
        float chainSegmentLength = (chainSourceRectangle.HasValue ? chainSourceRectangle.Value.Height : chainTexture.Height()) + chainHeightAdjustment;
        if (chainSegmentLength == 0)
        {
            chainSegmentLength = 10; // When the chain texture is being loaded, the height is 0 which would cause infinite loops.
        }
        float chainRotation = unitVectorFromProjectileToPlayerArms.ToRotation() + MathHelper.PiOver2;
        int chainCount = 0;
        float chainLengthRemainingToDraw = vectorFromProjectileToPlayerArms.Length() + chainSegmentLength / 2f;

        // This while loop draws the chain texture from the projectile to the player, looping to draw the chain texture along the path
        while (chainLengthRemainingToDraw > 0f)
        {
            // This code gets the lighting at the current tile coordinates
            Color chainDrawColor = Lighting.GetColor((int)chainDrawPosition.X / 16, (int)(chainDrawPosition.Y / 16f));

            // Flaming Mace and Drippler Crippler use code here to draw custom sprite frames with custom lighting.
            // Cycling through frames: sourceRectangle = asset.Frame(1, 6, 0, chainCount % 6);
            // This example shows how Flaming Mace works. It checks chainCount and changes chainTexture and draw color at different values

            var chainTextureToDraw = chainTexture;
            //if (chainCount >= 4)
            //{
            //    // Use normal chainTexture and lighting, no changes
            //}
            //else if (chainCount >= 2)
            //{
            //    // Near to the ball, we draw a custom chain texture and slightly make it glow if unlit.
            //    chainTextureToDraw = chainTextureExtra;
            //    byte minValue = 140;
            //    if (chainDrawColor.R < minValue)
            //        chainDrawColor.R = minValue;

            //    if (chainDrawColor.G < minValue)
            //        chainDrawColor.G = minValue;

            //    if (chainDrawColor.B < minValue)
            //        chainDrawColor.B = minValue;
            //}
            //else
            //{
            //    // Close to the ball, we draw a custom chain texture and draw it at full brightness glow.
            //    chainTextureToDraw = chainTextureExtra;
            //    chainDrawColor = Color.White;
            //}

            // Here, we draw the chain texture at the coordinates
            Main.spriteBatch.Draw(chainTextureToDraw.Value, chainDrawPosition - Main.screenPosition, chainSourceRectangle, chainDrawColor, chainRotation, chainOrigin, 1f, SpriteEffects.None, 0f);

            // chainDrawPosition is advanced along the vector back to the player by the chainSegmentLength
            chainDrawPosition += unitVectorFromProjectileToPlayerArms * chainSegmentLength;
            chainCount++;
            chainLengthRemainingToDraw -= chainSegmentLength;
        }

        // Add a motion trail when moving forward, like most flails do (don't add trail if already hit a tile)
        if (CurrentAIState == AIState.LaunchingForward)
        {
            Texture2D projectileTexture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(projectileTexture.Width * 0.5f, projectileTexture.Height * 0.5f);
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            for (int k = 0; k < Projectile.oldPos.Length && k < StateTimer; k++)
            {
                //Projectile.GetAlpha(lightColor)
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + (drawOrigin * 1.5f); //+ new Vector2(DrawOffsetX, Projectile.gfxOffY);
                drawPos.Y -= (projectileTexture.Height / 4) - 1;
                Color color = new Color(55, 33, 75, 0) * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.spriteBatch.Draw(projectileTexture, drawPos + Vector2.Normalize(Projectile.velocity) * 8, null, color, Projectile.rotation, drawOrigin, ((Projectile.scale + 0.1f) - k / (float)Projectile.oldPos.Length) * 1.4f, spriteEffects, 0f);
            }
        }
        return true;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        ParticleOrchestraSettings particleOrchestraSettings = default(ParticleOrchestraSettings);
        particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
        ParticleOrchestraSettings settings = particleOrchestraSettings;
        base.OnHitNPC(target, hit, damageDone);
        ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.NightsEdge, settings, Projectile.owner);

        target.AddBuff(BuffID.ShadowFlame, 120);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        if (!info.PvP)
        {
            return;
        }
        ParticleOrchestraSettings particleOrchestraSettings = default(ParticleOrchestraSettings);
        particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
        ParticleOrchestraSettings settings = particleOrchestraSettings;
        base.OnHitPlayer(target, info);
        ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.NightsEdge, settings, Projectile.owner);
    }
}
