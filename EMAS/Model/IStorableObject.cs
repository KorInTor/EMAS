using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model
{
    public interface IStorableObject
    {
        public int Id { get; set; }
        public string ShortInfo { get; }
    }
}
