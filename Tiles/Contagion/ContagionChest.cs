using Avalon.Common.Templates;
using Terraria;
using Terraria.ModLoader;

namespace Avalon.Tiles.Contagion
{
	public class ContagionChest : ChestTemplate
	{
		public override int DropItem => ModContent.ItemType<Items.Placeable.Furniture.ContagionChest>();
		protected override bool CanBeLocked => true;
		protected override int ChestKeyItemId => ModContent.ItemType<Items.Other.ContagionKey>();
		public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual)
		{
			return NPC.downedPlantBoss;
		}
		public override bool LockChest(int i, int j, ref short frameXAdjustment, ref bool manual)
		{
			if (!NPC.downedPlantBoss)
			{
				return false;
			}
			return base.LockChest(i, j, ref frameXAdjustment, ref manual);
		}
	}
}
