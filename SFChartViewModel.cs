using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Additional

namespace EyeMovementAnalyzer
{
    public class EyeDataForChart
    {
        public Double TimestampInMs { get; set; }
        public Double XCoordinate { get; set; }
        public Double AngularVelocity { get; set; }
    }

    class SFChartViewModel
    {
        public List<EyeDataForChart> Data { get; set; }

        public SFChartViewModel()
        {
            this.Data = new List<EyeDataForChart>();
        }
    }
}
