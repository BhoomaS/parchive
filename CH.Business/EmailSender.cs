using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using CH.Models.Email;
using CH.Models.Enums;
using System.Threading.Tasks;
using CH.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Extensions.Logging;

namespace CH.Business
{
  public interface IEmailSender  
  {
    //Task<bool> SendEmailAsync(string to, string cc, string subject, string message);
    Task<bool> SendEmailAsync(EmailService service);
  }
  public class EmailSender : IEmailSender
  {

    private readonly ILogger<EmailSender> _logger;

    public EmailSender(
      ILogger<EmailSender> logger)
    {
      _logger = logger;
    }

    //public async Task<bool> SendEmailAsync(string to, string cc, string subject, string message)
    //{
    //  bool isValid = false;
    //  using (MailMessage mail = new MailMessage())
    //  {
    //    mail.From = new MailAddress("no-reply@converginghealth.com");
    //    //mail.Attachments.Add(new Attachment(filename));
    //    if (cc != null)
    //    {
    //      mail.CC.Add("gladis.merlin@converginghealth.com");
    //    }
    //    //if (uploadedDocs != null)
    //    //{
    //    //  mail.Attachments.Add(new Attachment(uploadedDocs));
    //    //}

    //    mail.To.Add(to);
    //    mail.Subject = subject;
    //    mail.Body = message;
    //    using (SmtpClient smtp = new SmtpClient("smtp.office365.com"))
    //    {
    //      smtp.Port = 587;
    //      smtp.Credentials = new NetworkCredential("no-reply@converginghealth.com", "B$y@6U!23&*");
    //      smtp.EnableSsl = true;

    //      try
    //      {
    //        smtp.Send(mail);
    //        isValid = true;
    //        return isValid;
    //      }
    //      catch (Exception ex)
    //      {
    //        Console.WriteLine("Exception caught in CreateTestMessage2(): {0}",
    //            ex.ToString());
    //      }
    //    }

    //  }

    //  return isValid;
    //}


    public async Task<bool> SendEmailAsync(EmailService service)
    {
      bool isValid = false;
      if (service != null)
      {
        using (MailMessage mail = new MailMessage())
        {
          mail.From = new MailAddress("info@mypha.com");
          //mail.From = new MailAddress("no-reply@converginghealth.com");

          //mail.Attachments.Add(new Attachment(filename));
          if (service.cc != null)
          {
            //mail.CC.Add(service.cc);
            string[] multipleCC = service.cc.Split(",");
            foreach (string address in multipleCC)
            {

              if (address.Trim().ToString().Contains('@') && address.Trim().ToString().Contains('.'))
              {
                //_logger.LogInformation(address.ToString());
                mail.CC.Add(address.ToString());
              }

            }
          }
          if (service.uploadedDocs != null)
          {
            string folderName = @"C:\\EmailAttachment";
            string subdir = "C:\\EmailAttachment\\Reports";

            string[] filestoUpload = service.uploadedDocs.Split(',');


            foreach (string file in filestoUpload)
            {
             
              if (file.Contains('.'))
              {
                //_logger.LogInformation(file);
                var filename = subdir + @"\\" + file.ToString();
                //_logger.LogInformation(filename);
                mail.Attachments.Add(new Attachment(filename));
              }
             
            }
            //var filename = subdir + @"\\"+service.uploadedDocs;
            //mail.Attachments.Add(new Attachment(filename));
          }

          mail.To.Add(service.to);
          mail.Subject = service.subject;
          mail.Body = service.message + " \n \n" + service.notes + " \n \n" + service.signature;
          using (SmtpClient smtp = new SmtpClient("smtp.office365.com"))
          {
            smtp.Port = 587;
            smtp.Credentials = new NetworkCredential("info@mypha.com", "PH@q32023");
            //smtp.Credentials = new NetworkCredential("no-reply@converginghealth.com", "C$y@7V!23&*");
            smtp.EnableSsl = true;

            try
            {
              smtp.Send(mail);
              isValid = true;
              return isValid;
            }
            catch (Exception ex)
            {
              Console.WriteLine("Exception caught in CreateTestMessage2(): {0}",
                  ex.ToString());
            }
          }

        }
      }
      

      return isValid;
    }



  }
}
