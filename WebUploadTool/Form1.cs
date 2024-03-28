using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.IO;
using System.Text;

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
            try
            {
                //解壓縮

                var fileName = "v1.0.zip";
                var filePath = $"{config.ip}TEST/{fileName}";


                //上傳FTP
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(filePath);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                // This example assumes the FTP site uses anonymous logon.  
                request.Credentials = new NetworkCredential(config.user, config.password);
                request.Proxy = null;
                request.KeepAlive = true;
                request.UseBinary = true;
                request.Method = WebRequestMethods.Ftp.UploadFile;

                // Copy the contents of the file to the request stream.  
                using (StreamReader sourceStream = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}{fileName}"))
                {
                    byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                    request.ContentLength = fileContents.Length;

                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(fileContents, 0, fileContents.Length);
                    }
                }


                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

                //完成

                MessageBox.Show("更新完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }




        }


        public static void CreateFolder(config config, string folderName)
        {
            //上傳FTP
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{config.ip}{folderName}");
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            // This example assumes the FTP site uses anonymous logon.  
            request.Credentials = new NetworkCredential(config.user, config.password);
            request.Proxy = null;
            request.KeepAlive = true;
            request.UseBinary = true;
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Copy the contents of the file to the request stream.  

            using (var resp = (FtpWebResponse)request.GetResponse())
            {
                Console.WriteLine(resp.StatusCode);
            }
        }

        public static void UploadFile(config config, string path, string fileName)
        {
            //上傳FTP
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{config.ip}{path}{fileName}");
            request.Method = WebRequestMethods.Ftp.UploadFile;
            // This example assumes the FTP site uses anonymous logon.  
            request.Credentials = new NetworkCredential(config.user, config.password);
            request.Proxy = null;
            request.KeepAlive = true;
            request.UseBinary = true;
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Copy the contents of the file to the request stream.  
            using (StreamReader sourceStream = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}{fileName}"))
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
