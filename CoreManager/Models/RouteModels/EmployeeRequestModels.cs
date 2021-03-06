﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Resources;

namespace CoreManager.Models
{
    public class EmployeeRequestModels :RouteRequestModel
    {
        public string Name { set; get; }
        public string Family { set; get; }
        public string Mobile { set; get; }
        public string Email { set; get; }
        public DateTime? TimeStart { set; get; }
        public DateTime? TimeEnd { set; get; }
        public string Latitude { set; get; }
        public string Longitude { set; get; }
        public bool Hasreturn { set; get; }
        public string Routeselect { set; get; }
        public string Entry { set; get; }
        public string Introduce { set; get; }
        public string Enterprise { set; get; }
        public string SrcStation { set; get; }
        public string DstStation { set; get; }

    }


}
