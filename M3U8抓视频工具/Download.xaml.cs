using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace M3U8抓视频工具
{
    /// <summary>
    /// Download.xaml 的交互逻辑
    /// </summary>
    public partial class Download : Window
    {
        public string Url;

        public string Location;

        public Download()
        {
            InitializeComponent();
            new Task(() =>
            {
                try { File.Delete(Location); ProcessPlayList(Url, File.Create(Location)); MessageBox.Show("下载完成。", "Download"); }
                catch(Exception e) { MessageBox.Show(e.Message, e.GetType().FullName); }
                finally { Dispatcher.Invoke(() => { Close(); }); }
            }).Start();
        }

        private void ProcessPlayList(string url, FileStream file)
        {
            if (url.Contains(".m3u"))
            {
                var request = WebRequest.Create(url);
                var response = request.GetResponse();
                var listStream = new StreamReader(response.GetResponseStream());
                var list = new List<string>();
                while (!listStream.EndOfStream)
                {
                    var line = listStream.ReadLine();
                    if(!string.IsNullOrWhiteSpace(line) && line[0] != '#')
                    {
                        list.Add(line);
                    }
                }
                DownloadProgress.Dispatcher.Invoke(() => {
                    DownloadProgress.Value = 0;
                    DownloadProgress.Maximum = list.Count;
                });
                foreach(var line in list)
                {
                    if (!line.Contains("//"))
                    {
                        ProcessPlayList(url.Substring(0, url.LastIndexOf('/')) + "/" + line, file);
                    }
                    else
                    {
                        ProcessPlayList(line, file);
                    }
                }
            }
            else
            {
                var data = new WebClient().DownloadData(url);
                file.Write(data, 0, data.Length);
                DownloadProgress.Dispatcher.Invoke(() => {
                    DownloadProgress.Value++;
                });
            }
        }
    }
}
