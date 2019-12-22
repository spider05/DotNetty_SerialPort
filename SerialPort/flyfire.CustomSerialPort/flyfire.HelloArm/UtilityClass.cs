using flyfire.IO.Ports;
using System;
using System.Collections.Generic;
using System.Text;

namespace flyfire.HelloArm
{
   public class UtilityClass
    {
        /// <summary>
        /// 波特率
        /// </summary>
        private static readonly int baudRate = 115200;
        /// <summary>
        /// 选中的串口
        /// </summary>
        public static string SelectedComPort = "";
        /// <summary>
        /// 
        /// </summary>
        public static CustomSerialPort m_CSP = null;

        public static bool OpenUart(string portName)
        {
            CloseUart();
            try
            {
                m_CSP = new CustomSerialPort(portName);
                m_CSP.BaudRate = baudRate;
                m_CSP.ReceivedEvent += Csp_ReceivedEvent;
                m_CSP.Open();
                //OpenUartTestTimer();
                Console.WriteLine($"打开串口:{portName} 成功!波特率:{baudRate}");
                SelectedComPort = portName;
                return true;
            }
            catch (Exception ex)
            {
                SelectedComPort = null;
                m_CSP = null;
                Console.WriteLine($"打开串口失败:{ex}");
            }
            return false;
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
        /// 测试串口是否成功
        /// </summary>
        /// <param name="portName"></param>
        public static void TestUart(string portName)
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

        /// <summary>
        /// 关闭串口
        /// </summary>
        public static bool CloseUart()
        {
            bool flag = false;
            try
            {
                if (m_CSP != null)
                {
                    if (m_CSP.IsOpen)
                        m_CSP.Close();
                    m_CSP.ReceivedEvent -= Csp_ReceivedEvent;//Csp_DataReceived;

                    m_CSP = null;
                    Console.WriteLine($"关闭串口:{SelectedComPort} 成功!");
                    SelectedComPort = null;
                    flag = true;
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
                //if (sendTimer != null && sendTimer.Enabled)
                //{
                //    sendTimer.Stop();
                //    sendTimer.Elapsed -= SendTimer_Elapsed;
                //}
                //msgIndex = 0;

            }
            catch ( Exception err)
            {
                
            }
            return flag;
        }

        private static void RunService()
        {
            //Console.WriteLine("\r\nRun Mode:Service");
            //if (serailports.Length > 0)
            //{
            //    foreach (var name in serailports)
            //    {
            //        if (name.Contains("0"))
            //            continue;
            //        CustomSerialPort csp = new CustomSerialPort(name, baudRate);
            //        csp.ReceivedEvent += Csp_ReceivedEvent;// Csp_DataReceived;
            //        try
            //        {
            //            csp.Open();
            //            servicePorts.Add(name, csp);
            //            Console.WriteLine($"Service Open Uart [{name}] Succful!");
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine($"RunService Open Uart [{name}] Exception:{ex}");
            //        }
            //    }
            //    OpenUartTestTimer();
            //}
        }

        public  static void WriteMsg(string msg)
        {
            try
            {
                if (m_CSP != null && m_CSP.IsOpen)
                {
                    //string sendMsg = $"{SelectedComPort} send msg:{msgIndex:d4}\t{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff}\r\n";
                    byte[] bytes = null;
                    if (msg.Contains(" "))
                    {
                       string[] arr=  msg.Split(' ');
                        bytes = HexToBytes(msg);
                    }
                    else
                    {
                        bytes= Encoding.ASCII.GetBytes(msg);
                    }
                   
                    m_CSP.Write(bytes);
                }
            }
            catch(Exception err)
            {

            }
        }

        /// <summary>
        /// 字符hex转数组byte
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
      public  static byte[] HexToBytes(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException("hex");
            hex = hex.Replace(",", "");
            hex = hex.Replace("\n", "");
            hex = hex.Replace("\\", "");
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0)
            {
                hex += "20";//空格
                throw new ArgumentException("hex is not a valid number!", "hex");
            }
            // 需要将 hex 转换成 byte 数组。
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                try
                {
                    // 每两个字符是一个 byte。
                    bytes[i] = byte.Parse(hex.Substring(i * 2, 2),
                    System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    // Rethrow an exception with custom message.
                    throw new ArgumentException("hex is not a valid hex number!", "hex");
                }
            }
            return bytes;
        }

        /// <summary>
        /// 字符串转16进制
        /// </summary>
        /// <param name="s">字符串</param>
        /// <param name="charset">编码,如"utf-8","gb2312"</param>
        /// <param name="fenge">是否每字符用逗号分隔</param>
        /// <returns></returns>
        public static string ToHex(string s, string charset, bool fenge)
        {
            if ((s.Length % 2) != 0)
            {
                s += " ";//空格
                         //throw new ArgumentException("s is not valid chinese string!");
            }
            System.Text.Encoding chs = System.Text.Encoding.GetEncoding(charset);
            byte[] bytes = chs.GetBytes(s);
            byte[] b2 = Encoding.ASCII.GetBytes(s);
            string str = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                str += string.Format("{0:X}", bytes[i]);
                if (fenge && (i != bytes.Length - 1))
                {
                    str += string.Format("{0}", ",");
                }
            }
            return str.ToLower();
        }

        /// <summary>
        /// 从16进制转换成utf编码的字符串
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="charset">编码,如"utf-8","gb2312"</param>
        /// <returns></returns>
        public static string UnHex(string hex, string charset)
        {
            if (hex == null)
                throw new ArgumentNullException("hex");
            hex = hex.Replace(",", "");
            hex = hex.Replace("\n", "");
            hex = hex.Replace("\\", "");
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0)
            {
                hex += "20";//空格
                throw new ArgumentException("hex is not a valid number!", "hex");
            }
            // 需要将 hex 转换成 byte 数组。
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                try
                {
                    // 每两个字符是一个 byte。
                    bytes[i] = byte.Parse(hex.Substring(i * 2, 2),
                    System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    // Rethrow an exception with custom message.
                    throw new ArgumentException("hex is not a valid hex number!", "hex");
                }
            }
            System.Text.Encoding chs = System.Text.Encoding.GetEncoding(charset);
            return chs.GetString(bytes);
        }
    }
}
