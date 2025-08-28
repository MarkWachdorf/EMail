using Email.Contracts.Enums;
using Email.Contracts.Requests;
using Email.Contracts.Responses;
using Email.Infrastructure.Entities;
using Email.Infrastructure.Entities.Enums;

namespace Email.Application.Mappers;

/// <summary>
/// Provides mapping functionalities between Email entities and DTOs.
/// </summary>
public static class EmailMapper
{
    /// <summary>
    /// Maps a CreateEmailRequest to an EmailMessage entity.
    /// </summary>
    public static EmailMessage ToEntity(this CreateEmailRequest request)
    {
        return new EmailMessage
        {
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode,
            FromAddress = request.FromAddress,
            ToAddresses = request.ToAddresses,
            CcAddresses = request.CcAddresses,
            BccAddresses = request.BccAddresses,
            Subject = request.Subject,
            Body = request.Body,
            Header = request.Header,
            Footer = request.Footer,
            ImportanceFlag = (EmailImportanceFlag)request.ImportanceFlag,
            MaxRetries = request.MaxRetries,
            TemplateCode = request.TemplateCode,
            TemplateParameters = request.TemplateParameters,
            Status = EmailStatus.Pending, // Always pending on creation
            RetryCount = 0,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps a SendEmailRequest to an EmailMessage entity.
    /// </summary>
    public static EmailMessage ToEntity(this SendEmailRequest request)
    {
        return new EmailMessage
        {
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode,
            FromAddress = request.FromAddress,
            ToAddresses = request.ToAddresses,
            CcAddresses = request.CcAddresses,
            BccAddresses = request.BccAddresses,
            Subject = request.Subject,
            Body = request.Body,
            Header = request.Header,
            Footer = request.Footer,
            ImportanceFlag = (EmailImportanceFlag)request.ImportanceFlag,
            MaxRetries = request.MaxRetries,
            TemplateCode = request.TemplateCode,
            TemplateParameters = request.TemplateParameters,
            Status = EmailStatus.Pending, // Always pending on creation
            RetryCount = 0,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps a SendCachedEmailRequest to an EmailMessage entity.
    /// </summary>
    public static EmailMessage ToEmailMessageEntity(this SendCachedEmailRequest request)
    {
        return new EmailMessage
        {
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode,
            FromAddress = request.FromAddress,
            ToAddresses = request.ToAddresses,
            CcAddresses = request.CcAddresses,
            BccAddresses = request.BccAddresses,
            Subject = request.Subject,
            Body = request.Body,
            Header = request.Header,
            Footer = request.Footer,
            ImportanceFlag = (EmailImportanceFlag)request.ImportanceFlag,
            MaxRetries = request.MaxRetries,
            TemplateCode = request.TemplateCode,
            TemplateParameters = request.TemplateParameters,
            Status = EmailStatus.Cached, // Always cached on creation
            RetryCount = 0,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps a SendCachedEmailRequest to an EmailCache entity.
    /// </summary>
    public static EmailCache ToEmailCacheEntity(this SendCachedEmailRequest request, string cacheKey)
    {
        return new EmailCache
        {
            CacheKey = cacheKey,
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode,
            FromAddress = request.FromAddress,
            ToAddresses = request.ToAddresses,
            CcAddresses = request.CcAddresses,
            BccAddresses = request.BccAddresses,
            Subject = request.Subject,
            Body = request.Body,
            Header = request.Header,
            Footer = request.Footer,
            ImportanceFlag = (EmailImportanceFlag)request.ImportanceFlag,
            MessageCount = 1, // Initial count
            ExpiresAt = DateTime.UtcNow.AddMinutes(request.CacheExpirationMinutes),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps an EmailMessage entity to an EmailResponse.
    /// </summary>
    public static EmailResponse ToResponse(this EmailMessage entity)
    {
        return new EmailResponse
        {
            Id = entity.Id,
            CompanyCode = entity.CompanyCode,
            ApplicationCode = entity.ApplicationCode,
            FromAddress = entity.FromAddress,
            ToAddresses = entity.ToAddresses,
            CcAddresses = entity.CcAddresses,
            BccAddresses = entity.BccAddresses,
            Subject = entity.Subject,
            Body = entity.Body,
            Header = entity.Header,
            Footer = entity.Footer,
            Status = (EmailStatusDto)entity.Status,
            StatusMessage = entity.StatusMessage,
            ImportanceFlag = (EmailImportanceDto)entity.ImportanceFlag,
            RetryCount = entity.RetryCount,
            MaxRetries = entity.MaxRetries,
            LastAttemptedAt = entity.LastAttemptedAt,
            TemplateCode = entity.TemplateCode,
            TemplateParameters = entity.TemplateParameters,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted,
            RowVersion = entity.RowVersion
        };
    }

    /// <summary>
    /// Maps a LogErrorRequest to an ErrorLog entity.
    /// </summary>
    public static ErrorLog ToEntity(this LogErrorRequest request)
    {
        return new ErrorLog
        {
            Timestamp = DateTime.UtcNow,
            Level = (LogLevel)request.Level,
            Source = request.Source,
            Message = request.Message,
            StackTrace = request.StackTrace,
            AdditionalData = request.AdditionalData,
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode
        };
    }

    /// <summary>
    /// Maps an ErrorLog entity to an ErrorLogResponse.
    /// </summary>
    public static ErrorLogResponse ToResponse(this ErrorLog entity)
    {
        return new ErrorLogResponse
        {
            Id = entity.Id,
            Timestamp = entity.Timestamp,
            Level = (LogLevelDto)entity.Level,
            Source = entity.Source,
            Message = entity.Message,
            StackTrace = entity.StackTrace,
            AdditionalData = entity.AdditionalData,
            CompanyCode = entity.CompanyCode,
            ApplicationCode = entity.ApplicationCode
        };
    }
}
