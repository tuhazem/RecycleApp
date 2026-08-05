using System.Threading.Tasks;

namespace RecyclingApp.Application.Common.Interfaces;

/// <summary>
/// Interface for sending emails, implemented by the Infrastructure layer.
/// </summary>
public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
}
