using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SLOIncidentWS;
using SLOCommonWS;
using SLOAssetWS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Dynamic;

public partial class IncidentController : System.Web.UI.UserControl
{
    private const string _IFactorStreetlightoutagesURL = "http://test.streetlightoutages.com";
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        var cService = new IFCommonService();
        var userToken = GetUserToken();
        var orgRecord = cService.GetOrganizationRecordInfoForUserToken(userToken);
        var orgStatusTypes = cService.GetStatusTypeRecordsByOrg(userToken);

        txtStartDate.Text = DateTime.Now.AddDays(-120).ToShortDateString();  //todo: reset back to 30
        txtEndDate.Text = DateTime.Now.ToShortDateString();

        Session["SloUserToken"] = userToken;
        Session["SloOrganization"] = orgRecord;
        Session["SloStatusTypes"] = orgStatusTypes;
        Session["SloStartDate"] = txtStartDate.Text;
        Session["SloEndDate"] = txtEndDate.Text;
        
        var incidents = GetIncidents();
        loadSLOGridView(incidents);
    }

    #region PageDisplaySetters
    private void loadSLOGridView(List<IncidentStructure> incidents)
    {
        sloGridView.DataSource = incidents;
        sloGridView.DataBind();
        //loadStatusTypes();
        setMessage();
    }

    private void setMessage()
    {
        
        //lblCount.Text = string.Format("{0} record(s)", sloGridView.Rows.Count.ToString());

        var msg = string.Format("Data Properties: From {0} to {1} w/ status type of \"{2}\"", txtStartDate.Text, txtEndDate.Text, ddlStatusTypes.SelectedItem.Text);

        if (!string.IsNullOrEmpty(txtSearch.Text.Trim()))
            msg =  string.Format("Data Properties: From {0} to {1} w/ status type of \"{2}\" and search value of \"{3}\"", txtStartDate.Text, txtEndDate.Text, ddlStatusTypes.SelectedItem.Text, txtSearch.Text.Trim());

        lblMessage.Text = msg;
    }

    private void loadStatusTypes()
    {
        //reset
       // ddlStatusTypes.Items.Clear();

        var recCount = ((List<IncidentStructure>)Session["IncidentList"]).Count();

        ddlStatusTypes.Items.Add(new ListItem(string.Format("Select All ({0})", recCount), "select_all"));

        foreach (var st in (StatusType[])Session["SloStatusTypes"])
        {
            recCount = ((List<IncidentStructure>)Session["IncidentList"]).Where(x => x.Status.Equals(st.DisplayName.Trim())).Count();
            ddlStatusTypes.Items.Add(new ListItem(string.Format("{0} ({1})", st.DisplayName, recCount), st.Name));
        }

        ddlStatusTypes.DataBind();


    }
    #endregion

    private List<IncidentStructure> GetIncidents()
    {
        var iService = new IFIncidentService();
        var orgTimeZone = GetTimeZoneInfo(((Organization)Session["SloOrganization"]).Configuration);


        DateTime iStartDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(Convert.ToDateTime(txtStartDate.Text).ToString()), orgTimeZone);
        DateTime iEndDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(Convert.ToDateTime(txtEndDate.Text).AddDays(1).AddSeconds(-1).ToString()), orgTimeZone);

        var incidents = iService.GetAllIncidentsByOrganizationAndDateRange(Session["SloUserToken"].ToString(), iStartDate, iEndDate);
        var incidentList = ParseIncidentData((StatusType[])Session["SloStatusTypes"], incidents, ((Organization)Session["SloOrganization"]).OrganizationName);
        
        Session["IncidentList"] = incidentList;
        ddlStatusTypes.Items.Clear();
        loadStatusTypes();

        return incidentList;
    }

    private string GetUserToken()
    {
        var cService = new IFCommonService();
        var userName = "ifcslo+phi@gmail.com";
        var password = "ifactorsgr8";
        var expiryMinutes = 200;

        return cService.GetUserToken(userName, password, expiryMinutes);
    }

    private List<IncidentStructure> ParseIncidentData(StatusType[] statusTypes, Incident[] incidents, string orgName)
    {
        var incidentList = new List<IncidentStructure>();

        foreach (var incident in incidents)
        {
            var i = new IncidentStructure();
            i.IncidentType = statusTypes.Where(x => x.DbId.Equals(incident.StatusTypeDbid)).Select(x => x.Name.Trim()).FirstOrDefault();
            i.Status = statusTypes.Where(x => x.DbId.Equals(incident.StatusTypeDbid)).Select(x => x.DisplayName.Trim()).FirstOrDefault();
            i.ReportDate = incident.CreationTime;
            i.Name = GetJsonData("contactname", string.Empty, incident.JsonData);
            i.Phone = GetJsonData("phone", string.Empty, incident.JsonData);
            i.Email = GetJsonData("email", string.Empty, incident.JsonData);
            i.PoleNumber = GetJsonData("polenumber", string.Empty, incident.JsonData);
            i.ProblemType = GetJsonData("problemtype", string.Empty, incident.JsonData);
            i.Address = GetJsonData("sloStreetLightAddress", string.Empty, incident.JsonData);
            i.AdditionalInfo = TransformAdditionalInfo(incident.JsonData, orgName);
            i.LOA = GetJsonData("LOA", string.Empty, incident.JsonData);
            i.Comments = HttpUtility.HtmlDecode(GetJsonData("comments", string.Empty, incident.JsonData));
            i.Latitude = incident.Latitude;
            i.Longitude = incident.Longtitude;
            i.sloID = incident.DbId;
            i.Submit = HttpUtility.HtmlDecode(TransformSubmit(i.IncidentType, i.sloID));
            i.AssetDbId = incident.AssetDbId;

            if (i.IncidentType.ToLower().Equals("reported") || i.IncidentType.ToLower().Equals("resolved") || i.IncidentType.ToLower().Equals("in_progress"))
            {
                i.Locate = HttpUtility.HtmlDecode(string.Format("<a href=\"javascript:iFactor.StormCenter.gotoIncident('{0}','{1}', '{2}')\"  title = \"Go to Incident\">"
                                         + "<img src=\"{3}/images/goto.gif\"  style=\"width:15px;height:15px;display:inline\"/> </a>",
                                            i.Latitude, i.Longitude, i.sloID, _IFactorStreetlightoutagesURL));
            }

            incidentList.Add(i);
        }
        return incidentList;
    }

    #region Helper Functions
    private string TransformSubmit(string statusType, int sloId)
    {
        var statusTypeImgUrl = string.Format("{0}/images/", _IFactorStreetlightoutagesURL);
        var title = string.Empty;

        if (statusType.ToLower().Contains("report"))
        {
            statusTypeImgUrl = string.Format("{0}light_red.gif", statusTypeImgUrl);
            title = "Click to Manually Repair Light";
        }
        else if (statusType.ToLower().Contains("resolve") || statusType.ToLower().Contains("progress"))
        {
            statusTypeImgUrl = string.Format("{0}light_orange.gif", statusTypeImgUrl);
            title = "Click to Close Incident";
        }
        else if (statusType.ToLower().Contains("reject"))
        {
            statusTypeImgUrl = string.Format("{0}light_red.gif", statusTypeImgUrl);
            title = "Click to Resubmit Incident";
        }
        else
        {
            return string.Format("<img src=\"{0}light_green.gif\" style=\"display:inline\" title=\"{1}\" />", statusTypeImgUrl, statusType);
        }

        return string.Format("<a href=\"javascript:iFactor.StormCenter.changeStatusBySLOID('{0}', '{1}')\" title=\"{2}\" />"
                             + "<img src=\"{3}\" style=\"display:inline\" /> </a>", sloId, statusType, title, statusTypeImgUrl);
    }

    private string TransformAdditionalInfo(string jsonData, string orgName)
    {
        var problemType = GetJsonData("problemType", string.Empty, jsonData);
        var additionalInfo = string.Format("Address: {0}{1}", GetJsonData("sloStreetLightAddress", string.Empty, jsonData), Environment.NewLine);
        var comments = GetJsonData("comments", string.Empty, jsonData);

        // NIPSCO Requirement: Add "problem type" into the additional info section (with the address and comments) 
        // and replace this with the SLO ticket number.
        if (orgName.Trim().ToLower().Equals("nipsco"))
        {
            if (!string.IsNullOrEmpty(problemType))
                additionalInfo = string.Format("{0}{1}Problem Type: {2}{3}", additionalInfo, Environment.NewLine, problemType, Environment.NewLine);
        }

        if (!string.IsNullOrEmpty(comments))
            additionalInfo = string.Format("{0}<u>Comments</u>{1}", additionalInfo, WordWrap(comments, 100));

        return additionalInfo;
    }

    private string GetJsonData(string keyName, string defaultValue, string jsonRecord)
    {
        try
        {
            var o = JObject.Parse(jsonRecord);

            if (o[keyName] == null)
            {
                return defaultValue;
            }
            else
            {
                return o[keyName].Value<string>().Trim();
            }
        }
        catch (Exception e)
        {
        }

        return null;
    }

    private TimeZoneInfo GetTimeZoneInfo(string orgConfig)
    {
        try
        {
            // look at the organization's configuration data for timeZone.
            var timeZoneName = (String)GetJsonData("timeZone", "Eastern Standard Time", orgConfig);

            return System.TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    /// <summary>
    /// simple method to insert suitable newlines in aText so that each line is no longer than maxWidth
    /// </summary>
    /// <param name="aText"></param>
    /// <param name="maxWidth"></param>
    /// <returns></returns>
    private String WordWrap(String aText, int maxWidth)
    {
        String returnString = "";

        String[] lines = aText.Split(new String[] { Environment.NewLine }, StringSplitOptions.None); // splits by newline

        if (lines.Length > 1)
        {
            foreach (String aLine in lines)
            {
                returnString += WordWrap(aLine, maxWidth);
            }
        }
        else
        {
            String aString = lines[0];

            if (aString.Length <= maxWidth)
            {
                returnString = aString;
            }
            else
            {
                String remainingString = aString;

                while (remainingString.Length > 0)
                {
                    if (remainingString.Length <= maxWidth)
                    {
                        returnString += remainingString.Trim() + Environment.NewLine;
                        remainingString = "";
                    }
                    else
                    {
                        int idx = remainingString.LastIndexOf(" ", maxWidth);
                        if (idx == -1)
                        {
                            idx = maxWidth;
                        }

                        returnString += remainingString.Substring(0, idx).Trim() + Environment.NewLine;
                        remainingString = remainingString.Substring(idx).Trim();
                    }
                }
            }
        }
        return returnString;
    }

    #endregion

    #region Events
    protected void txtSearch_TextChanged(object sender, EventArgs e)
    {
        Session["FilteredIncidentList"] = null;
        RefreshGrid();
        txtSearch.Text = string.Empty;
        ddlStatusTypes.SelectedIndex = 0;
    }
    protected void btnSearchOnClick(object sender, EventArgs e)
    {
        Session["FilteredIncidentList"] = null;
        RefreshGrid();
        txtSearch.Text = string.Empty;
        ddlStatusTypes.SelectedIndex = 0;
    }

    protected void txtStartDate_TextChanged(object sender, EventArgs e)
    {
        RefreshGrid();
    }
    protected void txtEndDate_TextChanged(object sender, EventArgs e)
    {
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        var searchString = txtSearch.Text.ToLower().Trim();
        var incidents = new List<IncidentStructure>();

        if (!string.IsNullOrEmpty(searchString))
        {
            if (Session["SloStartDate"].ToString().Equals(txtStartDate.Text) ||
                Session["SloEndDate"].ToString().Equals(txtEndDate.Text))
            {
                //data has not change use session
                incidents = (List<IncidentStructure>)Session["IncidentList"];
            }
            else
            {
                //user wants a new dataset
                incidents = GetIncidents();

            }

            #region Search
            incidents = incidents.Where(i =>
                                 i.sloID.ToString().Contains(searchString) ||
                                         i.Status.ToLower().Contains(searchString) ||
                                         i.ReportDate.ToString().ToLower().Contains(searchString) ||
                                         i.Name.ToLower().Contains(searchString) ||
                                         i.Phone.ToLower().Contains(searchString) ||
                                         i.Email.ToLower().Contains(searchString) ||
                                         i.PoleNumber.ToLower().Contains(searchString) ||
                                         i.ProblemType.ToLower().Contains(searchString) ||
                                         i.AdditionalInfo.ToLower().Contains(searchString) ||
                                         i.Address.ToLower().Contains(searchString) ||
                                         i.Comments.ToLower().Contains(searchString) ||
                                         i.Latitude.ToString().ToLower().Contains(searchString) ||
                                         i.Longitude.ToString().ToLower().Contains(searchString) ||
                                         i.LOA.ToLower().Contains(searchString)).ToList();

            Session["FilteredIncidentList"] = incidents;      //keep this is session for reset option
            #endregion
        }
        else
        {
            if (!Session["SloStartDate"].ToString().Equals(txtStartDate.Text) ||
                !Session["SloEndDate"].ToString().Equals(txtEndDate.Text))
            {
                //user wants a new dataset
                incidents = GetIncidents();
            }
            else
            {
                if (Session["FilteredIncidentList"] != null)
                {
                    incidents = (List<IncidentStructure>)Session["FilteredIncidentList"];
                }
                else
                {
                    incidents = (List<IncidentStructure>)Session["IncidentList"];
                }
            }

        }
        loadSLOGridView(incidents);
    }

    protected void ddlStatusTypes_SelectedIndexChanged(object sender, EventArgs e)
    {
        var incidents = (List<IncidentStructure>)Session["IncidentList"];

        if (!ddlStatusTypes.SelectedItem.Value.Equals("select_all"))
        {
           incidents = incidents.Where(x => x.IncidentType.ToLower().Equals(ddlStatusTypes.SelectedItem.Value.ToString().ToLower())).ToList();
           Session["FilteredIncidentList"] = incidents;
        }
       
        loadSLOGridView(incidents);
    }

    protected void chkMinimized_CheckedChanged(object sender, EventArgs e)
    {
        if (chkMinimized.Checked)
        {
            sloGridView.Columns[5].Visible = false;
            sloGridView.Columns[6].Visible = false;
            sloGridView.Columns[7].Visible = false;
        }
        else
        {
            sloGridView.Columns[5].Visible = true;
            sloGridView.Columns[6].Visible = true;
            sloGridView.Columns[7].Visible = true;
        }
    }

    #region GridEvents

    #region GridSorting
    public SortDirection SloGridViewSortDirection
    {
        get {
            if (ViewState["sortDirection"] == null)
                ViewState["sortDirection"] = SortDirection.Ascending;

            return (SortDirection)ViewState["sortDirection"];
        }
        set {
            ViewState["sortDirection"] = value;
        }
    } 

    protected void sloGridView_Sorting(object sender, GridViewSortEventArgs e)
    {
        string sortExpression = e.SortExpression;
        int pageIndex = sloGridView.PageIndex;

        var sortedIncidents = Session["FilteredIncidentList"] == null ? ((List<IncidentStructure>)Session["IncidentList"]).AsQueryable() : ((List<IncidentStructure>)Session["FilteredIncidentList"]).AsQueryable();
        
        if (SloGridViewSortDirection.Equals(SortDirection.Ascending))
        {
            SloGridViewSortDirection = SortDirection.Descending;
            sortedIncidents = sortedIncidents.OrderBy(string.Format("{0} ASC", sortExpression));
        }
        else
        {
            SloGridViewSortDirection = SortDirection.Ascending;
            sortedIncidents = sortedIncidents.OrderBy(string.Format("{0} DESC", sortExpression));
        }

        loadSLOGridView(sortedIncidents.ToList());
        sloGridView.PageIndex = pageIndex;
    }
    #endregion

    #region GridPaging
    protected void sloGridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        var incidents = Session["FilteredIncidentList"] == null ? ((List<IncidentStructure>)Session["IncidentList"]) : ((List<IncidentStructure>)Session["FilteredIncidentList"]);

        incidents = incidents.Skip(e.NewPageIndex -1).ToList();

        loadSLOGridView(incidents);
        sloGridView.PageIndex = e.NewPageIndex;
        sloGridView.DataBind();

        mpe.Hide();
    }
    #endregion

    protected void sloGridView_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        try
        {
            var rowIndex = Convert.ToInt32(e.CommandArgument); //this will fail without the try catch

            var sloId = sloGridView.DataKeys[rowIndex].Value;

            var i = (((List<IncidentStructure>)Session["IncidentList"]).Where(x => x.sloID.Equals(sloId))).First();

            lblSloId.Text = i.sloID.ToString();
            lblReportDate.Text = i.ReportDate.ToString();
            lblStatus.Text = i.Status;
            lblName.Text = i.Name;
            lblPhone.Text = i.Phone;
            lblEmail.Text = i.Email;
            lblComments.Text = i.Comments;
            lblAddress.Text = i.Address;
            lblPoleNumber.Text = i.PoleNumber;
            lblProblemType.Text = i.ProblemType;
            lblAdditionalInfo.Text = i.AdditionalInfo;
            lblLatitude.Text = i.Latitude.ToString();
            lblLongitude.Text = i.Longitude.ToString();
            lblLoa.Text = i.LOA;
            Session["sloAssetDbId"] = i.AssetDbId;

            GetAssetInformation();
            //GetDisplayHtml();

            RenderIncidentMapLocation(i);

            mpe.Show();
        }
        catch (Exception ex)
        {
            //don't do anything: Try Catch handles the sorting event
        }

    }

    private void GetAssetInformation()
    {
        if (Session["sloAssetDbId"] != null)
        {
            var assetDbId = Convert.ToInt32(Session["sloAssetDbId"]);

            if (!assetDbId.Equals(-1))
            {
                var aService = new IFAssetService();
                var asset = (aService.GetAssetByDbId(Session["SloUserToken"].ToString(), assetDbId)).First();
                var jsonData = JObject.Parse(asset.JsonData).ToString().Replace("\"", string.Empty).Replace("{\r\n", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty).Split((char)',');

                var record = string.Empty;

                foreach (var data in jsonData)
                {
                    var fieldName = data.Substring(0, data.IndexOf(":") + 1).Trim();
                    var field = data.Substring(data.IndexOf(":") + 1).Trim();

                    record = record + string.Format("<b>{0}</b> {1}</br>", fieldName, field);

                }

                lblAssetDetail.Text = record;
            }
        }
    }

    //TODO: need to figure out image
    //private void GetDisplayHtml()
    //{
    //    var assetDbId = Convert.ToInt32(Session["sloAssetDbId"]);
    //    var aService = new IFAssetService();
    //    var display = (aService.GetDisplayHTML(Session["SloUserToken"].ToString(), assetDbId));

    //    lblAssetImage.Text = display.InnerXml.ToString();
    //}

    private void RenderIncidentMapLocation(IncidentStructure i)
    {
        var mDomain = string.Format("{0}/embed.aspx", _IFactorStreetlightoutagesURL);
        var mWidth = 490;
        var mHeight = 350;

        var userToken = HttpUtility.HtmlDecode(Session["SloUserToken"].ToString());
        userToken = HttpUtility.UrlEncode(userToken);

        var sourceValue = string.Format("{0}?width={1}&height={2}&usertoken={3}&start_lat={4}&start_long={5}&start_zoom=18&mgmt=true&loginlink=false&feedback=false",
                                            mDomain, mWidth, mHeight, userToken, i.Latitude.ToString(), i.Longitude.ToString());

        managementFrame.Attributes.Add("class", "streetlightmap");
        managementFrame.Attributes.Add("src", sourceValue);
        managementFrame.Attributes.Add("scrolling", "no");
        managementFrame.Attributes.Add("frameborder", "0");
        managementFrame.Attributes.Add("width", mWidth + "px");
        managementFrame.Attributes.Add("height", mHeight - 50 + "px");
        managementFrame.Style.Add("width", mWidth + "px");
        managementFrame.Style.Add("border", "1px solid #a5b2bd");

        managementFrame.InnerHtml = "Your browser does not support iFrames";
    }
    #endregion

    #endregion

    public class IncidentStructure
    {
        public int sloID { get; set; }
        public string IncidentType { get; set; }
        public string Status {get;set;}
        public DateTime? ReportDate {get;set;}
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string PoleNumber { get; set; }
        public string ProblemType { get; set; }
        public string AdditionalInfo { get; set; }
        public string Address { get; set; }
        public string Comments { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LOA { get; set; }
        public string Locate { get; set; }
        public string Submit { get; set; }
        public int AssetDbId { get; set; }
    }

    
}