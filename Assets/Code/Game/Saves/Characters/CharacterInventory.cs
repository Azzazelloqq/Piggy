using System;

namespace Code.Game.Saves.Characters
{
public readonly struct CharacterInventory
{
    public string[] InventoryItems { get; }
    public int InventoryCapacity { get; }

    public CharacterInventory(string[] inventoryItems, int inventoryCapacity)
    {
        InventoryItems = inventoryItems ?? Array.Empty<string>();
        InventoryCapacity = inventoryCapacity;
    }
}
}