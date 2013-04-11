<%@ Control Language="C#" AutoEventWireup="true" CodeFile="IncidentController.ascx.cs" Inherits="IncidentController" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register Assembly="SLOControlLibrary" Namespace="SLOControlLibrary" TagPrefix="cc1" %>
   
 <asp:ToolkitScriptManager ID="SloScriptManager1000" runat="server"/>
<style>
    .ModalBackground
    {
        background-color:Gray;
        filter:alpha(opacity=50);
        opacity:0.5;
    }
</style>
<div>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table border="0">
                <tr>
                    <td style="font-family:Verdana; font-size:small; text-align:right;width:150px; font-weight:bold">
                         Keyword Search:
                    </td>
                    <td>
                        <asp:TextBox ID="txtSearch" runat="server" ReadOnly="false" Width="120px" OnTextChanged="txtSearch_TextChanged" AutoPostBack="true" />
                        &nbsp;
                        <asp:Button ID="btnSearch" runat="server" OnClick="btnSearchOnClick" Text="Search" />
                    </td>
                </tr>
                <tr>
                    <td style="font-family:Verdana; font-size:small; text-align:right; font-weight:bold">
                        Status Types:
                    </td>
                    <td>
                        <asp:DropDownList ID="ddlStatusTypes" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlStatusTypes_SelectedIndexChanged" />
                    </td>
                </tr>
                <tr>
                    <td style="font-family:Verdana; font-size:small; text-align:right; font-weight:bold">
                        Date Range:
                    </td>
                    <td style="font-family:Verdana; font-size:small;">
                        From:&nbsp; 
                        <asp:TextBox ID="txtStartDate" runat="server"  Width="70px" OnTextChanged="txtStartDate_TextChanged" AutoPostBack="true" />
                        <asp:CalendarExtender ID="CalendarExtender1" TargetControlID="txtStartDate" runat="server"  />
                        To:&nbsp;
                         <asp:TextBox ID="txtEndDate" runat="server"  Width="70px" OnTextChanged="txtEndDate_TextChanged" AutoPostBack="true" />
                        <asp:CalendarExtender ID="CalendarExtender2" TargetControlID="txtEndDate" runat="server"/>
                    </td>
                </tr>
                <tr>
                    <td colspan="2"></td>
                </tr>
                 <tr>
                    <td colspan="2">
                        <table style="width:100%; font-size:x-small; font-family:Verdana;">
                            <tr>
                                <td>
                                    <asp:Label ID="lblMessage" runat="server" />
                                </td>
                                <td style="text-align:right">
                                   <%-- <asp:Label ID="lblCount" runat="server" />--%>
                                </td>
                            </tr>
                        </table>
                
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <asp:GridView id="sloGridView" AutoGenerateColumns="false" runat="server"
                            AllowPaging="true" OnPageIndexChanging="sloGridView_PageIndexChanging" 
                            AllowSorting="true" OnSorting="sloGridView_Sorting"
                            DataKeyNames="SloId" OnRowCommand="sloGridView_RowCommand"
                            EmptyDataText="No Incidents Available" >
                            <Columns>
                                <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <%#DataBinder.Eval(Container.DataItem, "Locate") %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Action" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <%#DataBinder.Eval(Container.DataItem, "Submit") %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="SloId"  ReadOnly="true" HeaderText="SLO Id" SortExpression="SloId" />
                                <asp:BoundField DataField="Status"  ReadOnly="true" HeaderText="Status" SortExpression="Status" />
                                <asp:BoundField DataField="ReportDate"  ReadOnly="true" HeaderText="Report Date (EST)" SortExpression="ReportDate" />
                                <asp:BoundField DataField="Name"  ReadOnly="true" HeaderText="Contact Name" SortExpression="Name" />
                                <asp:BoundField DataField="Phone"  ReadOnly="true"  HeaderText="Contact Phone" SortExpression="Phone"/>
                                <asp:BoundField DataField="Email"  ReadOnly="true"  HeaderText="Contact Email" SortExpression="Email"/>
                                <asp:BoundField DataField="PoleNumber"  ReadOnly="true" HeaderText="Pole Number" SortExpression="PoleNumber" />
                                <asp:BoundField DataField="ProblemType"  ReadOnly="true" HeaderText="Problem Type" SortExpression="ProblemType" />
                                <asp:ButtonField ButtonType="Link" Text="detail" CommandName="sloGridView_RowCommand" />
                            </Columns>
                        </asp:GridView>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <asp:CheckBox ID="chkMinimized" runat="server" Text="Minimized" OnCheckedChanged="chkMinimized_CheckedChanged" AutoPostBack="true" />
                    </td>
                </tr>
            </table>
            <asp:Button ID="clientButton" runat="server" BackColor="White" BorderStyle="None" />
            <asp:Panel ID="ModalPanel" runat="server" Width="500px" BackColor="White" Font-Names="Verdana" Font-Size="small">
                <table style="width:100%">
                    <tr>
                        <td style="text-align:center; background-color:darkorange; font-weight:bold; font-size:medium">StreetLightOutages.com Incident Report</td>
                    </tr>
                    <tr>
                        <td>
                            <asp:CollapsiblePanelExtender ID="cpe1" runat="server" TargetControlID="pnlIncidentDetail" 
                                ExpandControlID="pnlTitleIncidentDetail" CollapseControlID="pnlTitleIncidentDetail" 
                                CollapsedText="Incident (Show...)" ExpandedText="Incident (Hide...)" 
                                TextLabelID="lblIncidentPanelTitle" ExpandDirection="Vertical" Collapsed="false"  />
                            <asp:CollapsiblePanelExtender ID="cpe2" runat="server" TargetControlID="pnlAssetDetail" 
                                ExpandControlID="pnlTitleAssetDetail" CollapseControlID="pnlTitleAssetDetail" 
                                CollapsedText="Asset (Show...)" ExpandedText="Asset (Hide...)" 
                                TextLabelID="lblAssetPanelTitle" ExpandDirection="Vertical" Collapsed="true"/>  
                            <asp:CollapsiblePanelExtender ID="cpe3" runat="server" TargetControlID="pnlIncidentMap" 
                                ExpandControlID="pnlTitleMap" CollapseControlID="pnlTitleMap" 
                                CollapsedText="Map (Show...)" ExpandedText="Map (Hide...)" 
                                TextLabelID="lblMapPanelTitle" ExpandDirection="Vertical" Collapsed="true" />    
                            <asp:Panel ID="pnlTitleIncidentDetail" runat="server" BackColor="Orange" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px"><asp:Label ID="lblIncidentPanelTitle" Font-Bold="true"  runat="server" /></asp:Panel>
                                <asp:Panel id="pnlIncidentDetail" runat="server">
                                    <table style="width:100%">
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">Report Date:</td>
                                            <td><asp:Label ID="lblReportDate" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">Status:</td>
                                            <td><asp:Label ID="lblStatus" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">SLO Id:</td>
                                            <td><asp:Label ID="lblSloId" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">Contact Name:</td>
                                            <td><asp:Label ID="lblName" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">Contact Phone:</td>
                                            <td><asp:Label ID="lblPhone" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">Contact Email:</td>
                                            <td><asp:Label ID="lblEmail" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">Pole Number:</td>
                                            <td><asp:Label ID="lblPoleNumber" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">Address:</td>
                                            <td><asp:Label ID="lblAddress" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">Problem Type:</td>
                                            <td><asp:Label ID="lblProblemType" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">Comments:</td>
                                            <td><asp:Label ID="lblComments" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">Add'l Info:</td>
                                            <td><asp:Label ID="lblAdditionalInfo" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">Latitude:</td>
                                            <td><asp:Label ID="lblLatitude" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">Longitude:</td>
                                            <td><asp:Label ID="lblLongitude" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right; font-weight:bold">LOA:</td>
                                            <td><asp:Label ID="lblLoa" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td colspan="2" style="text-align:right">
                                                <asp:Button ID="CancelButton" runat="server" Text="Cancel" />
                                                &nbsp;&nbsp;
                                                <asp:Button ID="OKButton" runat="server" Text="Submit" />
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>    
                            <asp:Panel ID="pnlTitleAssetDetail" runat="server" BackColor="Orange" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px"><asp:Label ID="lblAssetPanelTitle" Font-Bold="true"  runat="server" />
                            </asp:Panel>
                                <asp:Panel ID="pnlAssetDetail" runat="server">
                                    <table style="width:100%">
                                        <tr>
                                            <td colspan="2">
                                                <asp:Label ID="lblAssetDetail" runat="server" />
                                                <asp:Label ID="lblAssetImage" runat="server" />
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                            <asp:Panel ID="pnlTitleMap" runat="server" BackColor="Orange" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px"><asp:Label ID="lblMapPanelTitle" Font-Bold="true"  runat="server" /></asp:Panel>    
                                <asp:Panel ID="pnlIncidentMap" runat="server">
                                    <table style="width:100%">
                                        <tr>
                                            <td colspan="2">
                                                <iframe name="managementFrame" id="managementFrame" runat="server" scrolling="no" frameborder="0">Your browser does not support iFrames</iframe>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:ModalPopupExtender ID="mpe" runat="server" TargetControlId="clientButton" PopupControlID="ModalPanel" OkControlID="OKButton" CancelControlID="CancelButton" 
                BackgroundCssClass="ModalBackground" RepositionMode="RepositionOnWindowResizeAndScroll"  />
            <asp:DragPanelExtender ID="dpe1" runat="server" TargetControlID="ModalPanel" />
        </ContentTemplate>
    </asp:UpdatePanel>
    
    
    
   

    

</div>