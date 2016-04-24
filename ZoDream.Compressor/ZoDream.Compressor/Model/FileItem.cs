using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ZoDream.Compressor.Model
{
    public class FileItem : ObservableObject
    {
        public string Name { get; set; }

        public string Extension { get; set; }

        public string FullName { get; set; }

        public FileKind Kind { get; set; }

        private FileStatus _status;

        public FileStatus Status
        {
            get { return _status; }
            set {
                _status = value;
                RaisePropertyChanged("Status");
            }
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set {
                _message = value;
                RaisePropertyChanged("Message");
            }
        }

        public string getCompressorPath()
        {
            if (Kind == FileKind.UnKown) return null;
            return Regex.Replace(FullName, @".(css|js)$", ".min$0");
        }

        public FileItem()
        {

        }

        public FileItem(string path)
        {
            FullName = path;
            Name = Path.GetFileNameWithoutExtension(path);
            Extension = Path.GetExtension(path);
            if (Regex.IsMatch(Name, @"\.min$") || (!Extension.Equals(".js", StringComparison.OrdinalIgnoreCase) && !Extension.Equals(".css", StringComparison.OrdinalIgnoreCase)))
            {
                Kind = FileKind.UnKown;
            } else
            {
                Kind = Extension.Equals(".js", StringComparison.OrdinalIgnoreCase) ? FileKind.Js : FileKind.Css;
            }
        }
    }

    public enum FileKind
    {
        Js,
        Css,
        UnKown
    }

    public enum FileStatus
    {
        None,
        Waiting,
        Failure,
        Complete
    }
}
