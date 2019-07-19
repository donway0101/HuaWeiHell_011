using System;
using System.ComponentModel;

namespace Bp.Mes
{
    /// <summary>
    /// 包基类
    /// </summary>
    public class PackBase : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// 发送时间
        /// </summary>
        [Newtonsoft.Json.JsonIgnore()]
        public string Json
        {
            get;
            set;
        }

        public DateTime Time { get; set; }
    }


    /// <summary>
    /// 数据包头；通讯编码：UTF8
    /// </summary>
    public class BpHead : PackBase
    {
        public BpHead()
        {
            this.time = DateTime.Now;
            //this.Object = new PackBase();
            this.from = "0000";
            this.id = "Z" + this.from + DateTime.Now.ToString("yyMMddHHmmssfff");
            this.to = "9999";
        }

        /// <summary>
        /// 创建数据包
        /// </summary>
        /// <param name="station">站号</param>
        /// <param name="obj">主体</param>
        public BpHead( object obj)
        {
            this.time = DateTime.Now;
            this.obj = obj;
            this.from = "";
            this.id = "P_" + this.from + DateTime.Now.ToString("yyMMddHHmmssfff");
            this.to = "";
        }

        /// <summary>
        /// 创建数据包
        /// </summary>
        /// <param name="station">站号</param>
        /// <param name="obj">主体</param>
        public BpHead(string station, object obj)
        {
            if (string.IsNullOrEmpty(station))
                station = "0000";

            this.time = DateTime.Now;
            this.obj = obj;
            this.from = station + "";
            this.id = "Z" + this.from + DateTime.Now.ToString("yyMMddHHmmssfff");
            this.to = "9999";
        }

        public string head
        {
            get
            {
                return "##";
            }
        }

        /// <summary>
        /// 数据包编号；MES,M;机台，Z;App,A  字母开头 +站号（4位）+ yyMMddHHmmssfff
        /// </summary>
        public string id
        {
            set;
            get;
        }

        /// <summary>
        /// 请求数据包ID,
        /// </summary>
        public string answerId
        {
            set;
            get;
        }

        /// <summary>
        /// 数据源点，站号。机台：2位系统号+2两位线号;Mes 9999;App 8888;空：0000
        /// 如：烤箱1,0101；烤箱2,0102；烤箱3,0103；烤箱4,0104；烤箱5,0105；
        /// </summary>
        public string from
        {
            set;
            get;
        }

        /// <summary>
        /// 数据目的，站号。机台：2位系统号+2两位线号;Mes 9999;App 8888;空：0000
        /// 如：烤箱1,0101；烤箱2,0102；烤箱3,0103；烤箱4,0104；烤箱5,0105；
        /// </summary>
        public string to
        {
            set;
            get;
        }

        /// <summary>
        /// 数据包主体类型,直发对象类名，如Bp.Mes.Pack.Socket
        /// </summary>
        public string type
        {
            set;
            get;
        }

        private object mObj = null;

        /// <summary>
        /// 数据包主体实例
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public object obj
        {
            set
            {
                if (this.mObj != value)
                {
                    this.mObj = value;
                    if (string.IsNullOrEmpty(this.type))
                        this.type = this.mObj.GetType().ToString();
                    this.NotifyPropertyChanged("Obj");
                }
            }
            get
            {
                return this.mObj;
            }
        }

        /// <summary>
        /// 发送时间
        /// </summary>
        // [Newtonsoft.Json.JsonIgnore()]
        public DateTime? time
        {
            set;
            get;
        }

        public string tail
        {
            get
            {
                return "$$";
            }
        }
    }
}
