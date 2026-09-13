using System.Net;
using Deuna.Merchant.Sdk.Abstractions;
using Deuna.Merchant.Sdk.DependencyInjection;
using Deuna.Merchant.Sdk.Exceptions;
using Deuna.Merchant.Sdk.Models.Enums;
using Deuna.Merchant.Sdk.Models.Requests;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Deuna.Merchant.Sdk.IntegrationTests;

/// <summary>
/// Integration tests that exercise the full DI pipeline (registration → resilience handler →
/// auth handler → deserialization) against a WireMock.Net stub server.
/// </summary>
public sealed class PaymentClientIntegrationTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly IDeunaMerchantClient _client;

    // Dummy credentials — never real values.
    private const string FakeApiKey = "test-api-key-abc";
    private const string FakeApiSecret = "test-api-secret-xyz";

    public PaymentClientIntegrationTests()
    {
        _server = WireMockServer.Start();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDeunaMerchantClient(opts =>
        {
            opts.BaseUrl = _server.Urls[0];
            opts.ApiKey = FakeApiKey;
            opts.ApiSecret = FakeApiSecret;
            opts.MaxRetryAttempts = 0; // disable retries in tests for speed
        });

        var provider = services.BuildServiceProvider();
        _client = provider.GetRequiredService<IDeunaMerchantClient>();
    }

    public void Dispose() => _server.Stop();

    // =========================================================================
    // RequestAsync integration test
    // =========================================================================

    [Fact]
    public async Task RequestAsync_FullPipeline_InjectsAuthHeadersAndDeserializesResponse()
    {
        // Arrange — stub the WireMock endpoint
        const string responseBody = """
            {
              "status": "1",
              "transactionId": "e91c7a59-b67d-40fe-b84b-2a29b055b543",
              "deeplink": "https://pagar.deuna.app/H92p/merchant?id=TEST001"
            }
            """;

        _server
            .Given(Request.Create()
                .WithPath("/merchant/v1/payment/request")
                .WithHeader("x-api-key", FakeApiKey)
                .WithHeader("x-api-secret", FakeApiSecret)
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(responseBody));

        // Act
        var response = await _client.Payments.RequestAsync(new PaymentRequest
        {
            PointOfSale = "462",
            Amount = 30m,
            InternalTransactionReference = "integration-test-001",
            Format = QrResponseFormat.DeeplinkOnly,
        });

        // Assert
        response.TransactionId.Should().Be("e91c7a59-b67d-40fe-b84b-2a29b055b543");
        response.Deeplink.Should().Contain("TEST001");
    }

    // =========================================================================
    // GetInfoAsync integration test
    // =========================================================================

    [Fact]
    public async Task GetInfoAsync_FullPipeline_ReturnsApprovedTransaction()
    {
        const string responseBody = """
            {
              "status": "APPROVED",
              "internalTransactionReference": "integration-test-001",
              "amount": 30,
              "transactionId": "21f10448-0c6c-4b6b-9664-f23b897b58d1",
              "transferNumber": "972454362424",
              "date": "7/5/2024, 1:47:29 PM",
              "branchId": "461",
              "posId": "462",
              "currency": "USD",
              "description": "test",
              "ordererName": "JOHN DOE",
              "ordererIdentification": "1234567890"
            }
            """;

        _server
            .Given(Request.Create()
                .WithPath("/merchant/v1/payment/info")
                .WithHeader("x-api-key", FakeApiKey)
                .WithHeader("x-api-secret", FakeApiSecret)
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(responseBody));

        var response = await _client.Payments.GetInfoAsync(new PaymentInfoRequest
        {
            IdTransactionReference = "21f10448-0c6c-4b6b-9664-f23b897b58d1",
            IdType = "0",
        });

        response.Status.Should().Be("APPROVED");
        response.TransferNumber.Should().Be("972454362424");
        response.TryGetParsedDate().Should().NotBeNull();
    }

    // =========================================================================
    // CancelAsync integration test
    // =========================================================================

    [Fact]
    public async Task CancelAsync_FullPipeline_ReturnsCancellationMessage()
    {
        const string responseBody = """
            {
              "transactionId": "ebc7f0eb-7dd4-4997-946c-edd613327add",
              "message": "The QR with id 250 has been successfully cleaned"
            }
            """;

        _server
            .Given(Request.Create()
                .WithPath("/merchant/v1/payment/cancel")
                .WithHeader("x-api-key", FakeApiKey)
                .WithHeader("x-api-secret", FakeApiSecret)
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(responseBody));

        var response = await _client.Payments.CancelAsync(new CancelTransactionRequest
        {
            TransactionId = "ebc7f0eb-7dd4-4997-946c-edd613327add",
            PointOfSale = "250",
        });

        response.Message.Should().Contain("successfully");
    }

    // =========================================================================
    // RefundAsync — success + already-processed error
    // =========================================================================

    [Fact]
    public async Task RefundAsync_FullPipeline_ReturnsSuccessResponse()
    {
        const string responseBody = """
            {
              "status": true,
              "message": "Refund executed successfully for transferNumber 018928363049",
              "transactionReverseId": "cbecab24-0f18-45b2-986f-e6c9bcd68728"
            }
            """;

        _server
            .Given(Request.Create()
                .WithPath("/merchant/v1/payment/void")
                .WithHeader("x-api-key", FakeApiKey)
                .WithHeader("x-api-secret", FakeApiSecret)
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(responseBody));

        var response = await _client.Payments.RefundAsync(new RefundRequest
        {
            TransferNumber = "018928363049",
        });

        response.Status.Should().BeTrue();
        response.TransactionReverseId.Should().Be("cbecab24-0f18-45b2-986f-e6c9bcd68728");
    }

    [Fact]
    public async Task RefundAsync_AlreadyProcessed_ThrowsDeunaApiExceptionWith400()
    {
        const string errorBody = """
            {
              "statusCode": 400,
              "message": "Refund was already processed for this transferNumber 018928363049. Please verify the transferNumber and try again"
            }
            """;

        _server
            .Given(Request.Create()
                .WithPath("/merchant/v1/payment/void")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithHeader("Content-Type", "application/json")
                .WithBody(errorBody));

        var act = async () => await _client.Payments.RefundAsync(new RefundRequest
        {
            TransferNumber = "018928363049",
        });

        var ex = await act.Should().ThrowAsync<DeunaApiException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ex.Which.Message.Should().Contain("already processed");
    }

    // =========================================================================
    // Options validation at startup
    // =========================================================================

    [Fact]
    public void AddDeunaMerchantClient_MissingApiKey_OptionsValidatorRejectsEmpty()
    {
        // Validate that the options validator rejects an empty ApiKey.
        // Note: ValidateOnStart() fires during IHost startup, not BuildServiceProvider().
        // Here we directly exercise the validator to confirm the contract.
        var validator = new Deuna.Merchant.Sdk.Configuration.DeunaClientOptionsValidator();
        var opts = new Deuna.Merchant.Sdk.Configuration.DeunaClientOptions
        {
            BaseUrl = "https://valid.deuna.local",
            ApiKey = "",         // intentionally empty
            ApiSecret = "secret",
        };

        var result = validator.Validate(null, opts);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("ApiKey"));
    }
}
