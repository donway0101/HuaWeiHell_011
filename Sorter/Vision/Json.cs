
namespace Bp.Mes
{
    public class Json
    {
        /// <summary>
        /// Json序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string Serializer<T>(T obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Json序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serializer(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string jsonString)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static object Deserialize(string jsonString)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
        }

        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static object Deserialize(string jsonString, System.Type type)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, type);
        }
    }
}
