#region "  � Copyright 2005-07 to Marcos Meli - http://www.devoo.net"

// Errors, suggestions, contributions, send a mail to: marcos@filehelpers.com.

#endregion

using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Text;
using System.Xml;

namespace FileHelpers.RunTime
{
	/// <summary>Base class for the field converters</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class FieldBuilder
	{

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string mFieldName;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string mFieldType;

		internal FieldBuilder(string fieldName, Type fieldType)
		{
			fieldName = fieldName.Trim();
			
			if (ValidIdentifierValidator.ValidIdentifier(fieldName) == false)
				throw new FileHelpersException(string.Format(ClassBuilder.sInvalidIdentifier, fieldName));
			
			mFieldName = fieldName;
			mFieldType = ClassBuilder.TypeToString(fieldType);
		}

		internal FieldBuilder(string fieldName, string fieldType)
		{
			fieldName = fieldName.Trim();

			if (ValidIdentifierValidator.ValidIdentifier(fieldName) == false)
				throw new FileHelpersException(string.Format(ClassBuilder.sInvalidIdentifier, fieldName));

			if (ValidIdentifierValidator.ValidIdentifier(fieldType, true) == false)
				throw new FileHelpersException(string.Format(ClassBuilder.sInvalidIdentifier, fieldType));

			mFieldName = fieldName;
			mFieldType = fieldType;
		}

		#region TrimMode

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrimMode mTrimMode = TrimMode.None;

		/// <summary>Indicates the TrimMode for the field.</summary>
		public TrimMode TrimMode
		{
			get { return mTrimMode; }
			set { mTrimMode = value; }
		}

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string mTrimChars = " \t";
		
		/// <summary>Indicates the trim chars used if TrimMode is set.</summary>
		public string TrimChars
		{
			get { return mTrimChars; }
			set { mTrimChars = value; }
		}

		#endregion

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal int mFieldIndex = -1;

		/// <summary>The position index inside the class.</summary>
		public int FieldIndex
		{
			get { return mFieldIndex; }
		}


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool mFieldInNewLine = false;

		/// <summary>Indicates that this field is at the begging of a new line.</summary>
		public bool FieldInNewLine
		{
			get { return mFieldInNewLine; }
			set { mFieldInNewLine = value; }
		}

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool mFieldIgnored = false;

		/// <summary>Indicates that this field must be ignored by the engine.</summary>
		public bool FieldIgnored
		{
			get { return mFieldIgnored; }
			set { mFieldIgnored = value; }
		}

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool mFieldOptional = false;

		/// <summary>Indicates that this field is optional.</summary>
		public bool FieldOptional
		{
			get { return mFieldOptional; }
			set { mFieldOptional = value; }
		}

		/// <summary>Uset to create the converter for the current field.</summary>
		public ConverterBuilder Converter
		{
			get { return mConverter; }
		}

		/// <summary>The name of the field.</summary>
		public string FieldName
		{
			get { return mFieldName; }
			set { mFieldName = value; }
		}

		/// <summary>The Type of the field</summary>
		public string FieldType
		{
			get { return mFieldType; }
			set { mFieldType = value; }
		}


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object mFieldNullValue = null;

		/// <summary>The null value of the field when their value not is in the file.</summary>
		public object FieldNullValue
		{
			get { return mFieldNullValue; }
			set { mFieldNullValue = value; }
		}


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ConverterBuilder mConverter = new ConverterBuilder();
		
		internal string GetFieldCode(NetLanguage lang)
		{
			StringBuilder sb = new StringBuilder(100);
			
			AttributesBuilder attbs = new AttributesBuilder(lang);
			
			AddAttributesInternal(attbs, lang);
			AddAttributesCode(attbs, lang);
			
			sb.Append(attbs.GetAttributesCode());
			
			NetVisibility visi = mVisibility;
			string currentName = mFieldName;
			
			if (mClassBuilder.GenerateProperties)
			{
				visi = NetVisibility.Private;
				currentName = "m" + mFieldName;
			}
			
			switch (lang)
			{
				case NetLanguage.VbNet:
					sb.Append(ClassBuilder.GetVisibility(lang, visi) + currentName + " As " + mFieldType);
					break;
				case NetLanguage.CSharp:
					sb.Append(ClassBuilder.GetVisibility(lang, visi) + mFieldType + " " + currentName+ ";");
					break;
				default:
					break;
			}

			sb.Append(StringHelper.NewLine);
			
			if (mClassBuilder.GenerateProperties)
			{
				sb.Append(StringHelper.NewLine);
				
				switch (lang)
				{
					case NetLanguage.VbNet:
						sb.Append("Public Property " + mFieldName + " As " + mFieldType);
						sb.Append(StringHelper.NewLine);
						sb.Append("   Get");
						sb.Append(StringHelper.NewLine);
						sb.Append("      Return m" + mFieldName);
						sb.Append(StringHelper.NewLine);
						sb.Append("   End Get");
						sb.Append(StringHelper.NewLine);
						sb.Append("   Set (value As " + mFieldType + ")");
						sb.Append(StringHelper.NewLine);
						sb.Append("      m" + mFieldName + " = value");
						sb.Append(StringHelper.NewLine);
						sb.Append("   End Set");
						sb.Append(StringHelper.NewLine);
						sb.Append("End Property");
						break;
					case NetLanguage.CSharp:
						sb.Append("public " + mFieldType + " " + mFieldName);
						sb.Append(StringHelper.NewLine);
						sb.Append("{");
						sb.Append(StringHelper.NewLine);
						sb.Append("   get { return m" + mFieldName + "; }");
						sb.Append(StringHelper.NewLine);
						sb.Append("   set { m" + mFieldName + " = value; }");
						sb.Append(StringHelper.NewLine);
						sb.Append("}");
						break;
					default:
						break;
				}

				sb.Append(StringHelper.NewLine);
				sb.Append(StringHelper.NewLine);
			}
			
			return sb.ToString();
		}
		
		
		internal abstract void AddAttributesCode(AttributesBuilder attbs, NetLanguage leng);

		private void AddAttributesInternal(AttributesBuilder attbs, NetLanguage leng)
		{

			if (mFieldOptional == true)
				attbs.AddAttribute("FieldOptional()");

			if (mFieldIgnored == true)
				attbs.AddAttribute("FieldIgnored()");

			if (mFieldInNewLine == true)
				attbs.AddAttribute("FieldInNewLine()");


			if (mFieldNullValue != null)
			{
				if (mFieldNullValue is string)
					attbs.AddAttribute("FieldNullValue(\"" + mFieldNullValue.ToString() + "\")");
				else
				{
					string t = ClassBuilder.TypeToString(mFieldNullValue.GetType());
					string gt = string.Empty;
					if (leng == NetLanguage.CSharp)
						gt = "typeof(" + t + ")";
					else if (leng == NetLanguage.VbNet)
						gt = "GetType(" + t + ")";

					attbs.AddAttribute("FieldNullValue("+ gt +", \""+ mFieldNullValue.ToString() +"\")");
				}
			}


			attbs.AddAttribute(mConverter.GetConverterCode(leng));
			
			if (mTrimMode != TrimMode.None)
			{
                if (" \t" == mTrimChars)
				    attbs.AddAttribute("FieldTrim(TrimMode."+ mTrimMode.ToString()+")");
                else
                    attbs.AddAttribute("FieldTrim(TrimMode." + mTrimMode.ToString() + ", \"" + mTrimChars.ToString() + "\")");
			}
		}


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal ClassBuilder mClassBuilder;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NetVisibility mVisibility = NetVisibility.Public;

		/// <summary>
		/// Gets or sets the visibility of the field.
		/// </summary>
		public NetVisibility Visibility
		{
			get { return mVisibility; }
			set { mVisibility = value; }
		}

		internal void SaveToXml(XmlHelper writer)
		{
			writer.mWriter.WriteStartElement("Field");
			writer.mWriter.WriteStartAttribute("Name", "");
			writer.mWriter.WriteString(mFieldName);
			writer.mWriter.WriteEndAttribute();
			writer.mWriter.WriteStartAttribute("Type", "");
			writer.mWriter.WriteString(mFieldType);
			writer.mWriter.WriteEndAttribute();
			WriteHeaderAttributes(writer);

			Converter.WriteXml(writer);

			writer.WriteElement("Visibility", this.Visibility.ToString(), "Public");
			writer.WriteElement("FieldIgnored", this.FieldIgnored);
			writer.WriteElement("FieldOptional", this.FieldOptional);
			writer.WriteElement("FieldInNewLine", this.FieldInNewLine);
			writer.WriteElement("TrimChars", this.TrimChars, " \t");
			writer.WriteElement("TrimMode", this.TrimMode.ToString(), "None");

			if (FieldNullValue != null)
			{
				writer.mWriter.WriteStartElement("FieldNullValue");
				writer.mWriter.WriteStartAttribute("Type", "");
				writer.mWriter.WriteString(ClassBuilder.TypeToString(mFieldNullValue.GetType()));
				writer.mWriter.WriteEndAttribute();
				
				writer.mWriter.WriteString(mFieldNullValue.ToString());

				writer.mWriter.WriteEndElement();				
			}

			WriteExtraElements(writer);
			writer.mWriter.WriteEndElement();

		}

		internal abstract void WriteHeaderAttributes(XmlHelper writer);
		internal abstract void WriteExtraElements(XmlHelper writer);

		internal void ReadField(XmlNode node)
		{
			XmlNode ele;
			
			ele = node["Visibility"];
			if (ele != null) Visibility = (NetVisibility) Enum.Parse(typeof(NetVisibility), ele.InnerText);

			FieldIgnored = node["FieldIgnored"] != null;
			FieldOptional = node["FieldOptional"] != null;
			FieldInNewLine = node["FieldInNewLine"] != null;
			
			ele = node["TrimChars"];
			if (ele != null) TrimChars = ele.InnerText;

			ele = node["TrimMode"];
			if (ele != null) TrimMode = (TrimMode) Enum.Parse(typeof(TrimMode), ele.InnerText);
			
			ele = node["FieldNullValue"];
			if (ele != null) FieldNullValue = Convert.ChangeType(ele.InnerText, Type.GetType(ele.Attributes["Type"].InnerText));

			ele = node["Converter"];
			if (ele != null) Converter.LoadXml(ele);

			
			ReadFieldInternal(node);
		}

		internal abstract void ReadFieldInternal(XmlNode node);

	}
}
