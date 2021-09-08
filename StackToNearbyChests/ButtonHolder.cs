using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using System.Collections.Generic;
using static StardewValley.Menus.ItemGrabMenu;

namespace StackToNearbyChests
{
	class ButtonHolder
	{
		private const int buttonID = 1070000;//A random number

		internal static Texture2D ButtonIcon { private get; set; }

		private static ClickableTextureComponent button;
		private static InventoryPage inventoryPage;
		private static bool drawHoverText = false;
		private static List<TransferredItemSprite> transferredItemSprites = new List<TransferredItemSprite>();

		//When InventoryPage constructed, create a new button
		public static void Constructor(InventoryPage inventoryPage, int x, int y, int width, int height)
		{
			ButtonHolder.inventoryPage = inventoryPage;

			button = new ClickableTextureComponent("",
				new Rectangle(inventoryPage.xPositionOnScreen + width, inventoryPage.yPositionOnScreen + height / 3 - 64 + 8 + 80, 64, 64),
				string.Empty,
				"Stack To Nearby Chests",
				ButtonIcon,
				Rectangle.Empty,
				4f,
				false)
			{
				myID = buttonID,
				downNeighborID = 105,  // trash can
				upNeighborID = 106,  // organize button
				leftNeighborID = 11  // top-right inventory slot
			};

			inventoryPage.organizeButton.downNeighborID = buttonID;
			inventoryPage.trashCan.upNeighborID = buttonID;
		}

		public static void ReceiveLeftClick(int x, int y)
		{
			if (button != null && button.containsPoint(x, y))
			{
				// TODO: Make button shake if successfully stacked
				/*if */StackLogic.StackToNearbyChests(ModEntry.Config.Radius, inventoryPage);
				/* {
				 *	this._iconShakeTimer[index] = Game1.currentGameTime.TotalGameTime.TotalSeconds + 0.5;
				 * }
				 */
				//(ItemGrabMenu)inventoryPage.
			}
		}

		public static void PerformHoverAction(int x, int y)
		{
			button.tryHover(x, y);
			drawHoverText = button.containsPoint(x, y);
		}

		public static void PopulateClickableComponentsList(InventoryPage inventoryPage)
		{
			inventoryPage.allClickableComponents.Add(button);
		}

		//Run before drawing hover texts. Use for drawing the button.
		public static void TrashCanDrawn(ClickableTextureComponent textureComponent, SpriteBatch spriteBatch)
		{
			if (inventoryPage != null && inventoryPage.trashCan == textureComponent)
			{
				// Draw transferred item sprites
				foreach (TransferredItemSprite transferredItemSprite in transferredItemSprites)
				{
					transferredItemSprite.Draw(spriteBatch);
				}

				// Check if button is shaking
				/*
				if (this._iconShakeTimer.ContainsKey(i) && Game1.currentGameTime.TotalGameTime.TotalSeconds >= this._iconShakeTimer[i])
				{
					this._iconShakeTimer.Remove(i);
				}
				{
					toDraw2 += 1f * new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
				}
				*/

				button?.draw(spriteBatch);
			}
		}


		//This is run after drawing everything else in InventoryPage. Use for drawing hover text (on top of everything)
		public static void PostDraw(SpriteBatch spriteBatch)
		{
			if (drawHoverText)
			{
				IClickableMenu.drawToolTip(spriteBatch, button.hoverText, string.Empty, null, false, -1, 0, /*166*/-1, -1, null, -1);

				/*
				 * TODO: Draw preview of all chests/inventories in range. (Should also show chest colors, fridge, hut, etc...)
				 */
			}
		}

		// Used to update transferredItemSprite animation
		public static void Update(GameTime time)
		{
			for (int i = 0; i < transferredItemSprites.Count; i++)
			{
				if (transferredItemSprites[i].Update(time))
				{
					transferredItemSprites.RemoveAt(i);
					i--;
				}
			}
		}

		public static void AddTransferredItemSprite(TransferredItemSprite itemSprite)
		{
			transferredItemSprites.Add(itemSprite);
		}
	}
}
