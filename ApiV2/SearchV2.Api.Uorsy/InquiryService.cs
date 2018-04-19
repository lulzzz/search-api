using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using RazorLight;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SearchV2.Api.Uorsy
{
    public sealed class InquiryRequest
    {
        [FromBody]
        public InquiryBody Body { get; set; }
        
        public class InquiryBody
        {
            [Required]
            public string Name { get; set; }

            [Required, RegularExpression(@"^\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b$")]
            public string Email { get; set; }

            [Required]
            public string Institution { get; set; }
            public string Comments { get; set; }

            public IDictionary<string, InquiryItem> InquiryItems { get; set; }

            public class InquiryItem
            {
                public int? AmountId { get; set; }
                public string Amount { get; set; }
            }
        }
    }


    public sealed class InquiryService
    {
        static ILogger _logger = Log.ForContext<InquiryService>();

        readonly IRazorLightEngine _engine;
        readonly string _host;
        readonly int _port;
        readonly MailboxAddress _emailFrom;
        readonly string _inquiryNotificationEmail;
        readonly string _username;
        readonly string _password;

        public InquiryService(string smtpHost, int smtpPort, string smtpUsername, string smtpPassword, string emailFrom, string inquiryNotificationEmail)
        {
            _engine = new RazorLightEngineBuilder()
              .UseFilesystemProject(Path.GetFullPath("Templates"))
              .UseMemoryCachingProvider()
              .Build();
            _host = smtpHost;
            _port = smtpPort;
            _emailFrom = new MailboxAddress("UORSY", emailFrom);
            _inquiryNotificationEmail = inquiryNotificationEmail;
            _username = smtpUsername;
            _password = smtpPassword;
        }

        public async Task Inquire(InquiryData data)
        {
            using (var client = new SmtpClient())
            {
                client.LocalDomain = "api.uorsy.local";
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                var connectTask = Task.Run(async () =>
                {
                    await client.ConnectAsync(_host, _port, true);
                    await client.AuthenticateAsync(_username, _password);
                });

                var notificationMail = new MimeMessage {
                    Subject = "UORSY Structure Search",
                    Body = new TextPart(TextFormat.Html) { Text = await _engine.CompileRenderAsync("InquiryNotificationTemplate.cshtml", data) }
                };
                notificationMail.From.Add(_emailFrom);
                notificationMail.To.Add(new MailboxAddress("UORSY Screenlibs", _inquiryNotificationEmail));
                
                await connectTask;
                var notificationTask = client.SendAsync(notificationMail);

                var mailToCustomer = new MimeMessage
                {
                    Subject = "UORSY Structure Search",
                    Body = new TextPart(TextFormat.Html) { Text = await _engine.CompileRenderAsync("InquiryCustomerTemplate.cshtml", data) }
                };
                mailToCustomer.From.Add(_emailFrom);
                mailToCustomer.To.Add(new MailboxAddress(data.Name, data.Email));

                await notificationTask;
                await client.SendAsync(mailToCustomer);
                
                await client.DisconnectAsync(true);
            }
        }
    }

    public class InquiryData
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Institution { get; set; }
        public string Comments { get; set; }

        public IEnumerable<InquiryItem> InquiryItems { get; set; }

        public class InquiryItem
        {
            public string Id { get; set; }
            public string Amount { get; set; }
            public string FormattedPrice { get; set; }
        }
    }
}
