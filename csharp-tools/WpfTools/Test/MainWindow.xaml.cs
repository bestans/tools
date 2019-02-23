using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Test
{
    public class MenuItem
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public bool isAll = false;
    }

    public class ExcelFileInfo : IComparable
    {
        public long time = 0;
        public string name = string.Empty;
        public int CompareTo(object obj)
        {
            var objInfo = (ExcelFileInfo)obj;
            return time > objInfo.time ? 0 : 1;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static StringBuilder ss = new StringBuilder();

        public delegate void DelReadStdOutput(string result);
        public delegate void DelReadErrOutput(string result);
        public delegate void ExitDelegate();

        public event DelReadStdOutput ReadStdOutput;
        public event DelReadErrOutput ReadErrOutput;
        public event ExitDelegate ExitHandler;

        public object Processprocess { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            //3.将相应函数注册到委托事件中
            ReadStdOutput += new DelReadStdOutput(ReadStdOutputAction);
            ReadErrOutput += new DelReadErrOutput(ReadErrOutputAction);
            ExitHandler += new ExitDelegate(ExitAction);

            button1.Content = "生成excel数据";
            InitMenus();
        }
        
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                // 4. 异步调用，需要invoke
                this.Dispatcher.Invoke(ReadStdOutput, new object[] { e.Data });
            }
        }

        private void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                this.Dispatcher.Invoke(ReadErrOutput, new object[] { e.Data });
            }
        }
        
        private void p_ExitReceived(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(ExitHandler);
        }
        private void ExitAction()
        {
            this.button1.IsEnabled = true;
        }

        private void ReadStdOutputAction(string result)
        {
            this.text1.AppendText(result + "\r");
        }

        private void ReadErrOutputAction(string result)
        {
            this.text1.AppendText(result + System.Environment.NewLine);
        }

        private void Test()
        {
            var list = (List<MenuItem>)cmd_list.ItemsSource;
            var index = cmd_list.SelectedIndex;
            Process p = new Process();
            p.StartInfo.FileName = "start.bat";
            string arg = "";
            if (index >= 0 && index < list.Count)
            {
                var item = list[index];
                if (!item.isAll)
                {
                    arg = item.Name;
                }
            }
            p.StartInfo.Arguments = arg;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;//true表示不显示黑框，false表示显示dos界面 
            p.StartInfo.UseShellExecute = false;


            p.EnableRaisingEvents = true;

            //p.Exited += new EventHandler(p_Exited);
            p.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
            p.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);
            p.Exited += new EventHandler(p_ExitReceived);

            p.Start();
            p.StandardInput.WriteLine("");
            p.StandardInput.WriteLine("");

            //开始异步读取输出
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            //调用WaitForExit会等待Exited事件完成后再继续往下执行。
            //p.WaitForExit();
            //p.Close();

            //Console.WriteLine("exit");
        }
        private void test2()
        {
            Process process = new Process();
            process.StartInfo.FileName = "start.bat";//这儿放你的exe文件的路径
            process.StartInfo.UseShellExecute = false;   // 是否使用外壳程序 
            process.StartInfo.CreateNoWindow = true;   //是否在新窗口中启动该进程的值 
            process.StartInfo.RedirectStandardInput = true;  // 重定向输入流 
            process.StartInfo.RedirectStandardOutput = true;  //重定向输出流 
            process.StartInfo.RedirectStandardError = true;  //重定向错误流 

            process.Start();
            //process.StandardInput.WriteLine(textBox1.Text);//读取textBox1的文本内容。
            //process.StandardInput.WriteLine(textBox2.Text);
            //textBox3.Text = process.StandardOutput.ReadLine();//将输出结果放在textBox3中
            text1.AppendText(process.StandardOutput.ReadToEnd());
            process.WaitForExit();
            process.Close();
        }

        private void InitMenus()
        {
            List<MenuItem> list = new List<MenuItem>();
            var files = new DirectoryInfo("./");
            int index = 1;
            var fileList = new List<ExcelFileInfo>();
            foreach (var it in files.GetFiles())
            {
                if (it.Extension == ".xlsx")
                {
                    var info = new ExcelFileInfo();
                    info.time = it.LastWriteTime.ToFileTime();
                    info.name = it.Name;
                    fileList.Add(info);
                }
            }
            fileList.Sort();
            var allItem = new MenuItem { ID = index++, Name = "[所有excel]", isAll = true };
            if (fileList.Count <= 0)
            {
                list.Add(allItem);
            }
            else
            {
                list.Add(new MenuItem { ID = index++, Name = fileList[0].name });
                list.Add(allItem);
                for (int i = 1; i < fileList.Count; ++i)
                {

                    list.Add(new MenuItem { ID = index++, Name = fileList[i].name });
                }
            }
            cmd_list.ItemsSource = list;
            cmd_list.SelectedIndex = 0;
        }

        public void test3()
        {
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            text1.Document.Blocks.Clear();
            button1.IsEnabled = false;
            Test();
            //text1.AppendText("aaaaaaaaaa\n");


            /**
            var process = Process.Start("start.bat");
            process.OutputDataReceived += (s, _e) => text1.AppendText(_e.Data);
            process.ErrorDataReceived += (s, _e) => text1.AppendText(_e.Data);
            //当EnableRaisingEvents为true，进程退出时Process会调用下面的委托函数
            process.Exited += (s, _e) => text1.AppendText("Exited with " + process.ExitCode);
            process.EnableRaisingEvents = true;
            //process.BeginOutputReadLine();
            //process.BeginErrorReadLine();
            process.WaitForExit();**/
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
