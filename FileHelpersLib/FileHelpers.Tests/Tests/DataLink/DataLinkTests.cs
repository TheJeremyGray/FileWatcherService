#if ! MINI

using System;
using System.Data.OleDb;
using FileHelpers;
using FileHelpers.DataLink;
using NUnit.Framework;

namespace FileHelpers.Tests.DataLink
{
	[TestFixture]
    [Ignore]
	public class DataLinks
	{
		FileDataLink mLink;


		#region "  FillRecordOrders  "

		
		protected void FillRecordOrders(object rec, object[] fields)
		{
			OrdersFixed record = (OrdersFixed) rec;

			record.OrderID = (int) fields[0];
			record.CustomerID = (string) fields[1];
			record.EmployeeID = (int) fields[2];
			record.OrderDate = (DateTime) fields[3];
			record.RequiredDate = (DateTime) fields[4];
			if (fields[5] != DBNull.Value)
				record.ShippedDate = (DateTime) fields[5];
			else
				record.ShippedDate = DateTime.MinValue;
			record.ShipVia = (int) fields[6];
			record.Freight = (decimal) fields[7];

		}

		#endregion

		[Test]
		public void OrdersDbToFile()
		{
			AccessStorage storage = new AccessStorage(typeof(OrdersFixed), @"..\data\TestData.mdb");
			storage.SelectSql = "SELECT * FROM Orders";
			storage.FillRecordCallback = new FillRecordHandler(FillRecordOrders);

			mLink = new FileDataLink(storage);
			mLink.ExtractToFile(@"..\data\temp.txt");
			int extractNum = mLink.LastExtractedRecords.Length;

			OrdersFixed[] records = (OrdersFixed[]) mLink.FileHelperEngine.ReadFile(@"..\data\temp.txt");

			Assert.AreEqual(extractNum, records.Length);
		}


		[Test]
		public void OrdersDbToFileEasy()
		{
			AccessStorage storage = new AccessStorage(typeof(OrdersFixed), @"..\data\TestData.mdb");
			storage.SelectSql = "SELECT * FROM Orders";
			storage.FillRecordCallback = new FillRecordHandler(FillRecordOrders);

			OrdersFixed[] records = (OrdersFixed[]) FileDataLink.EasyExtractToFile(storage,@"..\data\temp.txt");
			
			int extractNum = records.Length;

			records = (OrdersFixed[]) CommonEngine.ReadFile(typeof(OrdersFixed), @"..\data\temp.txt");

			Assert.AreEqual(extractNum, records.Length);
		}

		
		private void FillRecordCustomers(object rec, object[] fields)
		{
			CustomersVerticalBar record = (CustomersVerticalBar) rec;

			record.CustomerID = (string) fields[0];
			record.CompanyName = (string) fields[1];
			record.ContactName = (string) fields[2];
			record.ContactTitle = (string) fields[3];
			record.Address = (string) fields[4];
			record.City = (string) fields[5];
			record.Country = (string) fields[6];
		}

		[Test]
		public void CustomersDbToFile()
		{
			AccessStorage storage = new AccessStorage(typeof (CustomersVerticalBar), @"..\data\TestData.mdb");
			storage.SelectSql =  "SELECT * FROM Customers";
			storage.FillRecordCallback = new FillRecordHandler(FillRecordCustomers);

			mLink = new FileDataLink(storage);
			mLink.ExtractToFile(@"..\data\temp.txt");
			int extractNum = mLink.LastExtractedRecords.Length;

			CustomersVerticalBar[] records = (CustomersVerticalBar[]) mLink.FileHelperEngine.ReadFile(@"..\data\temp.txt");

			Assert.AreEqual(extractNum, records.Length);
		}

		private object FillRecord(object[] fields)
		{
			CustomersVerticalBar record = new CustomersVerticalBar();

			record.CustomerID = (string) fields[0];
			record.CompanyName = (string) fields[1];
			record.ContactName = (string) fields[2];
			record.ContactTitle = (string) fields[3];
			record.Address = (string) fields[4];
			record.City = (string) fields[5];
			record.Country = (string) fields[6];

			return record;
		}



		#region "  GetInsertSql  "

		protected string GetInsertSqlCust(object record)
		{
			CustomersVerticalBar obj = (CustomersVerticalBar) record;

			return String.Format("INSERT INTO CustomersTemp (Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID) " +
				" VALUES ( \"{0}\" , \"{1}\" , \"{2}\" , \"{3}\" , \"{4}\" , \"{5}\" , \"{6}\"  ); ",
				obj.Address,
				obj.City,
				obj.CompanyName,
				obj.ContactName,
				obj.ContactTitle,
				obj.Country,
				obj.CustomerID
				);

		}

		#endregion

		[Test]
		public void CustomersFileToDb()
		{
			AccessStorage storage = new AccessStorage(typeof(CustomersVerticalBar), @"..\data\TestData.mdb");
			storage.InsertSqlCallback = new InsertSqlHandler(GetInsertSqlCust);

			mLink = new FileDataLink(storage);
			ClearData(((AccessStorage) mLink.DataStorage).AccessFileName, "CustomersTemp");

			int count = Count(((AccessStorage) mLink.DataStorage).AccessFileName, "CustomersTemp");
			Assert.AreEqual(0, count);

			mLink.InsertFromFile(@"..\data\UpLoadCustomers.txt");

			count = Count(((AccessStorage) mLink.DataStorage).AccessFileName, "CustomersTemp");
			Assert.AreEqual(91, count);

			ClearData(((AccessStorage) mLink.DataStorage).AccessFileName, "CustomersTemp");
		}


			
		protected object FillRecordOrder(object[] fields)
		{
			OrdersFixed record = new OrdersFixed();

			record.OrderID = (int) fields[0];
			record.CustomerID = (string) fields[1];
			record.EmployeeID = (int) fields[2];
			record.OrderDate = (DateTime) fields[3];
			record.RequiredDate = (DateTime) fields[4];
			if (fields[5] != DBNull.Value)
				record.ShippedDate = (DateTime) fields[5];
			else
				record.ShippedDate = DateTime.MinValue;
			record.ShipVia = (int) fields[6];
			record.Freight = (decimal) fields[7];

			return record;
		}

		
		#region "  GetInsertSql  "

		protected string GetInsertSqlOrder(object record)
		{
			OrdersFixed obj = (OrdersFixed) record;

			return String.Format("INSERT INTO OrdersTemp (CustomerID, EmployeeID, Freight, OrderDate, OrderID, RequiredDate, ShippedDate, ShipVia) " +
				" VALUES ( \"{0}\" , \"{1}\" , \"{2}\" , \"{3}\" , \"{4}\" , \"{5}\" , \"{6}\" , \"{7}\"  ) ",
				obj.CustomerID,
				obj.EmployeeID,
				obj.Freight,
				obj.OrderDate,
				obj.OrderID,
				obj.RequiredDate,
				obj.ShippedDate,
				obj.ShipVia
				);

		}

		#endregion

		[Test]
		public void OrdersFileToDb()
		{

			AccessStorage storage = new AccessStorage(typeof(OrdersFixed), @"..\data\TestData.mdb");
			storage.InsertSqlCallback = new InsertSqlHandler(GetInsertSqlOrder);

			mLink = new FileDataLink(storage);
			ClearData(((AccessStorage) mLink.DataStorage).AccessFileName, "OrdersTemp");

			int count = Count(((AccessStorage) mLink.DataStorage).AccessFileName, "OrdersTemp");
			Assert.AreEqual(0, count);

			mLink.InsertFromFile(@"..\data\UpLoadOrders.txt");

			count = Count(((AccessStorage) mLink.DataStorage).AccessFileName, "OrdersTemp");
			Assert.AreEqual(830, count);

			ClearData(((AccessStorage) mLink.DataStorage).AccessFileName, "OrdersTemp");
		}

		private const string AccessConnStr = @"Jet OLEDB:Global Partial Bulk Ops=2;Jet OLEDB:Registry Path=;Jet OLEDB:Database Locking Mode=1;Jet OLEDB:Database Password=;Data Source=""<BASE>"";Password=;Jet OLEDB:Engine Type=5;Jet OLEDB:Global Bulk Transactions=1;Provider=""Microsoft.Jet.OLEDB.4.0"";Jet OLEDB:System database=;Jet OLEDB:SFP=False;Extended Properties=;Mode=Share Deny None;Jet OLEDB:New Database Password=;Jet OLEDB:Create System Database=False;Jet OLEDB:Don't Copy Locale on Compact=False;Jet OLEDB:Compact Without Replica Repair=False;User ID=Admin;Jet OLEDB:Encrypt Database=False";

		public void ClearData(string fileName, string table)
		{
			string conString = AccessConnStr.Replace("<BASE>", fileName);
			OleDbConnection conn = new OleDbConnection(conString);
			OleDbCommand cmd = new OleDbCommand("DELETE FROM " + table, conn);
			conn.Open();
			cmd.ExecuteNonQuery();
			conn.Close();
			int count = Count(((AccessStorage) mLink.DataStorage).AccessFileName, "OrdersTemp");
		}

		public int Count(string fileName, string table)
		{
			string conString = AccessConnStr.Replace("<BASE>", fileName);
			OleDbConnection conn = new OleDbConnection(conString);
			OleDbCommand cmd = new OleDbCommand("SELECT COUNT (*) FROM " + table, conn);
			conn.Open();
			int res = (int) cmd.ExecuteScalar();
			conn.Close();
			return res;
		}

	}
}

#endif