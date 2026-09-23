namespace InventoryPack.Features.Assets.Import;

public class ImportAlreadyExistsException(string message) : Exception(message);
