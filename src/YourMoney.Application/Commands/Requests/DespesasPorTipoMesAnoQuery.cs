using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourMoney.Application.Commands.Requests
{
    public class DespesasPorTipoMesAnoQuery
    {
        public int IdTipoDespesa { get; set; }
        public int Mes { get; set; }
        public int Ano { get; set; }
    }

}
