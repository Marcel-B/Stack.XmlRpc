using System.Collections.Generic;
using com.b_velop.XmlRpc.Models;

namespace com.b_velop.XmlRpc.BL
{
    public class HomematicValueList : List<HomematicValue>
    {
        public HomematicValue[] WithdrawItems()
        {
            var returnValues = ToArray();
            Clear();
            return returnValues;
        }
    }
}
