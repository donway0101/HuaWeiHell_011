using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter
{
    public enum StationId
    {
       V,
       L,
       GLine,
       GPoint,
    }

    /// <summary>
    /// For conveyors, and sucker vacuum head.
    /// </summary>
    public enum ProcedureId
    {
        Load,
        Unload,
    }
}
