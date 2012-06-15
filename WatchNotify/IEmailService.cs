using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WatchNotify.EmailService
{
    public interface IEmailService : IDisposable
    {
        System.Net.NetworkCredential Credentials { get; set; }
        int Timeout { get; set; }
        bool EnableDecompression { get; set; }
        bool UnsafeAuthenticatedConnectionSharing { get; set; }
        string Url { get; set; }
 
        bool CheckConnection();
        void EmailWebService(string addressee, string AuctionEmailFromAddress, string p, string p_2);
        void EmailWebServiceWithAttachment(string addressee, string AuctionEmailFromAddress, string p, string p_2, byte[] p_3, string p_4);
    }
}
