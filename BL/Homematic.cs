using System;
using System.Collections;
using System.Collections.Generic;

namespace XmlRpc.BL
{
    public class Homematic
    {
        public Homematic()
        {
        }

        public void Multicall(IEnumerable<IDictionary<string, object>> obj)
        {
            foreach (var item in obj)
            {
                var n = item["methodName"] as string;
                Console.WriteLine(n);

                var p = item["params"];
            }
        }
    }
}