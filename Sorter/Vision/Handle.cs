using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bp.Mes
{
    public class Handle
    {

        #region 单例....
        /// <summary>
        /// 静态实例
        /// </summary>
        /// 
        private static Handle instance = new Handle();
        /// <summary>
        /// 私有构造函数
        /// </summary>
        private Handle()
        {


        }

        /// <summary>
        /// 静态属性
        /// </summary>
        public static Handle Instance
        {
            get { return instance; }
        }
        #endregion


        /// <summary>
        /// json反序列成对象
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public Bp.Mes.BpHead JsonstringToObj(string json)
        {
            Bp.Mes.BpHead head = null;
            try
            {
                //反序列化报头
                head = Bp.Mes.Json.Deserialize<Bp.Mes.BpHead>(json);
                head.Json = json;
                //反序列化主体
                if (head != null)
                {
                    Type type = head.type.GetTypeByName();
                    string son = head.obj.ToString();
                    head.obj = Bp.Mes.Json.Deserialize(son, type);

                    Bp.Mes.PackBase pack = (Bp.Mes.PackBase)head.obj;
                    pack.Json = son;
                }
                head.Json = json;
            }
            catch (Exception)
            {
            }
            return head;
        }




        /// <summary>
        /// 对象序列化json
        /// </summary>
        /// <param name="obj"></param>
        public string ObjToJsonstring(Object obj)
        {
            Bp.Mes.BpHead head = new Bp.Mes.BpHead(obj);
            string json = Bp.Mes.Json.Serializer(head);
            return json;
        }

    }
}
