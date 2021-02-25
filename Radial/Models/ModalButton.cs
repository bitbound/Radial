using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Models
{
    public class ModalButton
    {
        public string Class { get; set; }
        public string Text { get; set; }

        public Action OnClick { get; set; }
    }
}
