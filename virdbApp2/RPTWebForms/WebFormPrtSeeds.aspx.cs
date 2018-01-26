using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

//vovkor
using System.Data;
using System.Data.SqlClient;
using Microsoft.Reporting.WebForms;


namespace virdbApp2.RPTWebForms
{
    public partial class WebFormPrtSeeds : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        //vovkor
        private void ShowReport()
        {
            //Reset
            rptViewer.Reset();

            //DataSource
            DataTable dt = GetData(txtAccenumber.Text);
            ReportDataSource rds = new ReportDataSource("DataSetRptSeeds", dt); // DataSet1 - это свойство DataSetName из Табликса
            rptViewer.LocalReport.DataSources.Add(rds);

            //Path
            rptViewer.LocalReport.ReportPath = "RPTReports/rptSeeds.rdlc";

            //Parameters
            ReportParameter[] rptParams = new ReportParameter[] {
                new ReportParameter("ACC", txtAccenumber.Text)
            };
            rptViewer.LocalReport.SetParameters(rptParams);

            //Refresh
            rptViewer.LocalReport.Refresh();

        }
        private DataTable GetData(string ACC)
        {
            DataTable dt = new DataTable();
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["virdb2ConnectionStringRpt"].ConnectionString;
            using (SqlConnection cn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("GetRptSeeds", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ACC", SqlDbType.NVarChar).Value = ACC; // связь параметра ReportViwer-a и хранимой процедуры GetBoxesReport1

                SqlDataAdapter adp = new SqlDataAdapter(cmd);
                adp.Fill(dt);

            }
            return dt;
        }
        protected void btnShow_Click(object sender, EventArgs e)
        {
            ShowReport();
        }
    }
}