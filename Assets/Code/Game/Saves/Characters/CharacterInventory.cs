using LocalSaveSystem;

namespace Code.Game.Saves.Characters
{
[SaveModel]
[SaveVersion(1)]
public struct CharacterInventory
{
    public string[] InventoryItems { get; set; }
    public int InventoryCapacity { get; set; }
}
}