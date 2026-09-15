using System.Net;
using System.Text.Json;
using Deuna.Merchant.Sdk.Exceptions;
using Deuna.Merchant.Sdk.Models.Enums;
using Deuna.Merchant.Sdk.Models.Requests;
using Deuna.Merchant.Sdk.UnitTests.Helpers;
using FluentAssertions;

namespace Deuna.Merchant.Sdk.UnitTests;

/// <summary>
/// Unit tests for <see cref="Clients.PaymentClient"/>.
/// All tests use a mocked <see cref="System.Net.Http.HttpMessageHandler"/>; no network is needed.
/// </summary>
public sealed class PaymentClientTests
{
    // ─── Fixture paths ────────────────────────────────────────────────────────
    private static string FixturePath(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    private static string ReadFixture(string name) =>
        File.ReadAllText(FixturePath(name));

    // =========================================================================
    // RequestAsync — happy path
    // =========================================================================

    [Fact]
    public async Task RequestAsync_FormatDeeplinkOnly_ReturnsDeeplinkAndTransactionId()
    {
        // Arrange
        var body = ReadFixture("payment_response_format0.json");
        var (client, handler) = PaymentClientFactory.Create(HttpStatusCode.OK, body);

        var request = new PaymentRequest
        {
            PointOfSale = "462",
            QrType = QrType.Dynamic,
            Amount = 30m,
            Detail = "test postman",
            InternalTransactionReference = "ref-001",
            Format = QrResponseFormat.DeeplinkOnly,
        };

        // Act
        var response = await client.RequestAsync(request);

        // Assert — response fields
        response.Status.Should().Be("1");
        response.TransactionId.Should().Be("e91c7a59-b67d-40fe-b84b-2a29b055b543");
        response.Deeplink.Should().Be("https://pagar.deuna.app/H92p/merchant?id=C9T69PE1B6YN");
        response.Qr.Should().BeNull();

        // Assert — outgoing request shape
        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Be("/merchant/v1/payment/request");

        // Assert — auth headers are present (injected by DeunaAuthHandler in production;
        // not present in unit test because we bypass the pipeline — test their absence shows
        // the handler is the only injection point)
        handler.LastRequestBody.Should().NotBeNullOrWhiteSpace();
        var sentBody = JsonDocument.Parse(handler.LastRequestBody!).RootElement;
        sentBody.GetProperty("pointOfSale").GetString().Should().Be("462");
        sentBody.GetProperty("amount").GetDecimal().Should().Be(30m);
        sentBody.GetProperty("format").GetString().Should().Be("0");
    }

    [Fact]
    public async Task RequestAsync_FormatQrOnly_ReturnsQrAndNoDeeplink()
    {
        // Arrange — use inline response matching Postman "format 1" sample
        const string qrData = "data:image/png;base64,abc123==";
        var body = $$"""{"status":"1","transactionId":"a24dd457-cabd-4c47-b176-6cf453cdbfa3","qr":"{{qrData}}"}""";
        var (client, _) = PaymentClientFactory.Create(HttpStatusCode.OK, body);

        var request = new PaymentRequest
        {
            PointOfSale = "462",
            Amount = 30m,
            InternalTransactionReference = "ref-002",
            Format = QrResponseFormat.QrOnly,
        };

        // Act
        var response = await client.RequestAsync(request);

        // Assert
        response.Qr.Should().Be(qrData);
        response.Deeplink.Should().BeNull();
    }

    [Fact]
    public async Task RequestAsync_FormatQrAndDeeplink_ReturnsBoth()
    {
        const string qrData = "data:image/png;base64,xyz==";
        var body = $$"""{"status":"1","transactionId":"21f10448-0c6c-4b6b-9664-f23b897b58d1","qr":"{{qrData}}","deeplink":"https://pagar.deuna.app/H92p/merchant?id=FMLZYLZDA749"}""";
        var (client, _) = PaymentClientFactory.Create(HttpStatusCode.OK, body);

        var request = new PaymentRequest
        {
            PointOfSale = "462",
            Amount = 30m,
            InternalTransactionReference = "ref-003",
            Format = QrResponseFormat.QrAndDeeplink,
        };

        var response = await client.RequestAsync(request);

        response.Qr.Should().NotBeNullOrWhiteSpace();
        response.Deeplink.Should().NotBeNullOrWhiteSpace();
    }

    // =========================================================================
    // GetInfoAsync — happy path
    // =========================================================================

    [Fact]
    public async Task GetInfoAsync_PendingTransaction_ReturnsPendingStatus()
    {
        var body = ReadFixture("payment_info_response_pending.json");
        var (client, handler) = PaymentClientFactory.Create(HttpStatusCode.OK, body);

        var request = new PaymentInfoRequest
        {
            IdTransactionReference = "a24dd457-cabd-4c47-b176-6cf453cdbfa3",
            IdType = "0",
        };

        var response = await client.GetInfoAsync(request);

        response.Status.Should().Be("PENDING");
        response.Amount.Should().Be(0);
        response.TransferNumber.Should().BeEmpty();
        response.Currency.Should().Be("USD");
        response.TryGetParsedDate().Should().BeNull();

        // Wire field name: idTransacionReference (API typo preserved)
        var sentBody = JsonDocument.Parse(handler.LastRequestBody!).RootElement;
        sentBody.GetProperty("idTransacionReference").GetString()
            .Should().Be("a24dd457-cabd-4c47-b176-6cf453cdbfa3");
        sentBody.GetProperty("idType").GetString().Should().Be("0");
    }

    [Fact]
    public async Task GetInfoAsync_ApprovedTransaction_ReturnsFullDetails()
    {
        var body = ReadFixture("payment_info_response_approved.json");
        var (client, _) = PaymentClientFactory.Create(HttpStatusCode.OK, body);

        var request = new PaymentInfoRequest
        {
            IdTransactionReference = "21f10448-0c6c-4b6b-9664-f23b897b58d1",
            IdType = "0",
        };

        var response = await client.GetInfoAsync(request);

        response.Status.Should().Be("APPROVED");
        response.Amount.Should().Be(30);
        response.TransferNumber.Should().Be("972454362424");
        response.OrdererName.Should().Be("LEANDRO GABRIEL ALVAREZ SAMANIEGO");
        response.OrdererIdentification.Should().Be("0603358920");
        response.BranchId.Should().Be("461");
        response.PosId.Should().Be("462");
        response.Currency.Should().Be("USD");

        // Convenience helper should parse the date from "7/5/2024, 1:47:29 PM"
        var parsed = response.TryGetParsedDate();
        parsed.Should().NotBeNull();
        parsed!.Value.Year.Should().Be(2024);
        parsed.Value.Month.Should().Be(7);
        parsed.Value.Day.Should().Be(5);
    }

    // =========================================================================
    // CancelAsync — happy path
    // =========================================================================

    [Fact]
    public async Task CancelAsync_ValidRequest_ReturnsCancellationConfirmation()
    {
        var body = ReadFixture("cancel_transaction_response.json");
        var (client, handler) = PaymentClientFactory.Create(HttpStatusCode.OK, body);

        var request = new CancelTransactionRequest
        {
            TransactionId = "ebc7f0eb-7dd4-4997-946c-edd613327add",
            PointOfSale = "250",
        };

        var response = await client.CancelAsync(request);

        response.TransactionId.Should().Be("ebc7f0eb-7dd4-4997-946c-edd613327add");
        response.Message.Should().Contain("successfully");

        var sentBody = JsonDocument.Parse(handler.LastRequestBody!).RootElement;
        sentBody.GetProperty("transactionId").GetString().Should().Be("ebc7f0eb-7dd4-4997-946c-edd613327add");
        sentBody.GetProperty("pointOfSale").GetString().Should().Be("250");
    }

    // =========================================================================
    // RefundAsync — happy path + 400 error shape
    // =========================================================================

    [Fact]
    public async Task RefundAsync_ValidRequest_ReturnsSuccessResponse()
    {
        var body = ReadFixture("refund_response_ok.json");
        var (client, handler) = PaymentClientFactory.Create(HttpStatusCode.OK, body);

        var request = new RefundRequest { TransferNumber = "018928363049" };

        var response = await client.RefundAsync(request);

        response.Status.Should().BeTrue();
        response.Message.Should().Contain("018928363049");
        response.TransactionReverseId.Should().Be("cbecab24-0f18-45b2-986f-e6c9bcd68728");

        var sentBody = JsonDocument.Parse(handler.LastRequestBody!).RootElement;
        sentBody.GetProperty("transferNumber").GetString().Should().Be("018928363049");
    }

    [Fact]
    public async Task RefundAsync_AlreadyProcessed_ThrowsDeunaApiExceptionWith400()
    {
        var body = ReadFixture("refund_response_400.json");
        var (client, _) = PaymentClientFactory.Create(HttpStatusCode.BadRequest, body);

        var request = new RefundRequest { TransferNumber = "018928363049" };

        var act = async () => await client.RefundAsync(request);

        var ex = await act.Should().ThrowAsync<DeunaApiException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ex.Which.Message.Should().Contain("already processed");
        ex.Which.RawResponse.Should().Contain("018928363049");
    }

    // =========================================================================
    // Guard-clause validation failures
    // =========================================================================

    [Fact]
    public async Task RequestAsync_NullRequest_ThrowsDeunaValidationException()
    {
        var (client, _) = PaymentClientFactory.Create(HttpStatusCode.OK, "{}");
        var act = async () => await client.RequestAsync(null!);
        await act.Should().ThrowAsync<DeunaValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RequestAsync_EmptyPointOfSale_ThrowsDeunaValidationException(string pos)
    {
        var (client, _) = PaymentClientFactory.Create(HttpStatusCode.OK, "{}");
        var request = new PaymentRequest
        {
            PointOfSale = pos,
            Amount = 10m,
            InternalTransactionReference = "ref",
            Format = QrResponseFormat.QrOnly,
        };
        var act = async () => await client.RequestAsync(request);
        await act.Should().ThrowAsync<DeunaValidationException>();
    }

    [Fact]
    public async Task RequestAsync_ZeroAmount_ThrowsDeunaValidationException()
    {
        var (client, _) = PaymentClientFactory.Create(HttpStatusCode.OK, "{}");
        var request = new PaymentRequest
        {
            PointOfSale = "462",
            Amount = 0m,
            InternalTransactionReference = "ref",
            Format = QrResponseFormat.QrOnly,
        };
        var act = async () => await client.RequestAsync(request);
        await act.Should().ThrowAsync<DeunaValidationException>();
    }

    [Fact]
    public async Task GetInfoAsync_NullRequest_ThrowsDeunaValidationException()
    {
        var (client, _) = PaymentClientFactory.Create(HttpStatusCode.OK, "{}");
        var act = async () => await client.GetInfoAsync(null!);
        await act.Should().ThrowAsync<DeunaValidationException>();
    }

    [Fact]
    public async Task CancelAsync_EmptyTransactionId_ThrowsDeunaValidationException()
    {
        var (client, _) = PaymentClientFactory.Create(HttpStatusCode.OK, "{}");
        var request = new CancelTransactionRequest { TransactionId = "", PointOfSale = "462" };
        var act = async () => await client.CancelAsync(request);
        await act.Should().ThrowAsync<DeunaValidationException>();
    }

    [Fact]
    public async Task RefundAsync_EmptyTransferNumber_ThrowsDeunaValidationException()
    {
        var (client, _) = PaymentClientFactory.Create(HttpStatusCode.OK, "{}");
        var request = new RefundRequest { TransferNumber = "" };
        var act = async () => await client.RefundAsync(request);
        await act.Should().ThrowAsync<DeunaValidationException>();
    }

    // =========================================================================
    // DeunaApiException properties
    // =========================================================================

    [Fact]
    public void DeunaApiException_ToString_DoesNotContainSecrets()
    {
        var ex = new DeunaApiException(HttpStatusCode.Unauthorized, "Unauthorized");
        ex.ToString().Should().NotContain("secret");
        ex.ToString().Should().Contain("401");
    }

    // =========================================================================
    // Content-Type handling & QrResponseFormat
    // =========================================================================

    [Fact]
    public async Task RequestAsync_HtmlResponse_ThrowsDeunaExceptionWithClearMessage()
    {
        const string htmlBody = "<!DOCTYPE html><html><head><title>BackOffice DeUna</title></head><body>Portal</body></html>";
        var (client, _) = PaymentClientFactory.Create(HttpStatusCode.OK, htmlBody, contentType: "text/html");

        var request = new PaymentRequest
        {
            PointOfSale = "161",
            Amount = 10m,
            InternalTransactionReference = "ref-html",
            Format = QrResponseFormat.DeeplinkOnly,
        };

        var act = async () => await client.RequestAsync(request);

        var ex = await act.Should().ThrowAsync<DeunaException>();
        ex.Which.Message.Should().Contain("unexpected Content-Type 'text/html'");
        ex.Which.Message.Should().Contain("User-Agent");
    }

    [Fact]
    public async Task RequestAsync_FormatAll_SerializesAsString5()
    {
        const string body = """{"status":"1","transactionId":"e91c7a59-b67d-40fe-b84b-2a29b055b543","deeplink":"https://pagar.deuna.app/H92p/merchant?id=TEST001"}""";
        var (client, handler) = PaymentClientFactory.Create(HttpStatusCode.OK, body);

        var request = new PaymentRequest
        {
            PointOfSale = "161",
            Amount = 10m,
            InternalTransactionReference = "ref-fmt-5",
            Format = QrResponseFormat.All,
        };

        await client.RequestAsync(request);

        var sentBody = JsonDocument.Parse(handler.LastRequestBody!).RootElement;
        sentBody.GetProperty("format").GetString().Should().Be("5");
    }
}

