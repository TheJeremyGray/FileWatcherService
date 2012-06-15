using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WatchNotifyService
{
    class TemporaryFile : IDisposable
    {
        private string filePath;
        public string FilePath { get { return filePath; } }

        private FileInfo fileInformation;
        public FileInfo FileInformation { get { return fileInformation; } }

        public TemporaryFile() :
            this(System.IO.Path.GetTempPath()) { }

        public TemporaryFile(string directory)
        {
            Create(System.IO.Path.Combine(directory, System.IO.Path.GetRandomFileName()));
        }

        public TemporaryFile(string directory, string name)
        {
            if (String.IsNullOrEmpty(directory))
                directory = Path.GetTempPath();

            if (String.IsNullOrEmpty(name))
                name = Path.GetRandomFileName();

            Create(System.IO.Path.Combine(directory, name));
        }

        private void Create(string path)
        {
            fileInformation = new FileInfo(path);
            filePath = path;
            using (File.Create(filePath)) { };
            fileInformation.Attributes = FileAttributes.Temporary;
        }

        private void Delete()
        {
            File.Delete(filePath);
            filePath = null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Delete();
            GC.SuppressFinalize(this);
        }

        ~TemporaryFile()
        {
            Delete();
        }
        #endregion

    }
}