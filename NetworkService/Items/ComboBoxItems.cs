using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Items
{
    public class ComboBoxItems
    {
        public static Dictionary<string, string> entityTypes = new Dictionary<string, string>()
        {
            { "Solar", "pack://application:,,,/NetworkService;component/Images/Solar.jpeg" },
            { "Wind", "pack://application:,,,/NetworkService;component/Images/Wind.jpg" }
        };
    }
}
