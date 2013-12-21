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
    public partial class JobDetails : System.Web.UI.Page
    {
        private ArrayList testRequests;
        private ArrayList tests;
        private ArrayList runOrders;
        private ArrayList restarts;
        private const char fieldDelimiter = ';';
        private const char objectDelimiter = '*';
        private short employeeID;
        private short technicianID;
        private string jobID;

        private ArrayList jobHangUps;
        private ArrayList hangUps;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                //If user has AutoComplete turned on in his/her browser (Internet Explorer | Tools |
                //Internet Options | Content | AutoComplete | Forms), a second drop-down list 
                //will appear on top of the "Matches" drop-down list.  The following prevents
                //that from happening:

                this.form1.Attributes.Add("autocomplete", "off");

                if (Request.QueryString["j"] != null)
                {
                    //Existing Request.  User got here by way of SearchResults.aspx.
                    jobID = Request.QueryString["j"];
                }
                else
                {
                    //New Request.  User got here by way of home.aspx.
                    jobID = "0";
                }

                this.ViewState["JobID"] = jobID;

                this.TextBoxEmployee.Attributes.Add("onKeyDown", "return Emp_CheckForTab(event,this)");
                this.TextBoxEmployee.Attributes.Add("onKeyUp", "ShowEmpMatches(event)");

                this.TextBoxTechnician.Attributes.Add("onKeyDown", "return Emp_CheckForTab(event,this)");
                this.TextBoxTechnician.Attributes.Add("onKeyUp", "ShowEmpMatches(event)");

                this.ButtonDelete.Attributes.Add("onclick", "return confirm('Delete data?')");
                this.ButtonSave.Attributes.Add("onclick", "return confirm('Save changes?');");

                //If the following is done in the Properties window, the date entered
                //by the user will not be retained on post back.  This is a known issue.
                TextBoxDate.Attributes.Add("readonly", "readonly");

                this.DropDownListQuantity.Items.Add(new ListItem("", "0"));

                for (int qty = 1; qty < 1000; qty++)
                {
                    this.DropDownListQuantity.Items.Add(new ListItem(qty.ToString(), qty.ToString()));
                }

                ShowJobData(jobID);

            } else {
                jobID = this.ViewState["JobID"].ToString();
                this.LabelFormStatus.Text = "";
                testRequests = UnpackageTestRequests(this.ViewState["TestRequests"].ToString());
                hangUps = UnpackageJobHangUps(this.ViewState["JobHangUps"].ToString());
            }
        }

        private void ShowJobData(string jobID)
        {
            string sql;
            SqlCommand sqlCommand;
            SqlDataReader sqlDataReader;
            DateTime startDate;
            DateTime requestDate;
            DateTime promiseDate;
            DateTime receiveDate;
            DateTime completionDate;

            if (jobID != "0")
            {
                //Existing Request.  User got here by way of SearchResults.aspx.

                BuildFormLink(jobID);   //build pdf link

                sql = "SELECT r.LastName + ', ' + r.FirstName + ' [' + r.Location + ']',j.Notes,j.Quantity," +
                             "j.StartDate,j.RequestDate,j.PromiseDate,j.ReceiveDate," +
                             "j.CompletionDate,j.EntryDate,t.LastName + ', ' + t.FirstName + ' [' + t.Location + ']' " +
                      "FROM   Job j " +
                             "JOIN Employee r ON j.RequestorID=r.EmployeeID " +
                             "JOIN Employee t ON j.TechnicianID=t.EmployeeID " +
                      "WHERE  j.JobID=" + jobID;

                sqlCommand = new SqlCommand(sql, GetConnection());
                sqlCommand.CommandType = CommandType.Text;
                sqlDataReader = sqlCommand.ExecuteReader();

                if (sqlDataReader.Read())
                {
                    this.LiteralJobNumber.Text = jobID;
                    this.TextBoxEmployee.Text = sqlDataReader.GetString(0);
                    this.TextBoxNotes.Text = sqlDataReader.GetString(1);
                    this.DropDownListQuantity.SelectedIndex = sqlDataReader.GetInt16(2);

                    if (!sqlDataReader.IsDBNull(3))
                    {
                        startDate = sqlDataReader.GetDateTime(3);
                        this.TextBoxStartDate.Text = ReformatDate(sqlDataReader.GetDateTime(3));
                    }
                    
                    if(!sqlDataReader.IsDBNull(4)){
                        requestDate = sqlDataReader.GetDateTime(4);
                        this.TextBoxRequestDate.Text = ReformatDate(sqlDataReader.GetDateTime(4));
                    }

                    if (!sqlDataReader.IsDBNull(5))
                    {
                        promiseDate = sqlDataReader.GetDateTime(5);
                        this.TextBoxPromiseDate.Text = ReformatDate(sqlDataReader.GetDateTime(5));
                        this.TextBoxPromiseDate.Enabled = false;
                            //need to disable calendar and button
                    }

                    if (!sqlDataReader.IsDBNull(6))
                    {
                        receiveDate = sqlDataReader.GetDateTime(6);
                        this.TextBoxReceiveDate.Text = ReformatDate(sqlDataReader.GetDateTime(6));
                    }

                    if (!sqlDataReader.IsDBNull(7))
                    {
                        completionDate = sqlDataReader.GetDateTime(7);
                        this.TextBoxDate.Text = ReformatDate(sqlDataReader.GetDateTime(7));
                    }

                    this.LiteralEntryDate.Text = ReformatDate(sqlDataReader.GetDateTime(8));
                    this.TextBoxTechnician.Text = sqlDataReader.GetString(9);

                }

                sqlDataReader.Close();
                sqlDataReader.Dispose();
                sqlCommand.Connection.Close();
                sqlCommand.Dispose();

                testRequests = GetTestRequests(jobID);
                jobHangUps = GetJobHangUps(jobID);

                this.ButtonBack.Visible = true;
                this.ButtonDelete.Visible = true;
            }
            else
            {
                //New Request.  User got here by way of home.aspx.
                testRequests = new ArrayList();
                this.PdfHyperLink.Visible = false;
                this.ButtonBack.Visible = false;
                this.ButtonDelete.Visible = false;
            }

            BindData(testRequests);
            BindDataHangUp(jobHangUps);
            
        }

        private void BuildFormLink(string jobID)
        {
            if (File.Exists(ConfigurationManager.AppSettings["FormsDirectory"] + jobID + ".pdf"))
            {
                this.PdfHyperLink.NavigateUrl = "TestRequestForms/" + jobID + ".pdf";
                this.PdfHyperLink.Visible = true;
            }
            else
            {
                this.PdfHyperLink.Visible = false;
            }
        }

        private void ClearForm()
        {
            this.LiteralJobNumber.Text = "";
            this.TextBoxEmployee.Text = "";
            this.LiteralEntryDate.Text = "";
            this.DropDownListQuantity.SelectedIndex = 0;
            testRequests = new ArrayList();
            this.TextBoxTechnician.Text = "";
            this.TextBoxNotes.Text = "";
            this.TextBoxDate.Text = "";
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            this.ViewState["TestRequests"] = PackageTestRequests(testRequests);
            this.ViewState["JobHangUps"] = PackageJobHangUps(jobHangUps);

            foreach (DictionaryEntry s in this.ViewState)
            {
                string srt = s.Key.ToString();
            }
        }

        private ArrayList UnpackageJobHangUps(string joinedJobHangUps)
        {
            char[] objectSeparator = { objectDelimiter };
            char[] fieldSeparator = { fieldDelimiter };
            string[] fields;
            FlattenedJobHangUps jh;

            jobHangUps = new ArrayList();

            if (joinedJobHangUps != "")
            {
                foreach (string request in joinedJobHangUps.Split(objectSeparator))
                {
                    fields = request.Split(fieldSeparator);
                    jh = new FlattenedJobHangUps();
                    jh.JobHangUpID = Convert.ToInt16(fields[0]);
                    jh.HangUpID = Convert.ToInt16(fields[1]);
                    jh.HangUpName = fields[2];
                    jh.Status = (FlattenedJobHangUps.TestHangUpStatus)Convert.ToInt16(fields[3]);

                    testRequests.Add(jh);
                }
            }

            return testRequests;
        }

        private ArrayList UnpackageTestRequests(string joinedTestRequests)
        {
            char[] objectSeparator = {objectDelimiter};
            char[] fieldSeparator = {fieldDelimiter};
            string[] fields;
            FlattenedTestRequest tr;

            testRequests = new ArrayList();

            if (joinedTestRequests != "")
            {
                foreach (string request in joinedTestRequests.Split(objectSeparator))
                {
                    fields = request.Split(fieldSeparator);
                    tr = new FlattenedTestRequest();
                    tr.TestRequestID = Convert.ToInt16(fields[0]);
                    tr.TestID = Convert.ToInt16(fields[1]);
                    tr.TestName = fields[2];
                    tr.Restarts = Convert.ToInt16(fields[3]);
                    tr.RunOrder = Convert.ToInt16(fields[4]);
                    tr.Status = (FlattenedTestRequest.TestRequestStatus)Convert.ToInt16(fields[5]);
                    
                    testRequests.Add(tr);
                }
            }

            return testRequests;
        }

        private string PackageJobHangUps(ArrayList jobHangUps)
        {
            if (jobHangUps.Count > 0)
            {
                StringBuilder joinedJobHangUps = new StringBuilder();

                foreach (FlattenedJobHangUps jh in jobHangUps)
                {
                    joinedJobHangUps.Append(jh.JobHangUpID.ToString());
                    joinedJobHangUps.Append(fieldDelimiter);
                    joinedJobHangUps.Append(jh.HangUpID.ToString());
                    joinedJobHangUps.Append(fieldDelimiter);
                    joinedJobHangUps.Append(jh.HangUpName);
                    joinedJobHangUps.Append(fieldDelimiter);
                    joinedJobHangUps.Append(jh.HangUpDate.ToShortDateString());
                    joinedJobHangUps.Append(fieldDelimiter);
                    joinedJobHangUps.Append(jh.ResolveDate.ToShortDateString());
                    joinedJobHangUps.Append(fieldDelimiter);
                    joinedJobHangUps.Append(Convert.ToInt16(jh.Status).ToString());

                    joinedJobHangUps.Append(objectDelimiter);
                }

                joinedJobHangUps.Remove(joinedJobHangUps.Length - 1, 1);

                return joinedJobHangUps.ToString();
            }
            else
            {
                return "";
            }
        }

        private string PackageTestRequests(ArrayList testRequests)
        {
            if (testRequests.Count > 0)
            {
                StringBuilder joinedTestRequests = new StringBuilder();

                foreach (FlattenedTestRequest tr in testRequests)
                {
                    joinedTestRequests.Append(tr.TestRequestID.ToString());
                    joinedTestRequests.Append(fieldDelimiter);
                    joinedTestRequests.Append(tr.TestID.ToString());
                    joinedTestRequests.Append(fieldDelimiter);
                    joinedTestRequests.Append(tr.TestName);
                    joinedTestRequests.Append(fieldDelimiter);
                    joinedTestRequests.Append(tr.Restarts.ToString());
                    joinedTestRequests.Append(fieldDelimiter);
                    joinedTestRequests.Append(tr.RunOrder.ToString());
                    joinedTestRequests.Append(fieldDelimiter);
                    joinedTestRequests.Append(Convert.ToInt16(tr.Status).ToString());

                    joinedTestRequests.Append(objectDelimiter);
                }

                //Remove last objectDelimiter:
                joinedTestRequests.Remove(joinedTestRequests.Length - 1, 1);

                return joinedTestRequests.ToString();
            }
            else
            {
                return "";
            }

        }

        private ArrayList GetJobHangUps(string jobID)
        {
            string sql;
            SqlCommand sqlCommand;
            SqlDataReader sqlDataReader;

            sql = "SELECT   jh.JobHangUpID, jh.JobID, jh.HangUpID, h.HangUpName, jh.HangUpDate, jh.ResolveDate " +
                  "FROM     JobHangUp jh " +
                           "JOIN HangUp h ON jh.HangUpID=h.HangUpID " +
                  "WHERE    JobID=" + jobID +
                 " ORDER BY jh.HangUpDate";

            sqlCommand = new SqlCommand(sql, GetConnection());
            sqlCommand.CommandType = CommandType.Text;
            sqlDataReader = sqlCommand.ExecuteReader();

            jobHangUps = new ArrayList();

            while (sqlDataReader.Read())
            {
                jobHangUps.Add(new FlattenedJobHangUps(sqlDataReader.GetInt16(0), sqlDataReader.GetInt16(1), sqlDataReader.GetInt16(2), sqlDataReader.GetString(3), sqlDataReader.GetDateTime(4), sqlDataReader.GetDateTime(5), FlattenedJobHangUps.TestHangUpStatus.Unchanged));
            }

            sqlDataReader.Close();
            sqlDataReader.Dispose();
            sqlCommand.Connection.Close();
            sqlCommand.Dispose();

            return jobHangUps;
        }

        private ArrayList GetTestRequests(string jobID)
        {
            string sql;
            SqlCommand sqlCommand;
            SqlDataReader sqlDataReader;

            sql = "SELECT   tr.TestRequestID,tr.JobID,tr.TestID,t.TestName,tr.Restarts,tr.RunOrder " +
                  "FROM     TestRequest tr " +
                           "JOIN Test t ON tr.TestID=t.TestID " +
                  "WHERE    JobID=" + jobID +
                 " ORDER BY RunOrder";

            sqlCommand = new SqlCommand(sql, GetConnection());
            sqlCommand.CommandType = CommandType.Text;
            sqlDataReader = sqlCommand.ExecuteReader();

            testRequests = new ArrayList();

            while (sqlDataReader.Read())
            {
                testRequests.Add(new FlattenedTestRequest(sqlDataReader.GetInt16(0), sqlDataReader.GetInt16(1), sqlDataReader.GetInt16(2), sqlDataReader.GetString(3), sqlDataReader.GetByte(4), sqlDataReader.GetByte(5), FlattenedTestRequest.TestRequestStatus.Unchanged));
            }

            sqlDataReader.Close();
            sqlDataReader.Dispose();
            sqlCommand.Connection.Close();
            sqlCommand.Dispose();

            return testRequests;
        }

        //binddata for hang ups
        private void BindDataHangUp(ArrayList jobHangUps)
        {
            ArrayList nonDeletedHUs = new ArrayList();

            foreach (FlattenedJobHangUps jh in jobHangUps)
            {
                if (jh.Status != FlattenedJobHangUps.TestHangUpStatus.Deleted)
                {
                    nonDeletedHUs.Add(jh);
                }
            }

            DataGridHangUps.DataKeyField = "JobHangUpID";
            DataGridHangUps.DataSource = nonDeletedHUs;
            DataGridHangUps.DataBind();

            if (nonDeletedHUs.Count > 0)
            {
                this.LiteralRequestInstructions.Visible = true;
            }
            else
            {
                this.LiteralRequestInstructions.Visible = false;
            }
        }

        private void BindData(ArrayList testRequests)
        {
            ArrayList nonDeletedTRs = new ArrayList();

            foreach (FlattenedTestRequest tr in testRequests)
            {
                if (tr.Status != FlattenedTestRequest.TestRequestStatus.Deleted)
                {
                    nonDeletedTRs.Add(tr);
                }
            }

            DataGridTests.DataKeyField = "TestRequestID";
            DataGridTests.DataSource = nonDeletedTRs;
            DataGridTests.DataBind();

            if (nonDeletedTRs.Count > 0)
            {
                this.LiteralRequestInstructions.Visible = true;
            }
            else
            {
                this.LiteralRequestInstructions.Visible = false;
            }
        }

        public ArrayList GetTests()
        {
            string sql = "";
            SqlCommand sqlCommand;
            SqlDataReader sqlDataReader;
            string testTypeName = "";

            sql = "SELECT   t.TestID,t.TestName,t.TestTypeID,tt.TestTypeName " +
                  "FROM     Test t " +
                           "JOIN TestType tt ON t.TestTypeID=tt.TestTypeID " +
                  "ORDER BY tt.TestTypeName,t.TestName";

            sqlCommand = new SqlCommand(sql, GetConnection());
            sqlCommand.CommandType = CommandType.Text;
            sqlDataReader = sqlCommand.ExecuteReader();

            tests = new ArrayList();

            while (sqlDataReader.Read())
            {
                if (sqlDataReader.GetString(3) != testTypeName)
                {
                    if (testTypeName != "")
                    {
                        tests.Add(new FlattenedTest(0,"",0,""));
                    }

                    testTypeName = sqlDataReader.GetString(3);

                    tests.Add(new FlattenedTest(0, "---------- " + testTypeName + " ------------------------------------------------------------",0,""));
                }

                tests.Add(new FlattenedTest(sqlDataReader.GetInt16(0), sqlDataReader.GetString(1), sqlDataReader.GetByte(2), sqlDataReader.GetString(3)));
            }

            sqlDataReader.Close();
            sqlDataReader.Dispose();
            sqlCommand.Connection.Close();
            sqlCommand.Dispose();

            return tests;
        }

        public ArrayList GetHangUps()
        {
            string sql = "";
            SqlCommand sqlCommand;
            SqlDataReader sqlDataReader;

            sql = "SELECT   h.HangUpID, h.HangUpName " +
                  "FROM     HangUp h " +
                  "ORDER BY h.HangUpID";

            sqlCommand = new SqlCommand(sql, GetConnection());
            sqlCommand.CommandType = CommandType.Text;
            sqlDataReader = sqlCommand.ExecuteReader();

            hangUps = new ArrayList();

            while (sqlDataReader.Read())
            {
                hangUps.Add(new FlattenedHangUp(sqlDataReader.GetInt16(0), sqlDataReader.GetString(1)));
            }

            sqlDataReader.Close();
            sqlDataReader.Dispose();
            sqlCommand.Connection.Close();
            sqlCommand.Dispose();

            return hangUps;

        }

        public ArrayList GetRunOrders()
        {
            runOrders = new ArrayList();

            for (short i = 1; i < 31; i++)
            {
                runOrders.Add(new RunOrder(i, i.ToString()));
            }

            return runOrders;
        }

        public ArrayList GetRestarts()
        {
            restarts = new ArrayList();

            for (short i = 0; i < 11; i++)
            {
                restarts.Add(new Restart(i, i.ToString()));
            }

            return restarts;
        }

        public int GetTestNameIndex(string testName)
        {
            int i = 0;

            foreach(FlattenedTest flatTest in GetTests())
            {
                if(flatTest.TestName == testName)
                {
                    break;
                }

                i += 1;
            }

            return i;
        }
        
        public int GetRestartIndex(string restartValue)
        {
            int i = 0;

            foreach(Restart restart in GetRestarts())
            {
                if(restart.RestartValue == restartValue)
                {
                    break;
                }

                i += 1;
            }

            return i;
        }

        public int GetRunOrderIndex(string runOrderValue)
        {
            int i = 0;

            foreach (RunOrder runOrder in GetRunOrders())
            {
                if (runOrder.RunOrderValue == runOrderValue)
                {
                    break;
                }

                i += 1;
            }

            return i;
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

        protected void DataGridTests_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            FlattenedTestRequest testRequest;
            short testID;

            switch (e.CommandName)
            {
                case "Add":
                    testID = Convert.ToInt16(((DropDownList)e.Item.FindControl("AddTest")).SelectedItem.Value);

                    if (testID == 0)
                    {
                        this.LabelFormStatus.Text = "A blank row or heading was selected instead of a test. Please try again.";
                    }
                    else
                    {
                        testRequest = new FlattenedTestRequest();

                        testRequest.TestRequestID = 0;
                        testRequest.TestID = testID;
                        testRequest.TestName = ((DropDownList)e.Item.FindControl("AddTest")).SelectedItem.Text;
                        testRequest.Restarts = Convert.ToInt16(((DropDownList)e.Item.FindControl("AddRestart")).SelectedItem.Value);
                        testRequest.RunOrder = Convert.ToInt16(((DropDownList)e.Item.FindControl("AddRunOrder")).SelectedItem.Value);
                        testRequest.Status = FlattenedTestRequest.TestRequestStatus.New;

                        testRequests.Add(testRequest);

                        DataGridTests.ShowFooter = false;
                        BindData(testRequests);
                        this.ButtonAddTest.Enabled = true;
                    }
                    break;
            }
        }

        protected void DataGridTests_CancelCommand(object source, DataGridCommandEventArgs e)
        {
            DataGridTests.EditItemIndex = -1;
            DataGridTests.ShowFooter = false;
            BindData(testRequests);
            this.ButtonAddTest.Enabled = true;
        }

        protected void DataGridTests_DeleteCommand(object source, DataGridCommandEventArgs e)
        {
            FlattenedTestRequest tr;
            int indexToDelete = e.Item.ItemIndex;

            for (int index = 0; index < testRequests.Count; index++)
            {
                tr = (FlattenedTestRequest)testRequests[index];

                if (tr.Status == FlattenedTestRequest.TestRequestStatus.Deleted && index <= indexToDelete)
                    {
                        //Because the dataGrid doesn't contain already-deleted items but
                        //testRequests does, e.Item.ItemIndex won't necessarily correspond
                        //to the testRequests item that needs to be deleted.  Thus, need
                        //to increment e.Item.ItemIndex (indexToDelete) for each already-deleted
                        //item in front of the item that needs to be deleted now.
                        indexToDelete += 1;
                }
            }

            tr = (FlattenedTestRequest)testRequests[indexToDelete];

            if (tr.Status == FlattenedTestRequest.TestRequestStatus.New)
            {
                testRequests.Remove(tr);
            }
            else
            {
                tr.Status = FlattenedTestRequest.TestRequestStatus.Deleted;
            }

            DataGridTests.ShowFooter = false;
            DataGridTests.EditItemIndex = -1;

            BindData(testRequests);
        }

        protected void DataGridTests_EditCommand(object source, DataGridCommandEventArgs e)
        {
            DataGridTests.ShowFooter = false;
            DataGridTests.EditItemIndex = e.Item.ItemIndex;
            BindData(testRequests);
            this.ButtonAddTest.Enabled = false;
        }

        protected void DataGridTests_UpdateCommand(object source, DataGridCommandEventArgs e)
        {
            FlattenedTestRequest tr;
            short restarts = Convert.ToInt16(((DropDownList)e.Item.FindControl("EditRestart")).SelectedItem.Text);
            short runOrder = Convert.ToInt16(((DropDownList)e.Item.FindControl("EditRunOrder")).SelectedItem.Text);
            int indexToUpdate = e.Item.ItemIndex;

            for (int index = 0; index < testRequests.Count; index++)
            {
                tr = (FlattenedTestRequest)testRequests[index];

                if (tr.Status == FlattenedTestRequest.TestRequestStatus.Deleted && index <= indexToUpdate)
                    {
                        //Because the dataGrid doesn't contain deleted items but
                        //testRequests does, e.Item.ItemIndex won't necessarily correspond
                        //to the testRequests item that needs to be updated.  Thus, need
                        //to increment e.Item.ItemIndex (indexToUpdate) for each deleted
                        //item in front of the item that needs to be updated.
                        indexToUpdate += 1;
                }
            }

            tr = (FlattenedTestRequest)testRequests[indexToUpdate];

            tr.Restarts = restarts;
            tr.RunOrder = runOrder;

            if (tr.Status == FlattenedTestRequest.TestRequestStatus.Unchanged)
            {
                tr.Status = FlattenedTestRequest.TestRequestStatus.Updated;
            }

            DataGridTests.EditItemIndex = -1;
            BindData(testRequests);
            this.ButtonAddTest.Enabled = true;
        }

        private class FlattenedJobHangUps
        {
            private short _JobHangUpID;
            private short _JobID;
            private short _HangUpID;
            private string _HangUpName;
            private DateTime _HangUpDate;
            private DateTime _ResolveDate;

            private TestHangUpStatus _Status;

            public enum TestHangUpStatus
            {
                Deleted,
                New,
                Unchanged,
                Updated,
            }

            public short JobHangUpID
            {
                get
                {
                    return _JobHangUpID;
                }
                set
                {
                    this._JobHangUpID = value;
                }
            }

            public short JobID
            {
                get
                {
                    return _JobID;
                }
                set
                {
                    this._JobID = value;
                }
            }

            public short HangUpID
            {
                get
                {
                    return _HangUpID;
                }
                set
                {
                    this._HangUpID = value;
                }
            }

            public string HangUpName
            {
                get
                {
                    return _HangUpName;
                }
                set{
                    this._HangUpName = value;
                }
            }

            public DateTime HangUpDate
            {
                get
                {
                    return _HangUpDate;
                }
                set
                {
                    this._HangUpDate = value;
                }
            }

            public DateTime ResolveDate
            {
                get
                {
                    return _ResolveDate;
                }
                set
                {
                    this.ResolveDate = value;
                }
            }

            public TestHangUpStatus Status
            {
                get
                {
                    return _Status;
                }
                set
                {
                    this._Status = value;
                }
            }

            public FlattenedJobHangUps()
            {

            }

            public FlattenedJobHangUps(short jobHangUpID, short jobID, short hangUpID, string hangUpName, DateTime hangUpDate, DateTime resolveDate, TestHangUpStatus status)
            {
                this._JobHangUpID = jobHangUpID;
                this._JobID = jobID;
                this._HangUpID = hangUpID;
                this._HangUpName = hangUpName;
                this._HangUpDate = hangUpDate;
                this._ResolveDate = resolveDate;
                this._Status = status;
            }
        }

        private class FlattenedTestRequest
        {
            private short _TestRequestID;
            private short _JobID;
            private short _TestID;
            private string _TestName;
            private short _Restarts;
            private short _RunOrder;
            private TestRequestStatus _Status;

            public enum TestRequestStatus
            {
                Deleted,
                New,
                Unchanged,
                Updated,
            }

            public short TestRequestID
            {
                get
                {
                    return _TestRequestID;
                }
                set
                {
                    _TestRequestID = value;
                }
            }

            public short JobID
            {
                get
                {
                    return _JobID;
                }
                set
                {
                    _JobID = value;
                }
            }

            public short TestID
            {
                get
                {
                    return _TestID;
                }
                set
                {
                    _TestID = value;
                }
            }

            public string TestName
            {
                get
                {
                    return _TestName;
                }
                set
                {
                    _TestName = value;
                }
            }

            public short Restarts
            {
                get
                {
                    return _Restarts;
                }
                set
                {
                    _Restarts = value;
                }
            }

            public short RunOrder
            {
                get
                {
                    return _RunOrder;
                }
                set
                {
                    _RunOrder = value;
                }
            }

            public TestRequestStatus Status
            {
                get
                {
                    return _Status;
                }
                set
                {
                    _Status = value;
                }

            }

            public FlattenedTestRequest()
            {

            }

            public FlattenedTestRequest(short testRequestID, short jobID, short testID, string testName, short restarts, short runOrder, TestRequestStatus status)
            {
                this._TestRequestID = testRequestID;
                this._JobID = jobID;
                this._TestID = testID;
                this._TestName = testName;
                this._Restarts = restarts;
                this._RunOrder = runOrder;
                this._Status = status;
            }

        }

        private class FlattenedTest
        {
            private short _TestID;
            private string _TestName;
            private short _TestTypeID;
            private string _TestTypeName;

            public short TestID
            {
                get
                {
                    return _TestID;
                }
            }

            public string TestName
            {
                get
                {
                    return _TestName;
                }

                set
                {
                    _TestName = value;
                }
            }

            public short TestTypeID
            {
                get
                {
                    return _TestTypeID;
                }
            }

            public string TestTypeName
            {
                get
                {
                    return _TestTypeName;
                }
            }

            public FlattenedTest(short testID, string testName, short testTypeID, string testTypeName)
            {
                this._TestID = testID;
                this._TestName = testName;
                this._TestTypeID = testTypeID;
                this._TestTypeName = testTypeName;
            }
        }

        private class FlattenedHangUp
        {
            private short _HangUpID;
            private string _HangUpName;

            public short HangUpID
            {
                get
                {
                    return _HangUpID;
                }
                set
                {
                    this._HangUpID = value;
                }
            }

            public string HangUpName
            {
                get
                {
                    return _HangUpName;
                }
                set
                {
                    this._HangUpName = value;
                }
            }

            public FlattenedHangUp(short hangUpID, string hangUpName)
            {
                this._HangUpID = hangUpID;
                this._HangUpName = hangUpName;
            }
        }

        public class RunOrder
        {
            private short _RunOrderID;
            private string _RunOrderValue;

            public short RunOrderID
            {
                get
                {
                    return _RunOrderID;
                }
            }

            public string RunOrderValue
            {
                get
                {
                    return _RunOrderValue;
                }
            }

            public RunOrder(short runOrderID, string runOrderValue)
            {
                this._RunOrderID = runOrderID;
                this._RunOrderValue = runOrderValue;
            }
        }

        public class Restart
        {
            private short _RestartID;
            private string _RestartValue;

            public short RestartID
            {
                get
                {
                    return _RestartID;
                }
            }

            public string RestartValue
            {
                get
                {
                    return _RestartValue;
                }
            }

            public Restart(short restartID, string restartValue)
            {
                this._RestartID = restartID;
                this._RestartValue = restartValue;
            }
        }

        protected void ButtonAddTest_Click(object sender, EventArgs e)
        {
            DataGridTests.EditItemIndex = -1;
            BindData(testRequests);
            DataGridTests.ShowFooter = true;
            this.ButtonAddTest.Enabled = false;
        }

        protected void ButtonAddHangUps_Click(object sender, EventArgs e)
        {
            DataGridHangUps.EditItemIndex = -1;
            BindDataHangUp(jobHangUps);
            DataGridHangUps.ShowFooter = true;
            this.ButtonAddTest.Enabled = false;
        }

        private string ValidateEmployee(string role, string name, out short empID)
        {
            string errMsg = "";
            string sql = "";
            SqlCommand sqlCommand;
            SqlDataReader sqlDataReader;
            string[] names;
            string location;
            empID = 0;

            if (name.Contains(", ") & name.Contains("[") & name.Contains("]"))
            {
                names = name.Split(',');
                location = names[1].Substring(names[1].IndexOf("[") + 1, names[1].Length - (names[1].IndexOf("[") + 2)).Trim();
                names[1] = names[1].Substring(0, names[1].IndexOf("[") - 1).Trim();


                sql = "SELECT EmployeeID " +
                      "FROM   Employee " +
                      "WHERE  LastName='" + names[0] + "' AND " +
                             "FirstName='" + names[1] + "' AND " +
                             "Location='" + location + "'";

                sqlCommand = new SqlCommand(sql, GetConnection());
                sqlCommand.CommandType = CommandType.Text;
                sqlDataReader = sqlCommand.ExecuteReader();

                while (sqlDataReader.Read())
                {
                    if (empID > 0)
                    {
                        errMsg = role + "'s name is not unique. Please report this to the Request Tracker admin.";
                        break;
                    }
                    else
                    {
                        empID = sqlDataReader.GetInt16(0);
                    }
                }

                sqlDataReader.Close();
                sqlDataReader.Dispose();
                sqlCommand.Connection.Close();
                sqlCommand.Dispose();

                if (errMsg == "" && empID == 0)
                {
                    errMsg = "The " + role + " could not be found in the database.  Please try again.";
                }
            }
            else
            {
                errMsg = "Please select a " + role + " from the list.";
            }

            return errMsg;
        }

        private bool DataIsValid()
        {
            string errMsg = "";

            //Requestor:
            errMsg = ValidateEmployee("Requestor", this.TextBoxEmployee.Text, out employeeID);

            if (errMsg == "")
            {
                //Technician:
                errMsg = ValidateEmployee("Technician", this.TextBoxTechnician.Text, out technicianID);
            }

            //Quantity:
            if (errMsg == "")
            {
                if(this.DropDownListQuantity.SelectedIndex == 0)
                {
                    errMsg = "Please select a Quantity";
                }
            }

            //Tests:
            if (errMsg == "")
            {
                bool noTests = true;

                foreach (FlattenedTestRequest tr in testRequests)
                {
                    if (tr.Status != FlattenedTestRequest.TestRequestStatus.Deleted)
                    {
                        noTests = false;
                        break;
                    }
                }

                if (noTests)
                {
                    errMsg = "Please add one or more tests";
                }
            }

            //RunOrder:
            if (errMsg == "")
            {
                short numTests = 0;
                bool runOrderFound;

                foreach (FlattenedTestRequest tr in testRequests)
                {
                    if (tr.Status != FlattenedTestRequest.TestRequestStatus.Deleted)
                    {
                        numTests += 1;
                    }
                }

                for (short runOrder = 1; runOrder <= numTests; runOrder++)
                {
                    runOrderFound = false;

                    foreach (FlattenedTestRequest tr in testRequests)
                    {
                        if (tr.Status != FlattenedTestRequest.TestRequestStatus.Deleted)
                        {
                            if (tr.RunOrder == runOrder)
                            {
                                runOrderFound = true;
                                break;
                            }
                        }
                    }

                    if (!runOrderFound)
                    {
                        errMsg = "There can be no gaps or duplicates in the \"Run Order\" column.";
                        break;
                    }
                }
            }

            //Notes:
            if (errMsg == "")
            {
                if (this.TextBoxNotes.Text == "")
                {
                    errMsg = "The Notes field cannot be empty.";
                }
            }

            if (errMsg != "")
            {
                this.LabelFormStatus.Text = "There is a problem. " + errMsg;
                return false;
            }
            else
            {
                return true;
            }
        }

        private string GetCurrentDateTime()
        {
            DateTime curDateTime = DateTime.Now;
            string month = curDateTime.Month.ToString();
            string day = curDateTime.Day.ToString();
            string year = curDateTime.Year.ToString();
            string hour = curDateTime.Hour.ToString();
            string minute = curDateTime.Minute.ToString();
            string second = curDateTime.Second.ToString();
            string millisecond = curDateTime.Millisecond.ToString();

            return year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + second + "." + millisecond.ToString();
        }

        private string ReformatDate(string dateValue)
        {
            //Format of dateValue is MMM-dd-yyyy

            string[] dateParts = dateValue.Split(Convert.ToChar("-"));
            string month = "";

            switch (dateParts[0])
            {
                case "Jan":
                    month = "01";
                    break;

                case "Feb":
                    month = "02";
                    break;

                case "Mar":
                    month = "03";
                    break;

                case "Apr":
                    month = "04";
                    break;

                case "May":
                    month = "05";
                    break;

                case "Jun":
                    month = "06";
                    break;

                case "Jul":
                    month = "07";
                    break;

                case "Aug":
                    month = "08";
                    break;

                case "Sep":
                    month = "09";
                    break;

                case "Oct":
                    month = "10";
                    break;

                case "Nov":
                    month = "11";
                    break;

                case "Dec":
                    month = "12";
                    break;
            }

            return month + "-" + dateParts[1] + "-" + dateParts[2];
        }

        private string ReformatDate(DateTime dateValue)
        {
            //This function converts a DateTime into a string whose format is compatible with the 
            //format used by the DateTimePicker.

            return dateValue.ToString("MMM-dd-yyyy");
        }

        private string FormatStringForDatabase(string stringValue)
        {
            return stringValue.Replace("'", "''");
        }

        protected void ButtonSave_Click(object sender, EventArgs e)
        {
            StringBuilder sql = new StringBuilder();
            SqlConnection connection;
            SqlCommand cmd;
            SqlParameter param;
            SqlTransaction trans;
            string notes = this.TextBoxNotes.Text;
            string errorMessage = "";

            if (DataIsValid())
            {
                if (jobID != "0")
                {
                    //UPDATE
                    sql.Append("UPDATE Job SET RequestorID=");
                    sql.Append(employeeID.ToString());
                    sql.Append(",Notes='");
                    sql.Append(FormatStringForDatabase(notes));
                    sql.Append("',TechnicianID=");
                    sql.Append(FormatStringForDatabase(technicianID.ToString()));
                    sql.Append(",Quantity=");
                    sql.Append(this.DropDownListQuantity.SelectedValue);

                    sql.Append(",StartDate=");
                    if (this.TextBoxStartDate.Text == "")
                    {
                        sql.Append("NULL");
                    }
                    else
                    {
                        sql.Append("'");
                        sql.Append(ReformatDate(this.TextBoxStartDate.Text));
                        sql.Append("'");
                    }

                    sql.Append(",RequestDate=");
                    if (this.TextBoxRequestDate.Text == "")
                    {
                        sql.Append("NULL");
                    }
                    else
                    {
                        sql.Append("'");
                        sql.Append(ReformatDate(this.TextBoxRequestDate.Text));
                        sql.Append("'");
                    }

                    sql.Append(",ReceiveDate=");
                    if (this.TextBoxReceiveDate.Text == "")
                    {
                        sql.Append("NULL");
                    }
                    else
                    {
                        sql.Append("'");
                        sql.Append(ReformatDate(this.TextBoxReceiveDate.Text));
                        sql.Append("'");
                    }

                    sql.Append(",PromiseDate=");
                    if (this.TextBoxPromiseDate.Text == "")
                    {
                        sql.Append("NULL");
                    }
                    else
                    {
                        sql.Append("'");
                        sql.Append(ReformatDate(this.TextBoxPromiseDate.Text));
                        sql.Append("'");
                    }

                    sql.Append(",CompletionDate=");

                    if (this.TextBoxDate.Text == "")
                    {
                        sql.Append("NULL");
                    }
                    else
                    {
                        sql.Append("'");
                        sql.Append(ReformatDate(this.TextBoxDate.Text));
                        sql.Append("'");
                    }

                    sql.Append(" WHERE JobID=");
                    sql.Append(jobID);                    
                }
                else
                {
                    //INSERT
                    sql.Append("INSERT Job(RequestorID,EntryDate,Notes,Quantity,StartDate,CompletionDate,TechnicianID) VALUES(");
                    sql.Append(employeeID.ToString());
                    sql.Append(",'");
                    sql.Append(GetCurrentDateTime());
                    sql.Append("','");
                    sql.Append(FormatStringForDatabase(notes));
                    sql.Append("',");
                    sql.Append(this.DropDownListQuantity.SelectedValue);

                    sql.Append(",");
                    if (this.TextBoxStartDate.Text == "")
                    {
                        sql.Append("NULL");
                    }
                    else
                    {
                        sql.Append("'");
                        sql.Append(ReformatDate(this.TextBoxStartDate.Text));
                        sql.Append("'");
                    }

                    sql.Append(",");

                    if (this.TextBoxDate.Text == "")
                    {
                        sql.Append("NULL");
                    }
                    else
                    {
                        sql.Append("'");
                        sql.Append(ReformatDate(this.TextBoxDate.Text));
                        sql.Append("'");
                    }

                    sql.Append(",");
                    sql.Append(technicianID.ToString());

                    sql.Append(")");

                    sql.Append(" SET @JobID=SCOPE_IDENTITY()");
                }

                foreach (FlattenedTestRequest tr in testRequests)
                {
                    switch (tr.Status)
                    {
                        case FlattenedTestRequest.TestRequestStatus.New:
                            //INSERT
                            sql.Append(" INSERT TestRequest(JobID,TestID,Restarts,RunOrder) VALUES(");

                            if (jobID == "0")
                            {
                                sql.Append("@JobID");
                            }
                            else
                            {
                                sql.Append(jobID); 
                            }
                            
                            sql.Append(",");
                            sql.Append(tr.TestID.ToString());
                            sql.Append(",");
                            sql.Append(tr.Restarts.ToString());
                            sql.Append(",");
                            sql.Append(tr.RunOrder.ToString());
                            sql.Append(")");

                            break;

                        case FlattenedTestRequest.TestRequestStatus.Updated:
                            //UPDATE
                            sql.Append(" UPDATE TestRequest SET Restarts=");
                            sql.Append(tr.Restarts.ToString());
                            sql.Append(",RunOrder=");
                            sql.Append(tr.RunOrder.ToString());
                            sql.Append(" WHERE  TestRequestID=");
                            sql.Append(tr.TestRequestID.ToString());
                            break;

                        case FlattenedTestRequest.TestRequestStatus.Deleted:
                            //DELETE
                            sql.Append(" DELETE TestRequest WHERE TestRequestID=");
                            sql.Append(tr.TestRequestID);
                            break;
                    }
                }

                connection = GetConnection();

                trans = connection.BeginTransaction();

                cmd = connection.CreateCommand();
                cmd.CommandText = sql.ToString();
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = trans;

                if (jobID == "0")
                {
                    param = new SqlParameter();
                    param.Direction = ParameterDirection.Output;
                    param.ParameterName = "JobID";
                    param.SqlDbType = SqlDbType.SmallInt;

                    cmd.Parameters.Add(param);
                }

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch(Exception exc)
                {
                    errorMessage = exc.Message; // +"[" + sql.ToString() + "]";
                }

                if (errorMessage == "")
                {
                    if (jobID == "0")
                    {
                        jobID = cmd.Parameters["JobID"].Value.ToString();
                        this.ViewState["JobID"] = jobID;
                    }

                    errorMessage = SaveForm(jobID);
                }

                if (errorMessage == "")
                {
                    trans.Commit();
                }
                else
                {
                    trans.Rollback();
                }

                trans.Dispose();
                cmd.Connection.Close();
                cmd.Dispose();

                if (errorMessage == "")
                {
                    errorMessage = "Data successfully saved.";
                    ClearForm();
                    ShowJobData(jobID);
                    this.ButtonDelete.Visible = true;
                }
                else
                {
                    errorMessage += "There is a problem. ";
                }

                this.LabelFormStatus.Text = errorMessage;
            }
        }

        private string SaveForm(string jobID)
        {
            string errorMessage = "";

            if (this.FileUploadForm.HasFile)
            {
                try
                {
                    this.FileUploadForm.SaveAs(ConfigurationManager.AppSettings["FormsDirectory"] + jobID + ".pdf");
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                }
            }

            return errorMessage;
        }

        protected void ButtonBack_Click(object sender, EventArgs e)
        {
            if (jobID == "0")
            {
                Response.Redirect("home.aspx");
            }
            else
            {
                Response.Redirect("SearchResults.aspx");
            }
        }

        protected void ButtonHome_Click(object sender, EventArgs e)
        {
            Response.Redirect("home.aspx");
        }

        protected void ButtonDelete_Click(object sender, EventArgs e)
        {
            StringBuilder sql = new StringBuilder();
            SqlConnection connection;
            SqlCommand cmd;
            SqlTransaction trans;
            string errorMessage = "";

            foreach (FlattenedTestRequest tr in testRequests)
            {
                if(tr.TestRequestID > 0)
                {
                    sql.Append(" DELETE TestRequest WHERE TestRequestID=");
                    sql.Append(tr.TestRequestID.ToString());
                }
            }

            sql.Append(" DELETE Job WHERE JobID=");
            sql.Append(jobID);

            connection = GetConnection();

            trans = connection.BeginTransaction();

            cmd = connection.CreateCommand();
            cmd.CommandText = sql.ToString();
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = trans;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            if (errorMessage == "")
            {
                if(File.Exists(ConfigurationManager.AppSettings["FormsDirectory"] + jobID + ".pdf"))
                {
                    try
                    {
                        File.Delete(ConfigurationManager.AppSettings["FormsDirectory"] + jobID + ".pdf");
                    }
                    catch(Exception ex)
                    {
                        errorMessage = ex.Message;
                    }
                }
            }

            if (errorMessage == "")
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }

            trans.Dispose();
            cmd.Connection.Close();
            cmd.Dispose();

            if (errorMessage == "")
            {
                errorMessage = "Data successfully deleted.";

                ClearForm();

                this.PdfHyperLink.Visible = false;
                this.ButtonDelete.Visible = false;

                BindData(testRequests);
            }
            else
            {
                errorMessage += "There is a problem. ";
            }

            this.LabelFormStatus.Text = errorMessage;
        }
    }
}
