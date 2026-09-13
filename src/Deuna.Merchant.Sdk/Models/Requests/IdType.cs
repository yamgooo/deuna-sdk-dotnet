namespace Deuna.Merchant.Sdk.Models.Requests;

/// <summary>
/// Defines the types of IDs that can be used to query a payment's status.
/// </summary>
public static class IdType
{
    /// <summary>
    /// Deuna-issued transaction ID.
    /// Expected length: 36 characters.
    /// </summary>
    public const string TransactionId = "0";

    /// <summary>
    /// Merchant-issued internal transaction reference.
    /// Expected length: 10 characters.
    /// </summary>
    public const string InternalTransactionReference = "1";

    /// <summary>
    /// Transfer number issued at payment execution.
    /// Expected length: &lt;= 20 characters.
    /// </summary>
    public const string TransferNumber = "2";
}
