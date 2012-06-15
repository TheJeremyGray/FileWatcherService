#region "  � Copyright 2005-07 to Marcos Meli - http://www.devoo.net"

// Errors, suggestions, contributions, send a mail to: marcos@filehelpers.com.

#endregion

using System;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace FileHelpers.RunTime
{
	/// <summary>Used to create fields that are part of a dilimited record class.</summary>
	public sealed class DelimitedFieldBuilder: FieldBuilder
	{

		internal DelimitedFieldBuilder(string fieldName, string fieldType): base(fieldName, fieldType)
		{}

		internal DelimitedFieldBuilder(string fieldName, Type fieldType): base(fieldName, fieldType)
		{}


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool mFieldQuoted = false;

		/// <summary>Indicates if the field is quoted with some char. (works with QuoteMode and QuoteChar)</summary>
		public bool FieldQuoted
		{
			get { return mFieldQuoted; }
			set { mFieldQuoted = value; }
		}


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private char mQuoteChar = '"';

		/// <summary>Indicates the char used to quote this field. (only used when FieldQuoted is true)</summary>
		public char QuoteChar
		{
			get { return mQuoteChar; }
			set { mQuoteChar = value; }
		}


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private QuoteMode mQuoteMode = QuoteMode.OptionalForRead;

		/// <summary>Indicates the QuoteMode for this field. (only used when FieldQuoted is true)</summary>
		public QuoteMode QuoteMode
		{
			get { return mQuoteMode; }
			set { mQuoteMode = value; }
		}


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private MultilineMode mQuoteMultiline = MultilineMode.AllowForRead;

		/// <summary>Indicates if this quoted field can span multiple lines. (only used when FieldQuoted is true)</summary>
		public MultilineMode QuoteMultiline
		{
			get { return mQuoteMultiline; }
			set { mQuoteMultiline = value; }
		}



		internal override void AddAttributesCode(AttributesBuilder attbs, NetLanguage leng)
		{
			
			if (mFieldQuoted == true)
			{
				if (leng == NetLanguage.CSharp)
				{
					string quoteStr = mQuoteChar.ToString();
					if (mQuoteChar == '\'') quoteStr = @"\'";

					attbs.AddAttribute("FieldQuoted('" + quoteStr + "', QuoteMode." + mQuoteMode.ToString()+", MultilineMode." + mQuoteMultiline.ToString() +")");
				}
				else if (leng == NetLanguage.VbNet)
				{
					string quoteStr = mQuoteChar.ToString();
					if (mQuoteChar == '"') quoteStr = "\"\"";

					attbs.AddAttribute("FieldQuoted(\"" + quoteStr + "\"c, QuoteMode." + mQuoteMode.ToString()+", MultilineMode." + mQuoteMultiline.ToString() +")");
				}
			}
			
		}

		internal override void WriteHeaderAttributes(XmlHelper writer)
		{
		}

		internal override void WriteExtraElements(XmlHelper writer)
		{
			writer.WriteElement("FieldQuoted", this.FieldQuoted);
			writer.WriteElement("QuoteChar", this.QuoteChar.ToString(), "\"");
			writer.WriteElement("QuoteMode", this.QuoteMode.ToString(), "OptionalForRead");
			writer.WriteElement("QuoteMultiline", this.QuoteMultiline.ToString(), "AllowForRead");
		}

		internal override void ReadFieldInternal(XmlNode node)
		{
			XmlNode ele;
			
			FieldQuoted = node["FieldQuoted"] != null;
			
			ele = node["QuoteChar"];
            
			if (ele != null && ele.InnerText.Length > 0) QuoteChar = ele.InnerText[0];

			ele = node["QuoteMode"];
			if (ele != null && ele.InnerText.Length > 0) QuoteMode = (QuoteMode) Enum.Parse(typeof(QuoteMode), ele.InnerText);

			ele = node["QuoteMultiline"];
            if (ele != null && ele.InnerText.Length > 0) QuoteMultiline = (MultilineMode)Enum.Parse(typeof(MultilineMode), ele.InnerText);

		}
	}
}
