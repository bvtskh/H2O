namespace UMC_Hydroelectricity.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Tbl_Hydroelectric
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string HydroelectricName { get; set; }

        public double WaterLevel { get; set; }

        public double DeadWaterLevel { get; set; }

        public double WaterFlow { get; set; }

        public DateTime UpdateTime { get; set; }

        public int AreaNumber { get; set; }
    }
}
