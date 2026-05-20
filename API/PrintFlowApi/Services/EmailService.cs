using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PrintFlowApi.Services;

public class EmailService(IConfiguration configuration, ILogger<EmailService> logger)
{
    public async Task SendPasswordResetAsync(string toEmail, string toName, string token, CancellationToken cancellationToken)
    {
        var frontendUrl = configuration["FrontendUrl"] ?? configuration["FRONTEND_URL"] ?? "http://localhost:5173";
        var resetUrl = $"{frontendUrl.TrimEnd('/')}/recuperar-senha?token={Uri.EscapeDataString(token)}";
        var subject = "Redefinicao de senha - CRM Grafica Modelo";
        var html = $"""
            <p>Ola, {System.Net.WebUtility.HtmlEncode(toName)}.</p>
            <p>Recebemos uma solicitacao para redefinir sua senha no CRM Grafica Modelo.</p>
            <p><a href="{resetUrl}">Clique aqui para criar uma nova senha</a>.</p>
            <p>Este link expira em 1 hora. Se voce nao solicitou a recuperacao, ignore este email.</p>
            """;

        await SendAsync(toEmail, toName, subject, html, cancellationToken);
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string html, CancellationToken cancellationToken)
    {
        var host = configuration["Smtp:Host"] ?? configuration["SMTP_HOST"];
        var user = configuration["Smtp:User"] ?? configuration["SMTP_USER"];
        var password = configuration["Smtp:Password"] ?? configuration["SMTP_PASSWORD"];
        var fromEmail = configuration["Smtp:FromEmail"] ?? configuration["SMTP_FROM_EMAIL"] ?? configuration["Smtp:From"];
        var fromName = configuration["Smtp:FromName"] ?? configuration["SMTP_FROM_NAME"] ?? "CRM Grafica Modelo";
        var secure = configuration.GetValue("Smtp:EnableSsl", configuration.GetValue("SMTP_SECURE", true));
        var port = configuration.GetValue("Smtp:Port", configuration.GetValue("SMTP_PORT", 465));

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fromEmail))
        {
            logger.LogWarning("SMTP nao configurado. Email de recuperacao para {Email} nao foi enviado.", toEmail);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, secure ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);
        await client.AuthenticateAsync(user, password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
