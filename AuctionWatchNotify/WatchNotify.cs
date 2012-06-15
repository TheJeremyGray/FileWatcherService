using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using FileHelpers.DataLink;
using Google.GData.Spreadsheets;
using Google.GData.Client;
using WatchNotify.EmailService;

namespace WatchNotify
{
    public class WatchNotify : IDisposable
    {
        private const string DESTINATION_ORIG_FILE = "OriginalAuctionResults";
        private const string DESTINATION_SETTLEMENT_REPORT = "SettlementReports";
        private List<string> EmailListSettlementReport;
        private List<string> EmailListAuctionDataReceived;
        private List<string> validTypes;
        private readonly SpreadsheetsService MySpreadsheetService;
        private IEmailService MyEmailService;
        private DirectoryInfo FileArchiveDirectory;

        public string FileArchiveLocation 
        { 
            get 
            {
                return FileArchiveDirectory.FullName;
            }  
            set
            {                
                FileArchiveDirectory = new DirectoryInfo(value);

                if (!FileArchiveDirectory.Exists)
                    FileArchiveDirectory.Create();
            } 
        }
        public string ValidAuctionTypes
        {
            get
            {
                return String.Join(",", validTypes.ToArray());
            }
        }        

        public string AuctionErrorNotificationEmailAddress 
        { 
            get 
            {
                return GetCellEntryArray(NotificationAdministrationSheetKey,"Admin")[0,1].Value;                
            } 
        }

        public string GoogleUserName { get; private set; }
        public string GooglePassword { get; private set; }
        public string NotificationAdministrationSheetKey { get; set; }
        public string AuctionEmailFromAddress { get; set; }
        
               
        public WatchNotify(string googleUserName, string googlePassword, IEmailService mailService)
        {
            if (String.IsNullOrEmpty(googleUserName) || String.IsNullOrEmpty(googlePassword))                 
                throw new ArgumentException("Please provide google credentials");            

            GoogleUserName = googleUserName;
            GooglePassword = googlePassword;

            MySpreadsheetService = new SpreadsheetsService("AuctionWatchNotify");
            MySpreadsheetService.setUserCredentials(GoogleUserName, GooglePassword);

            if (!mailService.CheckConnection())
                throw new ArgumentException("Please pass in a working email service account");

            MyEmailService = mailService;
        }
                                                   
        public bool FoundFile(string FilePath)
        {
            if (String.IsNullOrEmpty(NotificationAdministrationSheetKey))
                throw new ArgumentException("Please provide the key to the googledocs administration sheet");

            if (String.IsNullOrEmpty(FileArchiveLocation))
                throw new ArgumentException("Please an archive location");

            if (String.IsNullOrEmpty(AuctionEmailFromAddress))
                throw new ArgumentException("Who should emails come from?");

            FileInfo MyFile = new FileInfo(FilePath);
            string AuctionType = MyFile.Name.Substring(0, MyFile.Name.IndexOf('_'));

            FindValidAuctionTypes();

            if (!validTypes.Contains(AuctionType))
                throw new InvalidException("Acceptable auction types: " + ValidAuctionTypes, AuctionType);
            
            return FoundFile(MyFile,AuctionType);
        }

        private void FindValidAuctionTypes()
        {
            validTypes = new List<string>();

            WorksheetQuery wsQuery = new WorksheetQuery(String.Format(@"http://spreadsheets.google.com/feeds/worksheets/{0}/private/full", NotificationAdministrationSheetKey));
            AtomEntryCollection worksheetEntries = MySpreadsheetService.Query(wsQuery).Entries;

            foreach (WorksheetEntry sheet in worksheetEntries)
            {
                if (!"Admin,Template".Contains(sheet.Title.Text))
                    validTypes.Add(sheet.Title.Text);
            }            
        }

        private bool FoundFile(FileInfo MyFile, string AuctionType)
        {                                      
            // Find Email recipients, if any, from Google Docs
            FindEmailRecipientsForAuction(AuctionType);

            // Stage File Into New Location for Processing
            FileInfo NewFile = StageFileForProcessing(MyFile);

            // Copy staging file to "ConversationAndResults"
            ArchiveFullStagingFile(AuctionType, NewFile);

            // Create Settlement report for email / import into the blotter
            DirectoryInfo SettlementReportArchive = CreateSettlementReportDirectory(AuctionType);

            // Strip "Conversation" Sheets out of settlement report
            StripNonSettlementWorksheets(NewFile);

            //  Send this File to intended recipients 
            EmailToIntendedRecipients(NewFile,AuctionType);
                    
            // Move email copy to "SettlementReport" folder                                                
            NewFile.CopyTo(Path.Combine(SettlementReportArchive.FullName, NewFile.Name), true);
            NewFile.Delete();

            return true;
        }

        private DirectoryInfo CreateSettlementReportDirectory(string AuctionType)
        {
            DirectoryInfo SettlementReportArchive = new DirectoryInfo(Path.Combine(FileArchiveDirectory.FullName, AuctionType, DESTINATION_SETTLEMENT_REPORT));

            if (!SettlementReportArchive.Exists) SettlementReportArchive.Create();
            return SettlementReportArchive;
        }

        private FileInfo StageFileForProcessing(FileInfo MyFile)
        {
            // TODO: thinking about adding a GUID to the end of the filename so we don't have to worry about overwriting files
            FileInfo NewFile = new FileInfo(Path.Combine(FileArchiveDirectory.FullName, MyFile.Name)); 
            MyFile.CopyTo(NewFile.FullName, true);
            MyFile.Delete();
            return NewFile;
        }

        private void EmailToIntendedRecipients(FileInfo NewFile, string SubjectPrefix)
        {
            foreach (string addressee in EmailListSettlementReport)
            {
                MyEmailService.EmailWebServiceWithAttachment(addressee,
                    AuctionEmailFromAddress,
                    String.Format("[Auction Results] {0} Settlement Report", SubjectPrefix),
                    "See attached",
                    File.ReadAllBytes(NewFile.FullName),
                    NewFile.Name);
            }

            foreach (string addressee in EmailListAuctionDataReceived)
            {
                MyEmailService.EmailWebService(addressee,
                    AuctionEmailFromAddress,
                    String.Format(@"[Auction Results] {0} Data Received </end>", SubjectPrefix),
                    "");
            }
        }

        private void StripNonSettlementWorksheets(FileInfo NewFile)
        {
            using (ExcelStorage excelProvider = new ExcelStorage(typeof(Settlement)) { FileName = NewFile.FullName })
            {
                foreach (string item in excelProvider.SheetNames)
                {
                    if (item != "Settlement Report")
                        excelProvider.RemoveSheet(item);
                }
            }
        }

        private void FindEmailRecipientsForAuction(string AuctionType)
        {
            EmailListAuctionDataReceived = new List<string>();
            EmailListSettlementReport = new List<string>();      

            var cells = GetCellEntryArray(NotificationAdministrationSheetKey, AuctionType);

            for (int i = 1; i < cells.GetLength(0); i++)
            {
                if (null != cells[i, 0] && !String.IsNullOrEmpty(cells[i, 0].Value))
                    EmailListSettlementReport.Add(cells[i, 0].Value);

                if (null != cells[i, 2] && !String.IsNullOrEmpty(cells[i, 2].Value))
                    EmailListAuctionDataReceived.Add(cells[i, 2].Value);
            }
        }

        private CellEntry[,] GetCellEntryArray(string workbookKey, string SheetName)
        {
            WorksheetQuery wsQuery = new WorksheetQuery(String.Format(@"http://spreadsheets.google.com/feeds/worksheets/{0}/private/full", workbookKey));
            AtomEntryCollection worksheetEntries = MySpreadsheetService.Query(wsQuery).Entries;
            CellEntry[,] ce = null;

            AtomLink MySheetLink = worksheetEntries.First(x => x.Title.Text == SheetName).Links.FindService(GDataSpreadsheetsNameTable.CellRel, AtomLink.ATOM_TYPE);
            CellQuery MySheetQuery = new CellQuery(MySheetLink.HRef.Content);
            CellFeed MySheetFeed = MySpreadsheetService.Query(MySheetQuery);
            AtomEntryCollection MySheetEntries = MySheetFeed.Entries;
            ce = new CellEntry[MySheetFeed.RowCount.IntegerValue, MySheetFeed.ColCount.IntegerValue];

            for (int i = 0; i < MySheetEntries.Count; i++)
            {
                CellEntry entry = MySheetEntries[i] as CellEntry;
                //google doc is a 1 based array, convert to 0 based
                ce[entry.Row - 1, entry.Column - 1] = entry;
            }
            return ce;
        }

        private void ArchiveFullStagingFile(string AuctionType, FileInfo NewFile)
        {
            DirectoryInfo OriginalFileArchive = new DirectoryInfo(Path.Combine(FileArchiveDirectory.FullName, AuctionType, DESTINATION_ORIG_FILE));

            if (!OriginalFileArchive.Exists) OriginalFileArchive.Create();
            NewFile.CopyTo(Path.Combine(OriginalFileArchive.FullName, NewFile.Name), true);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                if (MyEmailService != null)
                {
                    MyEmailService.Dispose();
                    MyEmailService = null;
                }
        }

        ~WatchNotify()
        {
            Dispose(false);
        }

    }
}
