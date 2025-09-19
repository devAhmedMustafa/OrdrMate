using OrdrMate.Models;

namespace OrdrMate.Events;

public class ItemEvents
{
    public static event Action<Item> OnItemAdded = (item) => { };
    public static event Action<string> OnItemUpdated = (itemId) => { };
    public static event Action<string> OnItemDeleted = (itemId) => { };

    public static void ItemAdded(Item item) => OnItemAdded(item);
    public static void ItemUpdated(string itemId) => OnItemUpdated(itemId);
    public static void ItemDeleted(string itemId) => OnItemDeleted(itemId);
}