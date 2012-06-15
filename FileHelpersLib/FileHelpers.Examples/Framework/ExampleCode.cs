﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ExamplesFramework
{
    public sealed class ExampleCode
    {
        public class ExampleEventArgs : EventArgs
        {
            public ExampleCode Example { get; private set; }

            internal ExampleEventArgs(ExampleCode example)
            {
                this.Example = example;
            }
        }

        /// <summary>
        /// Message when new file is added to the list
        /// </summary>
        public class NewFileEventArgs : EventArgs
        {
            public ExampleFile File { get; private set; }

            internal NewFileEventArgs(ExampleFile file)
            {
                this.File = file;
            }
        }

        /// <summary>
        /// Notify application that a new file has been created
        /// </summary>
        public event EventHandler<NewFileEventArgs> AddedFile;

        /// <summary>
        /// Create a new demo class
        /// </summary>
        /// <param name="example">Demo structure from template parse</param>
        /// <param name="name">Title from TODO:</param>
        /// <param name="category">Category from TODO:</param>
        public ExampleCode(ExampleBase example, string name, string category)
        {
            Example = example;
            Example.Console.Changed += new EventHandler(Console_Changed); Name = name;
            Category = category;
            Runnable = true;
            AutoRun = false;
            Files = new List<ExampleFile>();
        }

        internal event EventHandler ConsoleChanged;

        private void OnConsoleChanged()
        {
            EventHandler handler = ConsoleChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        void Console_Changed(object sender, EventArgs e)
        {
            OnConsoleChanged();
        }

        /// <summary>
        /// Example class that runs
        /// </summary>
        public ExampleBase Example { get; private set; }

        /// <summary>
        /// Title set from code
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Description set from code
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Code from file.
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// Can be of the form "Async/Delimited" with multiple categories
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// List of logical files extracted from the code
        /// </summary>
        public List<ExampleFile> Files { get; set; }

      
        /// <summary>
        /// Is this test runnable
        /// </summary>
        public bool Runnable { get; set; }

        /// <summary>
        /// Is this test runnable
        /// </summary>
        public bool AutoRun { get; set; }

        /// <summary>
        /// Indicates if the Example has Console Output
        /// </summary>
        public bool HasOutput { get; set; }

        private static string TempPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp"); }
        }

        public void RunExample()
        {
            try
            {
                EnsureDirectoryExists(TempPath);
                Environment.CurrentDirectory = TempPath;
                
                foreach (ExampleFile file in this.Files)
                {
                    if (file.Status == ExampleFile.FileType.InputFile)
                        File.WriteAllText(file.Filename, file.Contents, Encoding.UTF8);
                }
                this.Example.RunExample();
            }
            catch (Exception ex)
            {
                this.Example.Exception = ex;
            }
            finally
            {
                foreach (ExampleFile file in this.Files)
                {
                    if (file.Status == ExampleFile.FileType.InputFile)
                    {
                        File.Delete(file.Filename);
                    }
                    if (file.Status == ExampleFile.FileType.OutputFile)
                    {
                        if (File.Exists(file.Filename))
                        {
                            file.Contents = File.ReadAllText(file.Filename);
                        }
                    }
                }
             
            }

        }

        private void EnsureDirectoryExists(string tempPath)
        {
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
        }
    }
}
