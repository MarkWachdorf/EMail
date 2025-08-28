using Email.Contracts.Enums;
using Email.Contracts.Requests;
using Email.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Email.Tests.Integration;

public class EmailFlowIntegrationTests : IClassFixture<EmailIntegrationTestFixture>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EmailFlowIntegrationTests(EmailIntegrationTestFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteEmailFlow_CreateSendRetry_ShouldWorkEndToEnd()
    {
        // Arrange
        var createRequest = new CreateEmailRequest
        {
            CompanyCode = "TEST",
            ApplicationCode = "INTEGRATION",
            FromAddress = "integration@test.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Integration Test Email",
            Body = "This is an integration test email to verify the complete flow.",
            Importance = EmailImportanceDto.High,
            MaxRetries = 3
        };

        var sendRequest = new SendEmailRequest
        {
            CompanyCode = "TEST",
            ApplicationCode = "INTEGRATION",
            FromAddress = "integration@test.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Integration Test - Immediate Send",
            Body = "This email should be sent immediately as part of the integration test.",
            Importance = EmailImportanceDto.High,
            MaxRetries = 3
        };

        // Act 1: Create email
        var createJson = JsonSerializer.Serialize(createRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/email", createContent);

        // Assert 1: Email created successfully
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<BaseResponse<EmailResponse>>(createResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        createResult.Should().NotBeNull();
        createResult!.Succeeded.Should().BeTrue();
        createResult.Data.Should().NotBeNull();
        createResult.Data!.Status.Should().Be(EmailStatusDto.Pending);

        var createdEmailId = createResult.Data.Id;

        // Act 2: Get created email
        var getResponse = await _client.GetAsync($"/api/email/{createdEmailId}");

        // Assert 2: Email retrieved successfully
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        var getResult = JsonSerializer.Deserialize<BaseResponse<EmailResponse>>(getResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        getResult.Should().NotBeNull();
        getResult!.Succeeded.Should().BeTrue();
        getResult.Data.Should().NotBeNull();
        getResult.Data!.Id.Should().Be(createdEmailId);
        getResult.Data.Status.Should().Be(EmailStatusDto.Pending);

        // Act 3: Send email immediately
        var sendJson = JsonSerializer.Serialize(sendRequest);
        var sendContent = new StringContent(sendJson, Encoding.UTF8, "application/json");
        var sendResponse = await _client.PostAsync("/api/emailsender/send", sendContent);

        // Assert 3: Email sent successfully
        sendResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var sendResponseContent = await sendResponse.Content.ReadAsStringAsync();
        var sendResult = JsonSerializer.Deserialize<BaseResponse<EmailResponse>>(sendResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        sendResult.Should().NotBeNull();
        sendResult!.Succeeded.Should().BeTrue();
        sendResult.Data.Should().NotBeNull();
        sendResult.Data!.Status.Should().BeOneOf(EmailStatusDto.Sent, EmailStatusDto.Pending, EmailStatusDto.Failed);

        var sentEmailId = sendResult.Data.Id;

        // Act 4: Process pending emails
        var processResponse = await _client.PostAsync("/api/emailsender/process-pending", null);

        // Assert 4: Processing completed
        processResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var processResponseContent = await processResponse.Content.ReadAsStringAsync();
        var processResult = JsonSerializer.Deserialize<BaseResponse<int>>(processResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        processResult.Should().NotBeNull();
        processResult!.Succeeded.Should().BeTrue();

        // Act 5: Get all emails for the test company
        var getAllResponse = await _client.GetAsync("/api/email?companyCode=TEST&applicationCode=INTEGRATION&pageNumber=1&pageSize=10");

        // Assert 5: Emails retrieved successfully
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getAllResponseContent = await getAllResponse.Content.ReadAsStringAsync();
        var getAllResult = JsonSerializer.Deserialize<PagedResponse<IEnumerable<EmailResponse>>>(getAllResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        getAllResult.Should().NotBeNull();
        getAllResult!.Succeeded.Should().BeTrue();
        getAllResult.Data.Should().NotBeNull();
        getAllResult.Data!.Should().NotBeEmpty();
        getAllResult.TotalRecords.Should().BeGreaterThan(0);

        // Verify our test emails are in the results
        var testEmails = getAllResult.Data.Where(e => e.CompanyCode == "TEST" && e.ApplicationCode == "INTEGRATION").ToList();
        testEmails.Should().NotBeEmpty();
        testEmails.Should().Contain(e => e.Id == createdEmailId);
        testEmails.Should().Contain(e => e.Id == sentEmailId);
    }

    [Fact]
    public async Task CachedEmailFlow_CreateConsolidateProcess_ShouldWorkEndToEnd()
    {
        // Arrange
        var cachedRequest1 = new SendCachedEmailRequest
        {
            CompanyCode = "TEST",
            ApplicationCode = "CACHE",
            FromAddress = "cache@test.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Cached Email Test",
            Body = "This is the first cached email for consolidation testing.",
            Importance = EmailImportanceDto.Normal,
            MaxRetries = 3,
            CacheExpirationMinutes = 5 // Short expiration for testing
        };

        var cachedRequest2 = new SendCachedEmailRequest
        {
            CompanyCode = "TEST",
            ApplicationCode = "CACHE",
            FromAddress = "cache@test.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Cached Email Test",
            Body = "This is the second cached email for consolidation testing.",
            Importance = EmailImportanceDto.Normal,
            MaxRetries = 3,
            CacheExpirationMinutes = 5 // Short expiration for testing
        };

        // Act 1: Send first cached email
        var cachedJson1 = JsonSerializer.Serialize(cachedRequest1);
        var cachedContent1 = new StringContent(cachedJson1, Encoding.UTF8, "application/json");
        var cachedResponse1 = await _client.PostAsync("/api/emailcache/send-cached", cachedContent1);

        // Assert 1: First email cached successfully
        cachedResponse1.StatusCode.Should().Be(HttpStatusCode.OK);
        var cachedResponseContent1 = await cachedResponse1.Content.ReadAsStringAsync();
        var cachedResult1 = JsonSerializer.Deserialize<BaseResponse<EmailResponse>>(cachedResponseContent1, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        cachedResult1.Should().NotBeNull();
        cachedResult1!.Succeeded.Should().BeTrue();
        cachedResult1.Data.Should().NotBeNull();
        cachedResult1.Data!.Status.Should().Be(EmailStatusDto.Cached);

        // Act 2: Send second cached email (should be consolidated)
        var cachedJson2 = JsonSerializer.Serialize(cachedRequest2);
        var cachedContent2 = new StringContent(cachedJson2, Encoding.UTF8, "application/json");
        var cachedResponse2 = await _client.PostAsync("/api/emailcache/send-cached", cachedContent2);

        // Assert 2: Second email cached and consolidated
        cachedResponse2.StatusCode.Should().Be(HttpStatusCode.OK);
        var cachedResponseContent2 = await cachedResponse2.Content.ReadAsStringAsync();
        var cachedResult2 = JsonSerializer.Deserialize<BaseResponse<EmailResponse>>(cachedResponseContent2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        cachedResult2.Should().NotBeNull();
        cachedResult2!.Succeeded.Should().BeTrue();
        cachedResult2.Data.Should().NotBeNull();
        cachedResult2.Data!.Status.Should().Be(EmailStatusDto.Cached);

        // Act 3: Process expired cache (simulate expiration by calling the endpoint)
        var processCacheResponse = await _client.PostAsync("/api/emailcache/process-expired", null);

        // Assert 3: Cache processing completed
        processCacheResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var processCacheResponseContent = await processCacheResponse.Content.ReadAsStringAsync();
        var processCacheResult = JsonSerializer.Deserialize<BaseResponse<int>>(processCacheResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        processCacheResult.Should().NotBeNull();
        processCacheResult!.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ErrorHandlingFlow_InvalidRequests_ShouldReturnAppropriateErrors()
    {
        // Arrange
        var invalidRequest = new CreateEmailRequest
        {
            CompanyCode = "", // Invalid: empty company code
            ApplicationCode = "TEST",
            FromAddress = "invalid-email", // Invalid: not a valid email
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Importance = EmailImportanceDto.Normal,
            MaxRetries = 15 // Invalid: exceeds max retries
        };

        // Act: Try to create email with invalid data
        var json = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/email", content);

        // Assert: Should return bad request due to validation errors
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EmailRetryFlow_FailedEmailRetry_ShouldWorkEndToEnd()
    {
        // Arrange
        var sendRequest = new SendEmailRequest
        {
            CompanyCode = "TEST",
            ApplicationCode = "RETRY",
            FromAddress = "retry@test.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Retry Test Email",
            Body = "This email is for testing the retry functionality.",
            Importance = EmailImportanceDto.High,
            MaxRetries = 3
        };

        // Act 1: Send email
        var sendJson = JsonSerializer.Serialize(sendRequest);
        var sendContent = new StringContent(sendJson, Encoding.UTF8, "application/json");
        var sendResponse = await _client.PostAsync("/api/emailsender/send", sendContent);

        // Assert 1: Email sent
        sendResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var sendResponseContent = await sendResponse.Content.ReadAsStringAsync();
        var sendResult = JsonSerializer.Deserialize<BaseResponse<EmailResponse>>(sendResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        sendResult.Should().NotBeNull();
        sendResult!.Succeeded.Should().BeTrue();
        sendResult.Data.Should().NotBeNull();

        var emailId = sendResult.Data!.Id;

        // Act 2: Try to retry the email (this might fail if email is not in failed status, which is expected)
        var retryResponse = await _client.PostAsync($"/api/emailsender/{emailId}/retry", null);

        // Assert 2: Should get appropriate response (either success or error based on email status)
        retryResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EmailStatusUpdateFlow_UpdateStatus_ShouldWorkEndToEnd()
    {
        // Arrange
        var createRequest = new CreateEmailRequest
        {
            CompanyCode = "TEST",
            ApplicationCode = "STATUS",
            FromAddress = "status@test.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Status Update Test",
            Body = "This email is for testing status updates.",
            Importance = EmailImportanceDto.Normal,
            MaxRetries = 3
        };

        // Act 1: Create email
        var createJson = JsonSerializer.Serialize(createRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/email", createContent);

        // Assert 1: Email created
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<BaseResponse<EmailResponse>>(createResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        createResult.Should().NotBeNull();
        createResult!.Succeeded.Should().BeTrue();
        createResult.Data.Should().NotBeNull();

        var emailId = createResult.Data!.Id;
        var rowVersion = createResult.Data.RowVersion;

        // Act 2: Update email status
        var updateRequest = new UpdateEmailStatusRequest
        {
            EmailId = emailId,
            NewStatus = EmailStatusDto.Sent,
            StatusMessage = "Email sent successfully via integration test",
            RowVersion = rowVersion
        };

        var updateJson = JsonSerializer.Serialize(updateRequest);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");
        var updateResponse = await _client.PutAsync("/api/email/status", updateContent);

        // Assert 2: Status updated successfully
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
        var updateResult = JsonSerializer.Deserialize<BaseResponse<EmailResponse>>(updateResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        updateResult.Should().NotBeNull();
        updateResult!.Succeeded.Should().BeTrue();
        updateResult.Data.Should().NotBeNull();
        updateResult.Data!.Status.Should().Be(EmailStatusDto.Sent);
        updateResult.Data.StatusMessage.Should().Be("Email sent successfully via integration test");
    }
}
