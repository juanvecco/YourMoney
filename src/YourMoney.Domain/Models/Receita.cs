﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourMoney.Domain.Models
{
    public class Receita : BaseEntity
    {
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string Categoria { get; set; }
    }
}
