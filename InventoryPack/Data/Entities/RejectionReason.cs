namespace InventoryPack.Data.Entities;

public enum RejectionReason
{
    NoValidCodes,
    MissingContext,
    RepeatedCodeWithinRow,
    CodeQuantityMismatch,
    InvalidQuantity,
    ConflictingNamesForCode
}
