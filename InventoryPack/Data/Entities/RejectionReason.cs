namespace InventoryPack.Data.Entities;

public enum RejectionReason
{
    NoSingleCode,
    MissingContext,
    DuplicateCode,
    CodeQuantityMismatch,
    InvalidQuantity
}