using System.Net;
using Deuna.Merchant.Sdk.Clients;
using Deuna.Merchant.Sdk.Exceptions;
using Deuna.Merchant.Sdk.Models.Enums;
using Deuna.Merchant.Sdk.Models.Requests;
using Deuna.Merchant.Sdk.UnitTests.Helpers;
using FluentAssertions;
using Xunit;

namespace Deuna.Merchant.Sdk.UnitTests;

public class ValidationTests
{
    [Theory]
    [InlineData(19, false)]
    [InlineData(20, true)]
    [InlineData(0, true)] // Empty string
    [InlineData(-1, true)] // Null (handled specially below)
    public async Task RequestAsync_InternalTransactionReference_LengthValidation(int length, bool shouldThrow)
    {
        var (client, _) = PaymentClientFactory.Create(HttpStatusCode.OK, "{}");
        var request = new PaymentRequest
        {
            PointOfSale = "462",
            Amount = 10m,
            InternalTransactionReference = length == -1 ? null! : new string('x', length),
            Format = QrResponseFormat.QrAndDeeplink
        };

        var act = async () => await client.RequestAsync(request);

        if (shouldThrow)
        {
            if (length == -1 || length == 0)
            {
                await act.Should().ThrowAsync<DeunaValidationException>();
            }
            else
            {
                await act.Should().ThrowAsync<DeunaValidationException>().WithMessage("*Must be 19 characters or less*");
            }
        }
        else
        {
            await act.Should().NotThrowAsync<DeunaValidationException>();
        }
    }

    [Theory]
    [InlineData(IdType.TransactionId, 36, false)]
    [InlineData(IdType.TransactionId, 35, true)]
    [InlineData(IdType.TransactionId, 37, true)]
    [InlineData(IdType.InternalTransactionReference, 10, false)]
    [InlineData(IdType.InternalTransactionReference, 9, true)]
    [InlineData(IdType.InternalTransactionReference, 11, true)]
    [InlineData(IdType.TransferNumber, 20, false)]
    [InlineData(IdType.TransferNumber, 10, false)]
    [InlineData(IdType.TransferNumber, 21, true)]
    [InlineData(IdType.TransactionId, 0, true)] // Empty string
    [InlineData(IdType.TransactionId, -1, true)] // Null
    public async Task GetInfoAsync_IdTransactionReference_LengthValidation(string idType, int length, bool shouldThrow)
    {
        var (client, _) = PaymentClientFactory.Create(HttpStatusCode.OK, "{}");
        var request = new PaymentInfoRequest
        {
            IdType = idType,
            IdTransactionReference = length == -1 ? null! : new string('x', length)
        };

        var act = async () => await client.GetInfoAsync(request);

        if (shouldThrow)
        {
            await act.Should().ThrowAsync<DeunaValidationException>();
        }
        else
        {
            await act.Should().NotThrowAsync<DeunaValidationException>();
        }
    }
}
