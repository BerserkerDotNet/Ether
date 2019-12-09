using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NPOI.SS.UserModel;

namespace Ether.Types.Excel
{
    public class ExcelColumn
    {
        public ExcelColumn(string name, bool isDisplayed = true)
        {
            Name = name;
            IsDisplayed = isDisplayed;
        }

        public string Name { get; private set; }

        public bool IsDisplayed { get; set; }
    }
}
