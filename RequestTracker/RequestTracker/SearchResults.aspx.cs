using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace RequestTracker
{
    public partial class SearchResults : System.Web.UI.Page
    {
        string newSortColumn = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                DataGridJobs.DataKeyField = "JobID";

                if (Session["SortColumn"] == null)
                {
                    //New Session. Use default sort column:
                    Session["SortColumn"] = "";
                    Session["DESC"] = false;
                    newSortColumn = "EntryDate";
                }
                else
                {
                    //newSortColumn == "".  Set it to last-used sort column:
                    newSortColumn = Session["SortColumn"].ToString();
                    Session["DESC"] = !(bool)Session["DESC"];
                }

                BindData();
            }
        }

        private void BindData()
        {
            ArrayList jobs = GetJobs();
            DataGridJobs.DataSource = jobs;
            DataGridJobs.DataBind();
            this.LiteralNumRecords.Text = jobs.Count.ToString();
        }

        private ArrayList GetJobs()
        {
            string sql;
            SqlCommand sqlCommand;
            SqlDataReader sqlDataReader;
            string whereClause = BuildWhereClause();
            string orderByClause = BuildOrderByClause();
            string formURL;
            string jobID;
            ArrayList jobs = new ArrayList();

            if (Session["Test"] != null)
            {
                sql = "SELECT   j.JobID,r.LastName + ', ' + r.FirstName," +
                               "CONVERT (varchar,j.EntryDate,107)," +
                               "ISNULL(CONVERT (varchar,j.CompletionDate,107),'')," +
                               "j.Quantity,nt.NumTests,SUBSTRING(j.Notes,0,40) + '...'," +
                               "t.LastName + ', ' + t.FirstName " +
                      "FROM     Job j " +
                               "JOIN Employee r ON j.RequestorID=r.EmployeeID " +
                               "JOIN Employee t ON j.TechnicianID=t.EmployeeID " +
                               "JOIN TestRequest tr ON j.JobID=tr.JobID " +
                               "JOIN (SELECT   j.JobID,COUNT(*) NumTests " +
                                     "FROM     Job j " +
                                              "JOIN TestRequest tr ON j.JobID=tr.JobID " +
                                     "GROUP BY j.JobID)nt ON j.JobID=nt.JobID " +
                               "JOIN Test t2 ON tr.TestID=t2.TestID " +
                       whereClause +
                       orderByClause;
            }
            else
            {
                sql = "SELECT   j.JobID,r.LastName + ', ' + r.FirstName," +
                               "CONVERT (varchar,j.EntryDate,107)," +
                               "ISNULL(CONVERT (varchar,j.CompletionDate,107),'')," +
                               "j.Quantity,nt.NumTests,SUBSTRING(j.Notes,0,40) + '...'," +
                               "t.LastName + ', ' + t.FirstName " +
                      "FROM     Job j " +
                               "JOIN Employee r ON j.RequestorID=r.EmployeeID " +
                               "JOIN Employee t ON j.TechnicianID=t.EmployeeID " +
                               "JOIN (SELECT   j.JobID,COUNT(*)NumTests " +
                                     "FROM     Job j " +
                                              "JOIN TestRequest tr ON j.JobID=tr.JobID " +
                                     "GROUP BY j.JobID)nt ON j.JobID=nt.JobID " +
                       whereClause +
                       orderByClause;
            }

            sqlCommand = new SqlCommand(sql, GetConnection());
            sqlCommand.CommandType = CommandType.Text;
            sqlDataReader = sqlCommand.ExecuteReader();

            jobs = new ArrayList();

            while (sqlDataReader.Read())
            {
                jobID = sqlDataReader.GetInt16(0).ToString();

                if (File.Exists(ConfigurationManager.AppSettings["FormsDirectory"] + jobID + ".pdf"))
                {
                    formURL = "<a href=\"TestRequestForms/" + jobID + ".pdf\" target=\"_blank\">form</a>";
                }
                else
                {
                    formURL = "";
                }

                jobs.Add(new FlattenedJob(jobID, sqlDataReader.GetString(1), sqlDataReader.GetString(2), sqlDataReader.GetString(3), sqlDataReader.GetInt16(4).ToString(), sqlDataReader.GetInt32(5).ToString(), sqlDataReader.GetString(6).ToString(), formURL, sqlDataReader.GetString(7)));
            }

            sqlDataReader.Close();
            sqlDataReader.Dispose();
            sqlCommand.Connection.Close();
            sqlCommand.Dispose();

            return jobs;
        }

        private string BuildWhereClause()
        {
            StringBuilder whereClause = new StringBuilder();
            string[] names;
            string location;
            int toYear;
            int toMonth;

            if (Session["JobID"] != null)
            {
                whereClause.Append(" j.JobID=");
                whereClause.Append(Session["JobID"]);
            }

            if (Session["Requestor"] != null)
            {
                if (Session["Requestor"].ToString().Contains(",") & Session["Requestor"].ToString().Contains("[") & Session["Requestor"].ToString().Contains("]"))
                {
                    //Session["Requestor"] = "Blow, Joe [Chanhassen]"
                    names = Session["Requestor"].ToString().Split(',');
                    location = names[1].Substring(names[1].IndexOf("[") + 1, names[1].Length - (names[1].IndexOf("[") + 2)).Trim();
                    names[1] = names[1].Substring(0, names[1].IndexOf("[") - 1).Trim();

                    if (whereClause.Length > 0)
                    {
                        whereClause.Append(" AND");
                    }

                    whereClause.Append(" r.LastName='");
                    whereClause.Append(names[0]);
                    whereClause.Append("' AND r.FirstName='");
                    whereClause.Append(names[1]);
                    whereClause.Append("' AND r.Location='");
                    whereClause.Append(location);
                    whereClause.Append("'");
                }
            }

            if (Session["Technician"] != null)
            {
                if (Session["Technician"].ToString().Contains(","))
                {
                    //Session["Technician"] = "Blow, Joe [Chanhassen]"
                    names = Session["Technician"].ToString().Split(',');
                    location = names[1].Substring(names[1].IndexOf("[") + 1, names[1].Length - (names[1].IndexOf("[") + 2)).Trim();
                    names[1] = names[1].Substring(0, names[1].IndexOf("[") - 1).Trim();

                    if (whereClause.Length > 0)
                    {
                        whereClause.Append(" AND");
                    }

                    whereClause.Append(" t.LastName='");
                    whereClause.Append(names[0]);
                    whereClause.Append("' AND t.FirstName='");
                    whereClause.Append(names[1]);
                    whereClause.Append("' AND t.Location='");
                    whereClause.Append(location);
                    whereClause.Append("'");
                }
            }

            if (Session["Status"] != null)
            {
                if (whereClause.Length > 0)
                {
                    whereClause.Append(" AND");
                }

                if (Session["Status"].ToString() == "Open")
                {
                    whereClause.Append(" j.CompletionDate IS NULL");
                }
                else
                {
                    whereClause.Append(" j.CompletionDate IS NOT NULL");
                }
            }

            if (Session["FromMonth"] != null)
            {
                toYear = Convert.ToInt32(Session["ToYear"]);
                toMonth = Convert.ToInt32(Session["ToMonth"]);

                if (toMonth == 12)
                {
                    toMonth = 1;
                    toYear += 1; 
                }
                else
                {
                    toMonth += 1;
                }

                if (whereClause.Length > 0)
                {
                    whereClause.Append(" AND");
                }

                whereClause.Append(" j.CompletionDate>='");
                whereClause.Append(Session["FromYear"]);
                whereClause.Append("-");
                whereClause.Append(Session["FromMonth"]);
                whereClause.Append("-01' AND j.CompletionDate<'");
                whereClause.Append(toYear.ToString());
                whereClause.Append("-");
                whereClause.Append(toMonth.ToString());
                whereClause.Append("-01'");
            }

            if (Session["Test"] != null)
            {
                if (whereClause.Length > 0)
                {
                    whereClause.Append(" AND");
                }

                whereClause.Append(" t2.TestID=");
                whereClause.Append(Session["Test"]);
            }

            if (Session["Notes"] != null)
            {
                if (whereClause.Length > 0)
                {
                    whereClause.Append(" AND");
                }

                whereClause.Append(" j.Notes LIKE '%");
                whereClause.Append(Session["Notes"]);
                whereClause.Append("%'");
            }

            if (whereClause.Length > 0)
            {
                whereClause.Insert(0, " WHERE");
                return whereClause.ToString();
            }
            else
            {
                return "";
            }
        }

        private string BuildOrderByClause()
        {
            string orderByClause = "";
            string currentSortColumn = Session["SortColumn"].ToString();
            bool currentDesc = (bool)Session["DESC"];
            bool newDesc = false;
            string arrowGif = "<img src=\"uparrow.gif\" height=\"13\" width=\"13\" border=\"0\">";

            if (currentSortColumn == newSortColumn)
            {
                if (currentDesc)
                {
                    //DESC to ASC:
                    newDesc = false;
                    arrowGif = "<img src=\"uparrow.gif\" height=\"13\" width=\"13\" border=\"0\">";
                }
                else
                {
                    //ASC to DESC:
                    newDesc = true;
                    arrowGif = "<img src=\"downarrow.gif\" height=\"13\" width=\"13\" border=\"0\">";
                }
            }

            Session["DESC"] = newDesc;

            //Reset Headers (to remove ArrowGif from last sort-by column):
            DataGridJobs.Columns[0].HeaderText = "Request #";
            DataGridJobs.Columns[1].HeaderText = "Requestor";
            DataGridJobs.Columns[2].HeaderText = "Quantity";
            DataGridJobs.Columns[3].HeaderText = "Entry Date";
            DataGridJobs.Columns[4].HeaderText = "Completion Date";
            DataGridJobs.Columns[5].HeaderText = "# of Tests";
            DataGridJobs.Columns[6].HeaderText = "Technician";
            DataGridJobs.Columns[8].HeaderText = "Notes";

            switch (newSortColumn)
            {
                case "JobID":
                    orderByClause = "j.JobID";

                    if (newDesc)
                    {
                        orderByClause += " DESC";
                    }

                    DataGridJobs.Columns[0].HeaderText = "Request #&nbsp;" + arrowGif;

                    break;

                case "Requestor":
                    if (newDesc)
                    {
                        orderByClause = "r.LastName DESC,r.FirstName DESC,j.JobID DESC";
                    }
                    else
                    {
                        orderByClause = "r.LastName,r.FirstName,j.JobID";
                    }

                    DataGridJobs.Columns[1].HeaderText = newSortColumn + "&nbsp;" + arrowGif;

                    break;

                case "Quantity":
                    orderByClause = "j.Quantity";

                    if (newDesc)
                    {
                        orderByClause += " DESC";
                    }

                    DataGridJobs.Columns[2].HeaderText = newSortColumn + "&nbsp;" + arrowGif;

                    break;

                case "EntryDate":
                    orderByClause = "j.EntryDate";

                    if (newDesc)
                    {
                        orderByClause += " DESC";
                    }

                    DataGridJobs.Columns[3].HeaderText = "Entry Date&nbsp;" + arrowGif;

                    break;

                case "CompletionDate":
                    orderByClause = "j.CompletionDate";

                    if (newDesc)
                    {
                        orderByClause += " DESC";
                    }

                    DataGridJobs.Columns[4].HeaderText = "Completion Date&nbsp;" + arrowGif;

                    break;

                case "TotalTests":
                    orderByClause = "nt.NumTests";

                    if (newDesc)
                    {
                        orderByClause += " DESC";
                    }

                    DataGridJobs.Columns[5].HeaderText = "# of Tests&nbsp;" + arrowGif;

                    break;

                case "Technician":
                    if (newDesc)
                    {
                        orderByClause = "t.LastName DESC,t.FirstName DESC,j.JobID DESC";
                    }
                    else
                    {
                        orderByClause = "t.LastName,t.FirstName,j.JobID";
                    }

                    DataGridJobs.Columns[6].HeaderText = newSortColumn + "&nbsp;" + arrowGif;

                    break;

                case "Notes":
                    orderByClause = "SUBSTRING(j.Notes,0,40)";

                    if (newDesc)
                    {
                        orderByClause += " DESC";
                    }

                    DataGridJobs.Columns[8].HeaderText = newSortColumn + "&nbsp;" + arrowGif;

                    break;
            }

            Session["SortColumn"] = newSortColumn;

            return " ORDER BY " + orderByClause;
        }

        protected void DataGridJobs_SortCommand(object source, DataGridSortCommandEventArgs e)
        {
            newSortColumn = e.SortExpression;
            BindData();
        }

        private SqlConnection GetConnection()
        {
            SqlConnection sqlConnection = new SqlConnection();
            string dataSource = "Data Source=" + ConfigurationManager.AppSettings["DataSource"];
            string initialCatalog = "Initial catalog=" + ConfigurationManager.AppSettings["InitialCatalog"];
            const string uid = "quality_web";
            const string pwd = "QualityUser1";

            sqlConnection.ConnectionString = dataSource + ";UID=" + uid + ";PWD=" + pwd + ";" + initialCatalog;

            sqlConnection.Open();

            return sqlConnection;
        }

        private class FlattenedJob
        {
            private string _JobID;
            private string _Requestor;
            private string _Quantity;
            private string _EntryDate;
            private string _CompletionDate;
            private string _TotalTests;
            private string _FormURL;
            private string _Notes;
            private string _Technician;

            public string JobID
            {
                get
                {
                    return _JobID;
                }
            }

            public string URL
            {
                get
                {
                    return "JobDetails.aspx?j=" + _JobID;
                }
            }

            public string Requestor
            {
                get
                {
                    return _Requestor;
                }
            }

            public string Quantity
            {
                get
                {
                    return _Quantity;
                }
            }

            public string EntryDate
            {
                get
                {
                    return _EntryDate;
                }
            }

            public string CompletionDate
            {
                get
                {
                    return _CompletionDate;
                }
            }

            public string TotalTests
            {
                get
                {
                    return _TotalTests;
                }
            }

            public string FormURL
            {
                get
                {
                    return _FormURL;
                }
            }

            public string Notes
            {
                get
                {
                    return _Notes;
                }
            }

            public string Technician
            {
                get
                {
                    return _Technician;
                }
            }

            public FlattenedJob(string jobID, string requestor, string entryDate, string completionDate, string quantity, string totalTests, string notes, string formURL, string technician)
            {
                this._JobID = jobID;
                this._Requestor = requestor;
                this._EntryDate = entryDate;
                this._CompletionDate = completionDate;
                this._Quantity = quantity;
                this._TotalTests = totalTests;
                this._Notes = notes;
                this._FormURL = formURL;
                this._Technician = technician;
            }

        }
    }
}
