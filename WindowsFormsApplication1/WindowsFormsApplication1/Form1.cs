﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // This line of code is generated by Data Source Configuration Wizard
            // Fill a ExcelDataSource asynchronously
            excelDataSource1.FillAsync();
        }

        private void imageSlider1_ImageChanged(object sender, DevExpress.XtraEditors.Controls.ImageChangedEventArgs e)
        {
            //labelControl1.Text = imageSlider1.CurrentImageIndex.ToString();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            //imageSlider1.SetCurrentImageIndex(1);
            imageSlider1.SlideNext();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            if (panelControl1.Visible ==true)
            {
                panelControl1.Visible = false;
            }
            else
            {
                panelControl1.Visible = true;
            }
        }
    }
}
