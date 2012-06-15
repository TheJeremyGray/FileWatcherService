using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using WatchNotifyService.EmailService;
using WatchNotify.EmailService;


namespace WatchNotifyService
{
    public partial class Service1 : ServiceBase
    {
        private WatchNotify.WatchNotify MyAuctionWatcher;
        private IEmailService MyEmailService;
        private const string SOURCE = "MyAuctionWatcher";
        private const string LOG = "Application";

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            fileSystemWatcher1.Path = ConfigurationManager.AppSettings["WatchPath"];
          
            MyEmailService = new EmailService.EmailService { Timeout = System.Threading.Timeout.Infinite, UnsafeAuthenticatedConnectionSharing = true, EnableDecompression = true };

            MyEmailService.Url = ConfigurationManager.AppSettings["EmailServiceURL"];
            MyEmailService.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["EmailServiceUserName"], ConfigurationManager.AppSettings["EmailServicePassword"]);

            MyAuctionWatcher = new WatchNotify.WatchNotify(ConfigurationManager.AppSettings["GoogleUsername"], ConfigurationManager.AppSettings["GooglePassword"], MyEmailService);
            
            MyAuctionWatcher.FileArchiveLocation = ConfigurationManager.AppSettings["ArchivePath"];
            MyAuctionWatcher.NotificationAdministrationSheetKey = ConfigurationManager.AppSettings["NotificationAdministrationSheetKey"];
            MyAuctionWatcher.AuctionEmailFromAddress = ConfigurationManager.AppSettings["AuctionEmailFromAddress"];

            if (!EventLog.SourceExists(SOURCE))
                EventLog.CreateEventSource(SOURCE, LOG);
        }

        protected override void OnStop()
        {
            MyAuctionWatcher.Dispose();
            MyEmailService.Dispose();
        }        

        private void fileSystemWatcher1_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            try
            {
                MyAuctionWatcher.FoundFile(e.FullPath);
            }
            catch (WatchNotify.InvalidException iae)
            {
                EventLog.WriteEntry(iae.Message, EventLogEntryType.Error);
                MyEmailService.EmailWebServiceWithAttachment(MyAuctionWatcher.AuctionErrorNotificationEmailAddress, MyAuctionWatcher.AuctionErrorNotificationEmailAddress, String.Format("[Auction Error] Invalid Auction Type ({0})", iae.theInvalidType), iae.Message, System.IO.File.ReadAllBytes(e.FullPath), e.Name);
            }
            catch (Exception Ex)
            {
                EventLog.WriteEntry(Ex.Message, EventLogEntryType.Error);
                MyEmailService.EmailWebService(MyAuctionWatcher.AuctionErrorNotificationEmailAddress, MyAuctionWatcher.AuctionErrorNotificationEmailAddress, "[Auction Error]", Ex.Message);
            }
        }
    }
}
