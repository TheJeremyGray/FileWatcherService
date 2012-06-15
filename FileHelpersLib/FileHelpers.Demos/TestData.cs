using System;
using System.Text;

namespace FileHelpersSamples
{
	/// <summary>
	/// Summary description for TestData.
	/// </summary>
	public class TestData
	{
		
	    /// <summary>
	    /// Block of sample data that will be repeated
	    /// </summary>
		public static string mCustomersTest = @"ALFKI|Alfreds Futterkiste|Maria Anders|Sales Representative|Obere Str. 57|Berlin|Germany" + "\r\n" +
			@"ANATR|Emparedados y Helados|Ana Trujillo|Owner|Avda. Constituci�n 2222|M�xico D.F.|Mexico" + "\r\n" +
			@"ANTON|Antonio Moreno Taquer�a|Antonio Moreno|Owner|Mataderos  2312|M�xico D.F.|Mexico" + "\r\n" +
			@"AROUT|Around the Horn|Thomas Hardy|Sales Representative|120 Hanover Sq.|London|UK" + "\r\n" +
			@"BERGS|Berglunds snabbk�p|Christina Berglund|Administrator|Berguvsv�gen  8|Lule�|Sweden" + "\r\n" +
			@"BLAUS|Blauer Delikatessen|Hanna Moos|Sales Rep|Forsterstr. 57|Mannheim|Germany" + "\r\n" +
			@"BLONP|Blondesddsl p�re et fils|Fr�d�rique Citeaux|Manager|24, Kl�ber|Strasbourg|France" + "\r\n" +
			@"BOLID|B�lido Comidas preparadas|Mart�n Sommer|Owner|C/ Araquil, 67|Madrid|Spain" + "\r\n";
		
        /// <summary>
        /// Block of sample numercial data
        /// </summary>
		static string mTestData = "10248|VINET|5|04071996|01081996|16071996|3|32.38|101248|VINET|5|04071996|01081996|16071996|3|352.38" + Environment.NewLine +
		"10249|TOMSP|6|05071996|16081996|10071996|1|111.61|102348|VINET|5|04071996|01081996|16071996|3|3432.38" + Environment.NewLine +
		"10250|HANAR|4|08071996|05081996|12071996|2|125.83|1043248|VINET|5|04071996|01081996|16071996|3|3422.38" + Environment.NewLine +
		"10251|VICTE|3|08071996|05081996|15071996|1|41.34|102648|VINET|5|04071996|01081996|16071996|3|32632.38" + Environment.NewLine +
		"10252|SUPRD|4|09071996|06081996|11071996|2|51.3|670248|VINET|5|04071996|01081996|16071996|3|4232.38" + Environment.NewLine +
		"10253|HANAR|3|10071996|24071996|16071996|2|658.17|853248|VINET|5|04071996|01081996|16071996|3|5532.38" + Environment.NewLine +
		"10254|CHOPS|5|11071996|08081996|23071996|2|22.98|4321248|VINET|5|04071996|01081996|16071996|3|3652.38" + Environment.NewLine +
		"10255|RICSU|9|12071996|09081996|15071996|3|148.33|443248|VINET|5|04071996|01081996|16071996|3|322.38" + Environment.NewLine +
		"10256|WELLI|3|15071996|12081996|17071996|2|13.97|102548|VINET|5|04071996|01081996|16071996|3|3742.38" + Environment.NewLine +
		"10257|HILAA|4|16071996|13081996|22071996|3|81.91|123248|VINET|5|04071996|01081996|16071996|3|5222.38" + Environment.NewLine;
//
//		static string mTestData2 = "10248|VINET|5|3|32.38" + Environment.NewLine +
//		"10249|TOMSP|6|1|11.61" + Environment.NewLine +
//		"10250|HANAR|4|2|65.83" + Environment.NewLine +
//		"10251|VICTE|3|1|41.34" + Environment.NewLine +
//		"10252|SUPRD|4|2|51.3" + Environment.NewLine +
//		"10253|HANAR|3|2|58.17" + Environment.NewLine +
//		"10254|CHOPS|5|2|22.98" + Environment.NewLine +
//		"10255|RICSU|9|3|148.33" + Environment.NewLine +
//		"10256|WELLI|3|2|13.97" + Environment.NewLine +
//		"10257|HILAA|4|3|81.91" + Environment.NewLine;
//
        /// <summary>
        /// Create a huge chunk of random data for testing
        /// </summary>
        /// <param name="records">Number of records to create</param>
        /// <returns>data as a string</returns>
		public static string CreateDelimitedString(int records)
		{
			StringBuilder sb = new StringBuilder(mTestData.Length * records);

			for (int i = 0; i < (records/10); i++)
			{
				sb.Append(mTestData);
			}

			return sb.ToString();
		}
	}
}
