using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace RequestTracker
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {                
                //If user has AutoComplete turned on in his/her browser (Internet Explorer | Tools |
                //Internet Options | Content | AutoComplete | Forms), a second drop-down list 
                //will appear on top of the "Matches" drop-down list.  The following prevents
                //that from happening:
                
                this.form1.Attributes.Add("autocomplete", "off");

                this.TextBoxEmployee.Attributes.Add("onKeyDown", "return Emp_CheckForTab(event,this)");
                this.TextBoxEmployee.Attributes.Add("onKeyUp", "ShowEmpMatches(event)");

                this.TextBoxTechnician.Attributes.Add("onKeyDown", "return Emp_CheckForTab(event,this)");
                this.TextBoxTechnician.Attributes.Add("onKeyUp", "ShowEmpMatches(event)");

                this.DropDownListFromMonth.Attributes.Add("onChange", "document.forms[0].DropDownListToMonth.selectedIndex=document.forms[0].DropDownListFromMonth.selectedIndex");
                this.DropDownListFromYear.Attributes.Add("onChange", "document.forms[0].DropDownListToYear.selectedIndex=document.forms[0].DropDownListFromYear.selectedIndex");

                this.DropDownListStatus.Items.Add(new ListItem("", ""));
                this.DropDownListStatus.Items.Add(new ListItem("In Progress", "Open"));
                this.DropDownListStatus.Items.Add(new ListItem("Completed", "Closed"));

                this.DropDownListFromMonth.Items.Add(new ListItem("", "0"));
                this.DropDownListToMonth.Items.Add(new ListItem("", "0"));

                for (int month = 1; month < 13; month++)
                {
                    this.DropDownListFromMonth.Items.Add(new ListItem(month.ToString(), month.ToString()));
                    this.DropDownListToMonth.Items.Add(new ListItem(month.ToString(), month.ToString()));
                }

                this.DropDownListFromYear.Items.Add(new ListItem("", "0"));
                this.DropDownListToYear.Items.Add(new ListItem("", "0"));

                for (int year = 2008; year < DateTime.Now.Year + 2; year++)
                {
                    this.DropDownListFromYear.Items.Add(new ListItem(year.ToString(), year.ToString()));
                    this.DropDownListToYear.Items.Add(new ListItem(year.ToString(), year.ToString()));
                }

                string sql = "";
                SqlCommand sqlCommand;
                SqlDataReader sqlDataReader;
                string testTypeName = "";

                sql = "SELECT   t.TestID,t.TestName,tt.TestTypeName " +
                      "FROM     Test t " +
                               "JOIN TestType tt ON t.TestTypeID=tt.TestTypeID " +
                      "ORDER BY tt.TestTypeName,t.TestName";

                sqlCommand = new SqlCommand(sql, GetConnection());
                sqlCommand.CommandType = CommandType.Text;
                sqlDataReader = sqlCommand.ExecuteReader();

                this.DropDownListTest.Items.Add(new ListItem("", "0"));

                while (sqlDataReader.Read())
                {
                    if (sqlDataReader.GetString(2) != testTypeName)
                    {
                        if (testTypeName != "")
                        {
                            this.DropDownListTest.Items.Add(new ListItem("", "0"));
                        }

                        testTypeName = sqlDataReader.GetString(2);

                        this.DropDownListTest.Items.Add(new ListItem("---------- " + testTypeName + " ------------------------------------------------------------", "0"));
                        //this.DropDownListTest.Items.Add(new ListItem("", "0"));
                    }

                    this.DropDownListTest.Items.Add(new ListItem(sqlDataReader.GetString(1), sqlDataReader.GetInt16(0).ToString()));
                }

                sqlDataReader.Close();
                sqlDataReader.Dispose();
                sqlCommand.Connection.Close();
                sqlCommand.Dispose();

                if (Session["JobID"] != null)
                {
                    this.TextBoxJobNumber.Text = Session["JobID"].ToString();
                }

                if (Session["Requestor"] != null)
                {
                    this.TextBoxEmployee.Text = Session["Requestor"].ToString();
                }

                if (Session["Technician"] != null)
                {
                    this.TextBoxTechnician.Text = Session["Technician"].ToString();
                }

                if (Session["Status"] != null)
                {
                    this.DropDownListStatus.SelectedValue = Session["Status"].ToString();
                }

                if (Session["FromMonth"] != null)
                {
                    this.DropDownListFromMonth.SelectedValue = Session["FromMonth"].ToString();
                    this.DropDownListFromYear.SelectedValue = Session["FromYear"].ToString();
                    this.DropDownListToMonth.SelectedValue = Session["ToMonth"].ToString();
                    this.DropDownListToYear.SelectedValue = Session["ToYear"].ToString();
                }

                if (Session["Test"] != null)
                {
                    this.DropDownListTest.SelectedValue = Session["Test"].ToString();
                }

                if (Session["Notes"] != null)
                {
                    this.TextBoxNotes.Text = Session["Notes"].ToString();
                }

                //Remove old search criteria:
                Session.Remove("JobID");
                Session.Remove("Requestor");
                Session.Remove("Technician");
                Session.Remove("Status");
                Session.Remove("FromMonth");
                Session.Remove("FromYear");
                Session.Remove("ToMonth");
                Session.Remove("ToYear");
                Session.Remove("Test");
                Session.Remove("Notes");


                //Remove old order-by criteria:
                Session.Remove("SortColumn");
                Session.Remove("DESC");
            }
        }

        private SqlConnection GetConnection()
        {
            SqlConnection sqlConnection = new SqlConnection();
            string dataSource = "Data Source=" +  ConfigurationManager.AppSettings["DataSource"];
            string initialCatalog = "Initial catalog=" + ConfigurationManager.AppSettings["InitialCatalog"];
            const string uid = "quality_web";
            const string pwd = "QualityUser1";

            sqlConnection.ConnectionString = dataSource + ";UID=" + uid + ";PWD=" + pwd + ";" + initialCatalog;

            sqlConnection.Open();

            return sqlConnection;
        }

        protected void ButtonSearch_Click(object sender, EventArgs e)
        {
            string fromMonth = this.DropDownListFromMonth.SelectedValue;
            string fromYear = this.DropDownListFromYear.SelectedValue;
            string toMonth = this.DropDownListToMonth.SelectedValue;
            string toYear = this.DropDownListToYear.SelectedValue;

            if (this.TextBoxJobNumber.Text != "")
            {
                Session["JobID"] = this.TextBoxJobNumber.Text;
                //When a JobID is provided, any other search criteria are ignored.
            }
            else
            {
                if (this.TextBoxEmployee.Text != "")
                {
                    Session["Requestor"] = this.TextBoxEmployee.Text;
                }

                if (this.TextBoxTechnician.Text != "")
                {
                    Session["Technician"] = this.TextBoxTechnician.Text;
                }

                if (this.DropDownListStatus.SelectedValue != "")
                {
                    Session["Status"] = this.DropDownListStatus.SelectedValue;
                }

                if (fromMonth != "0" && fromYear != "0" && toMonth != "0" && toYear != "0")
                {
                    Session["FromMonth"] = fromMonth;
                    Session["FromYear"] = fromYear;
                    Session["ToMonth"] = toMonth;
                    Session["ToYear"] = toYear;
                }

                if (this.DropDownListTest.SelectedIndex != 0)
                {
                    Session["Test"] = this.DropDownListTest.SelectedValue;
                }

                if (this.TextBoxNotes.Text != "")
                {
                    Session["Notes"] = this.TextBoxNotes.Text;
                }
            }

            Response.Redirect("SearchResults.aspx");
        }

        protected void ButtonClear_Click(object sender, EventArgs e)
        {
            this.TextBoxJobNumber.Text = "";
            this.TextBoxEmployee.Text = "";
            this.TextBoxTechnician.Text = "";
            this.DropDownListStatus.SelectedIndex = 0;
            this.DropDownListFromMonth.SelectedIndex = 0;
            this.DropDownListFromYear.SelectedIndex = 0;
            this.DropDownListToMonth.SelectedIndex = 0;
            this.DropDownListToYear.SelectedIndex = 0;
            this.DropDownListTest.SelectedIndex = 0;
            this.TextBoxNotes.Text = "";
        }

    }
}
