using RazorLight;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SearchV2.Api.Uorsy
{
    public sealed class InquiryRequest
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


    public sealed class InquiryService
    {
        static ILogger _logger = Log.ForContext<InquiryService>();

        readonly IRazorLightEngine _engine;
        readonly string _host;
        readonly int _port;
        readonly string _emailFrom;
        readonly string _inquiryNotificationEmail;

        public InquiryService(string pathToTemplates, string host, int port, string emailFrom, string inquiryNotificationEmail)
        {
            _engine = EngineFactory.CreatePhysical("templates");
            _host = host;
            _port = port;
            _emailFrom = emailFrom;
            _inquiryNotificationEmail = inquiryNotificationEmail;
        }

        public async Task Inquire(InquiryData data)
        {
            using (var client = new SmtpClient(_host, _port))
            {
                var mailToCustomer = new MailMessage(_emailFrom, data.Email)
                {
                    Subject = "UORSY Structure Search - your inquiry",
                    IsBodyHtml = true,
                    Body = _engine.Parse("InquiryCustomerTemplate.cshtml", data)
                };

                var notificationMail = new MailMessage(_emailFrom, _inquiryNotificationEmail)
                {
                    Subject = "UORSY Structure Search",
                    IsBodyHtml = true,
                    Body = _engine.Parse("InquiryNotificationTemplate.cshtml", data)
                };
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
            public int MyProperty { get; set; }
            public string FormattedPrice { get; set; }
        }
    }
}
