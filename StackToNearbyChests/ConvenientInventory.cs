using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using System.Collections.Generic;
using static StardewValley.Menus.ItemGrabMenu;

namespace StackToNearbyChests
{
	/*
	 * TODO: Implement favorited items, which will be ignored by "Quick Stack To Nearby Chests" button.
	 *       Prefix "Add To Existing Stacks" button logic to ignore favorited items as well.
	 */
	class ConvenientInventory
	{
		private const int ButtonID = 918021;  // Unique indentifier

		internal static Texture2D ButtonIcon { private get; set; }

		private static ClickableTextureComponent Button;
		private static InventoryPage Page;
		private static bool IsDrawToolTip = false;
		private static readonly List<TransferredItemSprite> TransferredItemSprites = new List<TransferredItemSprite>();

		public static void Constructor(InventoryPage inventoryPage, int x, int y, int width, int height)
		{
			Page = inventoryPage;

			Button = new ClickableTextureComponent("",
				new Rectangle(inventoryPage.xPositionOnScreen + width, inventoryPage.yPositionOnScreen + height / 3 - 64 + 8 + 80, 64, 64),
				string.Empty,
				"Quick Stack To Nearby Chests",
				ButtonIcon,
				Rectangle.Empty,
				4f,
				false)
			{
				myID = ButtonID,
				downNeighborID = 105,  // trash can
				upNeighborID = 106,  // organize button
				leftNeighborID = 11  // top-right inventory slot
			};

			inventoryPage.organizeButton.downNeighborID = ButtonID;
			inventoryPage.trashCan.upNeighborID = ButtonID;
		}

		public static void ReceiveLeftClick(int x, int y)
		{
			if (Button != null && Button.containsPoint(x, y))
			{
				StackLogic.StackToNearbyChests(ModEntry.Config.Range, Page);
			}
		}

		public static void PerformHoverAction(int x, int y)
		{
			Button.tryHover(x, y);
			IsDrawToolTip = Button.containsPoint(x, y);
		}

		public static void PopulateClickableComponentsList(InventoryPage inventoryPage)
		{
			inventoryPage.allClickableComponents.Add(Button);
		}

		// Called before drawing tooltip. Use for drawing the button.
		public static void TrashCanDrawn(ClickableTextureComponent textureComponent, SpriteBatch spriteBatch)
		{
			if (Page != null && Page.trashCan == textureComponent)
			{
				// Draw transferred item sprites
				foreach (TransferredItemSprite transferredItemSprite in TransferredItemSprites)
				{
					transferredItemSprite.Draw(spriteBatch);
				}

				Button?.draw(spriteBatch);
			}
		}


		// Called after drawing everything else in InventoryPage. Use for drawing tooltip.
		public static void PostDraw(SpriteBatch spriteBatch)
		{
			if (IsDrawToolTip)
			{
				IClickableMenu.drawToolTip(spriteBatch, Button.hoverText, string.Empty, null, false, -1, 0, /*166*/-1, -1, null, -1);

				/*
				 * TODO: Draw preview of all chests/inventories in range. (Should also show chest colors, fridge, hut, etc...)
				 */

			}
		}

		// Used to update transferredItemSprite animation
		public static void Update(GameTime time)
		{
			for (int i = 0; i < TransferredItemSprites.Count; i++)
			{
				if (TransferredItemSprites[i].Update(time))
				{
					TransferredItemSprites.RemoveAt(i);
					i--;
				}
			}
		}

		public static void AddTransferredItemSprite(TransferredItemSprite itemSprite)
		{
			TransferredItemSprites.Add(itemSprite);
		}
	}
}
