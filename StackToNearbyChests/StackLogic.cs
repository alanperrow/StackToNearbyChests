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
		internal static bool StackToNearbyChests(int range)
		{
			bool movedAtLeastOne = false;
			Farmer who = Game1.player;

			foreach (Chest chest in GetChestsAroundFarmer(who, range))
			{
				List<Item> itemsToRemoveFromPlayer = new List<Item>();

				//Find remaining stack size of CHEST item. check if player has the item, then remove as much as possible
				foreach (Item chestItem in chest.items)
				{
					if (chestItem == null)
					{
						continue;
					}

					foreach (Item playerItem in who.Items)
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

				itemsToRemoveFromPlayer.ForEach(x => who.removeItemFromInventory(x));
			}

			Game1.playSound(movedAtLeastOne ? "Ship" : "cancel");

			return movedAtLeastOne;
		}

		internal static bool StackToNearbyChests(int range, InventoryPage inventoryPage)
		{
			if (inventoryPage == null)
			{
				return StackToNearbyChests(range);
			}

			bool movedAtLeastOne = false;
			Farmer who = Game1.player;

			foreach (Chest chest in SortByDistance(who, GetChestsAroundFarmer(who, range)))
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

				itemsToRemoveFromPlayer.ForEach(x => who.removeItemFromInventory(x));
			}

			Game1.playSound(movedAtLeastOne ? "Ship" : "cancel");

			return movedAtLeastOne;
		}

		internal static IEnumerable<Chest> GetChestsAroundFarmer(Farmer who, int range)
		{
			if (who is null)
            {
				yield break;
            }

			Vector2 farmerTileLocation = who.getTileLocation();
			GameLocation gameLocation = who.currentLocation;

			// Chest objects (includes mini fridge, ...)
			/*
			 * TODO: Verify this works with stone chests as well
			 */
			for (int dx = -range; dx <= range; dx++)
			{
				for (int dy = -range; dy <= range; dy++)
				{
					Vector2 checkLocation = Game1.tileSize * (farmerTileLocation + new Vector2(dx, dy));
					StardewValley.Object blockObject = gameLocation.getObjectAt((int)checkLocation.X, (int)checkLocation.Y);

					if (blockObject is Chest)
					{
						Chest chest = blockObject as Chest;
						yield return chest;
					}
				}
			}

			// Fridge included with kitchen
			if (who.currentLocation is FarmHouse farmHouse && farmHouse.upgradeLevel >= 1) //Lvl 1,2,3 is where you have fridge upgrade
			{
				Point kitchenStandingSpot = farmHouse.getKitchenStandingSpot();
				Point fridgeTileLocation = new Point(kitchenStandingSpot.X + 2, kitchenStandingSpot.Y - 1); //Fridge spot relative to kitchen spot

				if (Math.Abs(farmerTileLocation.X - fridgeTileLocation.X) <= range && Math.Abs(farmerTileLocation.Y - fridgeTileLocation.Y) <= range)
				{
					if (farmHouse.fridge.Value != null)
						yield return farmHouse.fridge.Value;
					else
						Console.WriteLine("StackToNearbyChests: could not find fridge!");
				}
			}

			// Mills and Junimo Huts
			if (who.currentLocation is BuildableGameLocation buildableGameLocation)
			{
				foreach (Building building in buildableGameLocation.buildings)
				{
					if (Math.Abs(building.tileX.Value - farmerTileLocation.X) <= range && Math.Abs(building.tileY.Value - farmerTileLocation.Y) <= range)
					{
						if (building is JunimoHut junimoHut)
							yield return junimoHut.output.Value;
						if (building is Mill mill)
							yield return mill.output.Value;
					}
				}
			}
		}

		internal static IEnumerable<Vector2> GetNearbyTileLocationsSortedByDistance(Point origin, int range)
		{
			//from origin, return all nearby integer points sorted based on point distance formula, within a given square radius.


			yield break;
		}
	}
}
