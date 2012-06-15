using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net;
using System.Diagnostics;
using System.IO;
using FileHelpers.DataLink;
using FileHelpers;
using WatchNotify.EmailService;
using WatchNotify;
using WatchNotifyService.EmailService;


namespace TestWatchNotify
{
    [DelimitedRecord(",")]
    class TestDocument
    {
        public string No;
    }

    class Program
    {
        static void Main(string[] args)
        {
            // declarations
            IEmailService MyEmailService = null;
            WatchNotify.WatchNotify MyAuctionWatcher;

            // service onstart
            MyEmailService = new EmailService() { Timeout = System.Threading.Timeout.Infinite, UnsafeAuthenticatedConnectionSharing = true, EnableDecompression = true };
            MyEmailService.Url = ConfigurationManager.AppSettings["EmailServiceURL"];
            MyEmailService.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["EmailServiceUserName"], ConfigurationManager.AppSettings["EmailServicePassword"]);

            MyAuctionWatcher = new WatchNotify.WatchNotify(ConfigurationManager.AppSettings["GoogleUsername"], ConfigurationManager.AppSettings["GooglePassword"], MyEmailService);
            MyAuctionWatcher.FileArchiveLocation = ConfigurationManager.AppSettings["ArchivePath"];
            MyAuctionWatcher.NotificationAdministrationSheetKey = ConfigurationManager.AppSettings["NotificationAdministrationSheetKey"];
            MyAuctionWatcher.AuctionEmailFromAddress = ConfigurationManager.AppSettings["AuctionEmailFromAddress"];

            // filewatcher filecreated - Invalid Auction 
            FileInfo InvalidAuctionFile = new FileInfo(@"C:\Users\jgray\Desktop\JDG-Test.xls");
            try
            {
                MyAuctionWatcher.FoundFile(InvalidAuctionFile.FullName);
            }
            catch (WatchNotify.InvalidException)
            {
                MyEmailService.EmailWebServiceWithAttachment(MyAuctionWatcher.AuctionErrorNotificationEmailAddress, MyAuctionWatcher.AuctionErrorNotificationEmailAddress, "Invalid Auction Type", "", System.IO.File.ReadAllBytes(InvalidAuctionFile.FullName), InvalidAuctionFile.Name);
            }
            catch (Exception Ex)
            {
                MyEmailService.EmailWebService(MyAuctionWatcher.AuctionErrorNotificationEmailAddress, MyAuctionWatcher.AuctionErrorNotificationEmailAddress, "Auction Exception", Ex.Message);
            }

            // filewatcher filecreated
            FileInfo ValidAuctionFile = new FileInfo(@"C:\Users\jgray\Desktop\Test.xls");
            try
            {
                MyAuctionWatcher.FoundFile(ValidAuctionFile.FullName);
            }
            catch (WatchNotify.InvalidException)
            {
                MyEmailService.EmailWebServiceWithAttachment(MyAuctionWatcher.AuctionErrorNotificationEmailAddress, MyAuctionWatcher.AuctionErrorNotificationEmailAddress, "Invalid Auction Type", "", System.IO.File.ReadAllBytes(ValidAuctionFile.FullName), ValidAuctionFile.Name);
            }
            catch (Exception Ex)
            {
                MyEmailService.EmailWebService(MyAuctionWatcher.AuctionErrorNotificationEmailAddress, MyAuctionWatcher.AuctionErrorNotificationEmailAddress, "Auction Exception", Ex.Message);
            }


            #region "  Test Excel IDisposable  "
            
            // Remove sheet from an existing workbook
            //using (ExcelStorage excelProvider = new ExcelStorage(typeof(TestDocument)) { FileName = @"C:\Users\jgray\Desktop\Test PJM_11-Jun-2012-16_17.xls" })
            //{
            //    bool ThisShouldBeTrue;
            //    bool ThisShouldBeFalse;

            //    foreach (string item in excelProvider.SheetNames)
            //    {
            //        if (item != "Settlement Report")
            //            ThisShouldBeTrue = excelProvider.RemoveSheet(item);

            //        ThisShouldBeFalse = excelProvider.RemoveSheet("blahblahblah");
            //    }

            //    int i = 0;
            //}

            //InsertRecords
            //FileInfo TestFile = new FileInfo(@"C:\Users\jgray\Desktop\ATest.xlsx");
            //TestFile.Delete();

            //using (ExcelStorage excelProvider = new ExcelStorage(typeof(TestDocument)) { FileName = TestFile.FullName })
            //{
            //    List<TestDocument> TheTests = new List<TestDocument>();           
            //    TheTests.Add(new TestDocument() { No = "1" });

            //    excelProvider.InsertRecords(TheTests.ToArray());
            //}

            ////Extract Records
            //using (ExcelStorage excelProvider = new ExcelStorage(typeof(TestDocument)) { FileName = TestFile.FullName })
            //{
            //    object[] MyExtractedTests = excelProvider.ExtractRecords();
            //    int i = 0;
            //}

            #endregion

        }
    }
}
