using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models;

namespace CoreManager.DriverManager
{
    public interface IDriverManager
    {
        GasScore GetGasScores(Guid userId);
        List<GasRank> GasRanks(Guid uid);
    }
}
