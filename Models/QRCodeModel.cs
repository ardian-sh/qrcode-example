using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GeneratingQRCode.Models
{
    public class QRCodeModel
    {
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Enter QRCode Text")]
        public string QrText { get; set; }
    }
}
