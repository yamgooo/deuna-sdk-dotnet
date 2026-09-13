namespace Deuna.Merchant.Sdk.Models.Enums;

/// <summary>
/// Represents the target environment for the DEUNA Merchant API.
/// </summary>
public enum DeunaEnvironment
{
    /// <summary>
    /// QA / Testing environment.
    /// Base URL: https://apim-qa-deuna.azure-api.net
    /// </summary>
    Qa,

    /// <summary>
    /// Production environment.
    /// Base URL: https://apis-merchant.pdn.deunalab.com
    /// </summary>
    Production
}
