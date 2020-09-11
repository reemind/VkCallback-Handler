using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VkCallbackApi
{

    [AttributeUsage(AttributeTargets.Method)]
    public class VkMethod : Attribute
    {
        public string Method { get; }

        public VkMethod(string method)
        {
            Method = method;
        }
    }

    public static class VkHandler
    {
        public static void Handle<T>(T instance, CallbackResponse response)
            => Handle<T, object>(instance, response);

        public static TOut Handle<T, TOut>(T instance, CallbackResponse response)
        {
            var type = typeof(T);

            object result = null;

            foreach (var item in type.GetMethods())
            {
                var attributes = item.GetCustomAttributes(typeof(VkMethod), true);
                if (attributes.Select(t => (t as VkMethod).Method).Contains(response.Type))
                {
                    var pars = item.GetParameters();
                    var parlist = new List<object>();
                    foreach (var par in pars)
                    {
                        if (par.ParameterType == typeof(CallbackResponse))
                            parlist.Add(response);
                        else if (par.ParameterType == typeof(JsonElement))
                            parlist.Add(response.Object);
                        else
                            throw new ArgumentException("Unknown parameters");
                    }

                    result = item.Invoke(instance, parlist.ToArray());
                }
            }

            if (result is TOut)
                return (TOut)result;
            else
                return default;
        }
    }

    [Serializable]
    public class CallbackResponse
    {
        /// <summary>
        /// Тип события
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Объект, инициировавший событие
        /// Структура объекта зависит от типа уведомления
        /// </summary>
        [JsonPropertyName("object")]
        public JsonElement Object { get; set; }

        /// <summary>
        /// ID сообщества, в котором произошло событие
        /// </summary>
        [JsonPropertyName("group_id")]
        public int GroupId { get; set; }

        /// <summary>
        /// Secret сообщества
        /// </summary>
        [JsonPropertyName("secret")]
        public string Secret { get; set; }
    }
}
