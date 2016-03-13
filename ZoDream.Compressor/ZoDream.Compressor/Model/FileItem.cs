using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZoDream.Compressor.Model
{
    public class FileItem : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public FileKind Kind { get; set; } = FileKind.Js;

        public string Path { get; set; }


        private FileStatus _status;

        public FileStatus Status
        {
            get { return _status; }
            set {
                _status = value;
                OnPropertyChanged("Status");
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string getCompressorPath()
        {
            return Regex.Replace(Path, @".(css|js)$", ".min$0");
        }

        public FileItem()
        {

        }

        public FileItem(string path)
        {
            Match m = Regex.Match(path, @"\\(?<name>[^\\]+?).(?<ext>(css|js))$", RegexOptions.RightToLeft);
            Name = m.Groups["name"].Value;
            switch (m.Groups["ext"].Value)
            {
                case "js":
                    Kind = FileKind.Js;
                    break;
                case "css":
                    Kind = FileKind.Css;
                    break;
                default:
                    Kind = FileKind.Unkown;
                    break;
            }
            Path = path;
        }
    }

    public enum FileKind
    {
        Js,
        Css,
        Unkown
    }

    public enum FileStatus
    {
        None,
        Waiting,
        Failure,
        Complete
    }
}
