using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

using Ylamls.Numeric;

//[YamlIgnore]
namespace Ylamls
{
    public class Ylaml
    {
        public YamlMappingNode mapping;
        public YamlScalarNode node = null;
        public Encoding encode = Encoding.UTF8;

        /*コンストラクタ*/

        public Ylaml(){ }
        public Ylaml(string path) => Load(path);


        /*インデクサ*/

        public Ylaml this[string i]
        {
            get
            {
                var dst = new Ylaml();

                if (mapping == null) return dst;

                if (!mapping.Children.ContainsKey(new YamlScalarNode(i))) return dst;

                switch (mapping[new YamlScalarNode(i)])
                {
                    case YamlMappingNode n:
                        dst.mapping = n;
                        break;
                    case YamlScalarNode n:
                        dst.node = n;
                        break;
                    default:
                        break;
                }

                return dst;
            }
        }

        public T Value<T>(T defaultValue) 
        {
            return
                  typeof(T) == typeof(Int32) & int.TryParse(node.Value, out int i) ? (dynamic)(i)
                : typeof(T) == typeof(double) & double.TryParse(node.Value, out double j) ? (dynamic)(j)
                : typeof(T) == typeof(bool) & bool.TryParse(node.Value, out bool k) ? (dynamic)(k)
                : typeof(T) == typeof(string) ? (dynamic)(node.Value)
                : (T)Enum.Parse(typeof(T), node.Value, true);
            //switch (typeof(T))
            //{
            //    case :
            //        break;

            //}
            //return ;

            //foreach (var tuple in valuesMapping.Children)
            //{
            //    Console.WriteLine("{0} => {1}", tuple.Key, tuple.Value);
            //    Console.WriteLine(((YamlScalarNode)entry.Key).Value);
            //}
        }


        /*Deserialize*/

        public static T Deserialize<T>(string path)
        {
            using (var sr = new StreamReader(path))
            {
                var deserializer = new Deserializer();
                return deserializer.Deserialize<T>(sr);
            }
        }

        public void Load(string path)
        {
            using (var input = new StreamReader(path))
            {
                var yaml = new YamlStream();
                yaml.Load(input);

                //ルートマッピングの取得
                mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

                //var g = mapping[new YamlScalarNode("b")];
                //var Year = (YamlScalarNode)mapping.Children[new YamlScalarNode("b")];
            }
        }


        /*Serialize*/

        public static void Serialize(string path, object obj)
        {
            using (var sw = new StreamWriter(path))
            {
                var serializer = new Serializer();
                sw.WriteLine(serializer.Serialize(obj));
            }
        }

        public static string Serialize(object obj)
        {
            var serializer = new Serializer();
            return serializer.Serialize(obj);
        }

        public string Save()
        {
            var serializer = new Serializer();
            return serializer.Serialize(mapping);
        }


    }
}

//using (var input = new StreamReader("a.yaml", Encoding.UTF8))
//{
//    var yaml = new YamlStream();
//    yaml.Load(input);
//    var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

//    var g = mapping[new YamlScalarNode("b")];
//    //var Year = (YamlScalarNode)mapping.Children[new YamlScalarNode("b")];
//}

//var yamlSerializer = new Serializer<a>();
////C#nのオブジェクトに復元を行う
//var calendar = yamlSerializer.Deserialize(input);

//var deserializer = new Deserializer();
//var result = deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(reader);

//dst.mapping = (YamlMappingNode)mapping[new YamlScalarNode(i)];

/**/
