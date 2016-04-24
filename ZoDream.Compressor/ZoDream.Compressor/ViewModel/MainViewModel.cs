using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ZoDream.Compressor.Model;
using ZoDream.Helper.Compress;
using ZoDream.Helper.Local;

namespace ZoDream.Compressor.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
        }

        /// <summary>
        /// The <see cref="Message" /> property's name.
        /// </summary>
        public const string MessagePropertyName = "Message";

        private string _message = string.Empty;

        /// <summary>
        /// Sets and gets the Message property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                Set(MessagePropertyName, ref _message, value);
            }
        }

        /// <summary>
        /// The <see cref="Mode" /> property's name.
        /// </summary>
        public const string ModePropertyName = "Mode";

        private int _mode = 0;

        /// <summary>
        /// Sets and gets the Mode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                Set(ModePropertyName, ref _mode, value);
            }
        }

        /// <summary>
        /// The <see cref="IsGzip" /> property's name.
        /// </summary>
        public const string IsGzipPropertyName = "IsGzip";

        private bool _isGzip = false;

        /// <summary>
        /// Sets and gets the IsGzip property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsGzip
        {
            get
            {
                return _isGzip;
            }
            set
            {
                Set(IsGzipPropertyName, ref _isGzip, value);
            }
        }

        /// <summary>
        /// The <see cref="IsCover" /> property's name.
        /// </summary>
        public const string IsCoverPropertyName = "IsCover";

        private bool _isCover = false;

        /// <summary>
        /// Sets and gets the IsCover property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsCover
        {
            get
            {
                return _isCover;
            }
            set
            {
                Set(IsCoverPropertyName, ref _isCover, value);
            }
        }

        /// <summary>
        /// The <see cref="IsAll" /> property's name.
        /// </summary>
        public const string IsAllPropertyName = "IsAll";

        private bool _isAll = false;

        /// <summary>
        /// Sets and gets the IsAll property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsAll
        {
            get
            {
                return _isAll;
            }
            set
            {
                Set(IsAllPropertyName, ref _isAll, value);
                if (!value)
                {
                    for (int i = FileList.Count - 1; i > 0; i--)
                    {
                        if (FileList[i].Kind != FileKind.Js && FileList[i].Kind != FileKind.Css)
                        {
                            FileList.RemoveAt(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The <see cref="FileList" /> property's name.
        /// </summary>
        public const string FileListPropertyName = "FileList";

        private ObservableCollection<FileItem> _fileList = new ObservableCollection<FileItem>();

        /// <summary>
        /// Sets and gets the FileList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<FileItem> FileList
        {
            get
            {
                return _fileList;
            }
            set
            {
                Set(FileListPropertyName, ref _fileList, value);
            }
        }




        private RelayCommand _startCommand;

        /// <summary>
        /// Gets the StartCommand.
        /// </summary>
        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand
                    ?? (_startCommand = new RelayCommand(ExecuteStartCommand));
            }
        }

        private void ExecuteStartCommand()
        {
            _begin();
        }

        private void _begin()
        {
            if (FileList.Count <= 0)
            {
                _showMessage("请添加文件！");
                return;
            }
            _showMessage("压缩开始...");
            Task.Factory.StartNew(() =>
            {
                foreach (var item in FileList)
                {
                    _runOne(item);
                }
                _showMessage("压缩完成!");
            });
        }

        private void _runOne(FileItem item)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                item.Status = FileStatus.Waiting;
            });
            var file = item.getCompressorPath();
            var status = FileStatus.Complete;
            var message = string.Empty;
            if (item.Status == FileStatus.Complete || (!IsCover && File.Exists(file)))
            {
                message = "跳过！";
                goto end;
            }

            var content = Open.Read(item.FullName);
            if (string.IsNullOrWhiteSpace(content))
            {
                message = "文件为空";
                goto end;
            }

            try
            {
                if (Mode == 1)
                {
                    content = _yui(item, content);
                }
                else
                {
                    content = _ajaxMin(item, content);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                status = FileStatus.Failure;
                goto end;
            }

            if (IsGzip)
            {
                _gzip(file, content);
            }
            else
            {
                Open.Writer(file, content);
            }

            end:
            Application.Current.Dispatcher.Invoke(() =>
            {
                item.Status = status;
                item.Message = message;
            });
        }

        private static void _gzip(string file, string content)
        {
            using (var fs = new FileStream(file, FileMode.Create))
            {
                var gzip = new Gzip();
                gzip.Compress(content, fs);
            }
        }

        private static string _ajaxMin(FileItem item, string content)
        {
            var ajax = new AjaxMin();
            if (item.Kind == FileKind.Js)
            {
                content = ajax.Js(content);
            }
            else if (item.Kind == FileKind.Css)
            {
                content = ajax.Css(content);
            }

            return content;
        }

        private static string _yui(FileItem item, string content)
        {
            var yui = new Yui();
            if (item.Kind == FileKind.Js)
            {
                content = yui.Js(content);
            }
            else if (item.Kind == FileKind.Css)
            {
                content = yui.Css(content);
            }

            return content;
        }

        private RelayCommand<int> _deleteCommand;

        /// <summary>
        /// Gets the DeleteCommand.
        /// </summary>
        public RelayCommand<int> DeleteCommand
        {
            get
            {
                return _deleteCommand
                    ?? (_deleteCommand = new RelayCommand<int>(ExecuteDeleteCommand));
            }
        }

        private void ExecuteDeleteCommand(int index)
        {
            if (index < 0 || index >= FileList.Count) return;
            FileList.RemoveAt(index);
            _showMessage("已删除一个！");
        }

        private RelayCommand _clearCommand;

        /// <summary>
        /// Gets the ClearCommand.
        /// </summary>
        public RelayCommand ClearCommand
        {
            get
            {
                return _clearCommand
                    ?? (_clearCommand = new RelayCommand(ExecuteClearCommand));
            }
        }

        private void ExecuteClearCommand()
        {
            for (int i = FileList.Count - 1; i >= 0; i--)
            {
                if (FileList[i].Status == FileStatus.Complete)
                {
                    FileList.RemoveAt(i);
                }
            }
            _showMessage("删除成功的完成！");
        }

        private RelayCommand _clearAllCommand;

        /// <summary>
        /// Gets the ClearAllCommand.
        /// </summary>
        public RelayCommand ClearAllCommand
        {
            get
            {
                return _clearAllCommand
                    ?? (_clearAllCommand = new RelayCommand(ExecuteClearAllCommand));
            }
        }

        private void ExecuteClearAllCommand()
        {
            FileList.Clear();
            _showMessage("已清空");
        }

        private RelayCommand _openFileCommand;

        /// <summary>
        /// Gets the OpenFileCommand.
        /// </summary>
        public RelayCommand OpenFileCommand
        {
            get
            {
                return _openFileCommand
                    ?? (_openFileCommand = new RelayCommand(ExecuteOpenFileCommand));
            }
        }

        private void ExecuteOpenFileCommand()
        {
            _addFile(Open.ChooseFiles("脚本文件|*.js;*.css|所有文件|*.*"));
            _showMessage($"总共有{FileList.Count}个文件！");
        }

        private RelayCommand _openFolderCommand;

        /// <summary>
        /// Gets the OpenFolderCommand.
        /// </summary>
        public RelayCommand OpenFolderCommand
        {
            get
            {
                return _openFolderCommand
                    ?? (_openFolderCommand = new RelayCommand(ExecuteOpenFolderCommand));
            }
        }

        private void ExecuteOpenFolderCommand()
        {
            _addFile(Open.GetAllFile(Open.ChooseFolder()));
            _showMessage($"总共有{FileList.Count}个文件！");
        }

        private RelayCommand<int> _doubleCommand;

        /// <summary>
        /// Gets the DoubleCommand.
        /// </summary>
        public RelayCommand<int> DoubleCommand
        {
            get
            {
                return _doubleCommand
                    ?? (_doubleCommand = new RelayCommand<int>(ExecuteDoubleCommand));
            }
        }

        private void ExecuteDoubleCommand(int index)
        {
            if (index < 0 || index >= FileList.Count)
            {
                return;
            }
            Open.ExploreFile(FileList[index].FullName);

        }

        private RelayCommand<int> _gzipCommand;

        /// <summary>
        /// Gets the GzipCommand.
        /// </summary>
        public RelayCommand<int> GzipCommand
        {
            get
            {
                return _gzipCommand
                    ?? (_gzipCommand = new RelayCommand<int>(ExecuteGzipCommand));
            }
        }

        private void ExecuteGzipCommand(int index)
        {
            if (index < 0 || index >= FileList.Count) return;
            var content = Open.Read(FileList[index].FullName);
            try
            {
                _gzip(FileList[index].FullName, content);
                FileList[index].Status = FileStatus.Complete;
                _showMessage("GZIP压缩成功！");
            }
            catch (Exception ex)
            {
                FileList[index].Status = FileStatus.Failure;
                FileList[index].Message = ex.Message;
                _showMessage("GZIP压缩失败！");
            }
        }

        private RelayCommand<int> _yuiCommand;

        /// <summary>
        /// Gets the YUICommand.
        /// </summary>
        public RelayCommand<int> YUICommand
        {
            get
            {
                return _yuiCommand
                    ?? (_yuiCommand = new RelayCommand<int>(ExecuteYUICommand));
            }
        }

        private void ExecuteYUICommand(int index)
        {
            if (index < 0 || index >= FileList.Count) return;
            var content = Open.Read(FileList[index].FullName);
            try
            {
                Open.Writer(FileList[index].getCompressorPath(), _yui(FileList[index], content));
                FileList[index].Status = FileStatus.Complete;
                _showMessage("YUI压缩成功！");
            }
            catch (Exception ex)
            {
                FileList[index].Status = FileStatus.Failure;
                FileList[index].Message = ex.Message;
                _showMessage("YUI压缩失败！");
            }
        }

        private RelayCommand<int> _ajaxMinCommand;

        /// <summary>
        /// Gets the AjaxMinCommand.
        /// </summary>
        public RelayCommand<int> AjaxMinCommand
        {
            get
            {
                return _ajaxMinCommand
                    ?? (_ajaxMinCommand = new RelayCommand<int>(ExecuteAjaxMinCommand));
            }
        }

        private void ExecuteAjaxMinCommand(int index)
        {
            if (index < 0 || index >= FileList.Count) return;
            var content = Open.Read(FileList[index].FullName);
           
            try
            {
                Open.Writer(FileList[index].getCompressorPath(), _ajaxMin(FileList[index], content));
                FileList[index].Status = FileStatus.Complete;
                _showMessage("AjaxMin压缩成功！");
            }
            catch (Exception ex)
            {
                FileList[index].Status = FileStatus.Failure;
                FileList[index].Message = ex.Message;
                _showMessage("AjaxMin压缩失败！");
            }
        }

        private RelayCommand<DragEventArgs> _fileDrogCommand;

        /// <summary>
        /// Gets the FileDrogCommand.
        /// </summary>
        public RelayCommand<DragEventArgs> FileDrogCommand
        {
            get
            {
                return _fileDrogCommand
                    ?? (_fileDrogCommand = new RelayCommand<DragEventArgs>(ExecuteFileDrogCommand));
            }
        }

        private void ExecuteFileDrogCommand(DragEventArgs parameter)
        {
            if (parameter == null)
            {
                return;
            }
            Array files = (System.Array)parameter.Data.GetData(DataFormats.FileDrop);
            foreach (string item in files)
            {
                if (File.Exists(item))
                {
                    _addOne(item);
                }
                else if (Directory.Exists(item))
                {
                    _addFile(Open.GetAllFile(item));
                }
            }
            _showMessage($"总共有{FileList.Count}个文件！");
        }

        private void _addFile(IList<string> files)
        {
            foreach (var item in files)
            {
                _addOne(item);
            }
        }

        private void _addOne(string file)
        {
            var item = new FileItem(file);
            if (IsAll || item.Kind != FileKind.UnKown)
            {
                FileList.Add(item);
            }
        }

        private void _showMessage(string message)
        {
            Message = message;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(3000);
                Message = string.Empty;
            });
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}