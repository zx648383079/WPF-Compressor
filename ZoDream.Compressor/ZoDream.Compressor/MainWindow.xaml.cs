using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ZoDream.Compressor.Model;
using ZoDream.Compressor.Helper;
using ZoDream.Compressor.Helper.Compressor;

namespace ZoDream.Compressor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<FileItem> _filesList = new ObservableCollection<FileItem>();

        public MainWindow()
        {
            InitializeComponent();
            FilesList.ItemsSource = _filesList;
        }

        private void FilesList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            } else {
                e.Effects = DragDropEffects.None;
            }
        }

        private void FilesList_Drop(object sender, DragEventArgs e)
        {
            Array files = (System.Array)e.Data.GetData(DataFormats.FileDrop);
            foreach (string item in files)
            {
                if (File.Exists(item))
                {
                    _addOne(item);
                } else if (Directory.Exists(item))
                {
                    _findDir(item);
                }
            }
        }

        /// <summary>
        /// 遍历文件夹
        /// </summary>
        /// <param name="dir"></param>
        private void _findDir(string dir)
        {
            DirectoryInfo TheFolder = new DirectoryInfo(dir);

            DirectoryInfo[] dirInfo = TheFolder.GetDirectories();
            //遍历文件夹
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                _findDir(NextFolder.FullName);
            }

            FileInfo[] fileInfo = TheFolder.GetFiles();
            //遍历文件
            foreach (FileInfo NextFile in fileInfo)
            {
                _addOne(NextFile.FullName);
            }
        }

        /// <summary>
        /// 添加单个文件
        /// </summary>
        /// <param name="path"></param>
        private void _addOne(string path)
        {
            FileItem file = new FileItem(path);
            if (file.Kind != FileKind.Unkown)
            {
                _filesList.Add(file);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            switch (item.Header.ToString())
            {
                case "文件":
                    ChooseFile();
                    break;
                case "文件夹":
                    ChooseDir();
                    break;
                case "开始":
                    Begin();
                    break;
                case "删除":
                    if (FilesList.SelectedIndex >= 0)
                    {
                        _filesList.Remove(FilesList.SelectedItem as FileItem);
                    }
                    break;
                case "清空":
                    _filesList.Clear();
                    break;
                default:
                    break;
            }
        }

        private void ChooseDir()
        {
            System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog();
            folder.ShowNewFolderButton = false;
            if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _findDir(folder.SelectedPath);
            }
        }

        private void ChooseFile()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = true;
            open.Filter = "脚本文件|*.css;*.js|所有文件|*.*";
            open.Title = "选择需要压缩的JS、CSS";
            if (open.ShowDialog() == true)
            {
                foreach (string item in open.FileNames)
                {
                    _addOne(item);
                }
            }
        }

        private void Begin()
        {
            bool r = (bool)RemoveCb.IsChecked;
            bool g = (bool)GzipCb.IsChecked;
            foreach (FileItem item in _filesList)
            {
                if (item.Status == FileStatus.Complete)
                {
                    item.Message = "跳过";
                    break;
                }
                item.Status = FileStatus.Waiting;
                Task.Run(() =>
                {
                    CompressorHelper helper = new CompressorHelper();
                    helper.RemoveComments = r;
                    helper.GzipCompress = g;
                    helper.Compress(item);
                });
            }
        }
    }
}
