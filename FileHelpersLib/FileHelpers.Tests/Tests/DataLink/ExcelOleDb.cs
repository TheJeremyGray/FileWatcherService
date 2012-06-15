#if ! MINI

using System;
using System.Data;
using System.IO;
using FileHelpers;
using FileHelpers.DataLink;
using NUnit.Framework;

namespace FileHelpersTests.DataLink
{
	[TestFixture]
	public class ExcelOleDb
	{
		[Test]
		public void OneColumn()
		{
            ExcelStorageOleDb provider = new ExcelStorageOleDb(typeof(OneColumnType), 1, 1);
            provider.FileName = @"..\data\Excel\OneColumn.xls";

			object[] res = provider.ExtractRecords();

			Assert.AreEqual(50, res.Length);
		}

		[DelimitedRecord("|")]
		internal class OneColumnType
		{
			public string CustomerCode;
		}

		[Test]
		public void OrdersRead1()
		{
			ExcelStorageOleDb provider = new ExcelStorageOleDb(typeof (OrdersExcelType), 1, 1);

			provider.FileName = @"..\data\Excel\Orders.xls";
			object[] res = provider.ExtractRecords();

			Assert.AreEqual(830, res.Length);
		}

		[Test]
		public void OrdersRead2()
		{
			DataTable dt = ExcelStorageOleDb.ExtractDataTable(@"..\data\Excel\Orders.xls", 1, 1, false);
			Assert.AreEqual(830, dt.Rows.Count);
		}

//		[Test]
//		public void OrdersRead3()
//		{
//			ExcelReader reader = new ExcelReader(1, 1);
//			reader.HasHeaders = false;
//			DataTable dt = reader.ExtractDataTable(@"..\data\Excel\Orders.xls", 1, 1);
//
//			Assert.AreEqual(830, dt.Rows.Count);
//			Assert.AreEqual(typeof(double), dt.Rows[0][0].GetType());
//			Assert.AreEqual(10248.0, dt.Rows[0][0]);
//			
//		}
//
//		[Test]
//		public void OrdersRead4()
//		{
//			ExcelReader reader = new ExcelReader(1, 1);
//			reader.HasHeaders = false;
//			reader.ReadAllAsText = true;
//			DataTable dt = reader.ExtractDataTable(@"..\data\Excel\Orders.xls", 1, 1);
//
//			Assert.AreEqual(830, dt.Rows.Count);
//			Assert.AreEqual(typeof(string), dt.Rows[0][0].GetType());
//			Assert.AreEqual("10248", dt.Rows[0][0]);
//		}

		[Test]
		public void OrdersWrite()
		{
			FileHelperEngine engine = new FileHelperEngine(typeof(OrdersExcelType));

			OrdersExcelType[] resFile = (OrdersExcelType[]) Common.ReadTest(engine, @"Good\OrdersWithOutDates.txt");

			ExcelStorage provider = new ExcelStorage(typeof (OrdersExcelType));
			provider.StartRow = 1;
			provider.StartColumn = 1;
			provider.FileName = @"c:\tempex.xls";
			provider.OverrideFile = true;

			provider.InsertRecords(resFile);

			OrdersExcelType[] res = (OrdersExcelType[]) provider.ExtractRecords();
			
			if (File.Exists(@"c:\tempex.xls")) File.Delete(@"c:\tempex.xls");

			Assert.AreEqual(resFile.Length, res.Length);

			for(int i =0; i < res.Length; i++)
			{
				Assert.AreEqual(resFile[i].CustomerID, res[i].CustomerID);
				Assert.AreEqual(resFile[i].EmployeeID, res[i].EmployeeID);
				Assert.AreEqual(resFile[i].Freight, res[i].Freight);
				Assert.AreEqual(resFile[i].OrderID, res[i].OrderID);
				Assert.AreEqual(resFile[i].ShipVia, res[i].ShipVia);
			}

		}


		[DelimitedRecord("\t")]
		public class OrdersExcelType
		{
			public int OrderID;

			public string CustomerID;

			public int EmployeeID;

			public int ShipVia;

			public string Freight;
		}

		[DelimitedRecord("\t")]
		private class SmallEnumType
		{
			public NetVisibility Visibility;

			public SmallEnumType()
			{}

			public SmallEnumType(NetVisibility v)
			{
				Visibility = v;
			}
		}

		[Test]
		public void OrdersReadWithErrors()
		{
			ExcelStorage provider = new ExcelStorage(typeof (OrdersExcelType), 1, 1);
			provider.FileName = @"..\data\Excel\Orders.xls";
			provider.ErrorManager.ErrorMode = ErrorMode.SaveAndContinue;

			object[] res = provider.ExtractRecords();

			Assert.AreEqual(830, res.Length);
		}


		[Test]
		public void OrdersWithDate()
		{
			ExcelStorage provider = new ExcelStorage(typeof (OrdersExcelWithDate), 1, 1);

			provider.FileName = @"..\data\Excel\Orders.xls";
		
			object[] res = provider.ExtractRecords();

			Assert.AreEqual(830, res.Length);
		}

		[Test]
		[ExpectedException(typeof(ExcelBadUsageException))]
		public void NoTemplate()
		{
			ExcelStorage provider = new ExcelStorage(typeof (CustomersVerticalBar), 1, 1);
			provider.TemplateFile = @"..\the template is not there.xls";
			provider.FileName = @"output.xls";
		
			provider.InsertRecords(new object[] {new CustomersVerticalBar()});
		}

		[Test]
		public void OrdersWriteWithTemplate()
		{
			FileHelperEngine engine = new FileHelperEngine(typeof(OrdersExcelType));

			OrdersExcelType[] resFile = (OrdersExcelType[]) Common.ReadTest(engine, @"Good\OrdersWithOutDates.txt");

			ExcelStorage provider = new ExcelStorage(typeof (OrdersExcelType));
			provider.StartRow = 4;
			provider.StartColumn = 1;
			provider.FileName = @"c:\tempex.xls";
			provider.TemplateFile = Common.TestPath(@"Excel\Template.xls");
			provider.OverrideFile = true;

			provider.InsertRecords(resFile);

			OrdersExcelType[] res = (OrdersExcelType[]) provider.ExtractRecords();
			
			if (File.Exists(@"c:\tempex.xls")) File.Delete(@"c:\tempex.xls");

			Assert.AreEqual(resFile.Length, res.Length);

			for(int i =0; i < res.Length; i++)
			{
				Assert.AreEqual(resFile[i].CustomerID, res[i].CustomerID);
				Assert.AreEqual(resFile[i].EmployeeID, res[i].EmployeeID);
				Assert.AreEqual(resFile[i].Freight, res[i].Freight);
				Assert.AreEqual(resFile[i].OrderID, res[i].OrderID);
				Assert.AreEqual(resFile[i].ShipVia, res[i].ShipVia);
			}

		}


		[DelimitedRecord("\t")]
		public class OrdersExcelWithDate
		{
			public int OrderID;

			public string CustomerID;

			public DateTime WhyNotAllowMe;

		}

		[Test]
		public void EnumConverter()
		{
			ExcelStorage provider = new ExcelStorage(typeof(SmallEnumType), 1, 1);

			provider.FileName = @"..\data\Excel\OneColumnEnum.xls";

			SmallEnumType[] res = (SmallEnumType[]) provider.ExtractRecords();

			Assert.AreEqual(10, res.Length);
			Assert.AreEqual(NetVisibility.Public, res[0].Visibility);
			Assert.AreEqual(NetVisibility.Private, res[1].Visibility);
			Assert.AreEqual(NetVisibility.Protected, res[9].Visibility);
		}

		[Test]
		public void EnumConverterBad()
		{
			ExcelStorage provider = new ExcelStorage(typeof(SmallEnumType), 4, 2);
			provider.FileName = @"..\data\Excel\OneColumnEnumBad.xls";
			provider.ErrorManager.ErrorMode = ErrorMode.SaveAndContinue;
            
			SmallEnumType[] res = (SmallEnumType[]) provider.ExtractRecords();

			Assert.AreEqual(9, res.Length);
			Assert.AreEqual(NetVisibility.Public, res[0].Visibility);
			Assert.AreEqual(NetVisibility.Private, res[1].Visibility);
			Assert.AreEqual(NetVisibility.Protected, res[8].Visibility);

			Assert.AreEqual(1, provider.ErrorManager.ErrorCount);
			Assert.AreEqual(8, provider.ErrorManager.Errors[0].LineNumber);
			Assert.AreEqual("BadValue", provider.ErrorManager.Errors[0].RecordString);

		}

	
		[DelimitedRecord("\t")]
		public class OrdersDateExcelType
		{
			public int OrderID;

			public string CustomerID;

			public DateTime OrderDate;
		}

		[Test]
		public void OrdersDateRead()
		{
			ExcelStorage provider = new ExcelStorage(typeof (OrdersDateExcelType), 1, 1);
			provider.FileName = @"..\data\Excel\OrdersDate.xls";

			OrdersDateExcelType[] res = (OrdersDateExcelType[]) provider.ExtractRecords();

			Assert.AreEqual(830, res.Length);
			Assert.AreEqual(new DateTime(2006, 1, 1), res[0].OrderDate);
			Assert.AreEqual(new DateTime(2006, 3, 21), res[79].OrderDate);
			Assert.AreEqual(new DateTime(2007, 2, 4), res[399].OrderDate);
			Assert.AreEqual(new DateTime(2008, 4, 9), res[829].OrderDate);
		
			
		}


	}

}

#endif