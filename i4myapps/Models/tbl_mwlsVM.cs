using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace i4myapps.Models
{
    public class tbl_mwlsVM
    {

        public int pk { get; set; }
        [Required(ErrorMessage = "Please Enter Accession No.")]
        public string acc_no { get; set; }
        [Required(ErrorMessage = "Please Enter ID")]
        public string pat_id { get; set; }
        [Required(ErrorMessage = "Please Enter Patient Name")]
        public string pat_name { get; set; }
        [Required(ErrorMessage = "Please Enter Sex")]
        public string pat_sex { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyy}", ApplyFormatInEditMode = true)]
        public string pat_dob { get; set; }
        [Required(ErrorMessage = "Please Enter Referring Physician")]
        public string referring_physician { get; set; }
        [Required]
        public string sr_description { get; set; }
        [Required]
        public string status { get; set; }
        public bool IsDeleted { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyy}", ApplyFormatInEditMode = true)]
        public string scheddate { get; set; }

    }
}