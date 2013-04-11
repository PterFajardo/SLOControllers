<%@ Page Language="C#" AutoEventWireup="true" CodeFile="IncidentControllerViewer.aspx.cs" Inherits="IncidentControllerViewer" %>

<%@ Register Src="~/UserControls/IncidentController.ascx" TagPrefix="uc1" TagName="IncidentController" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <uc1:IncidentController runat="server" ID="IncidentController" />
    </div>
    </form>
</body>
</html>
