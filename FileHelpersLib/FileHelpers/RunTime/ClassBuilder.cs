#region "  � Copyright 2005-07 to Marcos Meli - http://www.devoo.net"

// Errors, suggestions, contributions, send a mail to: marcos@filehelpers.com.

#endregion

using System;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;
using System.Xml;

using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System.Security.Cryptography;
using System.Collections.Specialized;

namespace FileHelpers.RunTime
{

    //-> REGIONS !!!!

    /// <summary>The MAIN class to work with runtime defined records.</summary>
    public abstract class ClassBuilder
    {
        //---------------------
        //->  STATIC METHODS

        #region LoadFromString

        /// <summary>Compiles the source code passed and returns the FIRST Type of the assembly. (Code in C#)</summary>
        /// <param name="classStr">The Source Code of the class in C#</param>
        /// <returns>The Type generated by runtime compilation of the class source.</returns>
        public static Type ClassFromString(string classStr)
        {
            return ClassFromString(classStr, string.Empty);
        }

        /// <summary>Compiles the source code passed and returns the FIRST Type of the assembly.</summary>
        /// <param name="classStr">The Source Code of the class in the specified language</param>
        /// <returns>The Type generated by runtime compilation of the class source.</returns>
        /// <param name="lang">One of the .NET Languages</param>
        public static Type ClassFromString(string classStr, NetLanguage lang)
        {
            return ClassFromString(classStr, string.Empty, lang);
        }

        /// <summary>Compiles the source code passed and returns the Type with the name className. (Code in C#)</summary>
        /// <param name="classStr">The Source Code of the class in C#</param>
        /// <param name="className">The Name of the Type that must be returned</param>
        /// <returns>The Type generated by runtime compilation of the class source.</returns>
        public static Type ClassFromString(string classStr, string className)
        {
            return ClassFromString(classStr, className, NetLanguage.CSharp);
        }

        private static String[] mReferences;
        private static object mReferencesLock = new object();

        /// <summary>Compiles the source code passed and returns the Type with the name className.</summary>
        /// <param name="classStr">The Source Code of the class in the specified language</param>
        /// <param name="className">The Name of the Type that must be returned</param>
        /// <returns>The Type generated by runtime compilation of the class source.</returns>
        /// <param name="lang">One of the .NET Languages</param>
        public static Type ClassFromString(string classStr, string className, NetLanguage lang)
        {

            CompilerParameters cp = new CompilerParameters();
            //cp.ReferencedAssemblies.Add("System.dll");
            //cp.ReferencedAssemblies.Add("System.Data.dll");
            //cp.ReferencedAssemblies.Add(typeof(ClassBuilder).Assembly.GetModules()[0].FullyQualifiedName);

            bool mustAddSystemData = false;
            lock (mReferencesLock)
            {
                if (mReferences == null)
                {
                     ArrayList arr = new ArrayList();

                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        Module module = assembly.GetModules()[0];
                        if (module.Name == "mscorlib.dll" || module.Name == "<Unknown>")
                            continue;

                        if (module.Name == "System.Data.dll")
                            mustAddSystemData = true;

                        if (File.Exists(module.FullyQualifiedName)) 
                            arr.Add(module.FullyQualifiedName);
                    }

                    mReferences = (string[]) arr.ToArray(typeof (string));
                }
            }

            cp.ReferencedAssemblies.AddRange(mReferences);

            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;
            cp.IncludeDebugInformation = false;

            StringBuilder code = new StringBuilder();

            switch (lang)
            {
                case NetLanguage.CSharp:
                    code.Append("using System; using FileHelpers;");
                    if (mustAddSystemData) code.Append(" using System.Data;");
                    break;

                case NetLanguage.VbNet:
                    
                    if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(classStr, "Imports System", CompareOptions.IgnoreCase) == -1)
                        code.Append("Imports System\n");

                    if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(classStr, "Imports FileHelpers", CompareOptions.IgnoreCase) == -1)
                        code.Append("Imports FileHelpers\n");

                    if (mustAddSystemData && CultureInfo.CurrentCulture.CompareInfo.IndexOf(classStr, "Imports System.Data", CompareOptions.IgnoreCase) == -1)
                        code.Append("Imports System.Data\n");

                    break;
            }

            code.Append(classStr);

            CompilerResults cr;

            CodeDomProvider prov = null;

            switch (lang)
            {
                case NetLanguage.CSharp:
                    prov = CodeDomProvider.CreateProvider("cs");
                    break;

                case NetLanguage.VbNet:
                    prov = CodeDomProvider.CreateProvider("vb");
                    break;
            }

            cr = prov.CompileAssemblyFromSource(cp, code.ToString());

            if (cr.Errors.HasErrors)
            {
                StringBuilder error = new StringBuilder();
                error.Append("Error Compiling Expression: " + StringHelper.NewLine);
                foreach (CompilerError err in cr.Errors)
                {
                    error.AppendFormat("Line {0}: {1}\n", err.Line, err.ErrorText);
                }
                throw new RunTimeCompilationException(error.ToString(), classStr, cr.Errors);
            }

            //            Assembly.Load(cr.CompiledAssembly.);
            if (className != string.Empty)
                return cr.CompiledAssembly.GetType(className, true, true);
            else
            {
                Type[] ts = cr.CompiledAssembly.GetTypes();
                if (ts.Length > 0)
                    foreach (Type t in ts)
                    {
                        if (t.FullName.StartsWith("My.My") == false && t.IsDefined(typeof(TypedRecordAttribute), false))
                            return t;
                    }

                throw new BadUsageException("The Compiled assembly don�t have any Type inside.");
            }
        }

        #endregion

        #region CreateFromFile

        /// <summary>
        /// Create a class from a source file.
        /// </summary>
        /// <param name="filename">The filename with the source of the class.</param>
        /// <returns>The compiled class.</returns>
        public static Type ClassFromSourceFile(string filename)
        {
            return ClassFromSourceFile(filename, string.Empty);
        }

        /// <summary>
        /// Create a class from a source file.
        /// </summary>
        /// <param name="filename">The filename with the source of the class.</param>
        /// <param name="lang">The languaje used to compile the class.</param>
        /// <returns>The compiled class.</returns>
        public static Type ClassFromSourceFile(string filename, NetLanguage lang)
        {
            return ClassFromSourceFile(filename, string.Empty, lang);
        }

        /// <summary>
        /// Create a class from a source file.
        /// </summary>
        /// <param name="filename">The filename with the source of the class.</param>
        /// <param name="className">The name of the class to return.</param>
        /// <returns>The compiled class.</returns>
        public static Type ClassFromSourceFile(string filename, string className)
        {
            return ClassFromSourceFile(filename, className, NetLanguage.CSharp);
        }

        /// <summary>
        /// Create a class from a source file.
        /// </summary>
        /// <param name="filename">The filename with the source of the class.</param>
        /// <param name="className">The name of the class to return.</param>
        /// <param name="lang">The languaje used to compile the class.</param>
        /// <returns>The compiled class.</returns>
        public static Type ClassFromSourceFile(string filename, string className, NetLanguage lang)
        {
            StreamReader reader = new StreamReader(filename);
            string classDef = reader.ReadToEnd();
            reader.Close();

            return ClassFromString(classDef, className, lang);
        }



        /// <summary>
        /// Create a class from a encripted source file.
        /// </summary>
        /// <param name="filename">The filename with the source of the class.</param>
        /// <returns>The compiled class.</returns>
        public static Type ClassFromBinaryFile(string filename)
        {
            return ClassFromBinaryFile(filename, string.Empty, NetLanguage.CSharp);
        }

        /// <summary>
        /// Create a class from a encripted source file.
        /// </summary>
        /// <param name="filename">The filename with the source of the class.</param>
        /// <param name="lang">The languaje used to compile the class.</param>
        /// <returns>The compiled class.</returns>
        public static Type ClassFromBinaryFile(string filename, NetLanguage lang)
        {
            return ClassFromBinaryFile(filename, string.Empty, lang);
        }

        /// <summary>
        /// Create a class from a encripted source file.
        /// </summary>
        /// <param name="filename">The filename with the source of the class.</param>
        /// <param name="lang">The languaje used to compile the class.</param>
        /// <param name="className">The name of the class to return.</param>
        /// <returns>The compiled class.</returns>
        public static Type ClassFromBinaryFile(string filename, string className, NetLanguage lang)
        {

            StreamReader reader = new StreamReader(filename);
            string classDef = reader.ReadToEnd();
            reader.Close();

            classDef = Decrypt(classDef, "withthefilehelpers1.0.0youcancodewithoutproblems1.5.0");

            return ClassFromString(classDef, className, lang);
        }


        /// <summary>
        /// Create a class from a Xml file generated with the Wizard or saved using the SaveToXml Method.
        /// </summary>
        /// <param name="filename">The filename with the Xml definition.</param>
        /// <returns>The compiled class.</returns>
        public static Type ClassFromXmlFile(string filename)
        {
            ClassBuilder cb = LoadFromXml(filename);
            return cb.CreateRecordClass();
        }

        /// <summary>
        /// Encript the class source code and write it to a file.
        /// </summary>
        /// <param name="filename">The file name to write to.</param>
        /// <param name="classSource">The source code for the class.</param>
        public static void ClassToBinaryFile(string filename, string classSource)
        {
            classSource = Encrypt(classSource, "withthefilehelpers1.0.0youcancodewithoutproblems1.5.0");

            StreamWriter writer = new StreamWriter(filename);
            writer.Write(classSource);
            writer.Close();
        }


        #endregion

        #region SaveToFile


        /// <summary>Write the source code of the current class to a file. (In C#)</summary>
        /// <param name="filename">The file to write to.</param>
        public void SaveToSourceFile(string filename)
        {
            SaveToSourceFile(filename, NetLanguage.CSharp);
        }

        /// <summary>Write the source code of the current class to a file. (In the especified language)</summary>
        /// <param name="filename">The file to write to.</param>
        /// <param name="lang">The .NET Language used to write the source code.</param>
        public void SaveToSourceFile(string filename, NetLanguage lang)
        {
            StreamWriter writer = new StreamWriter(filename);
            writer.Write(GetClassSourceCode(lang));
            writer.Close();
        }

        /// <summary>Write the ENCRIPTED source code of the current class to a file. (In C#)</summary>
        /// <param name="filename">The file to write to.</param>
        public void SaveToBinaryFile(string filename)
        {
            SaveToBinaryFile(filename, NetLanguage.CSharp);
        }

        /// <summary>Write the ENCRIPTED source code of the current class to a file. (In C#)</summary>
        /// <param name="filename">The file to write to.</param>
        /// <param name="lang">The .NET Language used to write the source code.</param>
        public void SaveToBinaryFile(string filename, NetLanguage lang)
        {
            StreamWriter writer = new StreamWriter(filename);
            writer.Write(GetClassBinaryCode(lang));
            writer.Close();
        }

        #endregion

        internal ClassBuilder(string className)
        {
            className = className.Trim();
            if (ValidIdentifierValidator.ValidIdentifier(className) == false)
                throw new FileHelpersException(string.Format(sInvalidIdentifier, className));

            mClassName = className;
        }

        /// <summary>Generate the runtime record class to be used by the engines.</summary>
        /// <returns>The generated record class</returns>
        public Type CreateRecordClass()
        {
            string classCode = GetClassSourceCode(NetLanguage.CSharp);
            return ClassFromString(classCode, NetLanguage.CSharp);
        }


        //--------------
        //->  Fields 

        #region Fields

        /// <summary>Removes all the Fields of the current class.</summary>
        public void ClearFields()
        {
            mFields.Clear();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal ArrayList mFields = new ArrayList();

        internal void AddFieldInternal(FieldBuilder field)
        {
            field.mFieldIndex = mFields.Add(field);
            field.mClassBuilder = this;
        }

        /// <summary>Returns the current fields of the class.</summary>
        public FieldBuilder[] Fields
        {
            get { return (FieldBuilder[])mFields.ToArray(typeof(FieldBuilder)); }
        }

        /// <summary>Returns the current number of fields.</summary>
        public int FieldCount
        {
            get
            {
                return mFields.Count;
            }
        }


        /// <summary>Return the field at the specified index.</summary>
        /// <param name="index">The index of the field.</param>
        /// <returns>The field at the specified index.</returns>
        public FieldBuilder FieldByIndex(int index)
        {
            return (FieldBuilder)mFields[index];
        }

        #endregion

        #region ClassName


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string mClassName;
        /// <summary>The name of the Class.</summary>
        public string ClassName
        {
            get { return mClassName; }
            set { mClassName = value; }
        }

        #endregion


        //----------------------------
        //->  ATTRIBUTE MAPPING

        #region IgnoreFirstLines


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int mIgnoreFirstLines = 0;

        /// <summary>Indicates the number of FIRST LINES to be ignored by the engines.</summary>
        public int IgnoreFirstLines
        {
            get { return mIgnoreFirstLines; }
            set { mIgnoreFirstLines = value; }
        }

        #endregion

        #region IgnoreLastLines


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int mIgnoreLastLines = 0;

        /// <summary>Indicates the number of LAST LINES to be ignored by the engines.</summary>
        public int IgnoreLastLines
        {
            get { return mIgnoreLastLines; }
            set { mIgnoreLastLines = value; }
        }

        #endregion

        #region IgnoreEmptyLines


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool mIgnoreEmptyLines = false;

        /// <summary>Indicates that the engines must ignore the empty lines in the files.</summary>
        public bool IgnoreEmptyLines
        {
            get { return mIgnoreEmptyLines; }
            set { mIgnoreEmptyLines = value; }
        }

        #endregion


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool mGenerateProperties = false;

        /// <summary>Indicates if this ClassBuilder generates also the property accessors (Perfect for DataBinding)</summary>
        public bool GenerateProperties
        {
            get { return mGenerateProperties; }
            set { mGenerateProperties = value; }
        }


        /// <summary>
        /// Returns the ENCRIPTED code for the current class in the specified language.
        /// </summary>
        /// <param name="lang">The language for the return code.</param>
        /// <returns>The ENCRIPTED code for the class that are currently building.</returns>
        public string GetClassBinaryCode(NetLanguage lang)
        {
            return Encrypt(GetClassSourceCode(lang), "withthefilehelpers1.0.0youcancodewithoutproblems1.5.0");
        }

        /// <summary>
        /// Returns the source code for the current class in the specified language.
        /// </summary>
        /// <param name="lang">The language for the return code.</param>
        /// <returns>The Source Code for the class that are currently building.</returns>
        public string GetClassSourceCode(NetLanguage lang)
        {
            ValidateClass();

            StringBuilder sb = new StringBuilder(100);

            BeginNamespace(lang, sb);

            AttributesBuilder attbs = new AttributesBuilder(lang);

            AddAttributesInternal(attbs);
            AddAttributesCode(attbs);

            sb.Append(attbs.GetAttributesCode());

            switch (lang)
            {
                case NetLanguage.VbNet:
                    sb.Append(GetVisibility(lang, mVisibility) + GetSealed(lang) + "Class " + mClassName);
                    sb.Append(StringHelper.NewLine);
                    break;
                case NetLanguage.CSharp:
                    sb.Append(GetVisibility(lang, mVisibility) + GetSealed(lang) + "class " + mClassName);
                    sb.Append(StringHelper.NewLine);
                    sb.Append("{");
                    break;
            }

            sb.Append(StringHelper.NewLine);
            sb.Append(StringHelper.NewLine);

            foreach (FieldBuilder field in mFields)
            {
                sb.Append(field.GetFieldCode(lang));
                sb.Append(StringHelper.NewLine);
            }


            sb.Append(StringHelper.NewLine);

            switch (lang)
            {
                case NetLanguage.VbNet:
                    sb.Append("End Class");
                    break;
                case NetLanguage.CSharp:
                    sb.Append("}");
                    break;
            }

            EndNamespace(lang, sb);

            return sb.ToString();


        }

        private void ValidateClass()
        {
            
            if (ClassName.Trim().Length == 0)
                throw new FileHelpersException("The ClassName can't be empty");

            for (int i = 0; i < mFields.Count; i++)
            {

                if (((FieldBuilder) mFields[i]).FieldName.Trim().Length == 0)
                    throw new FileHelpersException("The " + (i+1).ToString() +  "th field name can't be empty");

                if (((FieldBuilder)mFields[i]).FieldType.Trim().Length == 0)
                    throw new FileHelpersException("The " + (i + 1).ToString() + "th field type can't be empty");

            }
        }

        internal abstract void AddAttributesCode(AttributesBuilder attbs);

        private void AddAttributesInternal(AttributesBuilder attbs)
        {

            if (mIgnoreFirstLines != 0)
                attbs.AddAttribute("IgnoreFirst(" + mIgnoreFirstLines.ToString() + ")");

            if (mIgnoreLastLines != 0)
                attbs.AddAttribute("IgnoreLast(" + mIgnoreLastLines.ToString() + ")");

            if (mIgnoreEmptyLines == true)
                attbs.AddAttribute("IgnoreEmptyLines()");

            if (mRecordConditionInfo.Condition != FileHelpers.RecordCondition.None)
                attbs.AddAttribute("ConditionalRecord(RecordCondition." + mRecordConditionInfo.Condition.ToString() + ", \"" + mRecordConditionInfo.Selector + "\")");

            if (mIgnoreCommentInfo.CommentMarker != null && mIgnoreCommentInfo.CommentMarker.Length > 0)
                attbs.AddAttribute("IgnoreCommentedLines(\"" + mIgnoreCommentInfo.CommentMarker + "\", " + mIgnoreCommentInfo.InAnyPlace.ToString().ToLower() + ")");

        }


        #region "  EncDec  "

        private static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms,
                alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearData, 0, clearData.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }

        private static string Encrypt(string clearText, string Password)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
                new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 
							   0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});
            byte[] encryptedData = Encrypt(clearBytes,
                pdb.GetBytes(32), pdb.GetBytes(16));
            return Convert.ToBase64String(encryptedData);
        }


        // Decrypt a byte array into a byte array using a key and an IV 
        private static byte[] Decrypt(byte[] cipherData,
            byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;

            CryptoStream cs = new CryptoStream(ms,
                alg.CreateDecryptor(), CryptoStreamMode.Write);

            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();

            byte[] decryptedData = ms.ToArray();

            return decryptedData;
        }

        private static string Decrypt(string cipherText, string Password)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
                new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 
							   0x64, 0x76, 0x65, 0x64, 0x65, 0x76});
            byte[] decryptedData = Decrypt(cipherBytes,
                pdb.GetBytes(32), pdb.GetBytes(16));
            return Encoding.Unicode.GetString(decryptedData);
        }



        #endregion




        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NetVisibility mVisibility = NetVisibility.Public;

        /// <summary>The Visibility for the class.</summary>
        public NetVisibility Visibility
        {
            get { return mVisibility; }
            set { mVisibility = value; }
        }


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool mSealedClass = true;

        /// <summary>Indicates if the generated class must be sealed.</summary>
        public bool SealedClass
        {
            get { return mSealedClass; }
            set { mSealedClass = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string mNamespace = string.Empty;

        /// <summary>The namespace used when creating the class.</summary>
        public string Namespace
        {
            get { return mNamespace; }
            set { mNamespace = value; }
        }


        internal static string GetVisibility(NetLanguage lang, NetVisibility visibility)
        {
            switch (lang)
            {
                case NetLanguage.CSharp:
                    switch (visibility)
                    {
                        case NetVisibility.Public:
                            return "public ";
                        case NetVisibility.Private:
                            return "private ";
                        case NetVisibility.Internal:
                            return "internal ";
                        case NetVisibility.Protected:
                            return "protected ";
                    }
                    break;

                case NetLanguage.VbNet:
                    switch (visibility)
                    {
                        case NetVisibility.Public:
                            return "Public ";
                        case NetVisibility.Private:
                            return "Private ";
                        case NetVisibility.Internal:
                            return "Friend ";
                        case NetVisibility.Protected:
                            return "Protected ";
                    }
                    break;
            }

            return string.Empty;
        }

        private string GetSealed(NetLanguage lang)
        {
            if (mSealedClass == false)
                return string.Empty;

            switch (lang)
            {
                case NetLanguage.CSharp:
                    return "sealed ";

                case NetLanguage.VbNet:
                    return "NotInheritable ";
            }

            return string.Empty;
        }

        private void BeginNamespace(NetLanguage lang, StringBuilder sb)
        {
            if (mNamespace == string.Empty)
                return;

            switch (lang)
            {
                case NetLanguage.CSharp:
                    sb.Append("namespace ");
                    sb.Append(mNamespace);
                    sb.Append(StringHelper.NewLine);
                    sb.Append("{");
                    break;

                case NetLanguage.VbNet:
                    sb.Append("Namespace ");
                    sb.Append(mNamespace);
                    sb.Append(StringHelper.NewLine);
                    break;
            }

            sb.Append(StringHelper.NewLine);
        }

        private void EndNamespace(NetLanguage lang, StringBuilder sb)
        {
            if (mNamespace == string.Empty)
                return;

            sb.Append(StringHelper.NewLine);

            switch (lang)
            {
                case NetLanguage.CSharp:
                    sb.Append("}");
                    break;

                case NetLanguage.VbNet:
                    sb.Append("End Namespace");
                    break;
            }
        }


        /// <summary>
        /// Loads the XML representation of a a ClassBuilder inheritor and return it. (for XML saved with SaveToXml method)
        /// </summary>
        /// <remarks>
        /// ClassBuilder inheritors: <see cref="DelimitedClassBuilder"/> or <see cref="FixedLengthClassBuilder"/>
        /// </remarks>
        /// <param name="xml">The XML representation of the record class.</param>
        /// <returns>A new instance of a ClassBuilder inheritor: <see cref="DelimitedClassBuilder"/> or <see cref="FixedLengthClassBuilder"/> </returns>
        public static ClassBuilder LoadFromXmlString(string xml)
        {
            XmlDocument document = new XmlDocument();
            document.Load(new StringReader(xml));

            return LoadFromXml(document);
        }

        /// <summary>
        /// Loads the XML representation of a a ClassBuilder inheritor and return it. (for XML saved with SaveToXml method)
        /// </summary>
        /// <remarks>
        /// ClassBuilder inheritors: <see cref="DelimitedClassBuilder"/> or <see cref="FixedLengthClassBuilder"/>
        /// </remarks>
        /// <param name="document">The XML document with the representation of the record class.</param>
        /// <returns>A new instance of a ClassBuilder inheritor: <see cref="DelimitedClassBuilder"/> or <see cref="FixedLengthClassBuilder"/> </returns>
        public static ClassBuilder LoadFromXml(XmlDocument document)
        {
            ClassBuilder res = null;

            string classtype = document.DocumentElement.LocalName;

            if (classtype == "DelimitedClass")
                res = DelimitedClassBuilder.LoadXmlInternal(document);
            else
                res = FixedLengthClassBuilder.LoadXmlInternal(document);

            XmlNode node = document.DocumentElement["IgnoreLastLines"];
            if (node != null) res.IgnoreLastLines = int.Parse(node.InnerText);

            node = document.DocumentElement["IgnoreFirstLines"];
            if (node != null) res.IgnoreFirstLines = int.Parse(node.InnerText);

            node = document.DocumentElement["IgnoreEmptyLines"];
            if (node != null) res.IgnoreEmptyLines = true;

            node = document.DocumentElement["CommentMarker"];
            if (node != null) res.IgnoreCommentedLines.CommentMarker = node.InnerText;

            node = document.DocumentElement["CommentInAnyPlace"];
            if (node != null) res.IgnoreCommentedLines.InAnyPlace = bool.Parse(node.InnerText.ToLower());

            node = document.DocumentElement["SealedClass"];
            res.SealedClass = node != null;

            node = document.DocumentElement["Namespace"];
            if (node != null) res.Namespace = node.InnerText;

            node = document.DocumentElement["Visibility"];
            if (node != null) res.Visibility = (NetVisibility)Enum.Parse(typeof(NetVisibility), node.InnerText); ;

            node = document.DocumentElement["RecordCondition"];
            if (node != null) res.RecordCondition.Condition = (RecordCondition)Enum.Parse(typeof(RecordCondition), node.InnerText); ;

            node = document.DocumentElement["RecordConditionSelector"];
            if (node != null) res.RecordCondition.Selector = node.InnerText;

            res.ReadClassElements(document);

            node = document.DocumentElement["Fields"];
            XmlNodeList nodes;

            if (classtype == "DelimitedClass")
                nodes = node.SelectNodes("/DelimitedClass/Fields/Field");
            else
                nodes = node.SelectNodes("/FixedLengthClass/Fields/Field");

            foreach (XmlNode n in nodes)
            {
                res.ReadField(n);
            }

            return res;


        }

        /// <summary>
        /// Loads the XML representation of a a ClassBuilder inheritor and return it. (for XML saved with SaveToXml method)
        /// </summary>
        /// <remarks>
        /// ClassBuilder inheritors: <see cref="DelimitedClassBuilder"/> or <see cref="FixedLengthClassBuilder"/>
        /// </remarks>
        /// <param name="filename">A file with the Xml representation of the record class.</param>
        /// <returns>A new instance of a ClassBuilder inheritor: <see cref="DelimitedClassBuilder"/> or <see cref="FixedLengthClassBuilder"/> </returns>
        public static ClassBuilder LoadFromXml(string filename)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filename);

            return LoadFromXml(document);
        }


        /// <summary>
        /// Creates the XML representation of the current record class.
        /// </summary>
        public string SaveToXmlString()
        {
            StringBuilder sb = new StringBuilder();

            using (StringWriter writer = new StringWriter(sb))
            {
                SaveToXml(writer);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Saves to a file the XML representation of the current record class.
        /// </summary>
        /// <param name="filename">A file name to write to.</param>
        public void SaveToXml(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Create))
            {
                SaveToXml(stream);
            }
        }



        /// <summary>
        /// Saves to an Stream the XML representation of the current record class.
        /// </summary>
        /// <param name="stream">Stream to be written.</param>
        public void SaveToXml(Stream stream)
        {
            using (TextWriter writer = new StreamWriter(stream))
            {
                SaveToXml(writer);
            }
        }

        /// <summary>
        /// Save to a TextWriter the XML representation of the current record class.
        /// </summary>
        /// <param name="writer">The TextWriter for the output Stream.</param>
        public void SaveToXml(TextWriter writer)
        {
            XmlHelper xml = new XmlHelper();

            xml.BeginWriteStream(writer);

            WriteHeaderElement(xml);

            xml.WriteElement("ClassName", ClassName);
            xml.WriteElement("Namespace", this.Namespace, string.Empty);

            xml.WriteElement("SealedClass", this.SealedClass);
            xml.WriteElement("Visibility", this.Visibility.ToString(), "Public");

            xml.WriteElement("IgnoreEmptyLines", this.IgnoreEmptyLines);
            xml.WriteElement("IgnoreFirstLines", this.IgnoreFirstLines.ToString(), "0");
            xml.WriteElement("IgnoreLastLines", this.IgnoreLastLines.ToString(), "0");

            xml.WriteElement("CommentMarker", this.IgnoreCommentedLines.CommentMarker, string.Empty);
            xml.WriteElement("CommentInAnyPlace", this.IgnoreCommentedLines.InAnyPlace.ToString().ToLower(), true.ToString().ToLower());

            xml.WriteElement("RecordCondition", this.RecordCondition.Condition.ToString(), "None");
            xml.WriteElement("RecordConditionSelector", this.RecordCondition.Selector, string.Empty);

            WriteExtraElements(xml);

            xml.mWriter.WriteStartElement("Fields");

            for (int i = 0; i < mFields.Count; i++)
                ((FieldBuilder)mFields[i]).SaveToXml(xml);

            xml.mWriter.WriteEndElement();

            xml.mWriter.WriteEndElement();
            xml.EndWrite();
        }

        internal abstract void WriteHeaderElement(XmlHelper writer);
        internal abstract void WriteExtraElements(XmlHelper writer);

        internal abstract void ReadClassElements(XmlDocument document);
        internal abstract void ReadField(XmlNode node);

        internal const string sInvalidIdentifier = "The string '{0}' not is a valid .NET identifier.";



        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private RecordConditionInfo mRecordConditionInfo = new RecordConditionInfo();

        /// <summary>Allow to tell the engine what records must be included or excluded while reading.</summary>
        public RecordConditionInfo RecordCondition
        {
            get { return mRecordConditionInfo; }
        }


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IgnoreCommentInfo mIgnoreCommentInfo = new IgnoreCommentInfo();

        /// <summary>Indicates that the engine must ignore the lines with this comment marker.</summary>
        public IgnoreCommentInfo IgnoreCommentedLines
        {
            get { return mIgnoreCommentInfo; }
        }


        /// <summary>Allow to tell the engine what records must be included or excluded while reading.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public sealed class RecordConditionInfo
        {
            internal RecordConditionInfo()
            { }

            RecordCondition mRecordCondition = FileHelpers.RecordCondition.None;

            /// <summary>Allow to tell the engine what records must be included or excluded while reading.</summary>
            public RecordCondition Condition
            {
                get { return mRecordCondition; }
                set { mRecordCondition = value; }
            }

            string mRecordConditionSelector = string.Empty;

            /// <summary>The selector used by the <see cref="RecordCondition"/>.</summary>
            public string Selector
            {
                get { return mRecordConditionSelector; }
                set { mRecordConditionSelector = value; }
            }

        }

        /// <summary>Indicates that the engine must ignore the lines with this comment marker.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public sealed class IgnoreCommentInfo
        {
            internal IgnoreCommentInfo()
            { }

            /// <summary>
            /// <para>Indicates that the engine must ignore the lines with this comment marker.</para>
            /// <para>An emty string or null indicates that the engine dont look for comments</para>
            /// </summary>
            public string CommentMarker
            {
                get { return mMarker; }
                set
                {
                    if (value != null)
                        value = value.Trim();

                    mMarker = value;
                }
            }
            private string mMarker = string.Empty;

            /// <summary>Indicates if the comment can have spaces or tabs at left (true by default)</summary>
            public bool InAnyPlace
            {
                get { return mInAnyPlace; }
                set { mInAnyPlace = value; }
            }
            private bool mInAnyPlace = true;
        }

        internal static string TypeToString(Type type)
        {

            if (type.IsGenericType)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(type.Name.Substring(0, type.Name.IndexOf("`", StringComparison.Ordinal)));
                sb.Append("<");

                Type[] args = type.GetGenericArguments();

                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                        sb.Append(",");

                    sb.Append(TypeToString(args[i]));
                }
                sb.Append(">");

                return sb.ToString();
            }
            else
                return type.FullName;
        }
    }
}
