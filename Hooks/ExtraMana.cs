using System;
using Avalon.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ModLoader;

namespace Avalon.Hooks;

[Autoload(Side = ModSide.Client)]
public class ExtraMana : ModHook {
    private const int ManaPerCrystal = 20;
    private const int MaxManaTier = 6;

    protected override void Apply() {
        On_PlayerStatsSnapshot.ctor += OnPlayerStatsSnapshotCtor;
        On_FancyClassicPlayerResourcesDisplaySet.StarFillingDrawer += OnStarFillingDrawer;
        On_HorizontalBarsPlayerResourcesDisplaySet.ManaFillingDrawer += OnManaFillingDrawer;
        IL_ClassicPlayerResourcesDisplaySet.DrawMana += ILClassicDrawMana;
    }

    private void OnPlayerStatsSnapshotCtor(On_PlayerStatsSnapshot.orig_ctor orig, ref PlayerStatsSnapshot self,
                                           Player player) {
        orig(ref self, player);
        self.AmountOfManaStars = Math.Min(self.AmountOfManaStars, 10);
    }

    private static int GetManaTier(int lastElementIndex, int elementIndex) {
        int maxManaCrystals = lastElementIndex + 1;
        int baseTier = Main.LocalPlayer.statManaMax2 / (maxManaCrystals * ManaPerCrystal);
        int manaAboveTier = Main.LocalPlayer.statManaMax2 % (maxManaCrystals * ManaPerCrystal);
        int manaTier = baseTier + Convert.ToInt32(manaAboveTier - elementIndex * ManaPerCrystal > 0);
        return Math.Min(manaTier, MaxManaTier);
    }

    private static void ILClassicDrawMana(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(instruction => instruction.MatchLdsfld(typeof(TextureAssets), nameof(TextureAssets.Mana)));
        cursor.GotoNext(instruction => instruction.MatchCall<ResourceOverlayDrawContext>(".ctor"));

        // i
        cursor.Emit(OpCodes.Ldloc, 8);

        // this.UIDisplay_ManaPerStar
        cursor.Emit(OpCodes.Ldarg, 0);
        cursor.Emit<ClassicPlayerResourcesDisplaySet>(OpCodes.Ldfld, "UIDisplay_ManaPerStar");

        cursor.EmitDelegate((Asset<Texture2D> origManaTexture, int elementIndex, float manaPerStar) => {
            elementIndex--;

            int lastElementIndex = Main.LocalPlayer.statManaMax2 / (int)manaPerStar - 1;

            int manaTier = GetManaTier(lastElementIndex, elementIndex);

            if (manaTier > 1) {
                return ExxoAvalonOrigins.Mod.Assets.Request<Texture2D>(
                    $"{ExxoAvalonOrigins.TextureAssetsPath}/UI/Mana{manaTier}", AssetRequestMode.ImmediateLoad);
            }

            return origManaTexture;
        });
    }

    private static void OnStarFillingDrawer(On_FancyClassicPlayerResourcesDisplaySet.orig_StarFillingDrawer orig,
                                            FancyClassicPlayerResourcesDisplaySet self, int elementIndex,
                                            int firstElementIndex, int lastElementIndex, out Asset<Texture2D> sprite,
                                            out Vector2 offset, out float drawScale, out Rectangle? sourceRect) {
        orig(self, elementIndex, firstElementIndex, lastElementIndex, out sprite, out offset, out drawScale,
            out sourceRect);

        int manaTier = GetManaTier(lastElementIndex, elementIndex);

        if (manaTier > 1) {
            sprite = ExxoAvalonOrigins.Mod.Assets.Request<Texture2D>(
                $"{ExxoAvalonOrigins.TextureAssetsPath}/UI/FancyMana{manaTier}", AssetRequestMode.ImmediateLoad);
        }
    }

    private static void OnManaFillingDrawer(On_HorizontalBarsPlayerResourcesDisplaySet.orig_ManaFillingDrawer orig,
                                            HorizontalBarsPlayerResourcesDisplaySet self, int elementIndex,
                                            int firstElementIndex, int lastElementIndex, out Asset<Texture2D> sprite,
                                            out Vector2 offset, out float drawScale, out Rectangle? sourceRect) {
        orig(self, elementIndex, firstElementIndex, lastElementIndex, out sprite, out offset, out drawScale,
            out sourceRect);

        int manaTier = GetManaTier(lastElementIndex, elementIndex);

        if (manaTier > 1) {
            sprite = ExxoAvalonOrigins.Mod.Assets.Request<Texture2D>(
                $"{ExxoAvalonOrigins.TextureAssetsPath}/UI/BarMana{manaTier}", AssetRequestMode.ImmediateLoad);
        }
    }
}
