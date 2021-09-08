using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using HarmonyLib;
using System;
using Microsoft.Xna.Framework;

namespace StackToNearbyChests.Patches
{
	public class InventoryPageConstructorPatch
	{
		public static void Postfix(InventoryPage __instance, int x, int y, int width, int height)
		{
			ButtonHolder.Constructor(__instance, x, y, width, height);
		}
	}

	[HarmonyPatch(typeof(InventoryPage))]
	public class InventoryPagePatches
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(InventoryPage.receiveLeftClick))]
		public static void ReceiveLeftClick_Postfix(int x, int y)
		{
			ButtonHolder.ReceiveLeftClick(x, y);
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(InventoryPage.draw))]
		public static void Draw_Postfix(SpriteBatch b)
		{
			ButtonHolder.PostDraw(b);
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(InventoryPage.performHoverAction))]
		public static void PerformHoverAction_Postfix(int x, int y)
		{
			ButtonHolder.PerformHoverAction(x, y);
		}
	}

	[HarmonyPatch(typeof(IClickableMenu))]
	public class IClickableMenuPatches
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(IClickableMenu.populateClickableComponentList))]
		public static void PopulateClickableComponentsList_Postfix(IClickableMenu __instance)
		{
			if (__instance is InventoryPage inventoryPage)
			{
				ButtonHolder.PopulateClickableComponentsList(inventoryPage);
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(IClickableMenu.update))]
		public static void Update_Postfix(IClickableMenu __instance, GameTime time)
		{
			if (__instance is InventoryPage inventoryPage)
			{
				ButtonHolder.Update(time);
			}
		}
	}

	[HarmonyPatch(typeof(ClickableTextureComponent))]
	public class ClickableTextureComponentPatches
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(ClickableTextureComponent.draw))]
		[HarmonyPatch(new Type[] { typeof(SpriteBatch) })]
		public static void Draw_Postfix(ClickableTextureComponent __instance, SpriteBatch b)
		{
			ButtonHolder.TrashCanDrawn(__instance, b);
		}
	}
}
