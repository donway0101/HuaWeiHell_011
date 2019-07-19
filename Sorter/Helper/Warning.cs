using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sorter
{
    public class Warning
    {
        public WarningCode Code { get; set; }
        public string Message { get; set; }
        public string Remarks { get; set; }
    }

    public enum WarningCode
    {

    }
}
