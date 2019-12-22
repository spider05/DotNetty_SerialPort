using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using flyfire.IO.Ports;

namespace flyfire.HelloArm
{
    public class Program
    {
        private static CustomSerialPort csp = null;
        private static string[] serailports;
        private static int curSerialPortOrder = 0;
        private static string selectedComPort = "";
        private static Dictionary<string, CustomSerialPort> servicePorts = new Dictionary<string, CustomSerialPort>();
        private static System.Timers.Timer sendTimer = new System.Timers.Timer();

        private static readonly int baudRate = 115200;

        static void Main(string[] args)
        {
          byte[] bytes=  UtilityClass.HexToBytes("31 32 33");

            SetLibPath();
            ShowWelcome();

            GetPortNames();
            ShowPortNames();

            if (serailports.Length == 0)
            {
                Console.WriteLine($"Press any key to exit");
                Console.ReadKey();

                return;
            }
#if RunIsService
            RunService();
#endif

            //              --打开成功--可关闭/可输入内容/退出--------
            //提示打开串口-|                                     |
            //              --打开失败--提示继续打开--打开成功--             
            //打开串口

            Open();
            //Console.WriteLine("\r\n请输入以下命令\r\n");
            Console.WriteLine("p:端口列表");
            Console.WriteLine($"c:关闭端口");
            Console.WriteLine("q:退出");
            Console.WriteLine();

            bool quit = false;
            while (!quit)
            {

                string key = Console.ReadLine();//.ReadKey().KeyChar;
                Console.WriteLine();

                switch (key)
                {
                    //case (Char)27:
                    case "q":
                    case "Q"://关闭
                        quit = true;
                        break;
                    case "p":
                        ShowPortNames();
                        break;
                    case "c":
                       bool isClose= UtilityClass.CloseUart();
                        if (isClose)
                        {
                            Open();
                        }
                        break;
                    default:
                        UtilityClass.WriteMsg(key);
                        break;
                }
            }
        }

       static void Open()
        {
            bool flag = false;
            while (!flag)
            {
                Console.WriteLine("\r\n请输入连接串口");
                string strCom = Console.ReadLine();
                if (strCom.ToLower() == "exit")
                {
                    flag = true;
                    return;
                }
                flag = UtilityClass.OpenUart(strCom);
            }
        }

        private static void RunService()
        {
            Console.WriteLine("\r\nRun Mode:Service");
            if (serailports.Length > 0)
            {
                foreach (var name in serailports)
                {
                    if (name.Contains("0"))
                        continue;
                    CustomSerialPort csp = new CustomSerialPort(name, baudRate);
                    csp.ReceivedEvent += Csp_ReceivedEvent;// Csp_DataReceived;
                    try
                    {
                        csp.Open();
                        servicePorts.Add(name, csp);
                        Console.WriteLine($"Service Open Uart [{name}] Succful!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"RunService Open Uart [{name}] Exception:{ex}");
                    }
                }
                OpenUartTestTimer();
            }
        }

        private static void Csp_ReceivedEvent(object sender, byte[] bytes)
        {
            try
            {
                CustomSerialPort sps = (CustomSerialPort)sender;
                string msg = Encoding.ASCII.GetString(bytes).Replace("\r", "").Replace("\n", "");
                string echo = $"{sps.PortName} Receive Data:[{msg}].Item already filtered crlf.";
                Console.WriteLine(echo);
                if (!echo.Contains($"{sps.PortName}"))
                    sps.WriteLine(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 配置依赖库环境变量
        /// 程序中设置对当前程序无效，需重启程序方能生效，仅用于测试
        /// </summary>
        private static void SetLibPath(string libPathVariable = "LD_LIBRARY_PATH", string lib_path = "lib\\serialportstream", string os = "unix")
        {
            try
            {
                if (!Environment.OSVersion.Platform.ToString().Contains(os, StringComparison.OrdinalIgnoreCase))
                    return;

                var path = Environment.GetEnvironmentVariable(libPathVariable);
                if (path == null || path == string.Empty)
                {
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, lib_path);
                    if (Directory.Exists(path))
                    {
                        Console.WriteLine($"Set Environment Variable LD_LIBRARY_PATH={path}");
                        Environment.SetEnvironmentVariable(libPathVariable, path, EnvironmentVariableTarget.User);
                    }
                    else
                        Console.WriteLine("The support library that the program depends on does not exist");
                }
                path = Environment.GetEnvironmentVariable(libPathVariable);
                Console.WriteLine($"LD_LIBRARY_PATH={path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Set Environment Variable Exception:{ex}");
            }
        }

        /// <summary>
        /// 循环切换选择串口
        /// </summary>
        private static void SelectSerialPort()
        {
            if (curSerialPortOrder < serailports.Length)
            {
                CloseUart();
                curSerialPortOrder++;
                curSerialPortOrder %= serailports.Length;
                selectedComPort = serailports[curSerialPortOrder];
                Console.WriteLine($"current selected serial port:{selectedComPort}");
            }
        }

        private static void ShowPortNames()
        {
            Console.WriteLine($"此电脑有 {serailports.Length} 个端口.");
            foreach (string serial in serailports)
                Console.WriteLine($"端口:{serial}");
        }

        private static void CloseUart()
        {
            if (csp != null)
            {
                if (csp.IsOpen)
                    csp.Close();
                csp.ReceivedEvent -= Csp_ReceivedEvent;//Csp_DataReceived;

                csp = null;
                Console.WriteLine($"close serial port:{selectedComPort} succful!");
            }

#if RunIsService
            foreach (var csp in servicePorts.Values)
            {
                if (csp.IsOpen)
                {
                    csp.Close();
                }
            }
#endif
            if (sendTimer != null && sendTimer.Enabled)
            {
                sendTimer.Stop();
                sendTimer.Elapsed -= SendTimer_Elapsed;
            }
            msgIndex = 0;
        }

        private static void OpenUart(string portName)
        {
            CloseUart();
            try
            {
                csp = new CustomSerialPort(portName);
                csp.BaudRate = baudRate;
                csp.ReceivedEvent += Csp_ReceivedEvent;
                csp.Open();
                OpenUartTestTimer();
                Console.WriteLine($"打开串口:{portName} 成功!波特率:{baudRate}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Open Uart Exception:{ex}");
            }
        }

        private static void OpenUartTestTimer()
        {
            sendTimer.Interval = 5000;
            sendTimer.Elapsed += SendTimer_Elapsed;
            sendTimer.Start();
        }

        static int msgIndex = 0;
        private static void SendTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (csp != null && csp.IsOpen)
            {
                msgIndex++;
                string sendMsg = $"{selectedComPort} send msg:{msgIndex:d4}\t{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff}\r\n";
                csp.Write(sendMsg);
            }

#if RunIsService
            if (servicePorts.Count > 0)
                msgIndex++;
            foreach (var sps in servicePorts.Values)
            {
                if (sps.IsOpen)
                {
                    string sendMsg = $"{sps.PortName} send msg:{msgIndex:d4}\t{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff}\r\n";
                    sps.Write(sendMsg);
                }
            }
#endif
        }

        private static void TestWinUart(string portName)
        {
            SerialPort sp;

            try
            {
                sp = new SerialPort()
                {
                    PortName = portName,
                    BaudRate = baudRate
                };
                sp.Open();
                string msg;
                msg = "Hello Uart";
                sp.WriteLine(msg);
                Console.WriteLine(msg);
                msg = "Byebye Uart";
                sp.WriteLine(msg);
                sp.Close();
                sp.Dispose();
                sp = null;
                Console.WriteLine(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Open Uart Exception:{ex}");
            }
        }

        private static void TestUart(string portName)
        {
            CustomSerialPort sp;

            try
            {
                sp = new CustomSerialPort(portName, baudRate);
                sp.Open();
                string msg;
                msg = "Hello Uart";
                sp.WriteLine(msg);
                Console.WriteLine(msg);
                msg = "Byebye Uart";
                sp.WriteLine(msg);
                sp.Close();
                sp.Dispose();
                sp = null;
                Console.WriteLine(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Open Uart Exception:{ex}");
            }
        }

        private static void ShowWelcome()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var Version = version.ToString();
            var buildDateTime = System.IO.File.GetLastWriteTime(new Program().GetType().Assembly.Location).ToString();

            Console.WriteLine($"Hello {Environment.OSVersion.Platform}!");
            Console.WriteLine($"This is .netcore application.Version:{Version}\r\n");
            Console.WriteLine($"系统信息:{Environment.OSVersion}");
            Console.WriteLine($"环境.版本:{Environment.Version}");
            Console.WriteLine($"环境目录:{Environment.CurrentDirectory}");
            Console.WriteLine($"应用程序目录:{AppDomain.CurrentDomain.BaseDirectory}");
            Console.WriteLine();
            Console.WriteLine("时间:{0:yyyy/MM/dd HH:mm:ss.fff}\r\n", DateTime.Now);
        }

        /// <summary>
        /// Get PortNames
        /// </summary>
        private static void GetPortNames()
        {
            serailports = CustomSerialPort.GetPortNames();

            if (serailports.Length > curSerialPortOrder)
                selectedComPort = serailports[curSerialPortOrder];
        }
    }
}
