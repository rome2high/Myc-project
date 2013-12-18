using System;
using System.Data;
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
    public partial class GetForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string jobID = this.Request.QueryString["id"];
            string fileName = ConfigurationManager.AppSettings["FormsDirectory"] + jobID + ".pdf";                            
            this.Response.ContentType = "Application/pdf";
            this.Response.WriteFile(fileName);
            this.Response.End();
        }
    }
}
