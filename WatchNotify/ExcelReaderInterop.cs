using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel;
using System.Runtime.InteropServices;

namespace WatchNotify
{
    public class ExcelReaderInterop : IDisposable
    {
        Application _excelApp;
        public Dictionary<string,Array> SheetsAndContents { get; set; }

        public ExcelReaderInterop()
        {
            _excelApp = new Application();
        }
        /// <summary>
        /// Open the file path received in Excel. Then, open the workbook
        /// within the file. Send the workbook to the next function, the internal scan
        /// function. Will throw an exception if a file cannot be found or opened.
        /// </summary>
        public void ExcelOpenSpreadsheets(string thisFileName)
        {
            try
            {               
                Workbook workBook = _excelApp.Workbooks.Open(thisFileName);
                
                ExcelScanIntenal(workBook);                
                workBook.Close(false, thisFileName, null);
                DisposeCOMObject(workBook);
            }
            catch 
            {
                //
                // Deal with exceptions.
                //                
            }
        }

        private void ExcelScanIntenal(Workbook workBookIn)
        {
            SheetsAndContents = new Dictionary<string, Array>();
            
            int numSheets = workBookIn.Sheets.Count;
            
            for (int sheetNum = 1; sheetNum < numSheets + 1; sheetNum++)
            {
                Worksheet sheet = (Worksheet)workBookIn.Sheets[sheetNum];                
                Range excelRange = sheet.UsedRange;                                
                var j = (System.Array)excelRange.Cells.Value;
                SheetsAndContents.Add(sheet.Name, j);
            }
        }

        public void Dispose()
        {
            CloseAndCleanUp();
        }

        private void CloseAndCleanUp()
        {  
            if (this._excelApp != null)
            {
                this._excelApp.Quit();
                DisposeCOMObject(_excelApp);
                this._excelApp = null;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        private void DisposeCOMObject(object comObject)
        {
            while (System.Runtime.InteropServices.Marshal.ReleaseComObject(comObject) > 1)
            { }
        }
    }
}
