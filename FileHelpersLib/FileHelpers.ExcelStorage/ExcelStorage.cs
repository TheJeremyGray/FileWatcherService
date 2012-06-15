using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading;
using Excel;
using FileHelpers.Events;
using System.Collections.Generic;

namespace FileHelpers.DataLink
{
	/// <summary><para>This class implements the <see cref="DataStorage"/> for Microsoft Excel Files.</para>
	/// <para><b>WARNING you need to have installed Microsoft Excel 2000 or newer to use this feature.</b></para>
	/// <para><b>To use this class you need to reference the FileHelpers.ExcelStorage.dll file.</b></para>
	/// </summary>
	/// <remmarks><b>This class is contained in the FileHelpers.ExcelStorage.dll and need the Interop.Office.dll and Interop.Excel.dll to work correctly.</b></remmarks>
	public sealed class ExcelStorage : DataStorage, IDisposable
	{

        private ExcelUpdateLinksMode mUpdateLinks = ExcelUpdateLinksMode.NeverUpdate;

        /// <summary>
        /// Specifies the way links in the file are updated. By default the library never update the links
        /// </summary>
        public ExcelUpdateLinksMode UpdateLinks
        {
            get { return mUpdateLinks; }
            set { mUpdateLinks = value; }
        }

		#region "  Constructors  "

		/// <summary>Create a new ExcelStorage to work with the specified type</summary>
		/// <param name="recordType">The type of records.</param>
		public ExcelStorage(Type recordType):base(recordType)
		{
            InitExcel();

			// Temporary

//			if (RecordHasDateFields())
//				throw new NotImplementedException("For now the ExcelStorage don´t work with DateTime fields, sorry for the problems.");
		}

		/// <summary>Create a new ExcelStorage to work with the specified type</summary>
		/// <param name="recordType">The type of records.</param>
		/// <param name="startRow">The row of the first data cell. Begining in 1.</param>
		/// <param name="startCol">The column of the first data cell. Begining in 1.</param>
		public ExcelStorage(Type recordType, int startRow, int startCol) : this(recordType)
		{
			mStartColumn = startCol;
			mStartRow = startRow;
		}

		/// <summary>Create a new ExcelStorage to work with the specified type</summary>
		/// <param name="recordType">The type of records.</param>
		/// <param name="startRow">The row of the first data cell. Begining in 1.</param>
		/// <param name="startCol">The column of the first data cell. Begining in 1.</param>
		/// <param name="fileName">The file path to work with.</param>
		public ExcelStorage(Type recordType, string fileName, int startRow, int startCol) : this(recordType, startRow, startCol)
		{
			mFileName = fileName;            
           OpenOrCreateWorkbook(mFileName);
		}

		#endregion

		#region "  Private Fields  "

		private string mSheetName = String.Empty;
		private string mFileName = String.Empty;

		private int mStartRow = 1;
		private int mStartColumn = 1;

		private int mHeaderRows = 0;

		private ApplicationClass mApp;
		private Workbook mBook;
		private Worksheet mSheet;
		//private RecordInfo mRecordInfo;

		private string mTemplateFile = string.Empty;
        private readonly Missing mv = Missing.Value;        

		#endregion

		#region "  Public Properties  "


		/// <summary>The Start Row where is the data. Starting at 1.</summary>
		public int StartRow
		{
			get { return mStartRow; }
			set { mStartRow = value; }
		}

		/// <summary>The Start Column where is the data. Starting at 1.</summary>
		public int StartColumn
		{
			get { return mStartColumn; }
			set { mStartColumn = value; }
		}

		/// <summary>The numbers of header rows.</summary>
		public int HeaderRows
		{
			get { return mHeaderRows; }
			set { mHeaderRows = value; }
		}

		/// <summary>The Excel File Name.</summary>
		public string FileName
		{
			get { return mFileName; }
			set { mFileName = value; }
		}

		/// <summary>The Excel Sheet Name, if empty means the current worksheet in the file.</summary>
		public string SheetName
		{
			get { return mSheetName; }
			set 
            {
                //System.Runtime.InteropServices.COMException was unhandled
                //  HelpLink=C:\Program Files (x86)\Microsoft Office\Office12\1033\XLMAIN11.CHM
                //  Message=You typed an invalid name for a sheet or chart. Make sure that:

                //• The name that you type does not exceed 31 characters.
                //• The name does not contain any of the following characters:  :  \  /  ?  *  [  or  ]
                //• You did not leave the name blank.

                mSheetName = value.Substring(0, value.Length > 31 ? 31 : value.Length).Replace(":", "").Replace(@"/", "").Replace(@"\", "").Replace("?", "").Replace("*", "").Replace("[", "").Replace("]", ""); 
            }
		}

		private bool mOverrideFile = true;

		/// <summary>Indicates what the Storage does if the file exist.</summary>
		public bool OverrideFile
		{
			get { return mOverrideFile; }
			set { mOverrideFile = value; }
		}

		/// <summary>
		/// Indicates the source xls file to be used as template when write data.
		/// </summary>
		public string TemplateFile
		{
			get { return mTemplateFile; }
			set { mTemplateFile = value; }
		}

        public List<string> SheetNames
        {
            get
            {
                List<string> names = new List<string>();
                
                OpenOrCreateWorkbook(mFileName);

                foreach (Worksheet sheet in mBook.Sheets)
                {
                    names.Add(sheet.Name);
                }  
               
                return names;
            }
        }

		#endregion

		#region "  InitExcel  "

		private void InitExcel()
		{
			try
			{
				mApp = new ApplicationClass();
			}
			catch (System.Runtime.InteropServices.COMException ex)
			{
				if (ex.Message.IndexOf("00024500-0000-0000-C000-000000000046") >= 0)
					throw new ExcelBadUsageException("Excel 2000 or newer is not installed in this system.");
				else
					throw;
			}


                mBook = null;
                mSheet = null;
                mApp.Visible = false;
                mApp.AlertBeforeOverwriting = false;
                mApp.ScreenUpdating = false;
                mApp.DisplayAlerts = false;
                mApp.EnableAnimations = false;
                mApp.Interactive = false;

		}

		#endregion

		#region "  CloseAndCleanUp  "

		private void CloseAndCleanUp()
		{
			if (mSheet != null)
			{
				DisposeCOMObject(mSheet);
				mSheet = null;
			}

			if (mBook != null)
			{
				mBook.Close(false, mv, mv);
				DisposeCOMObject(mBook);
				mBook = null;
			}
			
			//Thread.Sleep(100);
			
			if (mApp != null)
			{
				//mApp.Interactive = true;
				mApp.Quit();
				DisposeCOMObject(mApp);
				mApp = null;
			}
			
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
		private void DisposeCOMObject(object comObject)
		{
			while (System.Runtime.InteropServices.Marshal.ReleaseComObject(comObject) > 1)
			{}
		}

		#endregion		

		#region "  OpenWorkbook  "

		private void OpenWorkbook(string filename)
		{
			FileInfo info = new FileInfo(filename);
			if (info.Exists == false)
                throw new FileNotFoundException(String.Format("Excel File '{0}' not found.", filename), filename);

			mBook = mApp.Workbooks.Open(info.FullName, (int) mUpdateLinks, 
                mv, mv, mv, mv, mv, mv, mv, mv, mv, mv, mv);

			if (mSheetName == null || mSheetName == string.Empty)
				mSheet = (Worksheet) mBook.ActiveSheet;
			else
			{
                try
                {
                    mSheet = (Worksheet)mBook.Sheets[mSheetName];
                }
                catch (System.Runtime.InteropServices.COMException comEx)
                {
                    if (comEx.ErrorCode == -2147352565)
                    {
                        Worksheet newSheet = (Worksheet)mBook.Sheets.Add(mv, mv, 1, mv);
                        newSheet.Name = mSheetName;
                        mSheet = (Worksheet)mBook.Sheets[mSheetName];
                    }
                }
                catch
                {
                    throw new ExcelBadUsageException(String.Format("The sheet '{0}' was not found in the workbook.", mSheetName));
                }
			}

		}

		#endregion

		#region "  CreateWorkbook methods  "

		private void OpenOrCreateWorkbook(string filename)
		{
            if (IsSheetInitialized())
                return;

            if (File.Exists(filename))
                OpenWorkbook(filename);
            else
                CreateWorkbook();            
		}

		private void CreateWorkbook()
		{
			mBook = mApp.Workbooks.Add(mv);

            if (mSheetName == null || mSheetName == string.Empty)
                mSheet = (Worksheet)mBook.ActiveSheet;
            else  // If we have specified a sheet name, add it
            {
                try
                {
                    Worksheet newSheet = (Worksheet)mBook.Sheets.Add(mv, mv, 1, mv);
                    newSheet.Name = mSheetName;                    
                    mSheet = (Worksheet)mBook.Sheets[mSheetName];
                }
                catch
                {
                    throw new ExcelBadUsageException(String.Format("The sheet '{0}' could not be added.", mSheetName));
                }
            }
		}

		#endregion

		#region "  SaveWorkbook  "

		private void SaveWorkbook()
		{
			if (mBook != null)
				mBook.Save();
		}

		private void SaveWorkbook(string filename)
		{
			if (mBook != null)
				mBook.SaveAs(filename, mv, mv, mv, mv, mv, XlSaveAsAccessMode.xlNoChange, mv, mv, mv, mv);
		}

		#endregion

		#region "  CellAsString  "

		private string CellAsString(object row, object col)
		{
			if (mSheet == null)
			{
				return null;
			}
            Range r = (Range)mSheet.Cells[row, col];
			object res = r.Value;
			DisposeCOMObject(r);
			return Convert.ToString(res);
		}

		#endregion

		#region "  ColLeter  "

		string _ColLetter(int col /* 0 origin */) 
		{ 
			// col = [0...25] 
			if (col >= 0 && col <= 25) 
				return ((char)('A' + col)).ToString(); 
			return ""; 
		} 
		string ColLetter(int col /* 1 Origin */) 
		{ 
			if (col < 1 || col > 256) 
				throw new ExcelBadUsageException("Column out of range; must be between 1 and 256"); // Excel limits 
			col--; // make 0 origin 
			// good up to col ZZ 
			int col2 = (col / 26)-1; 
			int col1 = (col % 26); 
			return _ColLetter(col2) + _ColLetter(col1); 
		} 
		
		#endregion

		#region "  RowValues  "

		private object[] RowValues(int row, int startCol, int numberOfCols)
		{
			if (mSheet == null)
			{
				return null;
			}
			object[] res;

			Range r;
			if (numberOfCols == 1)
			{
				r = mSheet.get_Range(ColLetter(startCol) + row.ToString(), ColLetter(startCol + numberOfCols - 1) + row.ToString());
				res = new object[] {r.Value};
				//DisposeCOMObject(r);
			}
			else
			{
				r = mSheet.get_Range(ColLetter(startCol) + row.ToString(), ColLetter(startCol + numberOfCols - 1) + row.ToString());
				object[,] resTemp = (object[,]) r.Value2;
				//DisposeCOMObject(r);

				res = new object[numberOfCols];

				for (int i = 1; i <= numberOfCols; i++)
				{
					res[i - 1] = resTemp[1, i];
				}

			}

			return res;
		}

		private void WriteRowValues(object[] values, int row, int startCol)
		{
			if (mSheet == null)
				return;

			Range r = mSheet.get_Range(ColLetter(startCol) + row.ToString(), ColLetter(startCol + values.Length - 1) + row.ToString());

			r.Value2 = values;
		}

		#endregion

		#region "  InsertRecords  "

		/// <summary>Insert all the records in the specified Excel File.</summary>
		/// <param name="records">The records to insert.</param>
        public override void InsertRecords(object[] records)
		{
		    if (records == null || records.Length == 0)
		        return;

            System.Globalization.CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            try
		    {
		        int recordNumber = 0;
                OnProgress(new ProgressEventArgs(0, records.Length));	

		            if (mOverrideFile && File.Exists(mFileName))
		                File.Delete(mFileName);

		            if (mTemplateFile != string.Empty)
		            {
		                if (File.Exists(mTemplateFile) == false)
                            throw new ExcelBadUsageException(String.Format("Template file not found: '{0}'", mTemplateFile));

		                if (mTemplateFile != mFileName)
		                    File.Copy(mTemplateFile, mFileName, true);
		            }

		            OpenOrCreateWorkbook(mFileName);

		            for (int i = 0; i < records.Length; i++)
		            {
		                recordNumber++;
                        OnProgress(new ProgressEventArgs(recordNumber, records.Length));

		                WriteRowValues(RecordToValues(records[i]), mStartRow + i, mStartColumn);
		            }

		            SaveWorkbook(mFileName);
		    }
		    catch
		    {
		        throw;
		    }
		    finally
		    {
                Thread.CurrentThread.CurrentCulture = oldCulture;
		    }
		}

	    #endregion

		#region "  ExtractRecords  "

		/// <summary>Returns the records extracted from Excel file.</summary>
		/// <returns>The extracted records.</returns>
		public override object[] ExtractRecords()
		{
			if (mFileName == String.Empty)
				throw new ExcelBadUsageException("You need to specify the WorkBookFile of the ExcelDataLink.");


			ArrayList res = new ArrayList();

            System.Globalization.CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            try
            {
                int cRow = mStartRow;

                int recordNumber = 0;
                OnProgress(new ProgressEventArgs(0, -1));

                object[] colValues = new object[RecordFieldCount];

                    OpenWorkbook(mFileName);

                    while (CellAsString(cRow, mStartColumn) != String.Empty)
                    {
                        try
                        {
                            recordNumber++;
                            OnProgress(new ProgressEventArgs(recordNumber, -1));

                            colValues = RowValues(cRow, mStartColumn, RecordFieldCount);

                            object record = ValuesToRecord(colValues);
                            res.Add(record);
                        }
                        catch (Exception ex)
                        {
                            switch (mErrorManager.ErrorMode)
                            {
                                case ErrorMode.ThrowException:
                                    throw;
                                case ErrorMode.IgnoreAndContinue:
                                    break;
                                case ErrorMode.SaveAndContinue:
                                    AddError(cRow, ex, ColumnsToValues(colValues));
                                    break;
                            }
                        }
                        finally
                        {
                            cRow++;
                        }
                    }
            }
            catch
            {
                throw;
            }
			finally
			{
                Thread.CurrentThread.CurrentCulture = oldCulture;
            }

			return (object[]) res.ToArray(RecordType);
		}

		#endregion


        #region "  Remove Sheet  "

        public bool RemoveSheet(string SheetToRemove)
        {
            try
            {
                OpenOrCreateWorkbook(mFileName);

                Worksheet MySheet = ((Worksheet)mBook.Sheets[SheetToRemove]);
                MySheet.Delete();

                SaveWorkbook(mFileName);
            }
            catch
            {
                return false;
            }

            return true;
        }

        #endregion

        private string ColumnsToValues(object[] values)
		{
			if (values == null || values.Length == 0)
				return string.Empty;

            string res;
            if (values[0] != null)
                res = values[0].ToString();
            else
                res = string.Empty;

			for(int i = 1; i < values.Length; i++)
			{
				if (values[i] == null)
					res += ",";
				else
					res += "," + values[i].ToString();
			}
			
			return res;
		}

        private bool IsSheetInitialized()
        {
            if (null == mBook || null == mSheet)
                return false;
            return true;
        }

        public void Dispose()
        {
            CloseAndCleanUp();
        }
    }

    /// <summary>
    /// Specifies the way links in the file are updated.
    /// </summary>
    public enum ExcelUpdateLinksMode
	{
        /// <summary>User specifies how links will be updated</summary>
        UserPrompted = 1,
        /// <summary>Never update links for this workbook on opening</summary>
        NeverUpdate = 2,
        /// <summary>Always update links for this workbook on opening</summary>
        AlwaysUpdate = 3
	}
}