﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace DevExpressDemo1
{
    public partial class DataAndAnalytics_gridControl : DevExpress.XtraEditors.XtraForm
    {
        public DataAndAnalytics_gridControl()
        {
            InitializeComponent();


            // This line of code is generated by Data Source Configuration Wizard
            // Fill a ExcelDataSource asynchronously
            excelDataSource1.FillAsync();

        }
    }
}