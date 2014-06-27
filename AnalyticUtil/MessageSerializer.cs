#region .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

#region Third Party
using CalcFarm.AnalyticUtil.Entity;

using NetSerializer;
#endregion

namespace CalcFarm.AnalyticUtil
{
    public class MessageSerializer
    {
        static MessageSerializer()
        {
            Init();
        }

        public static void Init()
        {
            // Register all serialized types here.
            Type[] knownTypes = { typeof(Instrument), typeof(Price), typeof(EquityPrice), typeof(CalcResult), typeof(EqtyUpdPnlCalcResult), typeof(FxUpdCalcResult), typeof(RatesUpdCalcResult), typeof(ForwardFutureUpdCalcResult) }; 
            NetSerializer.Serializer.Initialize(knownTypes);
        }

        /*
         * Serialization is high traffic area, general concensus is that "System.Runtime.Serialization.DataContractSerializer" and "System.Xml.Serialization.XmlSerializer" are relatively slow.
         * For demo purpose, we've choosen "NetSerializer" - a free serializer from Codeproject.com. Source from Git: https://github.com/tomba/netserializer
         * REF:
         * http://maxondev.com/serialization-performance-comparison-c-net-formats-frameworks-xmldatacontractserializer-xmlserializer-binaryformatter-json-newtonsoft-servicestack-text/
         * http://www.codeproject.com/Articles/351538/NetSerializer-A-Fast-Simple-Serializer-for-NET
         */
        public static byte[] Serialize(object o)
        {
            byte[] bytes = null;
            System.IO.MemoryStream st = new System.IO.MemoryStream();
            NetSerializer.Serializer.Serialize(st, o);
            int Size = Convert.ToInt32(st.Length);
            bytes = new byte[Size];
            st.Position = 0;
            bytes = st.ToArray();
            return bytes;
        }

        public static object Deserialize(byte[] bytes)
        {
            object o = null;
            System.IO.MemoryStream st = new System.IO.MemoryStream(bytes);
            o = NetSerializer.Serializer.Deserialize(st);
            return o;
        }
    }
}
