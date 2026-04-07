using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CH.Business.Services
{
	public interface IEmailService
	{
		Task<bool> SendEmailAsync(string to, string subject, string body, bool isBodyHtml);
	}

	public class EmailService : IEmailService
	{
		private readonly IConfiguration _config;
		private readonly ILogger _logger;
		private readonly string _host;
		private readonly int _port;
		private readonly string _username;
		private readonly string _password;
		private readonly string _from;
		private readonly bool _enableSsl;

		private readonly SmtpClient _smtpClient;

		public EmailService(IConfiguration config, ILogger logger)
		{
			_config = config;
			_logger = logger;

			_host = _config.GetEmailSmtpHost();
			_port = _config.GetEmailSmtpPort();
			_username = _config.GetEmailSmtpUsername();
			_password = _config.GetEmailSmtpPassword();
			_from = _config.GetEmailSmtpFrom();
			_enableSsl = config.GetEmailSmtpEnableSsl();

			_smtpClient = new SmtpClient()
			{
				Host = _host,
				Port = _port,
				Credentials = new NetworkCredential(_username, _password),
				EnableSsl = _enableSsl,
			};
		}

		public async Task<bool> SendEmailAsync(
			string to, string subject, string body, bool isBodyHtml)
		{
			MailMessage mailMessage = new MailMessage(_from, to, subject, body)
			{
				IsBodyHtml = isBodyHtml,
			};

			try
			{
				await _smtpClient.SendMailAsync(mailMessage);
			}
			catch (Exception e)
			{
				_logger.Log(LogLevel.Error, 9999, $"An error occurred while attempting to send mail: {e}");
				return false;
			}

			return true;
		}

	}

	public class DummyEmailService : IEmailService
	{
		public Task<bool> SendEmailAsync(string to, string subject, string body, bool isBodyHtml)
		{
			throw new NotImplementedException();
		}
	}

}
