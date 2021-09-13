using System;
using System.Collections.Generic;
using System.Linq;
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

			foreach (Chest chest in GetChestsAroundFarmer(who, range, true))
			{
				List<Item> itemsToRemoveFromPlayer = new List<Item>();

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

								if (ModEntry.Config.IsStackOverflowItems)
								{
									// Reference StardewValley.ItemGrabMenu ~ line 1000
								}

								/*
								 * TODO: When chests are full and can't accept the item, make the item shake in inventory (as visual feedback for player).
								 *		 Reference the "Add To Existing Stacks" code for making items shake.
								 */
								if (amountToRemove != 0)
								{
									inventoryPage.inventory.ShakeItem(i);
								}
							}
							else
							{
								who.removeItemFromInventory(playerItem);

								ConvenientInventory.AddTransferredItemSprite(new TransferredItemSprite(
									playerItem.getOne(), inventoryPage.inventory.inventory[i].bounds.X, inventoryPage.inventory.inventory[i].bounds.Y)
								);
							}
						}
					}
				}
			}

			Game1.playSound(movedAtLeastOne ? "Ship" : "cancel");

			return movedAtLeastOne;
		}

		internal static IEnumerable<Chest> GetChestsAroundFarmer(Farmer who, int range, bool sorted = false)
		{
			if (who is null)
            {
				yield break;
            }

			Vector2 farmerPosition = who.getStandingPosition();
			Point farmerTileLocation = who.getTileLocationPoint();
			GameLocation gameLocation = who.currentLocation;

			// Chest objects (includes mini fridge, stone chest)
			if (sorted)
            {
				foreach (Chest chest in GetNearbyChestsWithDistance(farmerPosition, range, gameLocation).OrderBy(x => x.Distance).Select(x => x.Chest))
				{
					yield return chest;
				}
            }
			else
			{
				foreach (Chest chest in GetNearbyChests(farmerTileLocation, range, gameLocation))
				{
					yield return chest;
				}
			}
		}

		private class ChestWithDistance
		{
			public Chest Chest { get; private set; }

			public double Distance { get; private set; }

			public ChestWithDistance(Chest chest, double distance)
			{
				Chest = chest;
				Distance = distance;
			}
		}

		// From origin, return all nearby chests, including the point-distance from their tile-center to origin, within a given square range.
		private static List<ChestWithDistance> GetNearbyChestsWithDistance(Vector2 origin, int range, GameLocation gameLocation)
		{
			var dChests = new List<ChestWithDistance>((2 * range + 1) * (2 * range + 1));
			Vector2 originTileCenterPosition = GetTileCenterPosition(GetTileLocation(origin));
			Vector2 tileLocation = Vector2.Zero;
			int tx, ty, dx, dy;

			// (i/ii) Chests
			for (int x = -range; x <= range; x++)
			{
				for (int y = -range; y <= range; y++)
				{
					tx = (int)originTileCenterPosition.X + (x * Game1.tileSize);
					ty = (int)originTileCenterPosition.Y + (y * Game1.tileSize);

					tileLocation.X = tx / Game1.tileSize;
					tileLocation.Y = ty / Game1.tileSize;

					if (gameLocation.objects.TryGetValue(tileLocation, out StardewValley.Object obj) && obj is Chest chest)
					{
						dx = tx - (int)origin.X;
						dy = ty - (int)origin.Y;

						dChests.Add(new ChestWithDistance(chest, Math.Sqrt(dx * dx + dy * dy)));

						ModEntry.Context.Monitor.Log($"Chest found at [{tileLocation.X}, {tileLocation.Y}].", StardewModdingAPI.LogLevel.Debug);
					}
				}
			}

			// (iii) Kitchen fridge
			if (gameLocation is FarmHouse farmHouse && farmHouse.upgradeLevel > 0)  // Kitchen only exists when upgradeLevel > 0
			{
				Vector2 fridgeTileCenterPosition = GetTileCenterPosition(farmHouse.fridgePosition);

				if (IsPositionWithinRange(origin, fridgeTileCenterPosition, range))
				{
					if (farmHouse.fridge.Value != null)
					{
						dx = (int)fridgeTileCenterPosition.X - (int)origin.X;
						dy = (int)fridgeTileCenterPosition.Y - (int)origin.Y;

						dChests.Add(new ChestWithDistance(farmHouse.fridge.Value, Math.Sqrt(dx * dx + dy * dy)));

						ModEntry.Context.Monitor.Log($"Kitchen fridge found at [{tileLocation.X}, {tileLocation.Y}].", StardewModdingAPI.LogLevel.Debug);
					}
					else
					{
						ModEntry.Context.Monitor.Log("Could not find kitchen fridge!", StardewModdingAPI.LogLevel.Debug);
					}
				}
			}

			// (iv) Buildings
			if (gameLocation is BuildableGameLocation buildableGameLocation)
			{
				foreach (Building building in buildableGameLocation.buildings)
				{
					Vector2 buildingTileCenterPosition = GetTileCenterPosition(building.tileX.Value, building.tileY.Value);

					if (IsPositionWithinRange(origin, buildingTileCenterPosition, range))
					{
						if (building is JunimoHut junimoHut)
						{
							dx = (int)buildingTileCenterPosition.X - (int)origin.X;
							dy = (int)buildingTileCenterPosition.Y - (int)origin.Y;

							dChests.Add(new ChestWithDistance(junimoHut.output.Value, Math.Sqrt(dx * dx + dy * dy)));
						}
						else if (building is Mill mill)
						{
							dx = (int)buildingTileCenterPosition.X - (int)origin.X;
							dy = (int)buildingTileCenterPosition.Y - (int)origin.Y;

							dChests.Add(new ChestWithDistance(mill.input.Value, Math.Sqrt(dx * dx + dy * dy)));
						}
					}
				}
			}

			return dChests;
		}

		private static List<Chest> GetNearbyChests(Point originTile, int range, GameLocation gameLocation)
		{
			var chests = new List<Chest>((2 * range + 1) * (2 * range + 1));

			// Chests
			for (int dx = -range; dx <= range; dx++)
			{
				for (int dy = -range; dy <= range; dy++)
				{
					Vector2 tileLocation = new Vector2(originTile.X + dx, originTile.Y + dy);

					if (gameLocation.objects.TryGetValue(tileLocation, out StardewValley.Object obj) && obj is Chest chest)
					{
						chests.Add(chest);

						ModEntry.Context.Monitor.Log($"Chest found at [{tileLocation.X}, {tileLocation.Y}].", StardewModdingAPI.LogLevel.Debug);
					}
				}
			}

			// Kitchen fridge
			if (gameLocation is FarmHouse farmHouse && farmHouse.upgradeLevel >= 1) //Lvl 1,2,3 is where you have fridge upgrade
			{
				Point kitchenStandingSpot = farmHouse.getKitchenStandingSpot();
				Point fridgeTileLocation = new Point(kitchenStandingSpot.X + 2, kitchenStandingSpot.Y - 1); //Fridge spot relative to kitchen spot

				if (IsTileWithinRange(originTile, fridgeTileLocation, range))
				{
					if (farmHouse.fridge.Value != null)
					{
						chests.Add(farmHouse.fridge.Value);
					}
					else
					{
						ModEntry.Context.Monitor.Log("Could not find kitchen fridge!", StardewModdingAPI.LogLevel.Debug);
					}
				}
			}

			// Buildings
			if (gameLocation is BuildableGameLocation buildableGameLocation)
			{
				foreach (Building building in buildableGameLocation.buildings)
				{
					if (IsTileWithinRange(originTile, building.tileX.Value, building.tileY.Value, range))
					{
						if (building is JunimoHut junimoHut)
						{
							chests.Add(junimoHut.output.Value);
						}
						else if (building is Mill mill)
						{
							chests.Add(mill.input.Value);
						}
					}
				}
			}

			return chests;
		}

		private static Point GetTileLocation(Vector2 position)
		{
			return new Point(
				(int)position.X / Game1.tileSize,
				(int)position.Y / Game1.tileSize);
		}

		private static Vector2 GetTileCenterPosition(Point tileLocation)
		{
			return new Vector2(
				(tileLocation.X * Game1.tileSize) + (Game1.tileSize / 2),
				(tileLocation.Y * Game1.tileSize) + (Game1.tileSize / 2));
		}

		private static Vector2 GetTileCenterPosition(int tileX, int tileY)
		{
			return new Vector2(
				(tileX * Game1.tileSize) + (Game1.tileSize / 2),
				(tileY * Game1.tileSize) + (Game1.tileSize / 2));
		}

		private static bool IsTileWithinRange(Point origin, Point target, int range)
		{
			return Math.Abs(origin.X - target.X) <= range && Math.Abs(origin.Y - target.Y) <= range;
		}

		private static bool IsTileWithinRange(Point origin, int targetX, int targetY, int range)
		{
			return Math.Abs(origin.X - targetX) <= range && Math.Abs(origin.Y - targetY) <= range;
		}

		private static bool IsPositionWithinRange(Vector2 origin, Vector2 target, int range)
		{
			return Math.Abs(origin.X - target.X) <= (range * Game1.tileSize) && Math.Abs(origin.Y - target.Y) <= (range * Game1.tileSize);
		}
	}
}
