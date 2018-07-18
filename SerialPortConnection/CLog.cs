using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
namespace SerialPortConnection
{
    /// <summary>
    /// 日志消息类型分类
    /// </summary>
    public enum MsgType
    {
        UnKnown,        //未知类型
        Information,    //普通信息
        Warning,        //警告信息
        Error,          //错误信息 
        Success         //成功信息
    }
    /// <summary>
    /// 日志类型，按周、月、年创建日志文件
    /// </summary>
    public enum LogType
    {
        Daily,
        Weekly,
        Monthly,
        Annually
    }
    /// <summary>
    /// 日志记录表类
    /// </summary>
    public class Msg
    {
        private DateTime dateTime;
        private string text;
        private MsgType type;
        public Msg()
            : this("", MsgType.UnKnown)
        {

        }
        public Msg(string t, MsgType p)
            : this(DateTime.Now, t, p)
        {

        }
        public Msg(DateTime dt, string text, MsgType type)
        {
            dateTime = dt;
            this.text = text;
            this.type = type;
        }
        public DateTime DateTime
        {
            get
            {
                return dateTime;
            }
            set
            {
                dateTime = value;
            }
        }
        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        public MsgType Type
        {
            get { return type; }
            set { type = value; }
        }
        public new string ToString()
        {
            return dateTime.ToString() + "\t" + text + "\n";
        }
    }
    public class CLog : IDisposable
    {
        private static Queue<Msg> msgs;
        private static string path;
        private static bool state;
        private static LogType type;
        private static DateTime TimeSign;
        private static StreamWriter writer;
        public CLog()
            : this(".\\Log\\", LogType.Daily)
        {
            string path = ".\\log\\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public CLog(LogType t)
            : this(".\\Log\\", t)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public CLog(string p, LogType t)
        {
            if (msgs == null)
            {
                state = true;
                path = p;
                type = t;
                msgs = new Queue<Msg>();
                Thread thread = new Thread(work);
                thread.Start();
            }
        }
        //日志写入线程执行
        private void work()
        {
            while (true)
            {
                if (msgs.Count > 0)
                {
                    Msg msg = null;
                    lock (msgs)
                    {
                        msg = msgs.Dequeue();
                    }
                    if (msgs != null)
                    {
                        FileWrite(msg);
                    }
                }
                else
                {
                    if (state)
                    {
                        Thread.Sleep(1);
                    }
                    else
                    {
                        FileClose();
                    }
                }
            }
        }
        private string GetFileName()
        {
            DateTime now = DateTime.Now;
            string format = "";
            switch (type)
            {
                case LogType.Daily:
                    TimeSign = new DateTime(now.Year, now.Month, now.Day);
                    TimeSign = TimeSign.AddDays(1);
                    format = "yyyyMMdd'.log'";
                    break;
                case LogType.Weekly:
                    TimeSign = new DateTime(now.Year, now.Month, now.Day);
                    TimeSign = TimeSign.AddDays(7);
                    format = "yyyyMMdd'.log'";
                    break;
                case LogType.Monthly:
                    TimeSign = new DateTime(now.Year, now.Month, now.Day);
                    TimeSign = TimeSign.AddMonths(1);
                    format = "yyyyMM'.log'";
                    break;
                case LogType.Annually:
                    TimeSign = new DateTime(now.Year, now.Month, now.Day);
                    TimeSign = TimeSign.AddYears(1);
                    format = "yyyy'.log'";
                    break;
            }
            return now.ToString(format);
        }
        private void FileWrite(Msg msg)
        {
            try
            {
                if (writer == null)
                {
                    FileOpen();
                    Write(msg);
                }
                else
                {
                    if (DateTime.Now >= TimeSign)
                    {
                        FileClose();
                        FileOpen();
                    }
                    writer.Write(msg.DateTime.ToString("yyyy-MM-dd hh:mm:ss fff"));
                    writer.Write('\t');
                    writer.Write(msg.Type);
                    writer.Write('\t');
                    writer.WriteLine(msg.Text);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.Out.Write(ex);
            }
        }
        private void FileOpen()
        {
            writer = new StreamWriter(path + GetFileName(), true, Encoding.UTF8);

        }
        private void FileClose()
        {
            if (writer != null)
            {
                writer.Flush();
                writer.Close();
                writer.Dispose();
                writer = null;
            }
        }
        public void Write(Msg msg)
        {
            if (msg != null)
            {
                lock (msgs)
                {
                    msgs.Enqueue(msg);
                }
            }
        }
        public void Write(string text, MsgType type)
        {
            Write(new Msg(text, type));
        }
        public void Write(DateTime dt, string text, MsgType type)
        {
            Write(new Msg(dt, text, type));
        }
        public void Write(Exception ex, MsgType type)
        {
            Write(new Msg(ex.Message, type));
        }
        public void Dispose()
        {
            state = false;
        }
    }
}

