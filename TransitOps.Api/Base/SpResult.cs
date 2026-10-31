using TransitOps.Api.Exceptions;

namespace TransitOps.Api.Base;

public class SpResult
{
    public int ResultCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public int NewId { get; set; }

    public bool IsSuccess => ResultCode == 0;

    public void ThrowIfFailed()
    {
        switch (ResultCode)
        {
            case 0:
                return;
            case 1:
                throw new NotFoundException(string.IsNullOrWhiteSpace(Message) ? "Resource not found." : Message);
            case 2:
                throw new BusinessRuleException(string.IsNullOrWhiteSpace(Message) ? "Business rule violated." : Message);
            case 3:
                throw new ConflictException(string.IsNullOrWhiteSpace(Message) ? "Conflict or concurrency error." : Message);
            default:
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(Message) ? $"Database operation failed with code {ResultCode}." : Message);
        }
    }
}

