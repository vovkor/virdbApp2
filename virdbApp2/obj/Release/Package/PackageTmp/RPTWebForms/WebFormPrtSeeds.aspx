<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebFormPrtSeeds.aspx.cs" Inherits="virdbApp2.RPTWebForms.WebFormPrtSeeds" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <asp:ScriptManager ID="ScriptManagerSeeds" runat="server">
        </asp:ScriptManager>
        <br />
        <asp:Label ID="lblAccenumber" runat="server" Text="Accenumb"></asp:Label>
        <br />
        <asp:TextBox ID="txtAccenumber" runat="server" Width="782px"></asp:TextBox>
        <br />
        <asp:Button ID="btnShow" runat="server" OnClick="btnShow_Click" Text="Показать" />
        <br />
        <br />
        <rsweb:ReportViewer ID="rptViewer" runat="server" Width="90%">
        </rsweb:ReportViewer>
    
    </div>
    </form>
</body>
</html>
