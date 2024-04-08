using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace WebUploadTool
{
    public partial class Form1 : Form
    {
        config config = new config();

        public Form1()
        {
            InitializeComponent();
        }

        public class ComboboxItem
        {
            public string Text { get; set; } = string.Empty;
            public object Value { get; set; } = string.Empty;

            public override string ToString()
            {
                return Text;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                //讀設定
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "config.json") == false)
                {
                    throw new Exception("config.json檔案不存在");
                }

                using (StreamReader r = new StreamReader("config.json"))
                {
                    string json = r.ReadToEnd();
                    config = JsonConvert.DeserializeObject<config>(json);
                }

                //取得版本資訊
                string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);

                var fileList = files.Where(x => x.ToLower().Contains(".zip"));

                foreach (var file in fileList.Select(x => Path.GetFileName(x)).OrderByDescending(x => x))
                {
                    comboBox1.Items.Add(new ComboboxItem()
                    {
                        Text = Path.GetFileName(file),
                        Value = file
                    });
                }

                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }




        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var localStartPath = $"{AppDomain.CurrentDomain.BaseDirectory}TEMP";

            try
            {               

                var fileName = comboBox1.GetItemText(comboBox1.SelectedItem);

                if (!Directory.Exists(localStartPath))
                {
                    Directory.CreateDirectory(localStartPath);
                }


                //解壓縮
                System.IO.Compression.ZipFile.ExtractToDirectory($"{AppDomain.CurrentDomain.BaseDirectory}{fileName}", localStartPath);


                var ftpStartPath = $"{config.ip}";
                

                //處理資料
                FTPProcess(ftpStartPath, localStartPath, config);

                //完成

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                MessageBox.Show($"更新完成, 執行時間:{ts.ToString("mm\\:ss\\.ff")}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
            finally 
            {
                var dir = new DirectoryInfo(localStartPath);
                dir.Delete(true);
            }


        }


        public static void FTPProcess(string ftpPath, string localPath, config config)
        {
            DirectoryInfo d = new DirectoryInfo(localPath); //Assuming Test is your Folder

            var folders = d.GetDirectories();                       

            foreach (var folder in folders)
            {                
                CreateFolder($"{ftpPath}/{folder.Name}", config);
                FTPProcess($"{ftpPath}/{folder.Name}", $"{localPath}\\{folder.Name}", config);
            }

            var Files = d.GetFiles();

            foreach (var file in Files)
            {
                UploadFile($"{ftpPath}/{file.Name}", $"{localPath}\\{file.Name}", config);
            }

        }

        public static void CreateFolder(string ftpPath, config config)
        {

            //上傳FTP
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpPath);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            // This example assumes the FTP site uses anonymous logon.  
            request.Credentials = new NetworkCredential(config.user, config.password);
            request.Proxy = null;
            request.KeepAlive = true;
            request.UseBinary = true;

            try
            {
                using (var resp = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine(resp.StatusCode);
                }
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;

                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    return;
                }
                else
                {
                    throw;
                }
            }
        }

        public static void UploadFile(string ftpPath, string localPath, config config)
        {
            //上傳FTP
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpPath);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            // This example assumes the FTP site uses anonymous logon.  
            request.Credentials = new NetworkCredential(config.user, config.password);
            request.Proxy = null;
            request.KeepAlive = true;
            request.UseBinary = true;
        

            // Copy the contents of the file to the request stream.  
            using (StreamReader sourceStream = new StreamReader(localPath))
            {
                byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                request.ContentLength = fileContents.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileContents, 0, fileContents.Length);
                }
            }


            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
        }
    }
}
