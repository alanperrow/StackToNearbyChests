using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using static StardewValley.Menus.ItemGrabMenu;

namespace StackToNearbyChests
{
	static class StackLogic
	{
		internal static bool StackToNearbyChests(int radius)
		{
			bool movedAtLeastOne = false;
			Farmer farmer = Game1.player;

			foreach (Chest chest in GetChestsAroundFarmer(farmer, radius))
			{
				List<Item> itemsToRemoveFromPlayer = new List<Item>();

				//Find remaining stack size of CHEST item. check if player has the item, then remove as much as possible
				foreach (Item chestItem in chest.items)
				{
					if (chestItem == null)
					{
						continue;
					}

					foreach (Item playerItem in farmer.Items)
					{
						if (playerItem == null)
						{
							continue;
						}

						int remainingStackSize = chestItem.getRemainingStackSpace();

						if (!itemsToRemoveFromPlayer.Contains(playerItem) && playerItem.canStackWith(chestItem))
						{
							movedAtLeastOne = true;
							int amountToRemove = Math.Min(remainingStackSize, playerItem.Stack);
							chestItem.Stack += amountToRemove;

							if (playerItem.Stack > amountToRemove)
							{
								playerItem.Stack -= amountToRemove;
							}
							else
							{
								itemsToRemoveFromPlayer.Add(playerItem);
							}
						}
					}
				}

				itemsToRemoveFromPlayer.ForEach(x => farmer.removeItemFromInventory(x));
			}

			Game1.playSound(movedAtLeastOne ? "Ship" : "cancel");

			return movedAtLeastOne;
		}

		internal static bool StackToNearbyChests(int radius, InventoryPage inventoryPage)
		{
			if (inventoryPage == null)
			{
				return StackToNearbyChests(radius);
			}

			bool movedAtLeastOne = false;
			Farmer farmer = Game1.player;

			foreach (Chest chest in GetChestsAroundFarmer(farmer, radius))
			{
				List<Item> itemsToRemoveFromPlayer = new List<Item>();

				//Find remaining stack size of CHEST item. check if player has the item, then remove as much as possible
				foreach (Item chestItem in chest.items)
				{
					if (chestItem == null)
					{
						continue;
					}

					for(int i = 0; i < inventoryPage.inventory.actualInventory.Count; i++)
					{
						Item playerItem = inventoryPage.inventory.actualInventory[i];

						if (playerItem == null)
						{
							continue;
						}

						int remainingStackSize = chestItem.getRemainingStackSpace();

						if (playerItem.canStackWith(chestItem))
						{
							movedAtLeastOne = true;
							int amountToRemove = Math.Min(remainingStackSize, playerItem.Stack);
							chestItem.Stack += amountToRemove;

							if (playerItem.Stack > amountToRemove)
							{
								playerItem.Stack -= amountToRemove;
							}
							else
							{
								itemsToRemoveFromPlayer.Add(playerItem);

								ButtonHolder.AddTransferredItemSprite(new TransferredItemSprite(
									playerItem.getOne(), inventoryPage.inventory.inventory[i].bounds.X, inventoryPage.inventory.inventory[i].bounds.Y)
								);
							}
						}
					}
				}

				itemsToRemoveFromPlayer.ForEach(x => farmer.removeItemFromInventory(x));
			}

			Game1.playSound(movedAtLeastOne ? "Ship" : "cancel");

			return movedAtLeastOne;
		}

		internal static IEnumerable<Chest> GetChestsAroundFarmer(Farmer farmer, int radius)
		{
			Vector2 farmerLocation = farmer.getTileLocation();

			//Normal object chests
			/*
			 * TODO: Verify this works with stone chests as well
			 */
			for (int dx = -radius; dx <= radius; dx++)
			{
				for (int dy = -radius; dy <= radius; dy++)
				{
					Vector2 checkLocation = Game1.tileSize * (farmerLocation + new Vector2(dx, dy));
					StardewValley.Object blockObject = farmer.currentLocation.getObjectAt((int)checkLocation.X, (int)checkLocation.Y);

					if (blockObject is Chest)
					{
						Chest chest = blockObject as Chest;
						yield return chest;
					}
				}
			}

			/*
			 * TODO: Mini Fridge
			 */

			//Fridge
			if (farmer?.currentLocation is FarmHouse farmHouse && farmHouse.upgradeLevel >= 1) //Lvl 1,2,3 is where you have fridge upgrade
			{
				Point fridgeLocation = farmHouse.getKitchenStandingSpot();
				fridgeLocation.X += 2; fridgeLocation.Y += -1; //Fridge spot relative to kitchen spot

				if (Math.Abs(farmerLocation.X - fridgeLocation.X) <= radius && Math.Abs(farmerLocation.Y - fridgeLocation.Y) <= radius)
				{
					if (farmHouse.fridge.Value != null)
						yield return farmHouse.fridge.Value;
					else
						Console.WriteLine("StackToNearbyChests: could not find fridge!");
				}
			}

			//Mills and Junimo Huts
			if (farmer.currentLocation is BuildableGameLocation buildableGameLocation)
			{
				foreach (Building building in buildableGameLocation.buildings)
				{
					if (Math.Abs(building.tileX.Value - farmerLocation.X) <= radius && Math.Abs(building.tileY.Value - farmerLocation.Y) <= radius)
					{
						if (building is JunimoHut junimoHut)
							yield return junimoHut.output.Value;
						if (building is Mill mill)
							yield return mill.output.Value;
					}
				}
			}
		}
	}
}
