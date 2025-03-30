using System.Net.NetworkInformation;

namespace AuditLoggingWebAPI.Models
{
    public class SensitiveDataModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string CreditCard { get; set; }
    }
}
